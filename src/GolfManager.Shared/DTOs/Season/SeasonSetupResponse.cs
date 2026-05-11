namespace GolfManager.Shared.DTOs.Season;

/// <summary>
/// Result of bulk season setup.
/// </summary>
public class SeasonSetupResponse
{
    /// <summary>
    /// Total calendar rows parsed.
    /// </summary>
    public int CalendarWeeksParsed { get; set; }

    /// <summary>
    /// Calendar rows skipped because they were marked as no play.
    /// </summary>
    public int SkippedWeeks { get; set; }

    /// <summary>
    /// Events created for the season.
    /// </summary>
    public int EventsCreated { get; set; }

    /// <summary>
    /// Teams created for the season.
    /// </summary>
    public int TeamsCreated { get; set; }

    /// <summary>
    /// Season golfers created or updated from the submitted rosters.
    /// </summary>
    public int PlayersAssigned { get; set; }

    /// <summary>
    /// Team roster entries that could not be matched to league players.
    /// </summary>
    public List<string> MissingPlayers { get; set; } = new();

    /// <summary>
    /// Non-fatal warnings generated during setup.
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}