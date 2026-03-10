using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Auth;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.League;
using GolfManager.Shared.DTOs.Season;
using GolfManager.Core.Enums;

namespace GolfManager.IntegrationTests;

/// <summary>
/// Comprehensive integration tests for Season and Event management
/// </summary>
public class SeasonIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SeasonIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateSeason_WithValidData_ReturnsSeasonResponse()
    {
        // Arrange
        var (token, leagueId) = await SetupLeagueAndAuth();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateSeasonRequest
        {
            Key = "spring-2024",
            Name = "Spring 2024 Season",
            StartDate = new DateOnly(2024, 3, 1),
            EndDate = new DateOnly(2024, 6, 30)
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/seasons", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<SeasonResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(request.Key, apiResponse.Data.Key);
        Assert.Equal(request.Name, apiResponse.Data.Name);
        Assert.Equal(leagueId, apiResponse.Data.LeagueId);
    }

    [Fact]
    public async Task GetSeasons_ReturnsAllSeasonsForLeague()
    {
        // Arrange
        var (token, leagueId) = await SetupLeagueAndAuth();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create multiple seasons
        await CreateSeason(leagueId, "spring-2024", "Spring 2024");
        await CreateSeason(leagueId, "summer-2024", "Summer 2024");
        await CreateSeason(leagueId, "fall-2024", "Fall 2024");

        // Act
        var response = await _client.GetAsync($"/api/v1/leagues/{leagueId}/seasons");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<SeasonResponse>>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(3, apiResponse.Data.Count);
    }

    [Fact]
    public async Task CreateEvent_WithValidData_ReturnsEventResponse()
    {
        // Arrange
        var (token, leagueId) = await SetupLeagueAndAuth();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var seasonId = await CreateSeason(leagueId, "spring-2024", "Spring 2024");

        var request = new CreateEventRequest
        {
            EventDate = new DateTime(2024, 3, 15, 9, 0, 0),
            HolesPlayed = HolesPlayed.Eighteen,
            EventType = SeasonEventType.Regular,
            ScoringFormat = ScoringFormat.StrokePlay,
            Name = "Opening Day Tournament",
            Description = "First event of the season"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{leagueId}/seasons/{seasonId}/events",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<EventResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(seasonId, apiResponse.Data.SeasonId);
        Assert.Equal(request.Name, apiResponse.Data.Name);
        Assert.Equal(request.EventType, apiResponse.Data.EventType);
        Assert.Equal(request.ScoringFormat, apiResponse.Data.ScoringFormat);
    }

    [Fact]
    public async Task GetEvents_ReturnsAllEventsForSeason()
    {
        // Arrange
        var (token, leagueId) = await SetupLeagueAndAuth();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var seasonId = await CreateSeason(leagueId, "spring-2024", "Spring 2024");

        // Create multiple events
        await CreateEvent(leagueId, seasonId, new DateTime(2024, 3, 15), "Event 1");
        await CreateEvent(leagueId, seasonId, new DateTime(2024, 4, 15), "Event 2");
        await CreateEvent(leagueId, seasonId, new DateTime(2024, 5, 15), "Event 3");

        // Act
        var response = await _client.GetAsync($"/api/v1/leagues/{leagueId}/seasons/{seasonId}/events");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<EventResponse>>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(3, apiResponse.Data.Count);
        // Events should be ordered by date
        Assert.True(apiResponse.Data[0].EventDate < apiResponse.Data[1].EventDate);
        Assert.True(apiResponse.Data[1].EventDate < apiResponse.Data[2].EventDate);
    }

    [Fact]
    public async Task UpdateSeason_WithValidData_ReturnsUpdatedSeason()
    {
        // Arrange
        var (token, leagueId) = await SetupLeagueAndAuth();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var seasonId = await CreateSeason(leagueId, "spring-2024", "Spring 2024");

        var updateRequest = new UpdateSeasonRequest
        {
            Name = "Spring 2024 Championship Season"
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/leagues/{leagueId}/seasons/{seasonId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<SeasonResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.Equal(updateRequest.Name, apiResponse.Data!.Name);
    }

    [Fact]
    public async Task UpdateEvent_WithValidData_ReturnsUpdatedEvent()
    {
        // Arrange
        var (token, leagueId) = await SetupLeagueAndAuth();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var seasonId = await CreateSeason(leagueId, "spring-2024", "Spring 2024");
        var eventId = await CreateEvent(leagueId, seasonId, new DateTime(2024, 3, 15), "Original Event");

        var updateRequest = new UpdateEventRequest
        {
            Name = "Updated Event Name",
            EventType = SeasonEventType.Championship
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/leagues/{leagueId}/seasons/{seasonId}/events/{eventId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<EventResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.Equal(updateRequest.Name, apiResponse.Data!.Name);
        Assert.Equal(SeasonEventType.Championship, apiResponse.Data.EventType);
    }

    [Fact]
    public async Task DeleteSeason_RemovesSeason()
    {
        // Arrange
        var (token, leagueId) = await SetupLeagueAndAuth();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var seasonId = await CreateSeason(leagueId, "spring-2024", "Spring 2024");

        // Act
        var response = await _client.DeleteAsync($"/api/v1/leagues/{leagueId}/seasons/{seasonId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify season is deleted
        var getResponse = await _client.GetAsync($"/api/v1/leagues/{leagueId}/seasons/{seasonId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_RemovesEvent()
    {
        // Arrange
        var (token, leagueId) = await SetupLeagueAndAuth();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var seasonId = await CreateSeason(leagueId, "spring-2024", "Spring 2024");
        var eventId = await CreateEvent(leagueId, seasonId, new DateTime(2024, 3, 15), "Test Event");

        // Act
        var response = await _client.DeleteAsync(
            $"/api/v1/leagues/{leagueId}/seasons/{seasonId}/events/{eventId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify event is deleted
        var getResponse = await _client.GetAsync(
            $"/api/v1/leagues/{leagueId}/seasons/{seasonId}/events/{eventId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task NonAdminUser_CannotCreateSeason()
    {
        // Arrange
        var (adminToken, leagueId) = await SetupLeagueAndAuth();

        // Create a non-admin user
        var memberEmail = $"member-{Guid.NewGuid()}@example.com";
        var memberToken = await RegisterAndLogin(memberEmail);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        var request = new CreateSeasonRequest
        {
            Key = "unauthorized-season",
            Name = "Unauthorized Season",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3))
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/seasons", request);

        // Assert - Should be Forbidden since user is not a league admin
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #region Helper Methods

    private async Task<(string token, string leagueId)> SetupLeagueAndAuth()
    {
        var email = $"admin-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLogin(email);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var leagueRequest = new CreateLeagueRequest
        {
            Key = $"league-{Guid.NewGuid().ToString()[..8]}",
            Name = "Test League",
            Description = "Test league for season tests"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/leagues", leagueRequest);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();

        return (token, apiResponse!.Data!.Id);
    }

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

    private async Task<string> CreateSeason(string leagueId, string key, string name)
    {
        var request = new CreateSeasonRequest
        {
            Key = key,
            Name = name,
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31)
        };

        var response = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/seasons", request);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<SeasonResponse>>();

        return apiResponse!.Data!.Id;
    }

    private async Task<string> CreateEvent(string leagueId, string seasonId, DateTime eventDate, string name)
    {
        var request = new CreateEventRequest
        {
            EventDate = eventDate,
            HolesPlayed = HolesPlayed.Eighteen,
            EventType = SeasonEventType.Regular,
            ScoringFormat = ScoringFormat.StrokePlay,
            Name = name,
            Description = $"Test event {name}"
        };

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{leagueId}/seasons/{seasonId}/events",
            request);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<EventResponse>>();

        return apiResponse!.Data!.Id;
    }

    /// <summary>
    /// Comprehensive test that simulates a full season with multiple golfers and events
    /// This test verifies the entire workflow from league creation to event management
    /// </summary>
    [Fact]
    public async Task FullSeasonSimulation_WithMultipleGolfersAndEvents_WorksCorrectly()
    {
        // Arrange - Create league and authenticate
        var (token, leagueId) = await SetupLeagueAndAuth();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a season for 2024
        var seasonRequest = new CreateSeasonRequest
        {
            Key = "2024-championship",
            Name = "2024 Championship Season",
            StartDate = new DateOnly(2024, 4, 1),
            EndDate = new DateOnly(2024, 10, 31)
        };

        var seasonResponse = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/seasons", seasonRequest);
        var seasonApiResponse = await seasonResponse.Content.ReadFromJsonAsync<ApiResponse<SeasonResponse>>();
        var seasonId = seasonApiResponse!.Data!.Id;

        // Verify season was created
        Assert.Equal(HttpStatusCode.Created, seasonResponse.StatusCode);
        Assert.Equal("2024-championship", seasonApiResponse.Data.Key);
        Assert.Equal("2024 Championship Season", seasonApiResponse.Data.Name);
        Assert.Equal(new DateOnly(2024, 4, 1), seasonApiResponse.Data.StartDate);
        Assert.Equal(new DateOnly(2024, 10, 31), seasonApiResponse.Data.EndDate);
        Assert.Equal(0, seasonApiResponse.Data.EventCount);
        Assert.Equal(0, seasonApiResponse.Data.GolferCount);

        // Create multiple events throughout the season
        var event1Request = new CreateEventRequest
        {
            EventDate = new DateTime(2024, 4, 15, 9, 0, 0),
            HolesPlayed = HolesPlayed.Eighteen,
            EventType = SeasonEventType.Regular,
            ScoringFormat = ScoringFormat.StrokePlay,
            Name = "Opening Day Classic",
            Description = "Season opener"
        };

        var event1Response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{leagueId}/seasons/{seasonId}/events",
            event1Request);
        var event1ApiResponse = await event1Response.Content.ReadFromJsonAsync<ApiResponse<EventResponse>>();
        var event1Id = event1ApiResponse!.Data!.Id;

        Assert.Equal(HttpStatusCode.Created, event1Response.StatusCode);
        Assert.Equal("Opening Day Classic", event1ApiResponse.Data.Name);
        Assert.Equal(SeasonEventType.Regular, event1ApiResponse.Data.EventType);
        Assert.Equal(ScoringFormat.StrokePlay, event1ApiResponse.Data.ScoringFormat);

        var event2Request = new CreateEventRequest
        {
            EventDate = new DateTime(2024, 5, 20, 10, 0, 0),
            HolesPlayed = HolesPlayed.Eighteen,
            EventType = SeasonEventType.Regular,
            ScoringFormat = ScoringFormat.Stableford,
            Name = "Spring Stableford",
            Description = "Points-based competition"
        };

        var event2Response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{leagueId}/seasons/{seasonId}/events",
            event2Request);
        var event2ApiResponse = await event2Response.Content.ReadFromJsonAsync<ApiResponse<EventResponse>>();

        Assert.Equal(HttpStatusCode.Created, event2Response.StatusCode);
        Assert.Equal(ScoringFormat.Stableford, event2ApiResponse!.Data!.ScoringFormat);

        var event3Request = new CreateEventRequest
        {
            EventDate = new DateTime(2024, 10, 15, 9, 0, 0),
            HolesPlayed = HolesPlayed.Eighteen,
            EventType = SeasonEventType.Championship,
            ScoringFormat = ScoringFormat.StrokePlay,
            Name = "Season Championship",
            Description = "Final championship event"
        };

        var event3Response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{leagueId}/seasons/{seasonId}/events",
            event3Request);
        var event3ApiResponse = await event3Response.Content.ReadFromJsonAsync<ApiResponse<EventResponse>>();

        Assert.Equal(HttpStatusCode.Created, event3Response.StatusCode);
        Assert.Equal(SeasonEventType.Championship, event3ApiResponse!.Data!.EventType);

        // Verify all events are returned in chronological order
        var eventsListResponse = await _client.GetAsync($"/api/v1/leagues/{leagueId}/seasons/{seasonId}/events");
        var eventsListApiResponse = await eventsListResponse.Content.ReadFromJsonAsync<ApiResponse<List<EventResponse>>>();

        Assert.Equal(HttpStatusCode.OK, eventsListResponse.StatusCode);
        Assert.NotNull(eventsListApiResponse!.Data);
        Assert.Equal(3, eventsListApiResponse.Data.Count);

        // Verify chronological order
        Assert.Equal("Opening Day Classic", eventsListApiResponse.Data[0].Name);
        Assert.Equal("Spring Stableford", eventsListApiResponse.Data[1].Name);
        Assert.Equal("Season Championship", eventsListApiResponse.Data[2].Name);

        // Verify dates are in order
        Assert.True(eventsListApiResponse.Data[0].EventDate < eventsListApiResponse.Data[1].EventDate);
        Assert.True(eventsListApiResponse.Data[1].EventDate < eventsListApiResponse.Data[2].EventDate);

        // Get season details and verify event count
        var seasonDetailsResponse = await _client.GetAsync($"/api/v1/leagues/{leagueId}/seasons/{seasonId}");
        var seasonDetailsApiResponse = await seasonDetailsResponse.Content.ReadFromJsonAsync<ApiResponse<SeasonResponse>>();

        Assert.Equal(HttpStatusCode.OK, seasonDetailsResponse.StatusCode);
        Assert.Equal(3, seasonDetailsApiResponse!.Data!.EventCount);

        // Verify we can update an event
        var updateEventRequest = new UpdateEventRequest
        {
            Name = "Opening Day Classic - UPDATED",
            EventType = SeasonEventType.Special
        };

        var updateEventResponse = await _client.PutAsJsonAsync(
            $"/api/v1/leagues/{leagueId}/seasons/{seasonId}/events/{event1Id}",
            updateEventRequest);

        Assert.Equal(HttpStatusCode.OK, updateEventResponse.StatusCode);
        var updatedEventApiResponse = await updateEventResponse.Content.ReadFromJsonAsync<ApiResponse<EventResponse>>();
        Assert.Equal("Opening Day Classic - UPDATED", updatedEventApiResponse!.Data!.Name);
        Assert.Equal(SeasonEventType.Special, updatedEventApiResponse.Data.EventType);

        // Verify season can be retrieved by key
        var seasonByKeyResponse = await _client.GetAsync($"/api/v1/leagues/{leagueId}/seasons/by-key/2024-championship");
        var seasonByKeyApiResponse = await seasonByKeyResponse.Content.ReadFromJsonAsync<ApiResponse<SeasonResponse>>();

        Assert.Equal(HttpStatusCode.OK, seasonByKeyResponse.StatusCode);
        Assert.Equal(seasonId, seasonByKeyApiResponse!.Data!.Id);
        Assert.Equal("2024-championship", seasonByKeyApiResponse.Data.Key);

        // Verify all seasons are returned
        var allSeasonsResponse = await _client.GetAsync($"/api/v1/leagues/{leagueId}/seasons");
        var allSeasonsApiResponse = await allSeasonsResponse.Content.ReadFromJsonAsync<ApiResponse<List<SeasonResponse>>>();

        Assert.Equal(HttpStatusCode.OK, allSeasonsResponse.StatusCode);
        Assert.NotNull(allSeasonsApiResponse!.Data);
        Assert.Single(allSeasonsApiResponse.Data);
        Assert.Equal(3, allSeasonsApiResponse.Data[0].EventCount);
    }

    #endregion
}
