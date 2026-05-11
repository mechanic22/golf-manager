using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages.Profile;

public partial class ProfileStats : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<ProfileStats> Logger { get; set; } = null!;

    // TODO: Load actual stats from API
}
