using GolfManager.Core.Common;
using GolfManager.Core.Enums;

namespace GolfManager.Core.Entities;

/// <summary>
/// Round entity - A round of golf played by a golfer
/// Can be league-affiliated or casual
/// </summary>
public class Round : BaseEntity
{
    /// <summary>
    /// Global golfer ID (required)
    /// </summary>
    public string GolferId { get; set; } = string.Empty;

    /// <summary>
    /// League golfer ID (optional - null for casual rounds)
    /// </summary>
    public string? LeagueGolferId { get; set; }

    /// <summary>
    /// League ID (optional - null for casual rounds)
    /// </summary>
    public string? LeagueId { get; set; }

    /// <summary>
    /// Course ID
    /// </summary>
    public string CourseId { get; set; } = string.Empty;

    /// <summary>
    /// Tee ID
    /// </summary>
    public string TeeId { get; set; } = string.Empty;

    /// <summary>
    /// Date the round was played
    /// </summary>
    public DateTime RoundDate { get; set; }

    /// <summary>
    /// Number of holes played
    /// </summary>
    public HolesPlayed HolesPlayed { get; set; } = HolesPlayed.Eighteen;

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

    // One-Time Event Support

    /// <summary>
    /// One-time event ID (optional - null for league/casual rounds)
    /// </summary>
    public string? OneTimeEventId { get; set; }

    /// <summary>
    /// One-time event team ID (optional - for team events)
    /// </summary>
    public string? OneTimeEventTeamId { get; set; }

    /// <summary>
    /// Is this a team round? (true for scrambles, best ball, etc.)
    /// </summary>
    public bool IsTeamRound { get; set; }

    /// <summary>
    /// Scoring format override (if different from event default)
    /// </summary>
    public ScoringFormat? Format { get; set; }

    /// <summary>
    /// Round number within the event (1, 2, 3, etc.)
    /// For single-round events, this is always 1
    /// For multi-round events, indicates which round this is
    /// </summary>
    public int RoundNumber { get; set; } = 1;

    /// <summary>
    /// Optional round name/label (e.g., "Qualifier", "Finals", "Day 1", "Morning Round")
    /// Null for simple single-round events
    /// </summary>
    public string? RoundLabel { get; set; }

    // Navigation Properties

    /// <summary>
    /// Associated golfer (global)
    /// </summary>
    public Golfer Golfer { get; set; } = null!;

    /// <summary>
    /// Associated league golfer (if league round)
    /// </summary>
    public LeagueGolfer? LeagueGolfer { get; set; }

    /// <summary>
    /// Associated course
    /// </summary>
    public Course Course { get; set; } = null!;

    /// <summary>
    /// Associated tee
    /// </summary>
    public Tee Tee { get; set; } = null!;

    /// <summary>
    /// Scorecard (if exists)
    /// </summary>
    public Scorecard? Scorecard { get; set; }

    /// <summary>
    /// Hole-by-hole scores
    /// </summary>
    public ICollection<RoundHole> Holes { get; set; } = new List<RoundHole>();

    /// <summary>
    /// Associated one-time event (if applicable)
    /// </summary>
    public OneTimeEvent? OneTimeEvent { get; set; }

    /// <summary>
    /// Associated one-time event team (if applicable)
    /// </summary>
    public OneTimeEventTeam? OneTimeEventTeam { get; set; }
}

