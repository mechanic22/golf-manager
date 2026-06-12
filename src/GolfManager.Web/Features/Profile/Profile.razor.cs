using GolfManager.Shared.DTOs.User;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Profile;

public partial class Profile : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<Profile> Logger { get; set; } = null!;

    private UserProfileResponse? user;
    private bool isLoading = true;
    private string activeTab = "overview";
    private readonly HashSet<string> _mountedTabs = new();
    private double? currentHandicap;
    private int totalRounds;
    private int totalLeagues;

    // Edit profile modal state
    private bool showEditModal;
    private bool isSavingProfile;
    private string editFirstName = string.Empty;
    private string editLastName = string.Empty;
    private string? editError;

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
        isLoading = true;
        try
        {
            var response = await UserService.GetCurrentUserAsync();

            if (response?.Success == true && response.Data != null)
            {
                user = response.Data;
                totalLeagues = user.LeagueCount;
                currentHandicap = user.HandicapIndex;
                totalRounds = user.RoundsCount;

                Logger.LogInformation("Loaded profile for user {Email}: {Rounds} rounds, handicap {Handicap}",
                    user.Email, totalRounds, currentHandicap);
            }
            else
            {
                Logger.LogWarning("Failed to load user profile");
                user = null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading profile");
            user = null;
        }
        finally
        {
            isLoading = false;
        }
    }

    private string GetInitials()
    {
        if (user == null) return "?";
        var first = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName[0].ToString() : "";
        var last = !string.IsNullOrEmpty(user.LastName) ? user.LastName[0].ToString() : "";
        return $"{first}{last}".ToUpper();
    }

    private List<MaterialTab> GetTabs()
    {
        return new List<MaterialTab>
        {
            new MaterialTab
            {
                Value = "overview",
                Label = "Overview"
            },
            new MaterialTab
            {
                Value = "stats",
                Label = "Stats"
            },
            new MaterialTab
            {
                Value = "rounds",
                Label = "Rounds"
            },
            new MaterialTab
            {
                Value = "handicap",
                Label = "Handicap"
            }
        };
    }

    private void HandleTabChange(string tabValue)
    {
        activeTab = tabValue;
        _mountedTabs.Add(tabValue);
    }

    private void EditProfile()
    {
        if (user == null) return;
        editFirstName = user.FirstName;
        editLastName = user.LastName;
        editError = null;
        showEditModal = true;
    }

    private void CloseEditModal()
    {
        showEditModal = false;
        editError = null;
    }

    private async Task SaveProfile()
    {
        if (isSavingProfile) return;
        isSavingProfile = true;
        editError = null;

        try
        {
            var response = await UserService.UpdateCurrentUserAsync(new UpdateProfileRequest
            {
                FirstName = editFirstName.Trim(),
                LastName = editLastName.Trim()
            });

            if (response?.Success == true && response.Data != null)
            {
                user = response.Data;
                showEditModal = false;
            }
            else
            {
                editError = response?.Message ?? "Failed to save profile. Please try again.";
            }
        }
        catch (Exception ex)
        {
            editError = "An error occurred while saving.";
            Logger.LogError(ex, "Error saving profile");
        }
        finally
        {
            isSavingProfile = false;
        }
    }

    // Icon helpers
    private static RenderFragment EditIcon() => _ => { };
}
