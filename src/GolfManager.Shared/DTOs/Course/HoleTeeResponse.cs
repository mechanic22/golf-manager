namespace GolfManager.Shared.DTOs.Course;

public class HoleTeeResponse
{
    public string Id { get; set; } = string.Empty;
    public string TeeId { get; set; } = string.Empty;
    public int HoleNumber { get; set; }
    public int Par { get; set; }
    public int Yardage { get; set; }
    public int Handicap { get; set; }
}
