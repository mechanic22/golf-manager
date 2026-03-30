namespace GolfManager.Shared.DTOs.League;

/// <summary>
/// Request to update a league member's role
/// </summary>
public class UpdateLeagueMemberRequest
{
    /// <summary>
    /// Whether the user should be a league admin
    /// </summary>
    public bool IsLeagueAdmin { get; set; }
}

