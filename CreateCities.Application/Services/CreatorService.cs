using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using CreateCities.Application.CityHHModel;
using CreateCities.Application.CityRFModels;
using CreateCities.Application.FiasModels;
using CreateCities.Application.Interfaces;
using CreateCities.Application.YandexGeoModels;
using GeoTimeZone;
using Microsoft.Extensions.Configuration;

namespace CreateCities.Application.Services;

public class CreatorService : ICreatorService
{
    /// <summary>
    /// Конфигурация приложения
    /// </summary>
    private readonly IConfiguration _config;
    /// <summary>
    /// Определяет ограничение для количества запросов к API
    /// </summary>
    private const int CONSTRAIT_REQUEST_TO_API = 30;

    public CreatorService(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Основная функция, которая организует создание списка городов в России.
    /// Если файл Cities.json не существует, он считывает данные из Fias.json и AllCities.json,
    /// обрабатывает их и создает новый JSON-файл с результирующими данными.
    /// Если файл Cities.json существует, он вызывает функцию GetCoordinatesForCities для обновления координат и часовых поясов городов.
    /// </summary>
    public async Task Run()
    {
        // Проверка существования файла Cities.json
        if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\Data\\Cities.json"))
        {
            // Чтение данных из Fias.json и AllCities.json
            var fias = GetListObjectFromJsonFile<Fias>($"{AppDomain.CurrentDomain.BaseDirectory}\\Data\\Fias.json");
            var cityHHru =
                GetListObjectFromJsonFile<City_HHRU>($"{AppDomain.CurrentDomain.BaseDirectory}\\Data\\AllCities.json");

            // Инициализация объекта resultCountry
            var resultCountry = new CountryRf();
            var counterRegions = 1;
            var counterCities = 1;

            // Обработка данных и создание объекта resultCountry
            foreach (var country in cityHHru)
            {
                if (country.id.Equals("113"))
                {
                    resultCountry.Name = country.name;
                    resultCountry.Id = 1;
                    resultCountry.Regions = new List<Region_rf>();

                    foreach (var region in country.areas!)
                    {
                        var localRegion = new Region_rf
                        {
                            Name = region.name,
                            CountryId = 1,
                            Id = counterRegions,
                            Cities = new List<City_rf>()
                        };

                        foreach (var location in region.areas!)
                        {
                            double latitude = 0;
                            double longitude = 0;
                            string timeZone = "UTC";

                            // Поиск соответствующего города в данных Fias
                            var cityFias = fias.FirstOrDefault(x =>
                                x.City.Equals(location.name) && x.Region.Contains(localRegion.Name
                                    .Replace("Республика ", "").Replace(" область", "").Replace(" край", "")
                                    .Replace(" Республика", "")));

                            if (cityFias != null)
                            {
                                // Обновление широты, долготы и часового пояса из данных Fias
                                latitude = double.Parse(ReplaceSeparator(cityFias.Latitude));
                                longitude = double.Parse(ReplaceSeparator(cityFias.Longitude));
                                timeZone = cityFias.TimeZone;
                            }

                            // Добавление города в localRegion
                            localRegion.Cities.Add(new City_rf
                            {
                                Id = counterCities,
                                RegionId = localRegion.Id,
                                NameCity = location.name,
                                Latitude = latitude,
                                Longitude = longitude,
                                TimeZone = timeZone
                            });
                            counterCities++;
                        }

                        // Добавление localRegion в resultCountry
                        resultCountry.Regions.Add(localRegion);
                        counterRegions++;
                    }
                }
            }

            // Сериализация объекта resultCountry в файл Cities.json
            SerializeObjectToFile<CountryRf>(resultCountry,
                $"{AppDomain.CurrentDomain.BaseDirectory}\\Data\\Cities.json");
        }
        else
        {
            // Если файл Cities.json существует, вызов функции GetCoordinatesForCities для обновления координат и часовых поясов городов
            await GetCoordinatesForCities();
        }
    }

    /// <summary>
    /// Асинхронно получает координаты и часовой пояс для городов в данных страны.
    /// Если координаты не доступны, делает запрос к API Яндекс.Карты для получения их.
    /// Функция также обновляет часовой пояс для каждого города.
    /// </summary>
    private async Task GetCoordinatesForCities()
    {

        // Определяет путь к JSON-файлу, содержащему данные страны
        var path = $"{AppDomain.CurrentDomain.BaseDirectory}\\Data\\Cities.json";

        // Инициализирует HttpClient для выполнения запросов к API
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "CreatorCities");

        // Десериализует данные страны из JSON-файла
        var country = JsonSerializer.Deserialize<CountryRf>(File.ReadAllText(path));

        // Получает ключ API из конфигурации
        var apiKey = _config["ApiKey"];

        // Инициализирует счетчик для количества запросов
        var counter = 1;

        // Перебирает каждый регион в данных страны
        foreach (var region in country!.Regions)
        {
            // Перебирает каждый город в регионе
            foreach (var city in region.Cities)
            {
                // Прерывает цикл, если достигнуто ограничение
                if (counter >= CONSTRAIT_REQUEST_TO_API)
                {
                    break;
                }

                // Проверяет, если координаты города не доступны
                if (city.Latitude <= 0 && city.Longitude <= 0)
                {
                    // Задержка в 500 миллисекунд для избежания превышения лимита запросов к API
                    await Task.Delay(500);
                    
                    // Отправляет запрос к API Яндекс.Карты
                    var response =
                        await client.GetAsync(
                            $"https://geocode-maps.yandex.ru/1.x/?apikey={apiKey}&geocode={region.Name}+{city.NameCity}&format=json");

                    // Читает ответ как строку
                    var result = await response.Content.ReadAsStringAsync();

                    // Проверяет, если ответ не пуст
                    if (!string.IsNullOrEmpty(result))
                    {
                        // Десериализует ответ в объект YandexCityModel
                        var yacoord = GetObjectFromJSON<YandexCityModel>(result);

                        // Проверяет, если десериализованный объект не равен null
                        if (yacoord != null)
                        {
                            // Проверяет, если API вернул какие-либо географические объекты
                            if (yacoord.response.GeoObjectCollection.featureMember.Count > 0)
                            {
                                // Извлекает координаты из ответа API
                                var resStr = yacoord.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos;
                                var arrayStr = resStr.Split(" ");
                                var lattitude = double.Parse(ReplaceSeparator(arrayStr[1]));
                                var longitude = double.Parse(ReplaceSeparator(arrayStr[0]));

                                // Обновляет координаты и часовой пояс города
                                city.Latitude = lattitude;
                                city.Longitude = longitude;
                                var tz = TimeZoneLookup.GetTimeZone(lattitude, longitude);
                                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(tz.Result);
                                city.TimeZone = easternZone.DisplayName;

                                // Логирует имя обновленного города
                                Console.WriteLine($"{city.NameCity} координаты переписаны");
                            }
                        }
                    }

                    // Увеличивает счетчик
                    counter++;
                }
                else
                {
                    // Если координаты города уже доступны, обновляет его часовой пояс
                    var tz = TimeZoneLookup.GetTimeZone(city.Latitude, city.Longitude);
                    TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(tz.Result);
                    city.TimeZone = easternZone.DisplayName;
                }
            }

            // Прерывает цикл, если достигнуто ограничение
            if (counter >= CONSTRAIT_REQUEST_TO_API)
            {
                break;
            }
        }

        // Логирует завершение операции
        Console.WriteLine("Операция завершена");

        // Удаляет старый JSON-файл
        File.Delete(path);

        // Сериализует обновленные данные страны обратно в JSON-файл
        SerializeObjectToFile(country, path);
    }

    /// <summary>
    /// Десериализует JSON-строку в объект указанного типа T.
    /// </summary>
    /// <typeparam name="T">Тип, в который десериализуется JSON-строка.</typeparam>
    /// <param name="json">JSON-строка для десериализации.</param>
    /// <returns>Объект указанного типа T, или null, если десериализация завершается с ошибкой.</returns>
    /// <exception cref="Exception">Выбрасывается, когда десериализация завершается с ошибкой, с описанием ошибки.</exception>
    private T GetObjectFromJSON<T>(string json)
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(json);
            return result!;
        }
        catch
        {
            throw new Exception("Что-то пошло не так йопта");
        }
    }

    /// <summary>
    /// Метод сериализует объект указанного типа T в JSON-файл по указанному пути.
    /// </summary>
    /// <typeparam name="T">Тип объекта, который необходимо сериализовать.</typeparam>
    /// <param name="obj">Объект для сериализации.</param>
    /// <param name="filePath">Путь к файлу, в который будет сохранен сериализованный JSON.</param>
    private void SerializeObjectToFile<T>(T obj, string filePath)
    {
        try
        {
            // Открываем поток файла для записи
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                // Сериализуем объект в поток файла с использованием JSON-формата
                JsonSerializer.Serialize<T>(fs, obj, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic) });

                // Закрываем поток файла
                fs.Close();
            }
        }
        catch (Exception ex)
        {
            // Выводим информацию об ошибке в консоль
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Читает JSON-файл и десериализует его содержимое в список объектов указанного типа T.
    /// </summary>
    /// <typeparam name="T">Тип объектов для десериализации.</typeparam>
    /// <param name="filePath">Путь к JSON-файлу.</param>
    /// <returns>
    /// Список объектов указанного типа T или null, если файл не найден или во время десериализации произошла ошибка.
    /// </returns>
    private List<T> GetListObjectFromJsonFile<T>(string filePath)
    {
        try
        {
            // Проверяет, существует ли файл
            if (File.Exists(filePath))
            {
                // Читает содержимое JSON-файла
                var json = File.ReadAllText(filePath);

                // Десериализует содержимое JSON-файла в список объектов указанного типа
                var result = JsonSerializer.Deserialize<List<T>>(json);

                // Возвращает десериализованный список
                return result!;
            }
            else
            {
                // Выводит сообщение о том, что файл не найден
                Console.WriteLine("Файл не найден");

                return null!;
            }
        }
        catch (Exception ex)
        {
            // Выводит сообщение об ошибке
            Console.WriteLine(ex.Message);

            // Возвращает null, если произошла ошибка десериализации
            return null!;
        }
    }

    /// <summary>
    /// Заменяет запятые текущим разделителем десятичных чисел культуры и наоборот.
    /// </summary>
    /// <param name="value">Строковое значение, которое может содержать запятые или точки в качестве разделителей десятичных чисел.</param>
    /// <returns>Входная строка с замененными запятыми и точками на текущий разделитель десятичных чисел культуры.</returns>
    private string ReplaceSeparator(string value)
    {
        string dec_sep = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        return value.Replace(",", dec_sep).Replace(".", dec_sep);
    }
}