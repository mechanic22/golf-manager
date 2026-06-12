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
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<PrintScorecards> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [Parameter] public string EventKey { get; set; } = string.Empty;

    [CascadingParameter] public EventLayoutContext? EventContext { get; set; }

    private LeagueResponse? league;
    private SeasonResponse? season;
    private EventResponse? seasonEvent;
    private List<EventMatchupResponse> matchups = new();
    private List<PlayerResponse> seasonPlayers = new();
    private List<SeasonTeamResponse> seasonTeams = new();
    private bool accessDenied;
    private bool isLoading = true;

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

    private string GetSubName(string? seasonGolferId)
    {
        if (string.IsNullOrWhiteSpace(seasonGolferId))
            return "None";

        var player = seasonPlayers.FirstOrDefault(p => string.Equals(p.SeasonGolferId, seasonGolferId, StringComparison.OrdinalIgnoreCase));
        return player?.DisplayName ?? "Unknown";
    }
}
