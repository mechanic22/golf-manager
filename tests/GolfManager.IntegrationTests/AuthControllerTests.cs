using System.Net;
using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Auth;

namespace GolfManager.IntegrationTests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.Equal(request.Email, authResponse.Email);
        Assert.Equal(request.FirstName, authResponse.FirstName);
        Assert.Equal(request.LastName, authResponse.LastName);
        Assert.False(string.IsNullOrEmpty(authResponse.AccessToken));
        Assert.False(string.IsNullOrEmpty(authResponse.RefreshToken));
        Assert.False(string.IsNullOrEmpty(authResponse.UserId));
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Password = "SecurePassword123!",
            FirstName = "Jane",
            LastName = "Smith"
        };

        // Register first time
        await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Act - Try to register again with same email
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange - First register a user
        var registerRequest = new RegisterRequest
        {
            Email = "login@example.com",
            Password = "SecurePassword123!",
            FirstName = "Login",
            LastName = "Test"
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "login@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.Equal(loginRequest.Email, authResponse.Email);
        Assert.False(string.IsNullOrEmpty(authResponse.AccessToken));
        Assert.False(string.IsNullOrEmpty(authResponse.RefreshToken));
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange - First register a user
        var registerRequest = new RegisterRequest
        {
            Email = "wrongpass@example.com",
            Password = "CorrectPassword123!",
            FirstName = "Wrong",
            LastName = "Pass"
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "wrongpass@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewAuthResponse()
    {
        // Arrange - Register and get initial tokens
        var registerRequest = new RegisterRequest
        {
            Email = "refresh@example.com",
            Password = "SecurePassword123!",
            FirstName = "Refresh",
            LastName = "Test"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var initialAuth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = initialAuth!.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var newAuth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(newAuth);
        Assert.False(string.IsNullOrEmpty(newAuth.AccessToken));
        Assert.False(string.IsNullOrEmpty(newAuth.RefreshToken));
        Assert.NotEqual(initialAuth.RefreshToken, newAuth.RefreshToken); // Token should be rotated
        Assert.Equal(registerRequest.Email, newAuth.Email);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "invalid-refresh-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithValidToken_ReturnsSuccess()
    {
        // Arrange - Register and get tokens
        var registerRequest = new RegisterRequest
        {
            Email = "logout@example.com",
            Password = "SecurePassword123!",
            FirstName = "Logout",
            LastName = "Test"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Add authorization header
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        var logoutRequest = new RefreshTokenRequest
        {
            RefreshToken = authResponse.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/logout", logoutRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify token is revoked by trying to refresh
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", logoutRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task CompleteAuthFlow_RegisterLoginRefreshLogout_WorksCorrectly()
    {
        // 1. Register
        var registerRequest = new RegisterRequest
        {
            Email = "fullflow@example.com",
            Password = "SecurePassword123!",
            FirstName = "Full",
            LastName = "Flow"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        var registerAuth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // 2. Login
        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginAuth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // 3. Refresh Token
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginAuth!.RefreshToken
        };
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshAuth = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // 4. Logout
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", refreshAuth!.AccessToken);

        var logoutRequest = new RefreshTokenRequest
        {
            RefreshToken = refreshAuth.RefreshToken
        };
        var logoutResponse = await _client.PostAsJsonAsync("/api/v1/auth/logout", logoutRequest);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        // Verify all steps worked
        Assert.NotNull(registerAuth);
        Assert.NotNull(loginAuth);
        Assert.NotNull(refreshAuth);
        Assert.NotEqual(loginAuth.RefreshToken, refreshAuth.RefreshToken);
    }
}