using Microsoft.AspNetCore.Components;
using GolfManager.Shared.DTOs.Player;

namespace GolfManager.Web.Pages.Season;

public partial class SeasonDetail : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private ISeasonSettingsService SeasonSettingsService { get; set; } = null!;
    [Inject] private IPlayerService PlayerService { get; set; } = null!;
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<SeasonDetail> Logger { get; set; } = null!;
    [Inject] private HttpClient Http { get; set; } = null!;

    [Parameter]
    public string LeagueKey { get; set; } = string.Empty;

    [Parameter]
    public string SeasonKey { get; set; } = string.Empty;

    private LeagueResponse? league;
    private SeasonResponse? season;
    private List<PlayerResponse> golfers = new();
    private List<EventResponse> events = new();
    private bool isLoading = true;
    private bool isLoadingGolfers = false;
    private bool isLoadingEvents = false;

    // Season Settings State
    private SeasonSettingsResponse? settings;
    private bool isLoadingSettings = false;
    private bool isCreatingSettings = false;
    private bool showEditSettingsModal = false;
    private bool isSavingSettings = false;
    private string editSettingsError = string.Empty;

    // Edit Settings Form State
    private HandicapType editHandicapType;
    private int? editMaxHandicap;
    private MaxScoreForHandicap editMaxScoreForHandicap;
    private IndividualScoringType editIndividualScoringType;
    private TeamScoringType editTeamScoringType;
    private MissingPlayerType editMissingPlayerType;
    private MissingTeamType editMissingTeamType;
    private string? editDefaultStartTime;

    // Add Player Modal State
    private bool showAddGolferModal = false;
    private string newPlayerEmail = string.Empty;
    private string newPlayerFirstName = string.Empty;
    private string newPlayerLastName = string.Empty;
    private string newPlayerDisplayName = string.Empty;
    private string newPlayerNickname = string.Empty;
    private string? newPlayerHandicap;
    private string? foundUserId;

    // Tab navigation
    private string activeTab = "overview";
    private bool isSearchingUser = false;
    private bool isAddingPlayer = false;
    private string userSearchMessage = string.Empty;
    private bool userSearchSuccess = false;
    private string addPlayerError = string.Empty;

    private bool CanManageSeason => league?.IsCurrentUserAdmin == true || AuthService.IsGlobalAdmin;

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
                if (leagueResponse != null &&
                    !leagueResponse.Success &&
                    string.Equals(leagueResponse.Message, "Forbidden", StringComparison.OrdinalIgnoreCase))
                {
                    Navigation.NavigateTo($"/access-denied?scope=season&leagueKey={Uri.EscapeDataString(LeagueKey)}&seasonKey={Uri.EscapeDataString(SeasonKey)}");
                    return;
                }

                Logger.LogWarning("League not found: {LeagueKey}", LeagueKey);
                return;
            }
            league = leagueResponse.Data;

            // Load season from API
            var seasonResponse = await SeasonService.GetSeasonByKeyAsync(league.Id, SeasonKey);
            if (seasonResponse == null || !seasonResponse.Success || seasonResponse.Data == null)
            {
                if (seasonResponse != null &&
                    !seasonResponse.Success &&
                    string.Equals(seasonResponse.Message, "Forbidden", StringComparison.OrdinalIgnoreCase))
                {
                    Navigation.NavigateTo($"/access-denied?scope=season&leagueKey={Uri.EscapeDataString(LeagueKey)}&seasonKey={Uri.EscapeDataString(SeasonKey)}");
                    return;
                }

                Logger.LogWarning("Season not found: {SeasonKey}", SeasonKey);
                return;
            }
            season = seasonResponse.Data;

            // Load golfers
            await LoadGolfers();

            // Load events
            await LoadEvents();

            // Load season settings
            await LoadSettings();

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading season data");
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadGolfers()
    {
        if (league == null || season == null) return;

        isLoadingGolfers = true;
        try
        {
            // Load golfers for THIS SEASON only (not all league members)
            var response = await PlayerService.GetSeasonPlayersAsync(league.Id, season.Id);
            if (response != null && response.Success && response.Data != null)
            {
                golfers = response.Data.ToList();
                Logger.LogInformation("Loaded {Count} golfers for season {SeasonId}", golfers.Count, season.Id);
            }
            else
            {
                golfers = new List<PlayerResponse>();
                Logger.LogWarning("Failed to load season golfers: {Error}", response?.Message ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading season golfers from API");
            golfers = new List<PlayerResponse>();
        }
        finally
        {
            isLoadingGolfers = false;
        }
    }

    private async Task LoadEvents()
    {
        if (league == null || season == null) return;

        isLoadingEvents = true;
        try
        {
            var response = await EventService.GetSeasonEventsAsync(league.Id, season.Id);
            if (response != null && response.Success && response.Data != null)
            {
                events = response.Data.ToList();
                Logger.LogInformation("Loaded {Count} events for season {SeasonId}", events.Count, season.Id);
            }
            else
            {
                events = new List<EventResponse>();
                Logger.LogWarning("Failed to load events: {Error}", response?.Message ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading events from API");
            events = new List<EventResponse>();
        }
        finally
        {
            isLoadingEvents = false;
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

    private async Task SearchUser()
    {
        if (string.IsNullOrWhiteSpace(newPlayerEmail))
            return;

        isSearchingUser = true;
        userSearchMessage = string.Empty;
        userSearchSuccess = false;
        foundUserId = null;

        try
        {
            var response = await UserService.SearchByEmailAsync(newPlayerEmail);
            if (response?.Success == true && response.Data != null)
            {
                if (response.Data.Exists)
                {
                    foundUserId = response.Data.Id;
                    userSearchMessage = $"User found: {response.Data.Email}";
                    userSearchSuccess = true;

                    // Auto-fill display name if available
                    if (!string.IsNullOrEmpty(response.Data.FirstName) && !string.IsNullOrEmpty(response.Data.LastName))
                    {
                        if (string.IsNullOrWhiteSpace(newPlayerDisplayName))
                        {
                            newPlayerDisplayName = $"{response.Data.FirstName} {response.Data.LastName}";
                        }
                    }
                }
                else
                {
                    userSearchMessage = "User not found. A new account will be created.";
                    userSearchSuccess = false;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching for user");
            userSearchMessage = "Error searching for user";
        }
        finally
        {
            isSearchingUser = false;
        }
    }

    private async Task HandleAddPlayer()
    {
        if (league == null || string.IsNullOrWhiteSpace(newPlayerDisplayName))
            return;

        isAddingPlayer = true;
        addPlayerError = string.Empty;

        try
        {
            var request = new CreatePlayerRequest
            {
                UserId = foundUserId,
                Email = string.IsNullOrWhiteSpace(newPlayerEmail) ? null : newPlayerEmail,
                FirstName = string.IsNullOrWhiteSpace(newPlayerFirstName) ? null : newPlayerFirstName,
                LastName = string.IsNullOrWhiteSpace(newPlayerLastName) ? null : newPlayerLastName,
                DisplayName = newPlayerDisplayName,
                Nickname = string.IsNullOrWhiteSpace(newPlayerNickname) ? null : newPlayerNickname,
                LeagueHandicap = string.IsNullOrWhiteSpace(newPlayerHandicap) ? null : double.Parse(newPlayerHandicap)
            };

            if (season == null)
            {
                addPlayerError = "Season not loaded";
                return;
            }

            var response = await PlayerService.AddPlayerToSeasonAsync(league.Id, season.Id, request);

            if (response?.Success == true)
            {
                showAddGolferModal = false;
                ResetAddPlayerForm();
                await LoadGolfers(); // Reload the golfer list
            }
            else
            {
                addPlayerError = response?.Message ?? "Failed to add player";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding player");
            addPlayerError = ex.Message;
        }
        finally
        {
            isAddingPlayer = false;
        }
    }

    private void ResetAddPlayerForm()
    {
        newPlayerEmail = string.Empty;
        newPlayerFirstName = string.Empty;
        newPlayerLastName = string.Empty;
        newPlayerDisplayName = string.Empty;
        newPlayerNickname = string.Empty;
        newPlayerHandicap = null;
        foundUserId = null;
        userSearchMessage = string.Empty;
        userSearchSuccess = false;
        addPlayerError = string.Empty;
    }

    // Season Settings Methods
    private async Task LoadSettings()
    {
        if (league == null || season == null) return;

        isLoadingSettings = true;
        try
        {
            settings = await SeasonSettingsService.GetSeasonSettingsAsync(league.Id, season.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading season settings");
        }
        finally
        {
            isLoadingSettings = false;
        }
    }

    private async Task CreateDefaultSettings()
    {
        if (league == null || season == null) return;

        isCreatingSettings = true;
        try
        {
            var response = await SeasonSettingsService.CreateDefaultSettingsAsync(league.Id, season.Id);
            if (response?.Success == true && response.Data != null)
            {
                settings = response.Data;
            }
            else
            {
                Logger.LogError("Failed to create default settings: {Message}", response?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating default settings");
        }
        finally
        {
            isCreatingSettings = false;
        }
    }

    private void OpenEditSettingsModal()
    {
        if (settings == null) return;

        // Populate form with current settings
        editHandicapType = settings.HandicapType;
        editMaxHandicap = settings.MaxHandicap;
        editMaxScoreForHandicap = settings.MaxScoreForHandicap;
        editIndividualScoringType = settings.IndividualScoringType;
        editTeamScoringType = settings.TeamScoringType;
        editMissingPlayerType = settings.MissingPlayerType;
        editMissingTeamType = settings.MissingTeamType;
        editDefaultStartTime = settings.DefaultStartTime?.ToString("HH:mm");
        editSettingsError = string.Empty;

        showEditSettingsModal = true;
    }

    private async Task SaveSettings()
    {
        if (league == null || season == null || settings == null) return;

        isSavingSettings = true;
        editSettingsError = string.Empty;

        try
        {
            var request = new UpdateSeasonSettingsRequest
            {
                HandicapType = editHandicapType,
                MaxHandicap = editMaxHandicap,
                MaxScoreForHandicap = editMaxScoreForHandicap,
                IndividualScoringType = editIndividualScoringType,
                TeamScoringType = editTeamScoringType,
                MissingPlayerType = editMissingPlayerType,
                MissingTeamType = editMissingTeamType,
                DefaultStartTime = !string.IsNullOrEmpty(editDefaultStartTime)
                    ? TimeOnly.Parse(editDefaultStartTime)
                    : null
            };

            var response = await SeasonSettingsService.UpdateSeasonSettingsAsync(league.Id, season.Id, request);
            if (response?.Success == true && response.Data != null)
            {
                settings = response.Data;
                showEditSettingsModal = false;
            }
            else
            {
                editSettingsError = response?.Message ?? "Failed to save settings";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving settings");
            editSettingsError = ex.Message;
        }
        finally
        {
            isSavingSettings = false;
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

        // Only show Settings tab to admins (Global Admin OR League Admin OR Season Admin)
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
}
