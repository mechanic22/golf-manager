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
}

