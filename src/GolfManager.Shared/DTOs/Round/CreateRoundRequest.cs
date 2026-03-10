using System.ComponentModel.DataAnnotations;
using GolfManager.Core.Enums;

namespace GolfManager.Shared.DTOs.Round;

/// <summary>
/// Request to create a new round
/// </summary>
public class CreateRoundRequest
{
    /// <summary>
    /// League Golfer ID (required for league rounds)
    /// </summary>
    [Required]
    public string LeagueGolferId { get; set; } = string.Empty;

    /// <summary>
    /// Season Event ID (optional - null for casual rounds)
    /// </summary>
    public string? SeasonEventId { get; set; }

    /// <summary>
    /// Course ID
    /// </summary>
    [Required]
    public string CourseId { get; set; } = string.Empty;

    /// <summary>
    /// Tee ID
    /// </summary>
    [Required]
    public string TeeId { get; set; } = string.Empty;

    /// <summary>
    /// Date the round was played
    /// </summary>
    [Required]
    public DateTime RoundDate { get; set; }

    /// <summary>
    /// Number of holes played (9 or 18)
    /// </summary>
    [Required]
    public HolesPlayed HolesPlayed { get; set; } = HolesPlayed.Eighteen;

    /// <summary>
    /// Handicap used for this round
    /// </summary>
    public double? HandicapUsed { get; set; }

    /// <summary>
    /// Round notes
    /// </summary>
    public string? Notes { get; set; }
}

