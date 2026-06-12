using GolfManager.Core.Enums;

namespace GolfManager.Shared.DTOs.Round;

/// <summary>
/// Response containing round information
/// </summary>
public class RoundResponse
{
    /// <summary>
    /// Round ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Golfer ID
    /// </summary>
    public string GolferId { get; set; } = string.Empty;

    /// <summary>
    /// League Golfer ID
    /// </summary>
    public string? LeagueGolferId { get; set; }

    /// <summary>
    /// League ID
    /// </summary>
    public string? LeagueId { get; set; }

    /// <summary>
    /// Season Event ID (if this round is part of an event)
    /// </summary>
    public string? SeasonEventId { get; set; }

    /// <summary>
    /// Season ID (populated when SeasonEventId is set)
    /// </summary>
    public string? SeasonId { get; set; }

    /// <summary>
    /// Season name for display
    /// </summary>
    public string? SeasonName { get; set; }

    /// <summary>
    /// League name for display
    /// </summary>
    public string? LeagueName { get; set; }

    /// <summary>
    /// Event date from the season event (may differ from RoundDate by a few hours due to timezone)
    /// </summary>
    public DateTime? EventDate { get; set; }

    /// <summary>
    /// League key for deep linking to the event results page
    /// </summary>
    public string? LeagueKey { get; set; }

    /// <summary>
    /// Season key for deep linking
    /// </summary>
    public string? SeasonKey { get; set; }

    /// <summary>
    /// Course ID
    /// </summary>
    public string CourseId { get; set; } = string.Empty;

    /// <summary>
    /// Course name
    /// </summary>
    public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// Tee ID
    /// </summary>
    public string TeeId { get; set; } = string.Empty;

    /// <summary>
    /// Tee name
    /// </summary>
    public string TeeName { get; set; } = string.Empty;

    /// <summary>
    /// Date the round was played
    /// </summary>
    public DateTime RoundDate { get; set; }

    /// <summary>
    /// Number of holes played
    /// </summary>
    public HolesPlayed HolesPlayed { get; set; }

    /// <summary>
    /// Total score (gross)
    /// </summary>
    public int? TotalScore { get; set; }

    /// <summary>
    /// Net score (after handicap)
    /// </summary>
    public int? NetScore { get; set; }

    /// <summary>
    /// Handicap used for this round
    /// </summary>
    public double? HandicapUsed { get; set; }

    /// <summary>
    /// Is this round complete?
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Round notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Hole-by-hole scores
    /// </summary>
    public List<RoundHoleResponse> Holes { get; set; } = new();

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

