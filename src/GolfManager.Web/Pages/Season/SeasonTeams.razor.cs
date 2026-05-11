using Microsoft.AspNetCore.Components;
using GolfManager.Shared.DTOs.Player;
using GolfManager.Shared.DTOs.Season;
using MaterialComponents.Models;
using System.Linq;

namespace GolfManager.Web.Pages.Season;

public partial class SeasonTeams : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IPlayerService PlayerService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<SeasonTeams> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;

    private LeagueResponse? league;
    private SeasonResponse? season;
    private List<SeasonTeamResponse> teams = new();
    private List<PlayerResponse> allSeasonPlayers = new();
    private List<EventScoreboardResponse> eventScoreboards = new();

    private bool isLoading = true;
    private bool isLoadingTeams = false;
    private bool isSaving = false;

    private string activeTab = "teams";
    private string? errorMessage;
    private string? successMessage;

    private bool showAddTeamModal;
    private string newTeamName = string.Empty;
    private string? modalError;

    private bool showEditTeamModal;
    private SeasonTeamResponse? editingTeam;
    private string editTeamName = string.Empty;

    private bool showDeleteTeamConfirm;
    private SeasonTeamResponse? teamToDelete;

    private Dictionary<string, string> assignTargetPlayerId = new();

    private IEnumerable<PlayerResponse> unassignedPlayers =>
        allSeasonPlayers.Where(player => string.IsNullOrEmpty(player.TeamId));

    private List<TeamStandingView> RankedTeams =>
        teams
            .Select(team =>
            {
                var matches = eventScoreboards
                    .SelectMany(scoreboard => scoreboard.Matches)
                    .Where(match => string.Equals(match.HomeTeamId, team.Id, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(match.AwayTeamId, team.Id, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var wins = 0;
                var losses = 0;
                var ties = 0;
                var pointsFor = 0d;
                var pointsAgainst = 0d;

                foreach (var match in matches)
                {
                    var isHome = string.Equals(match.HomeTeamId, team.Id, StringComparison.OrdinalIgnoreCase);
                    var scored = isHome ? match.HomePoints ?? 0 : match.AwayPoints ?? 0;
                    var allowed = isHome ? match.AwayPoints ?? 0 : match.HomePoints ?? 0;

                    pointsFor += scored;
                    pointsAgainst += allowed;

                    if (scored > allowed)
                    {
                        wins++;
                    }
                    else if (scored < allowed)
                    {
                        losses++;
                    }
                    else
                    {
                        ties++;
                    }
                }

                return new TeamStandingView(team, wins, losses, ties, pointsFor, pointsAgainst, matches.Count);
            })
            .OrderByDescending(team => team.PointsFor)
            .ThenByDescending(team => team.PointDifferential)
            .ThenBy(team => team.Team.Name)
            .ToList();

    private bool CanManageSeason => league?.IsCurrentUserAdmin == true || AuthService.IsGlobalAdmin;

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await AuthorizationService.InitializeAsync();
        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        try
        {
            var leagueResponse = await LeagueService.GetLeagueByKeyAsync(LeagueKey);
            if (leagueResponse?.Success != true || leagueResponse.Data == null)
            {
                return;
            }

            league = leagueResponse.Data;

            var seasonResponse = await SeasonService.GetSeasonByKeyAsync(league.Id, SeasonKey);
            if (seasonResponse?.Success != true || seasonResponse.Data == null)
            {
                return;
            }

            season = seasonResponse.Data;
            await Task.WhenAll(LoadTeams(), LoadPlayers());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading data");
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadTeams()
    {
        if (league == null || season == null)
        {
            return;
        }

        isLoadingTeams = true;
        try
        {
            var response = await SeasonService.GetSeasonTeamsAsync(league.Id, season.Id);
            teams = response?.Success == true && response.Data != null ? response.Data : new();

            var eventsResponse = await EventService.GetSeasonEventsAsync(league.Id, season.Id);
            var seasonEvents = eventsResponse?.Success == true && eventsResponse.Data != null
                ? eventsResponse.Data.OrderBy(e => e.EventDate).ToList()
                : new List<EventResponse>();

            eventScoreboards = new List<EventScoreboardResponse>();
            foreach (var seasonEvent in seasonEvents)
            {
                var scoreboardResponse = await EventService.GetEventScoreboardAsync(league.Id, season.Id, seasonEvent.Id);
                if (scoreboardResponse is { Success: true, Data: not null })
                {
                    eventScoreboards.Add(scoreboardResponse.Data);
                }
            }

            assignTargetPlayerId = teams.ToDictionary(team => team.Id, _ => string.Empty);
        }
        finally
        {
            isLoadingTeams = false;
        }
    }

    private async Task LoadPlayers()
    {
        if (league == null || season == null)
        {
            return;
        }

        var response = await PlayerService.GetSeasonPlayersAsync(league.Id, season.Id);
        allSeasonPlayers = response?.Success == true && response.Data != null ? response.Data : new();
    }

    private void OpenAddTeamModal()
    {
        newTeamName = string.Empty;
        modalError = null;
        showAddTeamModal = true;
    }

    private void CloseAddTeamModal()
    {
        showAddTeamModal = false;
        modalError = null;
    }

    private async Task SaveNewTeam()
    {
        if (string.IsNullOrWhiteSpace(newTeamName) || league == null || season == null)
        {
            return;
        }

        isSaving = true;
        modalError = null;
        try
        {
            var response = await SeasonService.CreateSeasonTeamAsync(
                league.Id,
                season.Id,
                new CreateSeasonTeamRequest { Name = newTeamName.Trim() });

            if (response?.Success == true)
            {
                showAddTeamModal = false;
                ShowSuccess("Team created successfully");
                await LoadTeams();
            }
            else
            {
                modalError = response?.Message ?? "Failed to create team";
            }
        }
        finally
        {
            isSaving = false;
        }
    }

    private void OpenEditTeamModal(SeasonTeamResponse team)
    {
        editingTeam = team;
        editTeamName = team.Name;
        modalError = null;
        showEditTeamModal = true;
    }

    private void CloseEditTeamModal()
    {
        showEditTeamModal = false;
        editingTeam = null;
        modalError = null;
    }

    private async Task SaveEditTeam()
    {
        if (editingTeam == null || string.IsNullOrWhiteSpace(editTeamName) || league == null || season == null)
        {
            return;
        }

        isSaving = true;
        modalError = null;
        try
        {
            var response = await SeasonService.UpdateSeasonTeamAsync(
                league.Id,
                season.Id,
                editingTeam.Id,
                new UpdateSeasonTeamRequest { Name = editTeamName.Trim() });

            if (response?.Success == true)
            {
                showEditTeamModal = false;
                ShowSuccess("Team updated");
                await LoadTeams();
            }
            else
            {
                modalError = response?.Message ?? "Failed to update team";
            }
        }
        finally
        {
            isSaving = false;
        }
    }

    private void OpenDeleteTeamConfirm(SeasonTeamResponse team)
    {
        teamToDelete = team;
        showDeleteTeamConfirm = true;
    }

    private void CloseDeleteTeamConfirm()
    {
        showDeleteTeamConfirm = false;
        teamToDelete = null;
    }

    private async Task ConfirmDeleteTeam()
    {
        if (teamToDelete == null || league == null || season == null)
        {
            return;
        }

        isSaving = true;
        try
        {
            var success = await SeasonService.DeleteSeasonTeamAsync(league.Id, season.Id, teamToDelete.Id);
            if (success)
            {
                showDeleteTeamConfirm = false;
                ShowSuccess("Team deleted");
                await Task.WhenAll(LoadTeams(), LoadPlayers());
            }
            else
            {
                errorMessage = "Failed to delete team";
            }
        }
        finally
        {
            isSaving = false;
            teamToDelete = null;
        }
    }

    private async Task AssignSelectedPlayer(SeasonTeamResponse team)
    {
        if (!assignTargetPlayerId.TryGetValue(team.Id, out var seasonGolferId) || string.IsNullOrEmpty(seasonGolferId))
        {
            return;
        }

        if (league == null || season == null)
        {
            return;
        }

        isSaving = true;
        try
        {
            var success = await SeasonService.AssignPlayerToTeamAsync(
                league.Id,
                season.Id,
                seasonGolferId,
                new AssignPlayerToTeamRequest { TeamId = team.Id });

            if (success)
            {
                assignTargetPlayerId[team.Id] = string.Empty;
                ShowSuccess("Player assigned to team");
                await Task.WhenAll(LoadTeams(), LoadPlayers());
            }
            else
            {
                errorMessage = "Failed to assign player";
            }
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task UnassignPlayer(SeasonTeamMemberResponse member, SeasonTeamResponse team)
    {
        if (league == null || season == null)
        {
            return;
        }

        isSaving = true;
        try
        {
            var success = await SeasonService.AssignPlayerToTeamAsync(
                league.Id,
                season.Id,
                member.SeasonGolferId,
                new AssignPlayerToTeamRequest { TeamId = null });

            if (success)
            {
                ShowSuccess($"{member.DisplayName} removed from {team.Name}");
                await Task.WhenAll(LoadTeams(), LoadPlayers());
            }
            else
            {
                errorMessage = "Failed to remove player from team";
            }
        }
        finally
        {
            isSaving = false;
        }
    }

    private void ShowSuccess(string message)
    {
        successMessage = message;
        errorMessage = null;
        _ = Task.Delay(3000).ContinueWith(_ =>
        {
            successMessage = null;
            InvokeAsync(StateHasChanged);
        });
    }

    private static string GetInitials(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
        {
            return "?";
        }

        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 1
            ? parts[0][0].ToString().ToUpper()
            : (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
    }

    private List<MaterialTab> GetNavigationTabs()
    {
        var tabs = new List<MaterialTab>
        {
            new() { Value = "overview", Label = "Overview" },
            new() { Value = "events", Label = "Events" },
            new() { Value = "players", Label = "Players" },
            new() { Value = "teams", Label = "Teams" }
        };

        if (CanManageSeason)
        {
            tabs.Add(new() { Value = "settings", Label = "Settings" });
        }

        return tabs;
    }

    private void HandleTabChange(string tabValue)
    {
        var route = tabValue switch
        {
            "overview" => $"/league/{LeagueKey}/season/{SeasonKey}",
            "events" => $"/league/{LeagueKey}/season/{SeasonKey}/events",
            "players" => $"/league/{LeagueKey}/season/{SeasonKey}/players",
            "teams" => $"/league/{LeagueKey}/season/{SeasonKey}/teams",
            "settings" => $"/league/{LeagueKey}/season/{SeasonKey}/settings",
            _ => $"/league/{LeagueKey}/season/{SeasonKey}"
        };

        Navigation.NavigateTo(route);
    }

    private sealed record TeamStandingView(
        SeasonTeamResponse Team,
        int Wins,
        int Losses,
        int Ties,
        double PointsFor,
        double PointsAgainst,
        int MatchCount)
    {
        public double PointDifferential => PointsFor - PointsAgainst;
    }
}
