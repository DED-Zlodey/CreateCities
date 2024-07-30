namespace CreateCities.Application.CityRFModels;

public class CountryRf
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Region_rf> Regions { get; set; }
}