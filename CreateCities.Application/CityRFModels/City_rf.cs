namespace CreateCities.Application.CityRFModels;

public class City_rf
{
    public int Id { get; set; }
    public string NameCity { get; set; }
    public int RegionId { get; set; }
    public string TimeZone { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}