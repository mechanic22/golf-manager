namespace GolfManager.Shared.DTOs.Player;

/// <summary>
/// Response containing player information within a league
/// </summary>
public class PlayerResponse
{
    /// <summary>
    /// League golfer ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Global golfer ID
    /// </summary>
    public string GolferId { get; set; } = string.Empty;

    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// League ID
    /// </summary>
    public string LeagueId { get; set; } = string.Empty;

    /// <summary>
    /// Display name in this league
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Nickname in this league
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// League-specific handicap
    /// </summary>
    public double? LeagueHandicap { get; set; }

    /// <summary>
    /// When the league handicap was last updated
    /// </summary>
    public DateTime? LeagueHandicapUpdatedAt { get; set; }

    /// <summary>
    /// Total rounds played in this league
    /// </summary>
    public int TotalRounds { get; set; }

    /// <summary>
    /// Average score in this league
    /// </summary>
    public double? AverageScore { get; set; }

    /// <summary>
    /// Best score in this league
    /// </summary>
    public int? BestScore { get; set; }

    /// <summary>
    /// When the golfer joined this league
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// When the record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the record was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// The SeasonGolfer ID when this response is for a season context. Null when listing league players.
    /// </summary>
    public string? SeasonGolferId { get; set; }

    /// <summary>
    /// The team ID the player is assigned to in the season context. Null if not on a team.
    /// </summary>
    public string? TeamId { get; set; }

    /// <summary>
    /// Whether this player is marked paid for the season in season context.
    /// </summary>
    public bool? IsPaidForSeason { get; set; }

    /// <summary>
    /// When season payment was marked in season context.
    /// </summary>
    public DateTime? PaidAt { get; set; }
}

