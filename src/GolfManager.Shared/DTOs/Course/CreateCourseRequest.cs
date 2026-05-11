using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.Course;

public class CreateCourseRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly key (auto-generated from Name if omitted)
    /// </summary>
    [MaxLength(100)]
    public string? Key { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(50)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(50)]
    public string? Country { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(500)]
    [Url]
    public string? WebsiteUrl { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [Range(1, 27)]
    public int NumberOfHoles { get; set; } = 18;
}
