using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Dashboard;

public partial class OrganizerDashboard : ComponentBase
{
    [Inject] private IOneTimeEventService EventService { get; set; } = null!;
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private ILogger<OrganizerDashboard> Logger { get; set; } = null!;

    [Parameter]
    public string EventKey { get; set; } = string.Empty;

    private OneTimeEventResponse? eventData;
    private List<OneTimeEventTeamResponse> teams = new();
    private bool isLoading = true;
    private bool isOrganizer = false;

    private bool showAddPlayerModal = false;
    private bool isAddingPlayer = false;
    private AddPlayerRequest addPlayerRequest = new();
    private string? addPlayerErrorMessage;
    private string? selectedTeamId;

    private bool showEditEventModal = false;
    private bool isUpdatingEvent = false;
    private UpdateOneTimeEventRequest updateEventRequest = new();
    private string? editEventErrorMessage;

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

                if (isOrganizer)
                {
                    // Load teams
                    var teamsResponse = await EventService.GetEventTeamsAsync(eventData.Id);
                    if (teamsResponse?.Success == true && teamsResponse.Data != null)
                    {
                        teams = teamsResponse.Data;
                    }
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

    private void ShowEditEventModal()
    {
        if (eventData == null) return;

        // Populate the update request with current values
        updateEventRequest = new UpdateOneTimeEventRequest
        {
            Name = eventData.Name,
            Description = eventData.Description,
            EventDate = eventData.EventDate,
            OrganizationName = eventData.OrganizationName,
            OrganizerEmail = eventData.OrganizerEmail,
            OrganizerPhone = eventData.OrganizerPhone,
            Format = eventData.Format,
            TeamSize = eventData.TeamSize,
            UseHandicaps = eventData.UseHandicaps,
            MaxTeams = eventData.MaxTeams,
            TotalRounds = eventData.TotalRounds,
            AccessType = eventData.AccessType,
            RegistrationDeadline = eventData.RegistrationDeadline
        };
        editEventErrorMessage = null;
        showEditEventModal = true;
    }

    private void HideEditEventModal()
    {
        showEditEventModal = false;
    }

    private async Task HandleUpdateEvent()
    {
        if (eventData == null) return;

        isUpdatingEvent = true;
        editEventErrorMessage = null;

        try
        {
            var response = await EventService.UpdateEventAsync(eventData.Id, updateEventRequest);

            if (response?.Success == true)
            {
                showEditEventModal = false;
                await LoadEvent(); // Reload to show updated data
            }
            else
            {
                editEventErrorMessage = response?.Message ?? "Failed to update event. Please try again.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating event");
            editEventErrorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            isUpdatingEvent = false;
        }
    }

    private async Task HandlePublishEvent()
    {
        if (eventData == null) return;

        if (!await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to publish this event? Once published, teams can register."))
            return;

        try
        {
            var response = await EventService.PublishEventAsync(eventData.Id);
            if (response?.Success == true)
            {
                await LoadEvent(); // Reload to update status
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error publishing event");
        }
    }

    private async Task HandleCancelEvent()
    {
        if (eventData == null) return;

        if (!await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to cancel this event? This action cannot be undone and all registered teams will be notified."))
            return;

        try
        {
            var response = await EventService.CancelEventAsync(eventData.Id);
            if (response?.Success == true)
            {
                await LoadEvent(); // Reload to update status
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error canceling event");
        }
    }

    private async Task HandleCheckInTeam(string teamId)
    {
        try
        {
            var response = await EventService.CheckInTeamAsync(teamId);
            if (response?.Success == true)
            {
                await LoadEvent(); // Reload to update check-in status
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking in team");
        }
    }

    private async Task HandleRemoveTeam(string teamId, string teamName)
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to remove {teamName}?"))
            return;

        try
        {
            var response = await EventService.RemoveTeamAsync(teamId);
            if (response?.Success == true)
            {
                await LoadEvent(); // Reload to update teams list
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing team");
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
