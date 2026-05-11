namespace GolfManager.Shared.DTOs.Handicap;

/// <summary>
/// Request to trigger handicap calculation from rounds
/// </summary>
public class CalculateHandicapRequest
{
    /// <summary>
    /// League ID scope (null = global)
    /// </summary>
    public string? LeagueId { get; set; }

    /// <summary>
    /// Season ID scope (null = league/global)
    /// </summary>
    public string? SeasonId { get; set; }

    /// <summary>
    /// Calculation method to use
    /// </summary>
    public HandicapCalculationMethod Method { get; set; } = HandicapCalculationMethod.WorldHandicapSystem;

    /// <summary>
    /// Save the result to handicap history (true = persists, false = preview only)
    /// </summary>
    public bool Persist { get; set; } = true;
}
