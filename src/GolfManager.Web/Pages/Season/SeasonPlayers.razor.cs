using Microsoft.AspNetCore.Components;
using GolfManager.Shared.DTOs.Player;
using GolfManager.Shared.DTOs.Season;
using MaterialComponents.Models;

namespace GolfManager.Web.Pages.Season;

public partial class SeasonPlayers : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IPlayerService PlayerService { get; set; } = null!;
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<SeasonPlayers> Logger { get; set; } = null!;

    [Parameter]
    public string LeagueKey { get; set; } = string.Empty;

    [Parameter]
    public string SeasonKey { get; set; } = string.Empty;

    private LeagueResponse? league;
    private SeasonResponse? season;
    private List<PlayerResponse> golfers = new();
    private List<SeasonTeamResponse> teams = new();
    private bool isLoading = true;
    private bool isLoadingGolfers = false;

    // Add Player Modal State
    private bool showAddGolferModal = false;
    private string newPlayerDisplayName = string.Empty;
    private string newPlayerEmail = string.Empty;
    private double? newPlayerHandicap;
    private string? addPlayerError;
    private bool isAddingPlayer = false;

    // Remove Player State
    private bool showRemovePlayerConfirm = false;
    private PlayerResponse? playerToRemove;
    private bool isRemovingPlayer = false;
    private readonly HashSet<string> paymentUpdateInProgress = new();

    // Tab navigation
    private string activeTab = "players";

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
            if (leagueResponse == null || !leagueResponse.Success || leagueResponse.Data == null)
            {
                Logger.LogWarning("League not found: {LeagueKey}", LeagueKey);
                return;
            }
            league = leagueResponse.Data;

            var seasonResponse = await SeasonService.GetSeasonByKeyAsync(league.Id, SeasonKey);
            if (seasonResponse == null || !seasonResponse.Success || seasonResponse.Data == null)
            {
                Logger.LogWarning("Season not found: {SeasonKey}", SeasonKey);
                return;
            }
            season = seasonResponse.Data;

            await Task.WhenAll(LoadGolfers(), LoadTeams());
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

    private bool CanManageSeason => league?.IsCurrentUserAdmin == true || AuthService.IsGlobalAdmin;

    private async Task LoadGolfers()
    {
        if (league == null || season == null) return;

        isLoadingGolfers = true;
        try
        {
            var response = await PlayerService.GetSeasonPlayersAsync(league.Id, season.Id);
            if (response != null && response.Success && response.Data != null)
            {
                golfers = response.Data.ToList();
                Logger.LogInformation("Loaded {Count} golfers for season {SeasonId}", golfers.Count, season.Id);
            }
            else
            {
                golfers = new List<PlayerResponse>();
                Logger.LogWarning("Failed to load season golfers: {Error}", response?.Message ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading season golfers");
        }
        finally
        {
            isLoadingGolfers = false;
        }
    }

    private async Task LoadTeams()
    {
        if (league == null || season == null) return;
        var response = await SeasonService.GetSeasonTeamsAsync(league.Id, season.Id);
        teams = response?.Success == true && response.Data != null ? response.Data : new();
    }

    private string GetTeamName(string teamId)
    {
        return teams.FirstOrDefault(t => t.Id == teamId)?.Name ?? teamId;
    }

    // ── Add Player ─────────────────────────────────────────────────────────────

    private void CloseAddPlayerModal()
    {
        showAddGolferModal = false;
        newPlayerDisplayName = string.Empty;
        newPlayerEmail = string.Empty;
        newPlayerHandicap = null;
        addPlayerError = null;
    }

    private async Task AddPlayer()
    {
        if (string.IsNullOrWhiteSpace(newPlayerDisplayName) || league == null || season == null) return;

        isAddingPlayer = true;
        addPlayerError = null;
        try
        {
            var request = new CreatePlayerRequest
            {
                DisplayName = newPlayerDisplayName.Trim(),
                Email = string.IsNullOrWhiteSpace(newPlayerEmail) ? null : newPlayerEmail.Trim(),
                LeagueHandicap = newPlayerHandicap
            };

            var response = await PlayerService.AddPlayerToSeasonAsync(league.Id, season.Id, request);
            if (response?.Success == true)
            {
                CloseAddPlayerModal();
                await LoadGolfers();
            }
            else
            {
                addPlayerError = response?.Message ?? "Failed to add player";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding player");
            addPlayerError = "An unexpected error occurred";
        }
        finally
        {
            isAddingPlayer = false;
        }
    }

    // ── Remove Player ──────────────────────────────────────────────────────────

    private void OpenRemovePlayerConfirm(PlayerResponse golfer)
    {
        playerToRemove = golfer;
        showRemovePlayerConfirm = true;
    }

    private void CloseRemovePlayerConfirm()
    {
        showRemovePlayerConfirm = false;
        playerToRemove = null;
    }

    private async Task ConfirmRemovePlayer()
    {
        if (playerToRemove == null || season == null || league == null) return;
        if (string.IsNullOrEmpty(playerToRemove.SeasonGolferId))
        {
            CloseRemovePlayerConfirm();
            return;
        }

        isRemovingPlayer = true;
        try
        {
            var success = await SeasonService.RemovePlayerFromSeasonAsync(league.Id, season.Id, playerToRemove.SeasonGolferId);
            if (success)
            {
                showRemovePlayerConfirm = false;
                playerToRemove = null;
                await LoadGolfers();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing player from season");
        }
        finally
        {
            isRemovingPlayer = false;
        }
    }

    private bool IsPaymentUpdating(PlayerResponse golfer)
    {
        return !string.IsNullOrEmpty(golfer.SeasonGolferId)
            && paymentUpdateInProgress.Contains(golfer.SeasonGolferId);
    }

    private async Task TogglePaymentStatus(PlayerResponse golfer)
    {
        if (league == null || season == null || string.IsNullOrEmpty(golfer.SeasonGolferId))
        {
            return;
        }

        if (paymentUpdateInProgress.Contains(golfer.SeasonGolferId))
        {
            return;
        }

        paymentUpdateInProgress.Add(golfer.SeasonGolferId);
        try
        {
            var request = new UpdateSeasonPlayerPaymentRequest
            {
                IsPaidForSeason = golfer.IsPaidForSeason != true
            };

            var success = await SeasonService.UpdateSeasonPlayerPaymentAsync(league.Id, season.Id, golfer.SeasonGolferId, request);
            if (success)
            {
                golfer.IsPaidForSeason = request.IsPaidForSeason;
                golfer.PaidAt = request.IsPaidForSeason ? DateTime.UtcNow : null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating payment status for season golfer {SeasonGolferId}", golfer.SeasonGolferId);
        }
        finally
        {
            paymentUpdateInProgress.Remove(golfer.SeasonGolferId);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private string GetInitials(string displayName)
    {
        if (string.IsNullOrEmpty(displayName)) return "?";
        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "?";
        if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
        return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
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
}
