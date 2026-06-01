using GolfManager.Core.Enums;
using GolfManager.Shared.DTOs.League;
using GolfManager.Web.Services;
using MaterialComponents.Models;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages.League;

public partial class LeagueMembers : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private AppState AppState { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<LeagueMembers> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;

    private LeagueResponse? league;
    private List<LeagueMemberResponse>? members;
    private bool isLoading = true;
    private bool isLoadingMembers = true;
    private bool showAddMemberModal;
    private bool isAddingMember;
    private AddLeagueMemberRequest addMemberRequest = new();
    private string? addMemberError;
    private bool showRemoveConfirm;
    private bool isRemovingMember;
    private LeagueMemberResponse? memberToRemove;
    private string activeTab = "members";
    private static readonly LeagueMemberRole[] LeagueRoles =
    [
        LeagueMemberRole.Owner,
        LeagueMemberRole.Admin,
        LeagueMemberRole.Member,
        LeagueMemberRole.Viewer
    ];

    private bool CanManageLeague => league?.IsCurrentUserAdmin == true || AuthService.IsGlobalAdmin;
    private LeagueMemberRole CurrentUserRole => members?
        .FirstOrDefault(m => m.UserId == AuthService.UserId)?.Role ?? LeagueMemberRole.Member;
    private bool CanManageOwnerRole => AuthService.IsGlobalAdmin || CurrentUserRole == LeagueMemberRole.Owner;

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
        await LoadMembers();
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

    private async Task LoadMembers()
    {
        if (league == null)
        {
            return;
        }

        try
        {
            isLoadingMembers = true;

            var response = await LeagueService.GetLeagueMembersAsync(league.Id);
            if (response != null && response.Success && response.Data != null)
            {
                members = response.Data.ToList();
                Logger.LogInformation("Loaded {Count} members for league {LeagueId}", members.Count, league.Id);
            }
            else
            {
                members = new List<LeagueMemberResponse>();
                Logger.LogWarning("Failed to load members: {Error}", response?.Message ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading members");
        }
        finally
        {
            isLoadingMembers = false;
        }
    }

    private void ShowAddMemberModal()
    {
        addMemberRequest = new AddLeagueMemberRequest();
        addMemberError = null;
        showAddMemberModal = true;
    }

    private async Task HandleAddMember()
    {
        if (league == null)
        {
            return;
        }

        if (addMemberRequest.Role == LeagueMemberRole.Owner && !CanManageOwnerRole)
        {
            addMemberError = "Only the league owner or a global admin can assign owner role.";
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

    private async Task ChangeMemberRole(LeagueMemberResponse member, ChangeEventArgs args)
    {
        if (league == null)
        {
            return;
        }

        if (args.Value == null || !Enum.TryParse<LeagueMemberRole>(args.Value.ToString(), out var role))
        {
            return;
        }

        if (!CanEditMember(member) || !CanAssignRole(role))
        {
            return;
        }

        try
        {
            var request = new UpdateLeagueMemberRequest { Role = role };
            var response = await LeagueService.UpdateLeagueMemberAsync(league.Id, member.UserId, request);

            if (response?.Success == true)
            {
                await LoadMembers();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating member role");
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
        if (!CanRemoveMember(member))
        {
            return;
        }

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

    private static string GetInitials(string firstName, string lastName)
    {
        var first = !string.IsNullOrEmpty(firstName) ? firstName[0].ToString() : string.Empty;
        var last = !string.IsNullOrEmpty(lastName) ? lastName[0].ToString() : string.Empty;
        return (first + last).ToUpperInvariant();
    }

    private static MaterialChipColor GetRoleColor(LeagueMemberRole role)
    {
        return role switch
        {
            LeagueMemberRole.Owner => MaterialChipColor.Primary,
            LeagueMemberRole.Admin => MaterialChipColor.Info,
            LeagueMemberRole.Viewer => MaterialChipColor.Secondary,
            _ => MaterialChipColor.Default
        };
    }

    private bool CanEditMember(LeagueMemberResponse member)
    {
        if (!CanManageLeague)
        {
            return false;
        }

        if (member.Role == LeagueMemberRole.Owner && !CanManageOwnerRole)
        {
            return false;
        }

        return true;
    }

    private bool CanRemoveMember(LeagueMemberResponse member)
    {
        return CanEditMember(member);
    }

    private bool CanAssignRole(LeagueMemberRole role)
    {
        return role != LeagueMemberRole.Owner || CanManageOwnerRole;
    }

    private IEnumerable<LeagueMemberRole> GetAssignableRoles()
    {
        return CanManageOwnerRole
            ? LeagueRoles
            : LeagueRoles.Where(role => role != LeagueMemberRole.Owner);
    }

    private IEnumerable<LeagueMemberRole> GetAssignableRolesForMember(LeagueMemberResponse member)
    {
        if (!CanEditMember(member))
        {
            return new[] { member.Role };
        }

        return GetAssignableRoles();
    }
}
