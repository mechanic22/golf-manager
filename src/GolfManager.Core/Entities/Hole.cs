using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// Hole entity - A specific hole on a course
/// </summary>
public class Hole : BaseEntity
{
    /// <summary>
    /// Course ID
    /// </summary>
    public string CourseId { get; set; } = string.Empty;

    /// <summary>
    /// Hole number (1-18)
    /// </summary>
    public int HoleNumber { get; set; }

    /// <summary>
    /// Hole name (optional, e.g., "The Lighthouse")
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Hole description
    /// </summary>
    public string? Description { get; set; }

    // GPS Coordinates (Future Feature - for mobile app)

    /// <summary>
    /// Tee box latitude
    /// </summary>
    public double? TeeLatitude { get; set; }

    /// <summary>
    /// Tee box longitude
    /// </summary>
    public double? TeeLongitude { get; set; }

    /// <summary>
    /// Green center latitude
    /// </summary>
    public double? GreenLatitude { get; set; }

    /// <summary>
    /// Green center longitude
    /// </summary>
    public double? GreenLongitude { get; set; }

    /// <summary>
    /// Front of green latitude
    /// </summary>
    public double? GreenFrontLatitude { get; set; }

    /// <summary>
    /// Front of green longitude
    /// </summary>
    public double? GreenFrontLongitude { get; set; }

    /// <summary>
    /// Back of green latitude
    /// </summary>
    public double? GreenBackLatitude { get; set; }

    /// <summary>
    /// Back of green longitude
    /// </summary>
    public double? GreenBackLongitude { get; set; }

    // Navigation Properties

    /// <summary>
    /// Associated course
    /// </summary>
    public Course Course { get; set; } = null!;

    /// <summary>
    /// Tee-specific information for this hole
    /// </summary>
    public ICollection<HoleTee> HoleTees { get; set; } = new List<HoleTee>();
}

