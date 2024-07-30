namespace CreateCities.Application.CityRFModels;

public class Region_rf
{ 
    public int Id { get; set; }
    public string Name { get; set; }
    public int CountryId { get; set; }
    public List<City_rf> Cities { get; set; }
}