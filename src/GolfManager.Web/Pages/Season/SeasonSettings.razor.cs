using GolfManager.Web.Components.Icons;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages.Season;

public partial class SeasonSettings : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<SeasonSettings> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;

    private bool isLoading = true;
    private bool isAdmin = false;
    private LeagueResponse? league;
    private SeasonResponse? season;
    private string leagueName = string.Empty;
    private string seasonName = string.Empty;
    private DateTime startDate = DateTime.Today;
    private DateTime endDate = DateTime.Today.AddMonths(3);
    private string scoringFormat = "stroke";
    private bool useHandicaps = true;
    private bool dropWorstScores = false;
    private int scoresToDrop = 1;
    private bool allowRegistration = true;
    private DateTime? registrationDeadline;
    private int? maxPlayers;
    private bool isArchiving;
    private bool replaceExistingSetup;
    private bool isApplyingSeasonSetup;
    private bool seasonSetupSuccess;
    private string seasonSetupMessage = string.Empty;
    private string calendarText = string.Empty;
    private string teamsText = string.Empty;
    private SeasonSetupResponse? seasonSetupResult;

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await AuthorizationService.InitializeAsync();

        // Load league and season to get real IDs for authorization check
        var leagueResponse = await LeagueService.GetLeagueByKeyAsync(LeagueKey);
        if (leagueResponse?.Data != null)
        {
            league = leagueResponse.Data;
            leagueName = league.Name;

            var seasonResponse = await SeasonService.GetSeasonByKeyAsync(league.Id, SeasonKey);
            if (seasonResponse?.Data != null)
            {
                season = seasonResponse.Data;
                seasonName = season.Name;
                startDate = season.StartDate.ToDateTime(TimeOnly.MinValue);
                endDate = season.EndDate.HasValue ? season.EndDate.Value.ToDateTime(TimeOnly.MinValue) : DateTime.Today.AddMonths(3);
            }
        }

        isAdmin = league?.IsCurrentUserAdmin == true || AuthService.IsGlobalAdmin;

        isLoading = false;
    }

    private void BackToSeason()
    {
        Navigation.NavigateTo($"/league/{LeagueKey}/season/{SeasonKey}");
    }

    private async Task SaveSettings()
    {
        try
        {
            // TODO: Save settings to API
            Logger.LogInformation("Saving season settings for {SeasonKey}", SeasonKey);
            await Task.CompletedTask;

            BackToSeason();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving season settings");
        }
    }

    private async Task ArchiveSeason()
    {
        if (league == null || season == null)
        {
            return;
        }

        try
        {
            isArchiving = true;

            var response = await SeasonService.UpdateSeasonAsync(
                league.Id,
                season.Id,
                new UpdateSeasonRequest
                {
                    IsLocked = true
                });

            if (response?.Success == true && response.Data != null)
            {
                season = response.Data;
                Logger.LogInformation("Archived season {SeasonKey}", SeasonKey);
                Navigation.NavigateTo($"/league/{LeagueKey}/seasons");
                return;
            }

            Logger.LogWarning("Failed to archive season {SeasonKey}: {Message}", SeasonKey, response?.Message ?? "Unknown error");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error archiving season settings for {SeasonKey}", SeasonKey);
        }
        finally
        {
            isArchiving = false;
        }
    }

    private async Task ApplySeasonSetup()
    {
        if (league == null || season == null)
        {
            return;
        }

        isApplyingSeasonSetup = true;
        seasonSetupMessage = string.Empty;
        seasonSetupResult = null;

        try
        {
            var response = await SeasonService.SetupSeasonAsync(
                league.Id,
                season.Id,
                new SeasonSetupRequest
                {
                    CalendarText = calendarText,
                    TeamsText = teamsText,
                    ReplaceExistingData = replaceExistingSetup
                });

            if (response?.Success == true && response.Data != null)
            {
                seasonSetupSuccess = true;
                seasonSetupResult = response.Data;
                seasonSetupMessage = response.Message ?? "Season setup completed.";

                var refreshedSeason = await SeasonService.GetSeasonByKeyAsync(league.Id, SeasonKey);
                if (refreshedSeason?.Success == true && refreshedSeason.Data != null)
                {
                    season = refreshedSeason.Data;
                }

                return;
            }

            seasonSetupSuccess = false;
            seasonSetupMessage = response?.Message ?? "Season setup failed.";
        }
        catch (Exception ex)
        {
            seasonSetupSuccess = false;
            seasonSetupMessage = ex.Message;
            Logger.LogError(ex, "Error applying season setup for {SeasonKey}", SeasonKey);
        }
        finally
        {
            isApplyingSeasonSetup = false;
        }
    }

    private string GetApplySetupButtonText()
    {
        return isApplyingSeasonSetup ? "Applying Setup..." : "Apply Setup";
    }

    private async Task DeleteSeason()
    {
        // TODO: Implement delete confirmation and API call
        Logger.LogInformation("Deleting season {SeasonKey}", SeasonKey);
        await Task.CompletedTask;
    }
}
