using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GolfManager.Core.Entities;
using GolfManager.Shared.DTOs.Auth;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Player;
using Microsoft.Extensions.DependencyInjection;

namespace GolfManager.IntegrationTests;

public class PlayersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public PlayersControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string leagueId, string adminToken, string memberToken)> SetupLeagueWithUsersAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Data.GolfManagerDbContext>();

        // Create admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@league.com",
            PasswordHash = "hash",
            FirstName = "Admin",
            LastName = "User",
            IsGlobalAdmin = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };
        context.Users.Add(adminUser);

        // Create member user
        var memberUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "member@league.com",
            PasswordHash = "hash",
            FirstName = "Member",
            LastName = "User",
            IsGlobalAdmin = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };
        context.Users.Add(memberUser);

        // Create league
        var league = new League
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test League",
            Key = "test-league",
            Description = "Test league for player tests",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = adminUser.Id
        };
        context.Leagues.Add(league);

        // Add admin as league admin
        var adminMembership = new UserLeague
        {
            Id = Guid.NewGuid().ToString(),
            UserId = adminUser.Id,
            LeagueId = league.Id,
            IsLeagueAdmin = true,
            IsActive = true,
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = adminUser.Id
        };
        context.UserLeagues.Add(adminMembership);

        // Add member as regular member
        var memberMembership = new UserLeague
        {
            Id = Guid.NewGuid().ToString(),
            UserId = memberUser.Id,
            LeagueId = league.Id,
            IsLeagueAdmin = false,
            IsActive = true,
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = memberUser.Id
        };
        context.UserLeagues.Add(memberMembership);

        await context.SaveChangesAsync();

        // Generate tokens
        var jwtService = scope.ServiceProvider.GetRequiredService<Services.Auth.IJwtTokenService>();
        var adminToken = jwtService.GenerateAccessToken(adminUser, new List<string> { league.Id });
        var memberToken = jwtService.GenerateAccessToken(memberUser, new List<string> { league.Id });

        return (league.Id, adminToken, memberToken);
    }

    [Fact]
    public async Task GetPlayers_WithLeagueMember_ReturnsEmptyList()
    {
        // Arrange
        var (leagueId, _, memberToken) = await SetupLeagueWithUsersAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        // Act
        var response = await _client.GetAsync($"/api/v1/leagues/{leagueId}/players");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<PlayerResponse>>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Empty(apiResponse.Data);
    }

    [Fact]
    public async Task GetPlayers_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var (leagueId, _, _) = await SetupLeagueWithUsersAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/leagues/{leagueId}/players");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddPlayer_WithLeagueAdmin_CreatesPlayer()
    {
        // Arrange
        var (leagueId, adminToken, _) = await SetupLeagueWithUsersAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var request = new CreatePlayerRequest
        {
            Email = "newplayer@example.com",
            FirstName = "New",
            LastName = "Player",
            DisplayName = "New Player",
            Nickname = "Newbie",
            LeagueHandicap = 15.5
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/players", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PlayerResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(request.DisplayName, apiResponse.Data.DisplayName);
        Assert.Equal(request.Nickname, apiResponse.Data.Nickname);
        Assert.Equal(request.LeagueHandicap, apiResponse.Data.LeagueHandicap);
        Assert.Equal(request.Email, apiResponse.Data.Email);
    }

    [Fact]
    public async Task AddPlayer_WithLeagueMember_ReturnsForbidden()
    {
        // Arrange
        var (leagueId, _, memberToken) = await SetupLeagueWithUsersAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        var request = new CreatePlayerRequest
        {
            Email = "newplayer2@example.com",
            FirstName = "New",
            LastName = "Player",
            DisplayName = "New Player 2"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/players", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddPlayer_WithExistingUser_AddsToLeague()
    {
        // Arrange
        var (leagueId, adminToken, _) = await SetupLeagueWithUsersAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // First create a user
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Data.GolfManagerDbContext>();

        var existingUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "existing@example.com",
            PasswordHash = "hash",
            FirstName = "Existing",
            LastName = "User",
            IsGlobalAdmin = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var request = new CreatePlayerRequest
        {
            UserId = existingUser.Id,
            DisplayName = "Existing Player",
            LeagueHandicap = 10.0
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/players", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PlayerResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(existingUser.Id, apiResponse.Data.UserId);
        Assert.Equal(request.DisplayName, apiResponse.Data.DisplayName);
    }

    [Fact]
    public async Task UpdatePlayer_WithLeagueAdmin_UpdatesPlayer()
    {
        // Arrange
        var (leagueId, adminToken, _) = await SetupLeagueWithUsersAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // First create a player
        var createRequest = new CreatePlayerRequest
        {
            Email = "updatetest@example.com",
            FirstName = "Update",
            LastName = "Test",
            DisplayName = "Original Name",
            LeagueHandicap = 20.0
        };

        var createResponse = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/players", createRequest);
        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<PlayerResponse>>();
        var playerId = createApiResponse!.Data!.Id;

        // Act - Update the player
        var updateRequest = new UpdatePlayerRequest
        {
            DisplayName = "Updated Name",
            Nickname = "Updated Nickname",
            LeagueHandicap = 15.0
        };

        var response = await _client.PutAsJsonAsync($"/api/v1/leagues/{leagueId}/players/{playerId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PlayerResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(updateRequest.DisplayName, apiResponse.Data.DisplayName);
        Assert.Equal(updateRequest.Nickname, apiResponse.Data.Nickname);
        Assert.Equal(updateRequest.LeagueHandicap, apiResponse.Data.LeagueHandicap);
    }

    [Fact]
    public async Task UpdatePlayer_WithLeagueMember_ReturnsForbidden()
    {
        // Arrange
        var (leagueId, adminToken, memberToken) = await SetupLeagueWithUsersAsync();

        // Create player as admin
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var createRequest = new CreatePlayerRequest
        {
            Email = "updatetest2@example.com",
            FirstName = "Update",
            LastName = "Test2",
            DisplayName = "Test Player"
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/players", createRequest);
        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<PlayerResponse>>();
        var playerId = createApiResponse!.Data!.Id;

        // Act - Try to update as member
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
        var updateRequest = new UpdatePlayerRequest
        {
            DisplayName = "Hacked Name"
        };
        var response = await _client.PutAsJsonAsync($"/api/v1/leagues/{leagueId}/players/{playerId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RemovePlayer_WithLeagueAdmin_RemovesPlayer()
    {
        // Arrange
        var (leagueId, adminToken, _) = await SetupLeagueWithUsersAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // First create a player
        var createRequest = new CreatePlayerRequest
        {
            Email = "removetest@example.com",
            FirstName = "Remove",
            LastName = "Test",
            DisplayName = "Remove Me"
        };

        var createResponse = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/players", createRequest);
        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<PlayerResponse>>();
        var playerId = createApiResponse!.Data!.Id;

        // Act - Remove the player
        var response = await _client.DeleteAsync($"/api/v1/leagues/{leagueId}/players/{playerId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.True(apiResponse.Data);

        // Verify player is no longer in the list
        var getResponse = await _client.GetAsync($"/api/v1/leagues/{leagueId}/players");
        var getApiResponse = await getResponse.Content.ReadFromJsonAsync<ApiResponse<List<PlayerResponse>>>();
        Assert.NotNull(getApiResponse);
        Assert.DoesNotContain(getApiResponse.Data!, p => p.Id == playerId);
    }

    [Fact]
    public async Task GetPlayer_WithValidId_ReturnsPlayer()
    {
        // Arrange
        var (leagueId, adminToken, memberToken) = await SetupLeagueWithUsersAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Create a player
        var createRequest = new CreatePlayerRequest
        {
            Email = "gettest@example.com",
            FirstName = "Get",
            LastName = "Test",
            DisplayName = "Get Me"
        };

        var createResponse = await _client.PostAsJsonAsync($"/api/v1/leagues/{leagueId}/players", createRequest);
        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<PlayerResponse>>();
        var playerId = createApiResponse!.Data!.Id;

        // Act - Get the player as member
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
        var response = await _client.GetAsync($"/api/v1/leagues/{leagueId}/players/{playerId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PlayerResponse>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(playerId, apiResponse.Data.Id);
        Assert.Equal(createRequest.DisplayName, apiResponse.Data.DisplayName);
    }
}

