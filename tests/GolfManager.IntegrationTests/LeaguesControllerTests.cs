using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GolfManager.Core.Entities;
using GolfManager.Shared.DTOs.Auth;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.League;
using Microsoft.Extensions.DependencyInjection;

namespace GolfManager.IntegrationTests;

public class LeaguesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public LeaguesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginAsync(string email, string password = "Password123!")
    {
        // Register
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = password,
            FirstName = "Test",
            LastName = "User"
        };

        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Login
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        return loginResult!.AccessToken;
    }

    [Fact]
    public async Task CreateLeague_WithValidRequest_CreatesLeague()
    {
        // Arrange
        var token = await RegisterAndLoginAsync("createleague@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateLeagueRequest
        {
            Key = "new-league",
            Name = "New League",
            Description = "A brand new league",
            LogoUrl = "https://example.com/logo.png"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/leagues", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("new-league", result.Data.Key);
        Assert.Equal("New League", result.Data.Name);
        Assert.Equal("A brand new league", result.Data.Description);
        Assert.Equal("https://example.com/logo.png", result.Data.LogoUrl);
        Assert.True(result.Data.IsCurrentUserAdmin);
        Assert.Equal(1, result.Data.MemberCount);
    }

    [Fact]
    public async Task CreateLeague_WithDuplicateKey_ReturnsBadRequest()
    {
        // Arrange
        var token = await RegisterAndLoginAsync("duplicate@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateLeagueRequest
        {
            Key = "duplicate-league",
            Name = "Duplicate League"
        };

        // Create first league
        await _client.PostAsJsonAsync("/api/v1/leagues", request);

        // Act - Try to create duplicate
        var response = await _client.PostAsJsonAsync("/api/v1/leagues", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUserLeagues_ReturnsOnlyUserLeagues()
    {
        // Arrange
        var token = await RegisterAndLoginAsync("getleagues@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create two leagues
        await _client.PostAsJsonAsync("/api/v1/leagues", new CreateLeagueRequest
        {
            Key = "league-one",
            Name = "League One"
        });

        await _client.PostAsJsonAsync("/api/v1/leagues", new CreateLeagueRequest
        {
            Key = "league-two",
            Name = "League Two"
        });

        // Act
        var response = await _client.GetAsync("/api/v1/leagues");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<LeagueResponse>>>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Contains(result.Data, l => l.Key == "league-one");
        Assert.Contains(result.Data, l => l.Key == "league-two");
    }

    [Fact]
    public async Task GetLeagueById_WithValidId_ReturnsLeague()
    {
        // Arrange
        var token = await RegisterAndLoginAsync("getbyid@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/leagues", new CreateLeagueRequest
        {
            Key = "get-by-id",
            Name = "Get By ID League"
        });

        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
        var leagueId = createResult!.Data!.Id;

        // Act
        var response = await _client.GetAsync($"/api/v1/leagues/{leagueId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(leagueId, result.Data.Id);
        Assert.Equal("get-by-id", result.Data.Key);
    }

    [Fact]
    public async Task GetLeagueByKey_WithValidKey_ReturnsLeague()
    {
        // Arrange
        var token = await RegisterAndLoginAsync("getbykey@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/v1/leagues", new CreateLeagueRequest
        {
            Key = "get-by-key",
            Name = "Get By Key League"
        });

        // Act - No auth required for this endpoint
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/v1/leagues/by-key/get-by-key");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("get-by-key", result.Data.Key);
    }

    [Fact]
    public async Task UpdateLeague_WithLeagueAdmin_UpdatesLeague()
    {
        // Arrange
        var token = await RegisterAndLoginAsync("updateleague@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/leagues", new CreateLeagueRequest
        {
            Key = "update-league",
            Name = "Original Name"
        });

        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
        var leagueId = createResult!.Data!.Id;

        var updateRequest = new UpdateLeagueRequest
        {
            Name = "Updated Name",
            Description = "Updated description"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/leagues/{leagueId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Updated Name", result.Data.Name);
        Assert.Equal("Updated description", result.Data.Description);
    }

    [Fact]
    public async Task DeleteLeague_WithLeagueAdmin_DeletesLeague()
    {
        // Arrange
        var token = await RegisterAndLoginAsync("deleteleague@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/leagues", new CreateLeagueRequest
        {
            Key = "delete-league",
            Name = "Delete Me"
        });

        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
        var leagueId = createResult!.Data!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/v1/leagues/{leagueId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.True(result.Data);

        // Verify league is deleted (soft delete)
        var getResponse = await _client.GetAsync($"/api/v1/leagues/{leagueId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetLeagueById_WithNonMember_ReturnsForbidden()
    {
        // Arrange - Create league with one user
        var adminToken = await RegisterAndLoginAsync("getadmin@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/leagues", new CreateLeagueRequest
        {
            Key = "private-league",
            Name = "Private League"
        });

        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
        var leagueId = createResult!.Data!.Id;

        // Login as different user
        var otherToken = await RegisterAndLoginAsync("other@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);

        // Act
        var response = await _client.GetAsync($"/api/v1/leagues/{leagueId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
