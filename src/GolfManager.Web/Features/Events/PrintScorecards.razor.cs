using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Course;
using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.Player;
using GolfManager.Shared.DTOs.Season;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Events;

public partial class PrintScorecards : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private IPlayerService PlayerService { get; set; } = null!;
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<PrintScorecards> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [Parameter] public string EventKey { get; set; } = string.Empty;

    [CascadingParameter] public EventLayoutContext? EventContext { get; set; }

    private LeagueResponse? league;
    private SeasonResponse? season;
    private EventResponse? seasonEvent;
    private TeeResponse? tee;
    private List<EventMatchupResponse> matchups = new();
    private List<PlayerResponse> seasonPlayers = new();
    private List<SeasonTeamResponse> seasonTeams = new();
    private bool accessDenied;
    private bool isLoading = true;

    private static readonly TimeSpan TargetSideTime = TimeSpan.FromMinutes(165);

    private int HoleCount => seasonEvent?.HolesPlayed == Core.Enums.HolesPlayed.Eighteen ? 18 : 9;

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await LoadData();
    }

    private async Task LoadData()
    {
        if (EventContext == null) return;

        isLoading = true;
        try
        {
            league = EventContext.League;
            season = EventContext.Season;
            seasonEvent = EventContext.Event;

            if (!EventContext.IsAdmin)
            {
                accessDenied = true;
                return;
            }

            var matchupResponse = await EventService.GetEventMatchupsAsync(league.Id, season.Id, seasonEvent.Id);
            if (matchupResponse?.Success == true && matchupResponse.Data != null)
                matchups = matchupResponse.Data;

            var teamsResponse = await SeasonService.GetSeasonTeamsAsync(league.Id, season.Id);
            if (teamsResponse?.Success == true && teamsResponse.Data != null)
                seasonTeams = teamsResponse.Data;

            var playersResponse = await PlayerService.GetSeasonPlayersAsync(league.Id, season.Id);
            if (playersResponse?.Success == true && playersResponse.Data != null)
                seasonPlayers = playersResponse.Data;

            if (!string.IsNullOrEmpty(seasonEvent.CourseId) && !string.IsNullOrEmpty(seasonEvent.TeeId))
            {
                var teeResp = await Http.GetFromJsonAsync<ApiResponse<TeeResponse>>(
                    $"api/v1/courses/{seasonEvent.CourseId}/tees/{seasonEvent.TeeId}?includeHoles=true");
                if (teeResp?.Success == true)
                    tee = teeResp.Data;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading print scorecards for event {EventKey}", EventKey);
        }
        finally
        {
            isLoading = false;
        }
    }

    private string GetTeamName(string? teamId, string? fallbackName)
    {
        if (!string.IsNullOrWhiteSpace(fallbackName))
            return fallbackName;

        if (string.IsNullOrWhiteSpace(teamId))
            return "TBD";

        var team = seasonTeams.FirstOrDefault(t => string.Equals(t.Id, teamId, StringComparison.OrdinalIgnoreCase));
        return team?.Name ?? "TBD";
    }

    private IEnumerable<SeasonTeamMemberResponse> GetTeamMembers(string? teamId)
    {
        if (string.IsNullOrWhiteSpace(teamId)) return [];
        return seasonTeams
            .FirstOrDefault(t => string.Equals(t.Id, teamId, StringComparison.OrdinalIgnoreCase))
            ?.Members.OrderBy(m => m.DisplayName)
            ?? (IEnumerable<SeasonTeamMemberResponse>)[];
    }

    private string GetSubName(string? seasonGolferId)
    {
        if (string.IsNullOrWhiteSpace(seasonGolferId))
            return "None";

        var player = seasonPlayers.FirstOrDefault(p => string.Equals(p.SeasonGolferId, seasonGolferId, StringComparison.OrdinalIgnoreCase));
        return player?.DisplayName ?? "Unknown";
    }

    private static string DigitToLetter(int digit) =>
        digit <= 0 ? string.Empty : ((char)('A' + digit - 1)).ToString();

    private Dictionary<int, TimeSpan> BuildPaceTimeline(EventMatchupResponse match)
    {
        if (tee?.Holes == null)
            return Enumerable.Range(1, 18).ToDictionary(x => x, _ => TimeSpan.Zero);

        var startHole = match.StartingHole ?? 1;
        var sideStart = startHole <= 9 ? 1 : 10;
        var sideEnd = startHole <= 9 ? 9 : 18;

        var holes = tee.Holes
            .Where(x => x.HoleNumber >= sideStart && x.HoleNumber <= sideEnd)
            .OrderBy(x => x.HoleNumber < startHole ? 1 : 0)
            .ThenBy(x => x.HoleNumber)
            .ToArray();

        if (holes.Length == 0)
            return Enumerable.Range(1, 18).ToDictionary(x => x, _ => TimeSpan.Zero);

        var eventStart = seasonEvent?.EventDate ?? DateTime.MinValue;
        var flight = Math.Max(match.StartingFlight ?? 1, 1);
        var sideFlights = GetSideFlightCount(startHole);
        var holeWeights = holes.Select(x => Math.Max(x.Yardage, 1)).ToArray();
        var totalWeight = holeWeights.Sum();
        if (totalWeight <= 0) totalWeight = Math.Max(holeWeights.Length, 1);

        var elapsedMinutes = 0.0;
        var timeline = new Dictionary<int, TimeSpan>();
        for (var i = 0; i < holes.Length; i++)
        {
            var holeMinutes = TargetSideTime.TotalMinutes * holeWeights[i] / totalWeight;
            var flightOffset = (flight - 1) * (holeMinutes / sideFlights);
            timeline[holes[i].HoleNumber] = eventStart.TimeOfDay + TimeSpan.FromMinutes(elapsedMinutes + flightOffset);
            elapsedMinutes += holeMinutes;
        }
        return timeline;
    }

    private int GetSideFlightCount(int startHole)
    {
        var isFront = startHole <= 9;
        return matchups
            .Where(x => x.StartingHole.HasValue && ((x.StartingHole.Value <= 9) == isFront))
            .Select(x => Math.Max(x.StartingFlight ?? 1, 1))
            .DefaultIfEmpty(1)
            .Max();
    }

    private static string FormatPace(TimeSpan pace) =>
        DateTime.Today.Add(pace).ToString("h:mm tt");
}
