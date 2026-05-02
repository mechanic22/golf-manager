using GolfManager.Core.Common;
using GolfManager.Core.Enums;

namespace GolfManager.Core.Entities;

/// <summary>
/// UserLeague - User membership in a league
/// Allows users to belong to multiple leagues with different roles
/// </summary>
public class UserLeague : BaseEntity, ITenantEntity
{
    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// League ID (tenant isolation)
    /// </summary>
    public string LeagueId { get; set; } = string.Empty;

    /// <summary>
    /// Optional link to LeagueGolfer if user is also a player
    /// </summary>
    public string? LeagueGolferId { get; set; }

    /// <summary>
    /// Is this user an admin of this league?
    /// </summary>
    public bool IsLeagueAdmin { get; set; }

    /// <summary>
    /// User's role within this league.
    /// </summary>
    public LeagueMemberRole Role { get; set; } = LeagueMemberRole.Member;

    /// <summary>
    /// When the user joined this league
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties

    /// <summary>
    /// Associated user
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Associated league
    /// </summary>
    public League League { get; set; } = null!;

    /// <summary>
    /// Associated golfer profile in this league (if user is a player)
    /// </summary>
    public LeagueGolfer? LeagueGolfer { get; set; }
}
