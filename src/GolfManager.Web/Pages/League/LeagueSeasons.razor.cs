using GolfManager.Shared.DTOs.League;
using GolfManager.Shared.DTOs.Season;
using GolfManager.Web.Services;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages.League;

public partial class LeagueSeasons : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private AppState AppState { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<LeagueSeasons> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;

    private LeagueResponse? league;
    private List<SeasonResponse>? seasons;
    private bool isLoading = true;
    private bool isLoadingSeasons = true;
    private bool showAddSeasonModal;
    private bool isAddingSeason;
    private CreateSeasonRequest addSeasonRequest = new();
    private string? addSeasonError;
    private string activeTab = "seasons";
    private SeasonResponse? activeSeason;
    private string addSeasonStartDateText = string.Empty;
    private string addSeasonEndDateText = string.Empty;

    private bool CanManageLeague => league?.IsCurrentUserAdmin == true || AuthService.IsGlobalAdmin;

    private void HandleStartDateChanged(ChangeEventArgs args)
    {
        addSeasonStartDateText = args.Value?.ToString() ?? string.Empty;
    }

    private void HandleEndDateChanged(ChangeEventArgs args)
    {
        addSeasonEndDateText = args.Value?.ToString() ?? string.Empty;
    }

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await AuthorizationService.InitializeAsync();
        AppState.SetCurrentLeague(LeagueKey);

        await LoadLeague();
        await LoadSeasons();
    }

    private async Task LoadLeague()
    {
        try
        {
            isLoading = true;

            var response = await LeagueService.GetLeagueByKeyAsync(LeagueKey);
            if (response != null && response.Success && response.Data != null)
            {
                league = response.Data;
                Logger.LogInformation("Loaded league: {LeagueName}", league.Name);
            }
            else
            {
                Logger.LogWarning("League not found: {LeagueKey}", LeagueKey);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading league");
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadSeasons()
    {
        if (league == null)
        {
            return;
        }

        try
        {
            isLoadingSeasons = true;

            var seasonsResponse = await SeasonService.GetLeagueSeasonsAsync(league.Id);
            if (seasonsResponse != null && seasonsResponse.Success && seasonsResponse.Data != null)
            {
                seasons = seasonsResponse.Data.ToList();
                activeSeason = seasons
                    .OrderByDescending(s => s.StartDate)
                    .FirstOrDefault(s => !s.IsLocked);
                Logger.LogInformation("Loaded {Count} seasons for league {LeagueId}", seasons.Count, league.Id);
            }
            else
            {
                seasons = new List<SeasonResponse>();
                activeSeason = null;
                Logger.LogWarning("Failed to load seasons: {Error}", seasonsResponse?.Message ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading seasons");
        }
        finally
        {
            isLoadingSeasons = false;
        }
    }

    private void ShowAddSeasonModal()
    {
        addSeasonRequest = new CreateSeasonRequest
        {
            StartDate = activeSeason?.EndDate?.AddDays(1) ?? DateOnly.FromDateTime(DateTime.Today)
        };
        addSeasonStartDateText = addSeasonRequest.StartDate.ToString("yyyy-MM-dd");
        addSeasonEndDateText = addSeasonRequest.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty;
        addSeasonError = null;
        showAddSeasonModal = true;
    }

    private async Task HandleAddSeason()
    {
        if (league == null)
        {
            return;
        }

        addSeasonError = null;
        isAddingSeason = true;

        try
        {
            if (!DateOnly.TryParse(addSeasonStartDateText, out var startDate))
            {
                addSeasonError = "Please provide a valid start date.";
                return;
            }

            addSeasonRequest.StartDate = startDate;
            addSeasonRequest.EndDate = string.IsNullOrWhiteSpace(addSeasonEndDateText)
                ? null
                : DateOnly.TryParse(addSeasonEndDateText, out var endDate)
                    ? endDate
                    : null;

            if (!string.IsNullOrWhiteSpace(addSeasonEndDateText) && !addSeasonRequest.EndDate.HasValue)
            {
                addSeasonError = "Please provide a valid end date.";
                return;
            }

            if (activeSeason != null)
            {
                var lockResponse = await SeasonService.UpdateSeasonAsync(
                    league.Id,
                    activeSeason.Id,
                    new UpdateSeasonRequest
                    {
                        IsLocked = true
                    });

                if (lockResponse?.Success != true || lockResponse.Data == null)
                {
                    addSeasonError = lockResponse?.Message ?? "Failed to close the current active season.";
                    return;
                }
            }

            var response = await SeasonService.CreateSeasonAsync(league.Id, addSeasonRequest);
            if (response?.Success == true && response.Data != null)
            {
                showAddSeasonModal = false;
                await LoadSeasons();
                Navigation.NavigateTo($"/league/{LeagueKey}/season/{response.Data.Key}");
            }
            else
            {
                addSeasonError = response?.Message ?? "Failed to create season. Please try again.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating season");
            addSeasonError = "An error occurred while creating the season. Please try again.";
        }
        finally
        {
            isAddingSeason = false;
        }
    }
}
