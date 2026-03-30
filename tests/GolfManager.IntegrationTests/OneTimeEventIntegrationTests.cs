using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GolfManager.Core.Enums;
using GolfManager.Shared.DTOs.Auth;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.OneTimeEvent;

namespace GolfManager.IntegrationTests;

/// <summary>
/// Comprehensive integration tests for One-Time Events
/// Tests the full lifecycle: Create → Publish → Register → Check-in → Score
/// </summary>
public class OneTimeEventIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public OneTimeEventIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateEvent_WithValidData_ReturnsEventResponse()
    {
        // Arrange
        var token = await RegisterAndLogin($"organizer-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateOneTimeEventRequest
        {
            Key = $"scramble-{Guid.NewGuid().ToString()[..8]}",
            Name = "Spring Scramble 2024",
            Description = "Annual spring charity scramble",
            EventDate = DateTime.UtcNow.AddDays(30),
            RegistrationDeadline = DateTime.UtcNow.AddDays(25),
            OrganizationName = "Golf Club",
            OrganizerEmail = "contact@golfclub.com",
            OrganizerPhone = "555-1234",
            CourseId = "test-course-1",
            TeeId = "test-tee-1",
            HolesPlayed = HolesPlayed.Eighteen,
            Format = ScoringFormat.Scramble,
            TeamSize = 4,
            MaxTeams = 20,
            UseHandicaps = true,
            AccessType = EventAccessType.Public,
            TotalRounds = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/events/one-time", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(request.Name, apiResponse.Data.Name);
        Assert.Equal(request.Key, apiResponse.Data.Key);
        Assert.Equal(EventStatus.Draft, apiResponse.Data.Status);
        Assert.Equal(0, apiResponse.Data.RegisteredTeamsCount);
        Assert.Equal(20, apiResponse.Data.SpotsRemaining);
    }

    [Fact]
    public async Task GetEvents_PublicOnly_ReturnsOnlyPublicEvents()
    {
        // Arrange
        var token = await RegisterAndLogin($"organizer-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create public event
        var publicEvent = await CreateEvent("public-event", isPublic: true);

        // Create private event
        var privateEvent = await CreateEvent("private-event", isPublic: false, registrationCode: "SECRET123");

        // Act - Query without authentication
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/v1/events/one-time?publicOnly=true");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<OneTimeEventListResponse>>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        
        // Should contain the public event
        Assert.Contains(apiResponse.Data, e => e.Key == publicEvent.Key);
        
        // Should NOT contain the private event
        Assert.DoesNotContain(apiResponse.Data, e => e.Key == privateEvent.Key);
    }

    [Fact]
    public async Task PublishEvent_ChangesStatusToPublished()
    {
        // Arrange
        var token = await RegisterAndLogin($"organizer-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var eventResponse = await CreateEvent($"event-{Guid.NewGuid().ToString()[..8]}");

        // Act
        var response = await _client.PostAsync($"/api/v1/events/one-time/{eventResponse.Id}/publish", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.Equal(EventStatus.Published, apiResponse.Data!.Status);
    }

    [Fact]
    public async Task RegisterTeam_WithValidData_CreatesTeamAndPlayers()
    {
        // Arrange
        var token = await RegisterAndLogin($"organizer-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var eventResponse = await CreateEvent($"event-{Guid.NewGuid().ToString()[..8]}");
        await _client.PostAsync($"/api/v1/events/one-time/{eventResponse.Id}/publish", null);

        // Register as a different user (team captain)
        var captainToken = await RegisterAndLogin($"captain-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", captainToken);

        var registerRequest = new RegisterTeamRequest
        {
            TeamName = "The Eagles",
            CaptainName = "John Doe",
            CaptainEmail = "john@example.com",
            CaptainPhone = "555-5678",
            Players = new List<RegisterTeamRequest.PlayerInfo>
            {
                new() { PlayerName = "John Doe", Email = "john@example.com", Handicap = 12.5m, IsCaptain = true },
                new() { PlayerName = "Jane Smith", Email = "jane@example.com", Handicap = 15.0m },
                new() { PlayerName = "Bob Johnson", Email = "bob@example.com", Handicap = 8.5m },
                new() { PlayerName = "Alice Williams", Email = "alice@example.com", Handicap = 20.0m }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/events/one-time/{eventResponse.Id}/teams",
            registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventTeamResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("The Eagles", apiResponse.Data.TeamName);
        Assert.Equal(4, apiResponse.Data.Players.Count);
        Assert.Equal(1, apiResponse.Data.TeamNumber);

        // Verify captain is marked correctly
        var captain = apiResponse.Data.Players.FirstOrDefault(p => p.IsCaptain);
        Assert.NotNull(captain);
        Assert.Equal("John Doe", captain.PlayerName);
    }

    [Fact]
    public async Task RegisterTeam_ForPrivateEvent_WithoutCode_ReturnsBadRequest()
    {
        // Arrange
        var token = await RegisterAndLogin($"organizer-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var eventResponse = await CreateEvent($"event-{Guid.NewGuid().ToString()[..8]}", isPublic: false, registrationCode: "SECRET123");
        await _client.PostAsync($"/api/v1/events/one-time/{eventResponse.Id}/publish", null);

        var captainToken = await RegisterAndLogin($"captain-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", captainToken);

        var registerRequest = new RegisterTeamRequest
        {
            TeamName = "The Eagles",
            CaptainName = "John Doe",
            CaptainEmail = "john@example.com",
            // Missing RegistrationCode
            Players = new List<RegisterTeamRequest.PlayerInfo>
            {
                new() { PlayerName = "John Doe", IsCaptain = true }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/events/one-time/{eventResponse.Id}/teams",
            registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterTeam_WhenEventFull_ReturnsBadRequest()
    {
        // Arrange
        var token = await RegisterAndLogin($"organizer-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create event with only 1 team capacity
        var eventResponse = await CreateEvent($"event-{Guid.NewGuid().ToString()[..8]}", maxTeams: 1);
        await _client.PostAsync($"/api/v1/events/one-time/{eventResponse.Id}/publish", null);

        // Register first team
        var captain1Token = await RegisterAndLogin($"captain1-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", captain1Token);

        var team1Request = new RegisterTeamRequest
        {
            TeamName = "Team 1",
            CaptainName = "Captain 1",
            CaptainEmail = "captain1@example.com",
            Players = new List<RegisterTeamRequest.PlayerInfo>
            {
                new() { PlayerName = "Player 1", IsCaptain = true }
            }
        };

        await _client.PostAsJsonAsync($"/api/v1/events/one-time/{eventResponse.Id}/teams", team1Request);

        // Try to register second team
        var captain2Token = await RegisterAndLogin($"captain2-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", captain2Token);

        var team2Request = new RegisterTeamRequest
        {
            TeamName = "Team 2",
            CaptainName = "Captain 2",
            CaptainEmail = "captain2@example.com",
            Players = new List<RegisterTeamRequest.PlayerInfo>
            {
                new() { PlayerName = "Player 2", IsCaptain = true }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/events/one-time/{eventResponse.Id}/teams",
            team2Request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CheckInTeam_ByOrganizer_UpdatesStatusToInProgress()
    {
        // Arrange
        var organizerToken = await RegisterAndLogin($"organizer-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", organizerToken);

        var eventResponse = await CreateEvent($"event-{Guid.NewGuid().ToString()[..8]}");
        await _client.PostAsync($"/api/v1/events/one-time/{eventResponse.Id}/publish", null);

        // Register a team
        var captainToken = await RegisterAndLogin($"captain-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", captainToken);

        var teamResponse = await RegisterTeam(eventResponse.Id, "The Eagles");

        // Switch back to organizer
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", organizerToken);

        // Act
        var response = await _client.PostAsync(
            $"/api/v1/events/one-time/teams/{teamResponse.Id}/check-in",
            null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventTeamResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.True(apiResponse.Data!.IsCheckedIn);
        Assert.NotNull(apiResponse.Data.CheckedInAt);

        // Verify event status changed to InProgress
        var eventCheckResponse = await _client.GetAsync($"/api/v1/events/one-time/{eventResponse.Id}");
        var eventCheckApiResponse = await eventCheckResponse.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
        Assert.Equal(EventStatus.InProgress, eventCheckApiResponse!.Data!.Status);
    }

    [Fact]
    public async Task FullEventLifecycle_CreatePublishRegisterCheckIn_WorksCorrectly()
    {
        // 1. Create event as organizer
        var organizerToken = await RegisterAndLogin($"organizer-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", organizerToken);

        var eventResponse = await CreateEvent($"full-lifecycle-{Guid.NewGuid().ToString()[..8]}");
        Assert.Equal(EventStatus.Draft, eventResponse.Status);

        // 2. Publish event
        var publishResponse = await _client.PostAsync($"/api/v1/events/one-time/{eventResponse.Id}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);

        // 3. Register multiple teams
        var team1Response = await RegisterTeamAsNewUser(eventResponse.Id, "Team Alpha", "alpha");
        var team2Response = await RegisterTeamAsNewUser(eventResponse.Id, "Team Bravo", "bravo");
        var team3Response = await RegisterTeamAsNewUser(eventResponse.Id, "Team Charlie", "charlie");

        // 4. Verify event statistics
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", organizerToken);
        var eventCheckResponse = await _client.GetAsync($"/api/v1/events/one-time/{eventResponse.Id}");
        var eventCheck = await eventCheckResponse.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
        Assert.Equal(3, eventCheck!.Data!.RegisteredTeamsCount);
        Assert.Equal(17, eventCheck.Data.SpotsRemaining); // 20 - 3 = 17

        // 5. Check in teams
        await _client.PostAsync($"/api/v1/events/one-time/teams/{team1Response.Id}/check-in", null);
        await _client.PostAsync($"/api/v1/events/one-time/teams/{team2Response.Id}/check-in", null);

        // 6. Verify event is InProgress
        eventCheckResponse = await _client.GetAsync($"/api/v1/events/one-time/{eventResponse.Id}");
        eventCheck = await eventCheckResponse.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
        Assert.Equal(EventStatus.InProgress, eventCheck!.Data!.Status);
        Assert.Equal(2, eventCheck.Data.CheckedInTeamsCount);
    }

    [Fact]
    public async Task MultiRoundEvent_CreatesWithCorrectRoundCount()
    {
        // Arrange
        var token = await RegisterAndLogin($"organizer-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateOneTimeEventRequest
        {
            Key = $"two-day-{Guid.NewGuid().ToString()[..8]}",
            Name = "Two-Day Championship",
            Description = "36-hole championship event",
            EventDate = DateTime.UtcNow.AddDays(30),
            RegistrationDeadline = DateTime.UtcNow.AddDays(25),
            OrganizationName = "Golf Club",
            OrganizerEmail = "contact@golfclub.com",
            CourseId = "test-course-1",
            TeeId = "test-tee-1",
            HolesPlayed = HolesPlayed.Eighteen,
            Format = ScoringFormat.StrokePlay,
            TeamSize = 1, // Individual
            MaxTeams = 50,
            UseHandicaps = true,
            AccessType = EventAccessType.Public,
            TotalRounds = 2 // Two rounds
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/events/one-time", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
        Assert.NotNull(apiResponse);
        Assert.Equal(2, apiResponse.Data!.TotalRounds);
    }

    [Fact]
    public async Task CancelEvent_ChangesStatusToCancelled()
    {
        // Arrange
        var token = await RegisterAndLogin($"organizer-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var eventResponse = await CreateEvent($"event-{Guid.NewGuid().ToString()[..8]}");
        await _client.PostAsync($"/api/v1/events/one-time/{eventResponse.Id}/publish", null);

        // Act
        var response = await _client.PostAsync($"/api/v1/events/one-time/{eventResponse.Id}/cancel", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
        Assert.Equal(EventStatus.Cancelled, apiResponse!.Data!.Status);
    }

    [Fact]
    public async Task DeleteEvent_ByOrganizer_ReturnsSuccess()
    {
        // Arrange
        var token = await RegisterAndLogin($"organizer-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var eventResponse = await CreateEvent($"event-{Guid.NewGuid().ToString()[..8]}");

        // Act
        var response = await _client.DeleteAsync($"/api/v1/events/one-time/{eventResponse.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        Assert.True(apiResponse!.Success);
        Assert.True(apiResponse.Data);
    }

    [Fact]
    public async Task UpdateEvent_ByNonOrganizer_ReturnsForbidden()
    {
        // Arrange
        var organizerToken = await RegisterAndLogin($"organizer-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", organizerToken);

        var eventResponse = await CreateEvent($"event-{Guid.NewGuid().ToString()[..8]}");

        // Switch to different user
        var otherUserToken = await RegisterAndLogin($"other-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherUserToken);

        var updateRequest = new UpdateOneTimeEventRequest
        {
            Name = "Hacked Event Name"
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/events/one-time/{eventResponse.Id}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddPlayer_ToCaptainTeam_Succeeds()
    {
        // Arrange
        var organizerToken = await RegisterAndLogin($"organizer-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", organizerToken);

        var eventResponse = await CreateEvent($"event-{Guid.NewGuid().ToString()[..8]}");
        await _client.PostAsync($"/api/v1/events/one-time/{eventResponse.Id}/publish", null);

        // Register team with captain
        var captainToken = await RegisterAndLogin($"captain-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", captainToken);

        var teamResponse = await RegisterTeam(eventResponse.Id, "The Eagles", playerCount: 2);

        // Add another player
        var addPlayerRequest = new AddPlayerRequest
        {
            PlayerName = "New Player",
            Email = "newplayer@example.com",
            Handicap = 18.5m,
            IsCaptain = false
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/events/one-time/teams/{teamResponse.Id}/players",
            addPlayerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventPlayerResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.Equal("New Player", apiResponse.Data!.PlayerName);
        Assert.Equal(3, apiResponse.Data.PlayerNumber);
    }

    #region Helper Methods

    private async Task<string> RegisterAndLogin(string email)
    {
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = "SecurePassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        return authResponse!.AccessToken;
    }

    private async Task<OneTimeEventResponse> CreateEvent(
        string key,
        bool isPublic = true,
        string? registrationCode = null,
        int maxTeams = 20,
        int teamSize = 4)
    {
        var request = new CreateOneTimeEventRequest
        {
            Key = key,
            Name = $"Event {key}",
            Description = "Test event",
            EventDate = DateTime.UtcNow.AddDays(30),
            RegistrationDeadline = DateTime.UtcNow.AddDays(25),
            OrganizationName = "Test Organizer",
            OrganizerEmail = "organizer@example.com",
            OrganizerPhone = "555-1234",
            CourseId = "test-course-1",
            TeeId = "test-tee-1",
            HolesPlayed = HolesPlayed.Eighteen,
            Format = ScoringFormat.Scramble,
            TeamSize = teamSize,
            MaxTeams = maxTeams,
            UseHandicaps = true,
            AccessType = isPublic ? EventAccessType.Public : EventAccessType.Private,
            RegistrationCode = registrationCode,
            TotalRounds = 1
        };

        var response = await _client.PostAsJsonAsync("/api/v1/events/one-time", request);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
        return apiResponse!.Data!;
    }

    private async Task<OneTimeEventTeamResponse> RegisterTeam(
        string eventId,
        string teamName,
        int playerCount = 4,
        string? registrationCode = null)
    {
        var players = new List<RegisterTeamRequest.PlayerInfo>();
        for (int i = 1; i <= playerCount; i++)
        {
            players.Add(new RegisterTeamRequest.PlayerInfo
            {
                PlayerName = $"Player {i}",
                Email = $"player{i}@example.com",
                Handicap = 10.0m + i,
                IsCaptain = i == 1
            });
        }

        var request = new RegisterTeamRequest
        {
            TeamName = teamName,
            CaptainName = "Captain",
            CaptainEmail = "captain@example.com",
            CaptainPhone = "555-5678",
            RegistrationCode = registrationCode,
            Players = players
        };

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/events/one-time/{eventId}/teams",
            request);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventTeamResponse>>();
        return apiResponse!.Data!;
    }

    private async Task<OneTimeEventTeamResponse> RegisterTeamAsNewUser(
        string eventId,
        string teamName,
        string userPrefix)
    {
        var token = await RegisterAndLogin($"{userPrefix}-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await RegisterTeam(eventId, teamName);
    }

    #endregion
}


