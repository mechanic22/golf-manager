using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GolfManager.Data;
using GolfManager.Shared.DTOs.Auth;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.League;
using GolfManager.Shared.DTOs.Player;
using GolfManager.Shared.DTOs.Round;
using GolfManager.Shared.DTOs.Season;
using GolfManager.Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace GolfManager.IntegrationTests;

/// <summary>
/// Comprehensive integration tests for Round and Scoring functionality
/// </summary>
public class RoundIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public RoundIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateRound_WithValidData_ReturnsRoundResponse()
    {
        // Arrange
        var (token, leagueId, courseId, teeId, leagueGolferId) = await SetupTestData();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateRoundRequest
        {
            LeagueGolferId = leagueGolferId,
            CourseId = courseId,
            TeeId = teeId,
            RoundDate = DateTime.UtcNow,
            HolesPlayed = HolesPlayed.Eighteen,
            HandicapUsed = 12.5,
            Notes = "Great round!"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/rounds", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RoundResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(leagueGolferId, apiResponse.Data.LeagueGolferId);
        Assert.Equal(courseId, apiResponse.Data.CourseId);
        Assert.Equal(HolesPlayed.Eighteen, apiResponse.Data.HolesPlayed);
        Assert.Equal(12.5, apiResponse.Data.HandicapUsed);
    }

    [Fact]
    public async Task RecordHoleScore_WithValidData_UpdatesRound()
    {
        // Arrange
        var (token, leagueId, courseId, teeId, leagueGolferId) = await SetupTestData();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a round first
        var createRequest = new CreateRoundRequest
        {
            LeagueGolferId = leagueGolferId,
            CourseId = courseId,
            TeeId = teeId,
            RoundDate = DateTime.UtcNow,
            HolesPlayed = HolesPlayed.Eighteen
        };

        var createResponse = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/rounds", createRequest);
        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<RoundResponse>>();
        var roundId = createApiResponse!.Data!.Id;

        // Record a hole score
        var holeScoreRequest = new RecordHoleScoreRequest
        {
            HoleNumber = 1,
            GrossScore = 4,
            Putts = 2,
            FairwayHit = true,
            GreenInRegulation = true,
            Penalties = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{leagueId}/rounds/{roundId}/holes",
            holeScoreRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RoundResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Single(apiResponse.Data.Holes);
        Assert.Equal(1, apiResponse.Data.Holes[0].HoleNumber);
        Assert.Equal(4, apiResponse.Data.Holes[0].GrossScore);
        Assert.Equal(2, apiResponse.Data.Holes[0].Putts);
        Assert.True(apiResponse.Data.Holes[0].FairwayHit);
        Assert.True(apiResponse.Data.Holes[0].GreenInRegulation);
    }

    [Fact]
    public async Task RecordMultipleHoleScores_CalculatesTotalScore()
    {
        // Arrange
        var (token, leagueId, courseId, teeId, leagueGolferId) = await SetupTestData();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a round
        var createRequest = new CreateRoundRequest
        {
            LeagueGolferId = leagueGolferId,
            CourseId = courseId,
            TeeId = teeId,
            RoundDate = DateTime.UtcNow,
            HolesPlayed = HolesPlayed.Nine,
            HandicapUsed = 9.0
        };

        var createResponse = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/rounds", createRequest);
        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<RoundResponse>>();
        var roundId = createApiResponse!.Data!.Id;

        // Record scores for 9 holes
        int[] scores = { 4, 5, 3, 4, 6, 4, 5, 3, 4 }; // Total = 38

        for (int i = 0; i < scores.Length; i++)
        {
            var holeScoreRequest = new RecordHoleScoreRequest
            {
                HoleNumber = i + 1,
                GrossScore = scores[i]
            };

            await _client.PostAsJsonAsync(
                $"/api/v1/leagues/{leagueId}/rounds/{roundId}/holes",
                holeScoreRequest);
        }

        // Act - Get the round to verify totals
        var response = await _client.GetAsync($"/api/v1/leagues/{leagueId}/rounds/{roundId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RoundResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(9, apiResponse.Data.Holes.Count);
        Assert.Equal(38, apiResponse.Data.TotalScore);
        Assert.Equal(29, apiResponse.Data.NetScore); // 38 - 9 = 29
    }

    [Fact]
    public async Task GetRound_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var (token, leagueId, _, _, _) = await SetupTestData();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/v1/leagues/{leagueId}/rounds/invalid-round-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRound_WithValidData_UpdatesRound()
    {
        // Arrange
        var (token, leagueId, courseId, teeId, leagueGolferId) = await SetupTestData();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a round
        var createRequest = new CreateRoundRequest
        {
            LeagueGolferId = leagueGolferId,
            CourseId = courseId,
            TeeId = teeId,
            RoundDate = DateTime.UtcNow,
            HolesPlayed = HolesPlayed.Eighteen
        };

        var createResponse = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/rounds", createRequest);
        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<RoundResponse>>();
        var roundId = createApiResponse!.Data!.Id;

        // Update the round
        var updateRequest = new UpdateRoundRequest
        {
            HandicapUsed = 15.5,
            Notes = "Updated notes",
            IsComplete = true
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/leagues/{leagueId}/rounds/{roundId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RoundResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(15.5, apiResponse.Data.HandicapUsed);
        Assert.Equal("Updated notes", apiResponse.Data.Notes);
        Assert.True(apiResponse.Data.IsComplete);
    }

    [Fact]
    public async Task DeleteRound_WithValidId_DeletesRound()
    {
        // Arrange
        var (token, leagueId, courseId, teeId, leagueGolferId) = await SetupTestData();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a round
        var createRequest = new CreateRoundRequest
        {
            LeagueGolferId = leagueGolferId,
            CourseId = courseId,
            TeeId = teeId,
            RoundDate = DateTime.UtcNow,
            HolesPlayed = HolesPlayed.Eighteen
        };

        var createResponse = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/rounds", createRequest);
        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<RoundResponse>>();
        var roundId = createApiResponse!.Data!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/v1/leagues/{leagueId}/rounds/{roundId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify the round is deleted
        var getResponse = await _client.GetAsync($"/api/v1/leagues/{leagueId}/rounds/{roundId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private async Task<(string token, string leagueId, string courseId, string teeId, string leagueGolferId)> SetupTestData()
    {
        // Register and get token
        var registerRequest = new RegisterRequest
        {
            Email = $"roundtest{Guid.NewGuid()}@example.com",
            Password = "Test123!",
            FirstName = "Round",
            LastName = "Tester"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        var token = authResponse!.AccessToken;
        var userId = authResponse.UserId;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a league
        var leagueRequest = new CreateLeagueRequest
        {
            Key = $"round-test-{Guid.NewGuid().ToString().Substring(0, 8)}",
            Name = "Round Test League"
        };

        var leagueResponse = await _client.PostAsJsonAsync("/api/v1/leagues", leagueRequest);
        var leagueApiResponse = await leagueResponse.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
        var leagueId = leagueApiResponse!.Data!.Id;

        // Add a player to the league
        var playerRequest = new CreatePlayerRequest
        {
            UserId = userId,
            DisplayName = "Round Tester",
            LeagueHandicap = 12.5
        };

        var playerResponse = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/players", playerRequest);
        var playerApiResponse = await playerResponse.Content.ReadFromJsonAsync<ApiResponse<PlayerResponse>>();
        var leagueGolferId = playerApiResponse!.Data!.Id;

        // Create a course and tee for testing
        // Note: In a real scenario, you'd create these through the API
        // For now, we'll create them directly in the database
        var courseId = Guid.NewGuid().ToString();
        var teeId = Guid.NewGuid().ToString();

        // We need to create the course and tee entities directly in the database
        // This is a workaround until we have Course/Tee management endpoints
        var course = new Core.Entities.Course
        {
            Id = courseId,
            Key = $"test-course-{Guid.NewGuid().ToString().Substring(0, 8)}",
            Name = "Test Golf Course",
            City = "Test City",
            State = "TS",
            NumberOfHoles = 18,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var tee = new Core.Entities.Tee
        {
            Id = teeId,
            CourseId = courseId,
            Name = "Blue Tees",
            HtmlColorCode = "#0000FF",
            RatingOut = 36.0,
            SlopeOut = 113,
            RatingIn = 36.0,
            SlopeIn = 113,
            YardsOut = 3200,
            YardsIn = 3200,
            ParOut = 36,
            ParIn = 36,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Get the database context from the test server
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GolfManagerDbContext>();
            dbContext.Courses.Add(course);
            dbContext.Tees.Add(tee);
            await dbContext.SaveChangesAsync();
        }

        return (token, leagueId, courseId, teeId, leagueGolferId);
    }
}

