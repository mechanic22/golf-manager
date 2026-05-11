using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages;

public partial class EventDetail : ComponentBase
{
    [Inject] private IOneTimeEventService EventService { get; set; } = null!;
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private ILogger<EventDetail> Logger { get; set; } = null!;

    [Parameter]
    public string EventKey { get; set; } = string.Empty;

    private OneTimeEventResponse? eventData;
    private List<OneTimeEventTeamResponse> teams = new();
    private bool isLoading = true;
    private bool isOrganizer = false;
    private bool isRegistrationClosed = false;

    private bool showRegisterModal = false;
    private bool isRegistering = false;
    private RegisterTeamRequest registerRequest = new();
    private string? registerErrorMessage;

    private bool showAddPlayerModal = false;
    private bool isAddingPlayer = false;
    private AddPlayerRequest addPlayerRequest = new();
    private string? addPlayerErrorMessage;
    private string? selectedTeamId;

    protected override async Task OnInitializedAsync()
    {
        await LoadEvent();
    }

    private async Task LoadEvent()
    {
        isLoading = true;
        try
        {
            var response = await EventService.GetEventByKeyAsync(EventKey);
            if (response?.Success == true && response.Data != null)
            {
                eventData = response.Data;
                isOrganizer = AuthService.IsAuthenticated && AuthService.UserId == eventData.OrganizerId;
                isRegistrationClosed = eventData.RegistrationDeadline.HasValue && DateTime.UtcNow > eventData.RegistrationDeadline.Value;

                // Load teams
                var teamsResponse = await EventService.GetEventTeamsAsync(eventData.Id);
                if (teamsResponse?.Success == true && teamsResponse.Data != null)
                {
                    teams = teamsResponse.Data;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading event");
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ShowRegisterModal()
    {
        registerRequest = new RegisterTeamRequest
        {
            Players = new List<RegisterTeamRequest.PlayerInfo>()
        };
        registerErrorMessage = null;
        showRegisterModal = true;
    }

    private void HideRegisterModal()
    {
        showRegisterModal = false;
    }

    private async Task HandleRegisterTeam()
    {
        if (eventData == null) return;

        isRegistering = true;
        registerErrorMessage = null;

        try
        {
            // Add captain as first player
            registerRequest.Players = new List<RegisterTeamRequest.PlayerInfo>
            {
                new() { PlayerName = registerRequest.CaptainName, IsCaptain = true }
            };

            var response = await EventService.RegisterTeamAsync(eventData.Id, registerRequest);

            if (response?.Success == true)
            {
                showRegisterModal = false;
                await LoadEvent(); // Reload to show new team
            }
            else
            {
                registerErrorMessage = response?.Message ?? "Failed to register team. Please try again.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error registering team");
            registerErrorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            isRegistering = false;
        }
    }

    private void ShowAddPlayerModal(OneTimeEventTeamResponse team)
    {
        selectedTeamId = team.Id;
        addPlayerRequest = new AddPlayerRequest();
        addPlayerErrorMessage = null;
        showAddPlayerModal = true;
    }

    private void HideAddPlayerModal()
    {
        showAddPlayerModal = false;
        selectedTeamId = null;
    }

    private async Task HandleAddPlayer()
    {
        if (string.IsNullOrEmpty(selectedTeamId)) return;

        isAddingPlayer = true;
        addPlayerErrorMessage = null;

        try
        {
            var response = await EventService.AddPlayerAsync(selectedTeamId, addPlayerRequest);

            if (response?.Success == true)
            {
                showAddPlayerModal = false;
                await LoadEvent(); // Reload to update teams
            }
            else
            {
                addPlayerErrorMessage = response?.Message ?? "Failed to add player. Please try again.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding player");
            addPlayerErrorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            isAddingPlayer = false;
        }
    }

    private async Task HandleRemovePlayer(string playerId, string playerName)
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to remove {playerName}?"))
            return;

        try
        {
            var response = await EventService.RemovePlayerAsync(playerId);
            if (response?.Success == true)
            {
                await LoadEvent(); // Reload to update teams
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing player");
        }
    }

    private MaterialChipColor GetStatusColor(EventStatus status)
    {
        return status switch
        {
            EventStatus.Draft => MaterialChipColor.Default,
            EventStatus.Published => MaterialChipColor.Primary,
            EventStatus.InProgress => MaterialChipColor.Info,
            EventStatus.Completed => MaterialChipColor.Success,
            EventStatus.Cancelled => MaterialChipColor.Error,
            _ => MaterialChipColor.Default
        };
    }
}
