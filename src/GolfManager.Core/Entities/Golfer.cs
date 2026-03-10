using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// Golfer entity - Global player profile
/// Not tenant-specific - represents the golfer across all leagues
/// </summary>
public class Golfer : BaseEntity
{
    /// <summary>
    /// User ID - one-to-one relationship with User
    /// Every golfer must have a user account
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Primary display name for the golfer
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Optional nickname
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Avatar/profile picture URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Golfer biography/description
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Contact phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Home city
    /// </summary>
    public string? HomeCity { get; set; }

    /// <summary>
    /// Home state/province
    /// </summary>
    public string? HomeState { get; set; }

    /// <summary>
    /// Global handicap across all leagues and play
    /// </summary>
    public double? GlobalHandicap { get; set; }

    /// <summary>
    /// When the global handicap was last updated
    /// </summary>
    public DateTime? GlobalHandicapUpdatedAt { get; set; }

    // Navigation Properties

    /// <summary>
    /// Associated user account (required)
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// League-specific golfer profiles
    /// </summary>
    public ICollection<LeagueGolfer> LeagueGolfers { get; set; } = new List<LeagueGolfer>();

    /// <summary>
    /// All rounds played (league and casual)
    /// </summary>
    public ICollection<Round> Rounds { get; set; } = new List<Round>();

    /// <summary>
    /// Golfer's equipment/clubs
    /// </summary>
    public ICollection<GolferClub> Clubs { get; set; } = new List<GolferClub>();
}

