namespace GolfManager.Shared.DTOs.Course;

public class CourseResponse
{
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? PhoneNumber { get; set; }
    public string? WebsiteUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int NumberOfHoles { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Tee sets for this course (populated when ?includeTees=true)
    /// </summary>
    public List<TeeResponse>? Tees { get; set; }
}
