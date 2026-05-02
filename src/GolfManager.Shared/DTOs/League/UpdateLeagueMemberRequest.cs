using GolfManager.Core.Enums;

namespace GolfManager.Shared.DTOs.League;

/// <summary>
/// Request to update a league member's role and status
/// </summary>
public class UpdateLeagueMemberRequest
{
    /// <summary>
    /// Whether the user should be a league admin
    /// </summary>
    public bool? IsLeagueAdmin { get; set; }

    /// <summary>
    /// Updated role for this league member.
    /// </summary>
    public LeagueMemberRole? Role { get; set; }

    /// <summary>
    /// Whether the member is active in the league
    /// </summary>
    public bool? IsActive { get; set; }
}
