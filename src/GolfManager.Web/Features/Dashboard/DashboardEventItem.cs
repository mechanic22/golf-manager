namespace GolfManager.Web.Features.Dashboard;

public record DashboardEventItem(
    string Name,
    DateTime EventDate,
    string? CourseName,
    string? FormatLabel,
    string Url,
    string? LeagueName,
    bool IsCompleted,
    string? LeagueId = null,
    string? SeasonId = null,
    string? EventId = null,
    int? StartingHole = null,
    int? StartingFlight = null,
    string? OpponentName = null,
    bool IsLocked = false,
    string? ScoreEntryUrl = null
);
