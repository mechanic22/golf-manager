using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// Course entity - Golf course (global, not tenant-specific)
/// Courses are shared across all leagues
/// </summary>
public class Course : BaseEntity
{
    /// <summary>
    /// URL-friendly key (e.g., "pebble-beach")
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Course name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Course description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Street address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// City
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State/Province
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Postal/ZIP code
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Website URL
    /// </summary>
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Course latitude (for GPS features)
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Course longitude (for GPS features)
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Number of holes (typically 9 or 18)
    /// </summary>
    public int NumberOfHoles { get; set; } = 18;

    // Navigation Properties

    /// <summary>
    /// Tee sets for this course
    /// </summary>
    public ICollection<Tee> Tees { get; set; } = new List<Tee>();

    /// <summary>
    /// Holes on this course
    /// </summary>
    public ICollection<Hole> Holes { get; set; } = new List<Hole>();

    /// <summary>
    /// Rounds played at this course
    /// </summary>
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
}

