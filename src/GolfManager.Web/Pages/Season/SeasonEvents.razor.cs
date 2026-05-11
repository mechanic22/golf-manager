using Microsoft.AspNetCore.Components;
using GolfManager.Shared.DTOs.Player;
using GolfManager.Shared.DTOs.Season;
using System.Linq;

namespace GolfManager.Web.Pages.Season;

public partial class SeasonEvents : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private IPlayerService PlayerService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<SeasonEvents> Logger { get; set; } = null!;

    [Parameter]
    public string LeagueKey { get; set; } = string.Empty;

    [Parameter]
    public string SeasonKey { get; set; } = string.Empty;

    private LeagueResponse? league;
    private SeasonResponse? season;
    private List<EventResponse> events = new();
    private List<PlayerResponse> seasonPlayers = new();
    private List<SeasonTeamResponse> seasonTeams = new();
    private readonly Dictionary<string, UpdateEventRequest> eventEditRequests = new();
    private readonly HashSet<string> eventSavingInProgress = new();
    private readonly Dictionary<string, string> gameOfDayTitles = new();
    private readonly Dictionary<string, string> gameOfDayWinnerSeasonGolferIds = new();
    private readonly Dictionary<string, string> gameOfDayWinnerNames = new();
    private readonly HashSet<string> gameOfDaySavingInProgress = new();
    private readonly Dictionary<string, List<EventMatchupResponse>> eventMatchups = new();
    private readonly Dictionary<string, EventScoreboardResponse> eventScoreboards = new();
    private readonly HashSet<string> matchupSavingInProgress = new();
    private readonly HashSet<string> autoSetupInProgress = new();
    private readonly HashSet<string> scheduleNextWeekInProgress = new();
    private readonly HashSet<string> handicapRecalcInProgress = new();
    private readonly HashSet<string> overallRecalcInProgress = new();
    private bool isLoading = true;

    // Tab navigation
    private string activeTab = "events";
    private bool showEventManagerModal;
    private bool isCreatingEvent;
    private bool isEventManagerSaving;
    private bool isEventManagerDeleting;
    private string? eventManagerErrorMessage;
    private EventResponse? selectedEvent;
    private CreateEventRequest createEventRequest = new();

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        // Initialize authorization service
        await AuthorizationService.InitializeAsync();

        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        try
        {
            // Load league from API
            var leagueResponse = await LeagueService.GetLeagueByKeyAsync(LeagueKey);
            if (leagueResponse == null || !leagueResponse.Success || leagueResponse.Data == null)
            {
                Logger.LogWarning("League not found: {LeagueKey}", LeagueKey);
                return;
            }
            league = leagueResponse.Data;

            // Load season from API
            var seasonResponse = await SeasonService.GetSeasonByKeyAsync(league.Id, SeasonKey);
            if (seasonResponse == null || !seasonResponse.Success || seasonResponse.Data == null)
            {
                Logger.LogWarning("Season not found: {SeasonKey}", SeasonKey);
                return;
            }
            season = seasonResponse.Data;

            await Task.WhenAll(LoadEvents(), LoadPlayers(), LoadTeams());

            if (CanManageSeason)
            {
                foreach (var evt in events)
                {
                    await LoadMatchups(evt.Id);
                }
            }

            foreach (var evt in events)
            {
                await LoadScoreboard(evt.Id);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading data");
        }
        finally
        {
            isLoading = false;
        }
    }

    private bool CanManageSeason => league?.IsCurrentUserAdmin == true || AuthService.IsGlobalAdmin;

    private CreateEventRequest BuildDefaultCreateEventRequest()
    {
        var nextEventDate = events.LastOrDefault()?.EventDate.Date.AddDays(7)
            ?? season?.StartDate.ToDateTime(TimeOnly.MinValue)
            ?? DateTime.Today;

        return new CreateEventRequest
        {
            EventDate = nextEventDate,
            HolesPlayed = Core.Enums.HolesPlayed.Nine,
            EventType = Core.Enums.SeasonEventType.Regular,
            ScoringFormat = Core.Enums.ScoringFormat.MatchPlay
        };
    }

    private void OpenCreateEventManager()
    {
        createEventRequest = BuildDefaultCreateEventRequest();
        selectedEvent = null;
        isCreatingEvent = true;
        showEventManagerModal = true;
        eventManagerErrorMessage = null;
    }

    private async Task OpenEditEventManager(EventResponse evt)
    {
        selectedEvent = evt;
        isCreatingEvent = false;
        showEventManagerModal = true;
        eventManagerErrorMessage = null;

        if (!eventMatchups.ContainsKey(evt.Id))
        {
            await LoadMatchups(evt.Id);
        }

        if (!eventScoreboards.ContainsKey(evt.Id))
        {
            await LoadScoreboard(evt.Id);
        }
    }

    private void CloseEventManagerModal()
    {
        showEventManagerModal = false;
        eventManagerErrorMessage = null;
        selectedEvent = null;
        isCreatingEvent = false;
    }

    private async Task LoadEvents()
    {
        if (league == null || season == null) return;

        try
        {
            var response = await EventService.GetSeasonEventsAsync(league.Id, season.Id);
            if (response != null && response.Success && response.Data != null)
            {
                events = response.Data
                    .OrderBy(e => e.EventDate)
                    .ToList();

                foreach (var evt in events)
                {
                    eventEditRequests[evt.Id] = CreateEditRequest(evt);
                    gameOfDayTitles[evt.Id] = evt.GameOfDayTitle ?? string.Empty;
                    gameOfDayWinnerSeasonGolferIds[evt.Id] = evt.GameOfDayWinnerSeasonGolferId ?? string.Empty;
                    gameOfDayWinnerNames[evt.Id] = evt.GameOfDayWinnerDisplayName ?? string.Empty;
                }
            }
            else
            {
                events = new List<EventResponse>();
                Logger.LogWarning("Failed to load events for season {SeasonId}: {Message}", season.Id, response?.Message ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading season events for season {SeasonId}", season.Id);
            events = new List<EventResponse>();
        }
    }

    private async Task LoadTeams()
    {
        if (league == null || season == null) return;

        try
        {
            var response = await SeasonService.GetSeasonTeamsAsync(league.Id, season.Id);
            if (response != null && response.Success && response.Data != null)
            {
                seasonTeams = response.Data
                    .OrderByDescending(t => t.SeasonPoints ?? 0)
                    .ThenBy(t => t.Name)
                    .ToList();
            }
            else
            {
                seasonTeams = new List<SeasonTeamResponse>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading season teams for matchup setup");
            seasonTeams = new List<SeasonTeamResponse>();
        }
    }

    private static UpdateEventRequest CreateEditRequest(EventResponse evt)
    {
        return new UpdateEventRequest
        {
            Name = evt.Name ?? string.Empty,
            Description = evt.Description ?? string.Empty,
            EventDate = evt.EventDate,
            HolesPlayed = evt.HolesPlayed,
            EventType = evt.EventType,
            ScoringFormat = evt.ScoringFormat
        };
    }

    private bool IsSavingEvent(EventResponse evt) => eventSavingInProgress.Contains(evt.Id);

    private bool IsSelectedEventSaving => isEventManagerSaving || (selectedEvent != null && eventSavingInProgress.Contains(selectedEvent.Id));

    private UpdateEventRequest GetEventEditRequest(string eventId)
    {
        if (!eventEditRequests.TryGetValue(eventId, out var request))
        {
            request = new UpdateEventRequest();
            eventEditRequests[eventId] = request;
        }

        return request;
    }

    private async Task SaveEventDetails(EventResponse evt)
    {
        if (league == null || season == null || eventSavingInProgress.Contains(evt.Id))
        {
            return;
        }

        var request = GetEventEditRequest(evt.Id);
        eventSavingInProgress.Add(evt.Id);

        try
        {
            var updateRequest = new UpdateEventRequest
            {
                Name = string.IsNullOrWhiteSpace(request.Name) ? string.Empty : request.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? string.Empty : request.Description.Trim(),
                EventDate = request.EventDate,
                HolesPlayed = request.HolesPlayed,
                EventType = request.EventType,
                ScoringFormat = request.ScoringFormat
            };

            var result = await EventService.UpdateEventAsync(league.Id, season.Id, evt.Id, updateRequest);
            if (result?.Success == true && result.Data != null)
            {
                evt.Name = result.Data.Name;
                evt.Description = result.Data.Description;
                evt.EventDate = result.Data.EventDate;
                evt.HolesPlayed = result.Data.HolesPlayed;
                evt.EventType = result.Data.EventType;
                evt.ScoringFormat = result.Data.ScoringFormat;
                evt.UpdatedAt = result.Data.UpdatedAt;

                eventEditRequests[evt.Id] = CreateEditRequest(evt);
                events = events.OrderBy(e => e.EventDate).ToList();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving event details for event {EventId}", evt.Id);
        }
        finally
        {
            eventSavingInProgress.Remove(evt.Id);
        }
    }

    private async Task SaveEventManager()
    {
        if (league == null || season == null || isEventManagerSaving)
        {
            return;
        }

        isEventManagerSaving = true;
        eventManagerErrorMessage = null;

        try
        {
            if (isCreatingEvent)
            {
                var createRequest = new CreateEventRequest
                {
                    Name = string.IsNullOrWhiteSpace(createEventRequest.Name) ? null : createEventRequest.Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(createEventRequest.Description) ? null : createEventRequest.Description.Trim(),
                    EventDate = createEventRequest.EventDate,
                    HolesPlayed = createEventRequest.HolesPlayed,
                    EventType = createEventRequest.EventType,
                    ScoringFormat = createEventRequest.ScoringFormat,
                    GameOfDayTitle = string.IsNullOrWhiteSpace(createEventRequest.GameOfDayTitle) ? null : createEventRequest.GameOfDayTitle.Trim(),
                    GameOfDayWinnerSeasonGolferId = string.IsNullOrWhiteSpace(createEventRequest.GameOfDayWinnerSeasonGolferId) ? null : createEventRequest.GameOfDayWinnerSeasonGolferId,
                    GameOfDayWinnerDisplayName = string.IsNullOrWhiteSpace(createEventRequest.GameOfDayWinnerDisplayName) ? null : createEventRequest.GameOfDayWinnerDisplayName.Trim()
                };

                if (!string.IsNullOrWhiteSpace(createRequest.GameOfDayWinnerSeasonGolferId))
                {
                    var selectedPlayer = seasonPlayers.FirstOrDefault(p => p.SeasonGolferId == createRequest.GameOfDayWinnerSeasonGolferId);
                    if (!string.IsNullOrWhiteSpace(selectedPlayer?.DisplayName) && string.IsNullOrWhiteSpace(createRequest.GameOfDayWinnerDisplayName))
                    {
                        createRequest.GameOfDayWinnerDisplayName = selectedPlayer.DisplayName;
                    }
                }

                var created = await EventService.CreateEventAsync(league.Id, season.Id, createRequest);
                if (created?.Success == true && created.Data != null)
                {
                    await LoadEvents();
                    await LoadScoreboard(created.Data.Id);
                    if (CanManageSeason)
                    {
                        await LoadMatchups(created.Data.Id);
                    }

                    CloseEventManagerModal();
                }
                else
                {
                    eventManagerErrorMessage = created?.Message ?? "Failed to create event";
                }

                return;
            }

            if (selectedEvent == null)
            {
                return;
            }

            var request = GetEventEditRequest(selectedEvent.Id);
            var updateRequest = new UpdateEventRequest
            {
                Name = string.IsNullOrWhiteSpace(request.Name) ? string.Empty : request.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? string.Empty : request.Description.Trim(),
                EventDate = request.EventDate,
                HolesPlayed = request.HolesPlayed,
                EventType = request.EventType,
                ScoringFormat = request.ScoringFormat,
                GameOfDayTitle = string.IsNullOrWhiteSpace(gameOfDayTitles.GetValueOrDefault(selectedEvent.Id)) ? string.Empty : gameOfDayTitles[selectedEvent.Id].Trim(),
                GameOfDayWinnerSeasonGolferId = string.IsNullOrWhiteSpace(gameOfDayWinnerSeasonGolferIds.GetValueOrDefault(selectedEvent.Id)) ? string.Empty : gameOfDayWinnerSeasonGolferIds[selectedEvent.Id],
                GameOfDayWinnerDisplayName = string.IsNullOrWhiteSpace(gameOfDayWinnerNames.GetValueOrDefault(selectedEvent.Id)) ? string.Empty : gameOfDayWinnerNames[selectedEvent.Id].Trim()
            };

            if (!string.IsNullOrWhiteSpace(updateRequest.GameOfDayWinnerSeasonGolferId))
            {
                var selectedPlayer = seasonPlayers.FirstOrDefault(p => p.SeasonGolferId == updateRequest.GameOfDayWinnerSeasonGolferId);
                if (!string.IsNullOrWhiteSpace(selectedPlayer?.DisplayName) && string.IsNullOrWhiteSpace(updateRequest.GameOfDayWinnerDisplayName))
                {
                    updateRequest.GameOfDayWinnerDisplayName = selectedPlayer.DisplayName;
                }
            }

            var updated = await EventService.UpdateEventAsync(league.Id, season.Id, selectedEvent.Id, updateRequest);
            if (updated?.Success == true && updated.Data != null)
            {
                selectedEvent.Name = updated.Data.Name;
                selectedEvent.Description = updated.Data.Description;
                selectedEvent.EventDate = updated.Data.EventDate;
                selectedEvent.HolesPlayed = updated.Data.HolesPlayed;
                selectedEvent.EventType = updated.Data.EventType;
                selectedEvent.ScoringFormat = updated.Data.ScoringFormat;
                selectedEvent.GameOfDayTitle = updated.Data.GameOfDayTitle;
                selectedEvent.GameOfDayWinnerSeasonGolferId = updated.Data.GameOfDayWinnerSeasonGolferId;
                selectedEvent.GameOfDayWinnerDisplayName = updated.Data.GameOfDayWinnerDisplayName;
                selectedEvent.UpdatedAt = updated.Data.UpdatedAt;

                eventEditRequests[selectedEvent.Id] = CreateEditRequest(selectedEvent);
                gameOfDayTitles[selectedEvent.Id] = selectedEvent.GameOfDayTitle ?? string.Empty;
                gameOfDayWinnerSeasonGolferIds[selectedEvent.Id] = selectedEvent.GameOfDayWinnerSeasonGolferId ?? string.Empty;
                gameOfDayWinnerNames[selectedEvent.Id] = selectedEvent.GameOfDayWinnerDisplayName ?? string.Empty;
                events = events.OrderBy(e => e.EventDate).ToList();
            }
            else
            {
                eventManagerErrorMessage = updated?.Message ?? "Failed to save event";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving event manager state");
            eventManagerErrorMessage = ex.Message;
        }
        finally
        {
            isEventManagerSaving = false;
        }
    }

    private async Task DeleteSelectedEvent()
    {
        if (league == null || season == null || selectedEvent == null || isEventManagerDeleting)
        {
            return;
        }

        isEventManagerDeleting = true;
        eventManagerErrorMessage = null;

        try
        {
            var deleted = await EventService.DeleteEventAsync(league.Id, season.Id, selectedEvent.Id);
            if (deleted?.Success == true)
            {
                eventMatchups.Remove(selectedEvent.Id);
                eventScoreboards.Remove(selectedEvent.Id);
                eventEditRequests.Remove(selectedEvent.Id);
                gameOfDayTitles.Remove(selectedEvent.Id);
                gameOfDayWinnerSeasonGolferIds.Remove(selectedEvent.Id);
                gameOfDayWinnerNames.Remove(selectedEvent.Id);
                events = events.Where(e => e.Id != selectedEvent.Id).ToList();
                CloseEventManagerModal();
            }
            else
            {
                eventManagerErrorMessage = deleted?.Message ?? "Failed to delete event";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting event {EventId}", selectedEvent.Id);
            eventManagerErrorMessage = ex.Message;
        }
        finally
        {
            isEventManagerDeleting = false;
        }
    }

    private async Task LoadPlayers()
    {
        if (league == null || season == null) return;

        try
        {
            var response = await PlayerService.GetSeasonPlayersAsync(league.Id, season.Id);
            if (response != null && response.Success && response.Data != null)
            {
                seasonPlayers = response.Data
                    .Where(p => !string.IsNullOrWhiteSpace(p.SeasonGolferId))
                    .OrderBy(p => p.DisplayName)
                    .ToList();
            }
            else
            {
                seasonPlayers = new List<PlayerResponse>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading season players for game-of-day picker");
            seasonPlayers = new List<PlayerResponse>();
        }
    }

    private bool IsSavingGameOfDay(EventResponse evt) => gameOfDaySavingInProgress.Contains(evt.Id);

    private async Task SaveGameOfDay(EventResponse evt)
    {
        if (league == null || season == null || gameOfDaySavingInProgress.Contains(evt.Id))
        {
            return;
        }

        gameOfDaySavingInProgress.Add(evt.Id);
        try
        {
            var winnerSeasonGolferId = gameOfDayWinnerSeasonGolferIds.GetValueOrDefault(evt.Id, string.Empty);
            var winnerName = gameOfDayWinnerNames.GetValueOrDefault(evt.Id, string.Empty);

            var request = new UpdateEventRequest
            {
                GameOfDayTitle = string.IsNullOrWhiteSpace(gameOfDayTitles.GetValueOrDefault(evt.Id, string.Empty))
                    ? string.Empty
                    : gameOfDayTitles[evt.Id].Trim(),
                GameOfDayWinnerSeasonGolferId = string.IsNullOrWhiteSpace(winnerSeasonGolferId)
                    ? string.Empty
                    : winnerSeasonGolferId,
                GameOfDayWinnerDisplayName = string.IsNullOrWhiteSpace(winnerName)
                    ? string.Empty
                    : winnerName.Trim()
            };

            if (!string.IsNullOrWhiteSpace(request.GameOfDayWinnerSeasonGolferId))
            {
                var selectedPlayer = seasonPlayers.FirstOrDefault(p => p.SeasonGolferId == request.GameOfDayWinnerSeasonGolferId);
                if (!string.IsNullOrWhiteSpace(selectedPlayer?.DisplayName))
                {
                    request.GameOfDayWinnerDisplayName = selectedPlayer.DisplayName;
                    gameOfDayWinnerNames[evt.Id] = selectedPlayer.DisplayName;
                }
            }

            var result = await EventService.UpdateEventAsync(league.Id, season.Id, evt.Id, request);
            if (result?.Success == true && result.Data != null)
            {
                var updated = result.Data;
                evt.GameOfDayTitle = string.IsNullOrWhiteSpace(updated.GameOfDayTitle) ? null : updated.GameOfDayTitle;
                evt.GameOfDayWinnerSeasonGolferId = string.IsNullOrWhiteSpace(updated.GameOfDayWinnerSeasonGolferId) ? null : updated.GameOfDayWinnerSeasonGolferId;
                evt.GameOfDayWinnerDisplayName = string.IsNullOrWhiteSpace(updated.GameOfDayWinnerDisplayName) ? null : updated.GameOfDayWinnerDisplayName;

                gameOfDayTitles[evt.Id] = evt.GameOfDayTitle ?? string.Empty;
                gameOfDayWinnerSeasonGolferIds[evt.Id] = evt.GameOfDayWinnerSeasonGolferId ?? string.Empty;
                gameOfDayWinnerNames[evt.Id] = evt.GameOfDayWinnerDisplayName ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving game-of-day settings for event {EventId}", evt.Id);
        }
        finally
        {
            gameOfDaySavingInProgress.Remove(evt.Id);
        }
    }

    private async Task LoadMatchups(string eventId)
    {
        if (league == null || season == null)
        {
            return;
        }

        try
        {
            var response = await EventService.GetEventMatchupsAsync(league.Id, season.Id, eventId);
            if (response?.Success == true && response.Data != null)
            {
                eventMatchups[eventId] = response.Data;
            }
            else
            {
                eventMatchups[eventId] = new List<EventMatchupResponse>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading matchups for event {EventId}", eventId);
            eventMatchups[eventId] = new List<EventMatchupResponse>();
        }
    }

    private async Task LoadScoreboard(string eventId)
    {
        if (league == null || season == null)
        {
            return;
        }

        try
        {
            var response = await EventService.GetEventScoreboardAsync(league.Id, season.Id, eventId);
            if (response?.Success == true && response.Data != null)
            {
                eventScoreboards[eventId] = response.Data;
            }
            else
            {
                eventScoreboards.Remove(eventId);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading scoreboard for event {EventId}", eventId);
            eventScoreboards.Remove(eventId);
        }
    }

    private List<EventMatchupResponse> GetMatchups(string eventId)
    {
        return eventMatchups.TryGetValue(eventId, out var matchups)
            ? matchups
            : new List<EventMatchupResponse>();
    }

    private bool IsAutoSetupInProgress(string eventId) => autoSetupInProgress.Contains(eventId);

    private bool IsSchedulingNextWeek(string eventId) => scheduleNextWeekInProgress.Contains(eventId);

    private bool IsRecalculatingHandicaps(string eventId) => handicapRecalcInProgress.Contains(eventId);

    private bool IsRecalculatingOverall(string eventId) => overallRecalcInProgress.Contains(eventId);

    private async Task AutoSetupMatchups(EventResponse evt)
    {
        if (league == null || season == null || autoSetupInProgress.Contains(evt.Id))
        {
            return;
        }

        autoSetupInProgress.Add(evt.Id);
        try
        {
            var result = await EventService.AutoSetupEventMatchupsAsync(league.Id, season.Id, evt.Id);
            if (result?.Success == true && result.Data != null)
            {
                eventMatchups[evt.Id] = result.Data;
                await LoadScoreboard(evt.Id);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error auto-generating matchups for event {EventId}", evt.Id);
        }
        finally
        {
            autoSetupInProgress.Remove(evt.Id);
        }
    }

    private async Task RecalculateEventHandicaps(EventResponse evt)
    {
        if (league == null || season == null || handicapRecalcInProgress.Contains(evt.Id))
        {
            return;
        }

        handicapRecalcInProgress.Add(evt.Id);
        try
        {
            var result = await EventService.RecalculateEventHandicapsAsync(league.Id, season.Id, evt.Id);
            if (result?.Success != true)
            {
                Logger.LogWarning("Failed to recalculate handicaps for event {EventId}: {Message}", evt.Id, result?.Message ?? "Unknown error");
            }

            await LoadScoreboard(evt.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error recalculating handicaps for event {EventId}", evt.Id);
        }
        finally
        {
            handicapRecalcInProgress.Remove(evt.Id);
        }
    }

    private async Task RecalculateOverallStandings(EventResponse evt)
    {
        if (league == null || season == null || overallRecalcInProgress.Contains(evt.Id))
        {
            return;
        }

        overallRecalcInProgress.Add(evt.Id);
        eventManagerErrorMessage = null;
        try
        {
            var result = await EventService.RecalculateOverallStandingsAsync(league.Id, season.Id, evt.Id);
            if (result?.Success != true)
            {
                eventManagerErrorMessage = result?.Message ?? "Failed to calculate overall standings.";
                return;
            }

            await LoadTeams();
            await LoadScoreboard(evt.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error recalculating overall standings for season {SeasonId}", season.Id);
            eventManagerErrorMessage = "Failed to calculate overall standings. Check logs for details.";
        }
        finally
        {
            overallRecalcInProgress.Remove(evt.Id);
        }
    }

    private async Task ScheduleNextWeek(EventResponse evt)
    {
        if (league == null || season == null || scheduleNextWeekInProgress.Contains(evt.Id))
        {
            return;
        }

        scheduleNextWeekInProgress.Add(evt.Id);
        eventManagerErrorMessage = null;

        try
        {
            var result = await EventService.ScheduleNextWeekFromEventAsync(league.Id, season.Id, evt.Id);
            if (result?.Success != true || result.Data == null)
            {
                eventManagerErrorMessage = result?.Message ?? "Unable to schedule next week from this event.";
                return;
            }

            await LoadEvents();
            await LoadTeams();
            await LoadMatchups(result.Data.Id);
            await LoadScoreboard(evt.Id);
            await LoadScoreboard(result.Data.Id);

            selectedEvent = events.FirstOrDefault(e => string.Equals(e.Id, result.Data.Id, StringComparison.OrdinalIgnoreCase))
                ?? result.Data;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error scheduling next week from event {EventId}", evt.Id);
            eventManagerErrorMessage = "Failed to schedule next week. Check logs for details.";
        }
        finally
        {
            scheduleNextWeekInProgress.Remove(evt.Id);
        }
    }

    private bool IsSavingMatchup(string matchupId) => matchupSavingInProgress.Contains(matchupId);

    private async Task SaveMatchup(EventResponse evt, EventMatchupResponse matchup)
    {
        if (league == null || season == null || matchupSavingInProgress.Contains(matchup.Id))
        {
            return;
        }

        matchupSavingInProgress.Add(matchup.Id);
        try
        {
            var request = new UpdateEventMatchupRequest
            {
                HomeTeamId = string.IsNullOrWhiteSpace(matchup.HomeTeamId) ? string.Empty : matchup.HomeTeamId,
                AwayTeamId = string.IsNullOrWhiteSpace(matchup.AwayTeamId) ? string.Empty : matchup.AwayTeamId,
                HomeSubSeasonGolferId = string.IsNullOrWhiteSpace(matchup.HomeSubSeasonGolferId) ? string.Empty : matchup.HomeSubSeasonGolferId,
                AwaySubSeasonGolferId = string.IsNullOrWhiteSpace(matchup.AwaySubSeasonGolferId) ? string.Empty : matchup.AwaySubSeasonGolferId,
                StartingFlight = matchup.StartingFlight,
                StartingHole = matchup.StartingHole
            };

            var result = await EventService.UpdateEventMatchupAsync(league.Id, season.Id, evt.Id, matchup.Id, request);
            if (result?.Success == true && result.Data != null)
            {
                var matchups = GetMatchups(evt.Id);
                var index = matchups.FindIndex(m => m.Id == matchup.Id);
                if (index >= 0)
                {
                    matchups[index] = result.Data;
                    eventMatchups[evt.Id] = matchups;
                }

                await LoadScoreboard(evt.Id);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving matchup {MatchupId}", matchup.Id);
        }
        finally
        {
            matchupSavingInProgress.Remove(matchup.Id);
        }
    }

    private List<MaterialTab> GetNavigationTabs()
    {
        var tabs = new List<MaterialTab>
        {
            new MaterialTab
            {
                Value = "overview",
                Label = "Overview"
            },
            new MaterialTab
            {
                Value = "events",
                Label = "Events"
            },
            new MaterialTab
            {
                Value = "players",
                Label = "Players"
            },
            new MaterialTab
            {
                Value = "teams",
                Label = "Teams"
            }
        };

        // Only show Settings tab to users who can manage this season
        if (CanManageSeason)
        {
            tabs.Add(new MaterialTab
            {
                Value = "settings",
                Label = "Settings"
            });
        }

        return tabs;
    }

    private void HandleTabChange(string tabValue)
    {
        var route = tabValue switch
        {
            "overview" => $"/league/{LeagueKey}/season/{SeasonKey}",
            "events" => $"/league/{LeagueKey}/season/{SeasonKey}/events",
            "players" => $"/league/{LeagueKey}/season/{SeasonKey}/players",
            "teams" => $"/league/{LeagueKey}/season/{SeasonKey}/teams",
            "settings" => $"/league/{LeagueKey}/season/{SeasonKey}/settings",
            _ => $"/league/{LeagueKey}/season/{SeasonKey}"
        };

        Navigation.NavigateTo(route);
    }

    private bool CanManageCurrentSeason()
    {
        return CanManageSeason;
    }

    private void OpenScoreEntry(EventResponse evt)
    {
        if (!CanManageCurrentSeason())
        {
            return;
        }

        Navigation.NavigateTo($"/league/{LeagueKey}/season/{SeasonKey}/event/{evt.Id}/scores");
    }

    private void OpenPrintScorecards(EventResponse evt)
    {
        Navigation.NavigateTo($"/league/{LeagueKey}/season/{SeasonKey}/event/{evt.Id}/scorecards");
    }

    private IEnumerable<PlayerResponse> GetTeamOrSubCandidates(string? teamId)
    {
        var candidates = seasonPlayers
            .Where(p => !string.IsNullOrWhiteSpace(p.SeasonGolferId));

        if (!string.IsNullOrWhiteSpace(teamId))
        {
            var teamPlayers = candidates
                .Where(p => string.Equals(p.TeamId, teamId, StringComparison.OrdinalIgnoreCase));

            var combined = teamPlayers
                .Concat(candidates.Where(p => string.IsNullOrWhiteSpace(p.TeamId)))
                .GroupBy(p => p.SeasonGolferId, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(p => p.DisplayName)
                .ToList();

            return combined;
        }

        return candidates
            .OrderBy(p => p.DisplayName)
            .ToList();
    }

    private EventScoreboardResponse? GetScoreboard(string eventId)
    {
        return eventScoreboards.TryGetValue(eventId, out var scoreboard)
            ? scoreboard
            : null;
    }
}
