using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.Event;

/// <summary>
/// Request to update an existing event matchup.
/// </summary>
public class UpdateEventMatchupRequest
{
    [StringLength(50)]
    public string? HomeTeamId { get; set; }

    [StringLength(50)]
    public string? AwayTeamId { get; set; }

    [StringLength(50)]
    public string? HomeSubSeasonGolferId { get; set; }

    [StringLength(50)]
    public string? AwaySubSeasonGolferId { get; set; }

    [Range(1, 18)]
    public int? StartingHole { get; set; }

    [Range(1, 99)]
    public int? StartingFlight { get; set; }
}
