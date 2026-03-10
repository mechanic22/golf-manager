namespace GolfManager.Shared.DTOs.Round;

/// <summary>
/// Request to update an existing round
/// </summary>
public class UpdateRoundRequest
{
    /// <summary>
    /// Handicap used for this round
    /// </summary>
    public double? HandicapUsed { get; set; }

    /// <summary>
    /// Round notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Mark the round as complete
    /// </summary>
    public bool? IsComplete { get; set; }
}

