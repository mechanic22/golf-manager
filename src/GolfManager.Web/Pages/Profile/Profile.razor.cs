using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages.Profile;

public partial class Profile : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<Profile> Logger { get; set; } = null!;

    private UserProfileResponse? user;
    private bool isLoading = true;
    private string activeTab = "overview";
    private double? currentHandicap;
    private int totalRounds;
    private int totalLeagues;

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

            if (response != null && response.Success && response.Data != null)
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
    }

    private void EditProfile()
    {
        // TODO: Implement edit profile modal or navigate to edit page
        Logger.LogInformation("Edit profile clicked");
    }

    // Icon helpers
    private static RenderFragment EditIcon() => _ => { };
}
