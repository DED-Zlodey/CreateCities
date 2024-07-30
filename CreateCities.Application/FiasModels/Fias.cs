namespace CreateCities.Application.FiasModels;

    public class Fias
    {
        public string Id { get; set; }
        /// <summary>
        /// Почтовый индекс
        /// </summary>
        public string PostIndex { get; set; }
        /// <summary>
        /// Тип региона (край, область, республика...)
        /// </summary>
        public string TypeRegion { get; set; }
        /// <summary>
        /// Код типа региона
        /// </summary>
        public short TypeRegionNum { get; set; }
        /// <summary>
        /// Название региона
        /// </summary>
        public string Region { get; set; }
        /// <summary>
        /// Название региона в верхнем регистре
        /// </summary>
        public string RegionNormal { get; set; }
        /// <summary>
        /// Тип района
        /// </summary>
        public string TypeDistrict { get; set; }
        /// <summary>
        /// Район
        /// </summary>
        public string District { get; set; }
        /// <summary>
        /// Тип населенного пункта
        /// </summary>
        public string TypeCity { get; set; }
        /// <summary>
        /// Название населенного пункта
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// Название населенного пункта в верхнем регистре
        /// </summary>
        public string CityNormal { get; set; }
        /// <summary>
        /// Тип населенного пункта
        /// </summary>
        public string LocalityType { get; set; }
        /// <summary>
        /// Населенный пункт
        /// </summary>
        public string Locality { get; set; }
        /// <summary>
        /// Код КЛАДР
        /// </summary>
        public string CodeKLADR { get; set; }
        /// <summary>
        /// Код ФИАС
        /// </summary>
        public string CodeFIAS { get; set; }
        /// <summary>
        /// Уровень по ФИАС
        /// 0 — страна; 1 — регион; 3 — район; 4 — город; 5 — район города; 6 — населенный пункт; 7 — улица; 8 — дом; 9 — квартира; 65 — планировочная структура;
        /// 90 — доп.территория; 91 — улица в доп.территории; -1 — иностранный или пустой;
        /// </summary>
        public string LevelFIAS { get; set; }
        /// <summary>
        /// Признак центра района или региона
        /// 1 — центр района (Московская обл, Одинцовский р-н, г Одинцово); 2 — центр региона (Новосибирская обл, г Новосибирск); 3 — центр района и региона (Томская обл, г Томск);
        /// 4 — центральный район региона (Тюменская обл, Тюменский р-н); 0 — ничего из перечисленного (Московская обл, г Балашиха);
        /// </summary>
        public string SignDistrict { get; set; }
        /// <summary>
        /// Код ОКАТО
        /// </summary>
        public string CodeOKATO { get; set; }
        /// <summary>
        /// Код ОКТМО
        /// </summary>
        public string CodeOKTMO { get; set; }
        /// <summary>
        /// Код ИФНС
        /// </summary>
        public string CodeIFNS { get; set; }
        /// <summary>
        /// Часовой пояс
        /// </summary>
        public string TimeZone { get; set; }
        /// <summary>
        /// Широта
        /// </summary>
        //[StringLength(12, ErrorMessage = "Длина поля {0} не должна превышать {1} символов.")]
        public string Latitude { get; set; }
        /// <summary>
        /// Долгота
        /// </summary>
        //[StringLength(12, ErrorMessage = "Длина поля {0} не должна превышать {1} символов.")]
        public string Longitude { get; set; }
        /// <summary>
        /// Федеральный округ
        /// </summary>
        public string FederalDistrict { get; set; }
        /// <summary>
        /// Численность населения
        /// </summary>
        public string Population { get; set; }
    }
