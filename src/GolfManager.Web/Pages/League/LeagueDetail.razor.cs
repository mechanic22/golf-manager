using GolfManager.Core.Enums;
using GolfManager.Shared.DTOs.League;
using GolfManager.Shared.DTOs.Season;
using GolfManager.Web.Services;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages.League;

public partial class LeagueDetail : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private AppState AppState { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<LeagueDetail> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;

    private LeagueResponse? league;
    private bool isLoading = true;

    private bool hasLoadedDashboard;
    private bool hasLoadedSeasons;
    private bool hasLoadedMembers;
    private bool hasLoadedSettings;
    private int seasonsTabVersion;

    private bool showAddMemberModal;
    private bool isAddingMember;
    private AddLeagueMemberRequest addMemberRequest = new();
    private string? addMemberError;

    private bool showRemoveConfirm;
    private bool isRemovingMember;
    private LeagueMemberResponse? memberToRemove;

    private bool showAddSeasonModal;
    private bool isAddingSeason;
    private CreateSeasonRequest addSeasonRequest = new();
    private string? addSeasonError;

    private string activeTab = "dashboard";
    private static readonly LeagueMemberRole[] LeagueRoles =
    [
        LeagueMemberRole.Owner,
        LeagueMemberRole.Admin,
        LeagueMemberRole.Member,
        LeagueMemberRole.Viewer
    ];

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

        hasLoadedDashboard = true;
        hasLoadedSeasons = true;
        hasLoadedMembers = true;
        hasLoadedSettings = true;
    }

    private bool CanManageLeague => league?.IsCurrentUserAdmin == true || AuthService.IsGlobalAdmin;

    protected override async Task OnParametersSetAsync()
    {
        await LoadActiveTab();
    }

    private async Task LoadActiveTab()
    {
        switch (activeTab)
        {
            case "dashboard":
                if (!hasLoadedDashboard)
                {
                    hasLoadedDashboard = true;
                    StateHasChanged();
                }
                break;
            case "seasons":
                if (!hasLoadedSeasons)
                {
                    hasLoadedSeasons = true;
                    StateHasChanged();
                }
                break;
            case "members":
                if (!hasLoadedMembers)
                {
                    hasLoadedMembers = true;
                    StateHasChanged();
                }
                break;
            case "settings":
                if (!hasLoadedSettings)
                {
                    hasLoadedSettings = true;
                    StateHasChanged();
                }
                break;
        }

        await Task.CompletedTask;
    }

    private async Task LoadLeague()
    {
        isLoading = true;
        try
        {
            var response = await LeagueService.GetLeagueByKeyAsync(LeagueKey);
            if (response != null && response.Success && response.Data != null)
            {
                league = response.Data;
                Logger.LogInformation("Loaded league: {LeagueName}", league.Name);
            }
            else if (response != null && !response.Success && string.Equals(response.Message, "Forbidden", StringComparison.OrdinalIgnoreCase))
            {
                Navigation.NavigateTo($"/access-denied?scope=league&leagueKey={Uri.EscapeDataString(LeagueKey)}");
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

    private async Task ReloadLeague()
    {
        await LoadLeague();
        StateHasChanged();
    }

    private async Task LoadMembers()
    {
        hasLoadedMembers = false;
        hasLoadedMembers = true;
        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task LoadSeasons()
    {
        seasonsTabVersion++;
        hasLoadedSeasons = true;
        StateHasChanged();
        await Task.CompletedTask;
    }

    private void ShowAddMemberModal()
    {
        addMemberRequest = new AddLeagueMemberRequest();
        addMemberError = null;
        showAddMemberModal = true;
    }

    private void ShowAddSeasonModal()
    {
        addSeasonRequest = new CreateSeasonRequest
        {
            StartDate = DateOnly.FromDateTime(DateTime.Today)
        };
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
            var response = await SeasonService.CreateSeasonAsync(league.Id, addSeasonRequest);
            if (response?.Success == true && response.Data != null)
            {
                showAddSeasonModal = false;
                await LoadSeasons();
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

    private async Task HandleAddMember()
    {
        if (league == null)
        {
            return;
        }

        addMemberError = null;
        isAddingMember = true;

        try
        {
            var response = await LeagueService.AddLeagueMemberAsync(league.Id, addMemberRequest);

            if (response?.Success == true)
            {
                showAddMemberModal = false;
                await LoadMembers();
                await LoadLeague();
            }
            else
            {
                addMemberError = response?.Message ?? "Failed to add member";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding member");
            addMemberError = "An error occurred while adding the member";
        }
        finally
        {
            isAddingMember = false;
        }
    }

    private async Task PromoteToAdmin(LeagueMemberResponse member)
    {
        if (league == null)
        {
            return;
        }

        try
        {
            var request = new UpdateLeagueMemberRequest { Role = LeagueMemberRole.Admin };
            var response = await LeagueService.UpdateLeagueMemberAsync(league.Id, member.UserId, request);

            if (response?.Success == true)
            {
                await LoadMembers();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error promoting member");
        }
    }

    private async Task DemoteFromAdmin(LeagueMemberResponse member)
    {
        if (league == null)
        {
            return;
        }

        try
        {
            var request = new UpdateLeagueMemberRequest { Role = LeagueMemberRole.Member };
            var response = await LeagueService.UpdateLeagueMemberAsync(league.Id, member.UserId, request);

            if (response?.Success == true)
            {
                await LoadMembers();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error demoting member");
        }
    }

    private void ShowRemoveMemberConfirm(LeagueMemberResponse member)
    {
        memberToRemove = member;
        showRemoveConfirm = true;
    }

    private async Task HandleRemoveMember()
    {
        if (league == null || memberToRemove == null)
        {
            return;
        }

        isRemovingMember = true;

        try
        {
            var response = await LeagueService.RemoveLeagueMemberAsync(league.Id, memberToRemove.UserId);

            if (response?.Success == true)
            {
                showRemoveConfirm = false;
                memberToRemove = null;
                await LoadMembers();
                await LoadLeague();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing member");
        }
        finally
        {
            isRemovingMember = false;
        }
    }
}
