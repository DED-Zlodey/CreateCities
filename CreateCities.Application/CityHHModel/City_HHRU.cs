namespace CreateCities.Application.CityHHModel;

public class City_HHRU
{
    public string id { get; set; }
    public string? parent_id { get; set; }
    public string name { get; set; }
    public List<City_HHRU>? areas { get; set; }
}