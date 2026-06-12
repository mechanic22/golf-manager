using GolfManager.Core.Common;
using GolfManager.Core.Enums;

namespace GolfManager.Core.Entities;

/// <summary>
/// SeasonEvent - An event/round within a season
/// </summary>
public class SeasonEvent : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Season ID
    /// </summary>
    public string SeasonId { get; set; } = string.Empty;

    /// <summary>
    /// League ID (tenant isolation)
    /// </summary>
    public string LeagueId { get; set; } = string.Empty;

    /// <summary>
    /// Event date
    /// </summary>
    public DateTime EventDate { get; set; }

    /// <summary>
    /// Course ID (optional - may be TBD)
    /// </summary>
    public string? CourseId { get; set; }

    /// <summary>
    /// Tee ID (optional)
    /// </summary>
    public string? TeeId { get; set; }

    /// <summary>
    /// Number of holes played (9 or 18)
    /// </summary>
    public HolesPlayed HolesPlayed { get; set; } = HolesPlayed.Eighteen;

    /// <summary>
    /// Event type (regular, playoff, championship, etc.)
    /// </summary>
    public SeasonEventType EventType { get; set; } = SeasonEventType.Regular;

    /// <summary>
    /// Scoring format (stroke play, match play, stableford, etc.)
    /// </summary>
    public ScoringFormat ScoringFormat { get; set; } = ScoringFormat.StrokePlay;

    /// <summary>
    /// Event name/title
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Event description/notes
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional title for game-of-the-day configured for this event.
    /// </summary>
    public string? GameOfDayTitle { get; set; }

    /// <summary>
    /// Optional season golfer winner ID for game-of-the-day.
    /// </summary>
    public string? GameOfDayWinnerSeasonGolferId { get; set; }

    /// <summary>
    /// Cached winner display name for quick rendering.
    /// </summary>
    public string? GameOfDayWinnerDisplayName { get; set; }

    /// <summary>
    /// Is the event locked (scores finalized)?
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Team size for team events (1 = individual, 2+ = team)
    /// </summary>
    public int TeamSize { get; set; } = 1;

    /// <summary>
    /// Whether to use handicaps for scoring
    /// </summary>
    public bool UseHandicaps { get; set; } = true;

    /// <summary>
    /// Event status (Draft, Published, InProgress, Completed, Cancelled)
    /// </summary>
    public EventStatus Status { get; set; } = EventStatus.Draft;

    // Navigation Properties

    /// <summary>
    /// Associated season
    /// </summary>
    public Season Season { get; set; } = null!;

    /// <summary>
    /// Associated course (if set)
    /// </summary>
    public Course? Course { get; set; }

    /// <summary>
    /// Associated tee (if set)
    /// </summary>
    public Tee? Tee { get; set; }

    /// <summary>
    /// Calculated player scores for this event (when locked)
    /// </summary>
    public ICollection<SeasonEventPlayerScore> PlayerScores { get; set; } = new List<SeasonEventPlayerScore>();

    /// <summary>
    /// Calculated match scores for this event (when locked)
    /// </summary>
    public ICollection<SeasonEventMatchScore> MatchScores { get; set; } = new List<SeasonEventMatchScore>();
}

