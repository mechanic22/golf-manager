using GolfManager.Shared.DTOs.Season;
using GolfManager.Web.Layout;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Season.Components;

public partial class SeasonSettingsTab : ComponentBase
{
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<SeasonSettingsTab> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [CascadingParameter] private SeasonLayoutContext? SeasonCtx { get; set; }

    private string LeagueId => SeasonCtx!.League.Id;
    private string SeasonId => SeasonCtx!.Season.Id;

    private string? _loadedSeasonId;
    private bool isLoading = true;

    private bool isArchiving;
    private bool isDeleting;
    private bool confirmDelete;
    private string? deleteError;

    private bool isApplyingSeasonSetup;
    private bool seasonSetupSuccess;
    private string seasonSetupMessage = string.Empty;
    private string calendarText = string.Empty;
    private string teamsText = string.Empty;
    private SeasonSetupResponse? seasonSetupResult;
    private bool replaceExistingSetup;

    protected override Task OnParametersSetAsync()
    {
        if (SeasonCtx?.Season.Id != null && SeasonCtx.Season.Id != _loadedSeasonId)
        {
            _loadedSeasonId = SeasonCtx.Season.Id;
            isLoading = false;
        }
        return base.OnParametersSetAsync();
    }

    private async Task ArchiveSeason()
    {
        try
        {
            isArchiving = true;
            var response = await SeasonService.UpdateSeasonAsync(LeagueId, SeasonId, new UpdateSeasonRequest { IsLocked = true });
            if (response?.Success == true)
            {
                Navigation.NavigateTo($"/league/{LeagueKey}/seasons");
                return;
            }
            Logger.LogWarning("Failed to archive season {SeasonKey}: {Message}", SeasonKey, response?.Message ?? "Unknown error");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error archiving season {SeasonKey}", SeasonKey);
        }
        finally
        {
            isArchiving = false;
        }
    }

    private async Task ApplySeasonSetup()
    {
        isApplyingSeasonSetup = true;
        seasonSetupMessage = string.Empty;
        seasonSetupResult = null;

        try
        {
            var response = await SeasonService.SetupSeasonAsync(LeagueId, SeasonId,
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

    private string GetApplySetupButtonText() =>
        isApplyingSeasonSetup ? "Applying Setup..." : "Apply Setup";

    private async Task DeleteSeason()
    {
        if (!confirmDelete)
        {
            confirmDelete = true;
            return;
        }

        isDeleting = true;
        deleteError = null;

        try
        {
            var success = await SeasonService.DeleteSeasonAsync(LeagueId, SeasonId);
            if (success)
                Navigation.NavigateTo($"/league/{LeagueKey}/seasons");
            else
            {
                deleteError = "Failed to delete season. Please try again.";
                confirmDelete = false;
            }
        }
        catch (Exception ex)
        {
            deleteError = "An error occurred while deleting the season.";
            confirmDelete = false;
            Logger.LogError(ex, "Error deleting season {SeasonKey}", SeasonKey);
        }
        finally
        {
            isDeleting = false;
        }
    }
}
