using GolfManager.Shared.DTOs.League;
using GolfManager.Web.Services;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages.League;

public partial class LeagueSettings : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private AppState AppState { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<LeagueSettings> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;

    private LeagueResponse? league;
    private UpdateLeagueRequest updateRequest = new();
    private bool isLoading = true;
    private bool isSaving;
    private bool isVerifying;
    private string? saveError;
    private string? saveSuccess;
    private string? verifyError;
    private string? verifySuccess;
    private string activeTab = "settings";

    private bool UpdateRequestUseCustomDomain
    {
        get => updateRequest.UseCustomDomain ?? false;
        set => updateRequest.UseCustomDomain = value;
    }

    private bool UpdateRequestRequireAnonymousPassword
    {
        get => updateRequest.RequireAnonymousPassword ?? false;
        set => updateRequest.RequireAnonymousPassword = value;
    }

    private bool UpdateRequestClearAnonymousPassword
    {
        get => updateRequest.ClearAnonymousAccessPassword ?? false;
        set => updateRequest.ClearAnonymousAccessPassword = value;
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
                return;
            }

            updateRequest = new UpdateLeagueRequest
            {
                Name = league.Name,
                Description = league.Description,
                LogoUrl = league.LogoUrl,
                WelcomeHeadline = league.WelcomeHeadline,
                WelcomeSubhead = league.WelcomeSubhead,
                EmptyStateMessage = league.EmptyStateMessage,
                CommissionerName = league.CommissionerName,
                AnnouncementTitle = league.AnnouncementTitle,
                AnnouncementBody = league.AnnouncementBody,
                CustomDomain = league.CustomDomain,
                UseCustomDomain = league.UseCustomDomain,
                RequireAnonymousPassword = league.RequireAnonymousPassword
            };
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

    private void ResetForm()
    {
        if (league == null)
        {
            return;
        }

        updateRequest = new UpdateLeagueRequest
        {
            Name = league.Name,
            Description = league.Description,
            LogoUrl = league.LogoUrl,
            WelcomeHeadline = league.WelcomeHeadline,
            WelcomeSubhead = league.WelcomeSubhead,
            EmptyStateMessage = league.EmptyStateMessage,
            CommissionerName = league.CommissionerName,
            AnnouncementTitle = league.AnnouncementTitle,
            AnnouncementBody = league.AnnouncementBody,
            CustomDomain = league.CustomDomain,
            UseCustomDomain = league.UseCustomDomain,
            RequireAnonymousPassword = league.RequireAnonymousPassword
        };

        saveError = null;
        saveSuccess = null;
        verifyError = null;
        verifySuccess = null;
    }

    private async Task HandleSave()
    {
        if (league == null)
        {
            return;
        }

        saveError = null;
        saveSuccess = null;
        isSaving = true;

        try
        {
            var response = await LeagueService.UpdateLeagueAsync(league.Id, updateRequest);

            if (response?.Success == true && response.Data != null)
            {
                league = response.Data;
                updateRequest.WelcomeHeadline = league.WelcomeHeadline;
                updateRequest.WelcomeSubhead = league.WelcomeSubhead;
                updateRequest.EmptyStateMessage = league.EmptyStateMessage;
                updateRequest.CommissionerName = league.CommissionerName;
                updateRequest.AnnouncementTitle = league.AnnouncementTitle;
                updateRequest.AnnouncementBody = league.AnnouncementBody;
                updateRequest.CustomDomain = league.CustomDomain;
                updateRequest.UseCustomDomain = league.UseCustomDomain;
                updateRequest.RequireAnonymousPassword = league.RequireAnonymousPassword;
                updateRequest.AnonymousAccessPassword = null;
                updateRequest.ClearAnonymousAccessPassword = null;
                saveSuccess = "League settings saved successfully!";

                _ = Task.Delay(3000).ContinueWith(_ =>
                {
                    saveSuccess = null;
                    InvokeAsync(StateHasChanged);
                });
            }
            else
            {
                saveError = response?.Message ?? "Failed to save settings. Please try again.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving league settings");
            saveError = "An error occurred while saving settings. Please try again.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task HandleVerifyDomain()
    {
        if (league == null)
        {
            return;
        }

        verifyError = null;
        verifySuccess = null;
        isVerifying = true;

        try
        {
            var response = await LeagueService.VerifyLeagueCustomDomainAsync(league.Id);
            if (response?.Success == true && response.Data != null)
            {
                league = response.Data;
                updateRequest.CustomDomain = league.CustomDomain;
                updateRequest.UseCustomDomain = league.UseCustomDomain;
                verifySuccess = "Custom domain verified successfully.";

                _ = Task.Delay(3000).ContinueWith(_ =>
                {
                    verifySuccess = null;
                    InvokeAsync(StateHasChanged);
                });
            }
            else
            {
                verifyError = response?.Message ?? "Failed to verify custom domain. Please check your DNS settings and try again.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error verifying custom domain");
            verifyError = "An error occurred while verifying the custom domain. Please try again.";
        }
        finally
        {
            isVerifying = false;
        }
    }
}
