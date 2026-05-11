using GolfManager.Shared.DTOs.Player;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages;

public partial class PlayerDetail : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private IPlayerService PlayerService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<PlayerDetail> Logger { get; set; } = null!;

    [Parameter]
    public string LeagueKey { get; set; } = string.Empty;

    [Parameter]
    public string PlayerId { get; set; } = string.Empty;

    private LeagueResponse? league;
    private PlayerResponse? player;
    private bool isLoading = true;

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
            // Load league first to get the ID
            var leagueResponse = await LeagueService.GetLeagueByKeyAsync(LeagueKey);
            if (leagueResponse?.Success == true && leagueResponse.Data != null)
            {
                league = leagueResponse.Data;

                // Load player
                var playerResponse = await PlayerService.GetPlayerAsync(league.Id, PlayerId);
                if (playerResponse?.Success == true && playerResponse.Data != null)
                {
                    player = playerResponse.Data;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading player data");
        }
        finally
        {
            isLoading = false;
        }
    }

    private string GetInitials(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return "?";

        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }
}
