using GolfManager.Shared.DTOs.User;
using GolfManager.Web.Services;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages.Admin;

public partial class AdminUsers : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<AdminUsers> Logger { get; set; } = null!;

    private bool isLoading = true;
    private bool isGlobalAdmin;
    private bool showInviteModal;
    private bool showEditModal;
    private bool isSaving;
    private string searchQuery = string.Empty;
    private string roleFilter = "all";
    private string statusFilter = "all";
    private List<UserResponse> allUsers = new();

    private UserResponse? editingUser;
    private string editFirstName = string.Empty;
    private string editLastName = string.Empty;
    private string editEmail = string.Empty;
    private bool editIsGlobalAdmin;
    private bool editIsActive = true;
    private string editError = string.Empty;

    private List<UserResponse> FilteredUsers => allUsers
        .Where(u => string.IsNullOrEmpty(searchQuery) ||
                    u.FirstName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    u.LastName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
        .Where(u => roleFilter == "all" ||
                    (roleFilter == "admin" && u.IsGlobalAdmin) ||
                    (roleFilter == "league_admin" && u.LeagueAdminCount > 0))
        .Where(u => statusFilter == "all" ||
                    (statusFilter == "active" && u.IsActive) ||
                    (statusFilter == "inactive" && !u.IsActive))
        .ToList();

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        isGlobalAdmin = AuthorizationService.IsGlobalAdmin();

        if (!isGlobalAdmin)
        {
            isLoading = false;
            return;
        }

        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        try
        {
            var response = await UserService.GetAllUsersAsync(includeInactive: true);
            allUsers = response is { Success: true, Data: not null }
                ? response.Data
                : new List<UserResponse>();

            Logger.LogInformation("Loaded {Count} users from API", allUsers.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading users");
            allUsers = new List<UserResponse>();
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OpenEditModal(UserResponse user)
    {
        editingUser = user;
        editFirstName = user.FirstName;
        editLastName = user.LastName;
        editEmail = user.Email;
        editIsGlobalAdmin = user.IsGlobalAdmin;
        editIsActive = user.IsActive;
        editError = string.Empty;
        showEditModal = true;
    }

    private void CloseEditModal()
    {
        showEditModal = false;
        editingUser = null;
        editError = string.Empty;
    }

    private async Task SaveUser()
    {
        if (editingUser == null)
        {
            return;
        }

        isSaving = true;
        editError = string.Empty;

        try
        {
            var request = new UpdateUserRequest
            {
                FirstName = editFirstName,
                LastName = editLastName,
                Email = editEmail,
                IsGlobalAdmin = editIsGlobalAdmin,
                IsActive = editIsActive
            };

            var response = await UserService.UpdateUserAsync(editingUser.Id, request);

            if (response?.Success == true)
            {
                Logger.LogInformation("User {UserId} updated successfully", editingUser.Id);
                CloseEditModal();
                await LoadData();
            }
            else
            {
                editError = response?.Message ?? "Failed to update user";
                Logger.LogWarning("Failed to update user: {Error}", editError);
            }
        }
        catch (Exception ex)
        {
            editError = "An error occurred while saving";
            Logger.LogError(ex, "Error saving user");
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task ToggleUserStatus(UserResponse user)
    {
        try
        {
            var request = new UpdateUserRequest
            {
                IsActive = !user.IsActive
            };

            var response = await UserService.UpdateUserAsync(user.Id, request);

            if (response?.Success == true)
            {
                Logger.LogInformation("User {UserId} status toggled to {Status}", user.Id, !user.IsActive);
                await LoadData();
            }
            else
            {
                Logger.LogWarning("Failed to toggle user status: {Error}", response?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error toggling user status");
        }
    }

    private async Task SendPasswordReset(UserResponse user)
    {
        try
        {
            var response = await UserService.SendPasswordResetAsync(user.Id);

            if (response?.Success == true)
            {
                Logger.LogInformation("Password reset sent for user {UserId}", user.Id);
            }
            else
            {
                Logger.LogWarning("Failed to send password reset: {Error}", response?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending password reset");
        }
    }

    private static string GetInitials(string firstName, string lastName)
    {
        var first = !string.IsNullOrEmpty(firstName) ? firstName[0].ToString() : string.Empty;
        var last = !string.IsNullOrEmpty(lastName) ? lastName[0].ToString() : string.Empty;
        return string.Concat(first, last).ToUpperInvariant();
    }

    private static string GetRoleBadgeClass(UserResponse user)
    {
        return user.IsGlobalAdmin
            ? "bg-red-100 text-red-800"
            : "bg-gray-100 text-gray-800";
    }

    private static string GetRoleText(UserResponse user)
    {
        if (user.IsGlobalAdmin)
        {
            return "Global Admin";
        }

        if (user.LeagueAdminCount > 0)
        {
            return $"League Admin ({user.LeagueAdminCount})";
        }

        return "Player";
    }
}