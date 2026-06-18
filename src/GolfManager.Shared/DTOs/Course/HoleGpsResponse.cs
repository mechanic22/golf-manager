namespace GolfManager.Shared.DTOs.Course;

public class HoleGpsResponse
{
    public string Id { get; set; } = string.Empty;
    public int HoleNumber { get; set; }
    public string? Name { get; set; }
    public double? TeeLatitude { get; set; }
    public double? TeeLongitude { get; set; }
    public double? GreenLatitude { get; set; }
    public double? GreenLongitude { get; set; }
    public double? GreenRadius { get; set; }
}
