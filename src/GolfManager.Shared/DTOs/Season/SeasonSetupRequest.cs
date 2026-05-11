namespace GolfManager.Shared.DTOs.Season;

/// <summary>
/// Request to bulk configure a season from pasted calendar and team text.
/// </summary>
public class SeasonSetupRequest
{
    /// <summary>
    /// Raw calendar text pasted by an admin.
    /// </summary>
    public string CalendarText { get; set; } = string.Empty;

    /// <summary>
    /// Raw team roster text pasted by an admin.
    /// </summary>
    public string TeamsText { get; set; } = string.Empty;

    /// <summary>
    /// When true, existing season events, teams, and season golfers are replaced.
    /// </summary>
    public bool ReplaceExistingData { get; set; }
}