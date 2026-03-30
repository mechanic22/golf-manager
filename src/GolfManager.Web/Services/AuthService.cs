using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using GolfManager.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for handling authentication operations
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly IWebAssemblyHostEnvironment _environment;
    private readonly SeedDataService _seedData;
    private const string TokenKey = "authToken";
    private const string RefreshTokenKey = "refreshToken";
    private const string ExpiresAtKey = "expiresAt";
    private const string UserEmailKey = "userEmail";
    private const string UserIdKey = "userId";
    private const string FirstNameKey = "firstName";
    private const string LastNameKey = "lastName";
    private const string IsGlobalAdminKey = "isGlobalAdmin";

    private AuthResponse? _currentAuth;
    private Timer? _refreshTimer;
    private bool _initialized = false;

    public AuthService(HttpClient httpClient, IJSRuntime jsRuntime, IWebAssemblyHostEnvironment environment, SeedDataService seedData)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
        _environment = environment;
        _seedData = seedData;
    }

    // Safe properties that don't trigger initialization
    public bool IsAuthenticated => _initialized && _currentAuth != null;
    public string? UserEmail => _initialized ? _currentAuth?.Email : null;
    public string? UserId => _initialized ? _currentAuth?.UserId : null;
    public string? AccessToken => _initialized ? _currentAuth?.AccessToken : null;
    public bool IsGlobalAdmin => _initialized && (_currentAuth?.IsGlobalAdmin ?? false);

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/register", request);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    await StoreAuthDataAsync(authResponse);
                    return authResponse;
                }
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            Console.WriteLine($"[AuthService] Attempting login for: {request.Email}");

            // In development, use mock authentication with seed data
            if (_environment.IsDevelopment())
            {
                Console.WriteLine($"[AuthService] Using mock authentication (Development mode)");

                var user = _seedData.Users.FirstOrDefault(u =>
                    u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) &&
                    u.Password == request.Password);

                if (user != null)
                {
                    var authResponse = new AuthResponse
                    {
                        AccessToken = $"mock-token-{Guid.NewGuid()}",
                        RefreshToken = $"mock-refresh-{Guid.NewGuid()}",
                        ExpiresAt = DateTime.UtcNow.AddHours(24),
                        UserId = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsGlobalAdmin = user.IsGlobalAdmin
                    };

                    Console.WriteLine($"[AuthService] Mock login successful for: {request.Email}");
                    await StoreAuthDataAsync(authResponse);
                    return authResponse;
                }
                else
                {
                    Console.WriteLine($"[AuthService] Mock login failed - invalid credentials");
                    return null;
                }
            }

            // Production: use real API
            Console.WriteLine($"[AuthService] API Base URL: {_httpClient.BaseAddress}");

            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", request);

            Console.WriteLine($"[AuthService] Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    Console.WriteLine($"[AuthService] Login successful for: {request.Email}");
                    await StoreAuthDataAsync(authResponse);
                    return authResponse;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AuthService] Login failed. Status: {response.StatusCode}, Error: {errorContent}");
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Exception during login: {ex.GetType().Name}");
            Console.WriteLine($"[AuthService] Exception message: {ex.Message}");
            Console.WriteLine($"[AuthService] Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        _currentAuth = null;
        _refreshTimer?.Dispose();
        _refreshTimer = null;

        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", ExpiresAtKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserEmailKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserIdKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", FirstNameKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", LastNameKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", IsGlobalAdminKey);
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            var refreshToken = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);
            var expiresAtStr = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", ExpiresAtKey);
            var email = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", UserEmailKey);
            var userId = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", UserIdKey);
            var firstName = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", FirstNameKey);
            var lastName = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", LastNameKey);
            var isGlobalAdminStr = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", IsGlobalAdminKey);

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(userId))
            {
                var expiresAt = DateTime.TryParse(expiresAtStr, out var exp) ? exp : DateTime.UtcNow;
                var isGlobalAdmin = bool.TryParse(isGlobalAdminStr, out var admin) && admin;

                _currentAuth = new AuthResponse
                {
                    AccessToken = token,
                    RefreshToken = refreshToken ?? string.Empty,
                    ExpiresAt = expiresAt,
                    Email = email,
                    UserId = userId,
                    FirstName = firstName ?? string.Empty,
                    LastName = lastName ?? string.Empty,
                    IsGlobalAdmin = isGlobalAdmin
                };

                // Schedule token refresh
                ScheduleTokenRefresh();
            }

            _initialized = true;
        }
        catch
        {
            // LocalStorage not available yet
            _initialized = true; // Mark as initialized even on failure to prevent retry loops
        }
    }

    private async Task StoreAuthDataAsync(AuthResponse authResponse)
    {
        _currentAuth = authResponse;
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, authResponse.AccessToken);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, authResponse.RefreshToken);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", ExpiresAtKey, authResponse.ExpiresAt.ToString("O"));
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserEmailKey, authResponse.Email);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserIdKey, authResponse.UserId);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", FirstNameKey, authResponse.FirstName);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", LastNameKey, authResponse.LastName);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", IsGlobalAdminKey, authResponse.IsGlobalAdmin.ToString());

        // Schedule token refresh
        ScheduleTokenRefresh();
    }

    private void ScheduleTokenRefresh()
    {
        if (_currentAuth == null || string.IsNullOrEmpty(_currentAuth.RefreshToken))
        {
            return;
        }

        // Cancel existing timer
        _refreshTimer?.Dispose();

        // Calculate time until token expires (refresh 5 minutes before expiration)
        var now = DateTime.UtcNow;
        var expiresAt = _currentAuth.ExpiresAt;
        var refreshAt = expiresAt.AddMinutes(-5);

        var delay = refreshAt - now;

        // If token expires in less than 5 minutes, refresh immediately
        if (delay.TotalSeconds < 0)
        {
            delay = TimeSpan.FromSeconds(5);
        }

        Console.WriteLine($"[AuthService] Token expires at {expiresAt:O}, will refresh in {delay.TotalMinutes:F1} minutes");

        // Schedule refresh
        _refreshTimer = new Timer(async _ => await RefreshTokenAsync(), null, delay, Timeout.InfiniteTimeSpan);
    }

    private async Task RefreshTokenAsync()
    {
        try
        {
            if (_currentAuth == null || string.IsNullOrEmpty(_currentAuth.RefreshToken))
            {
                Console.WriteLine("[AuthService] ❌ Cannot refresh - no refresh token available");
                Console.WriteLine($"[AuthService] _currentAuth is null: {_currentAuth == null}");
                return;
            }

            Console.WriteLine("[AuthService] 🔄 Refreshing access token...");
            Console.WriteLine($"[AuthService] Current token expires at: {_currentAuth.ExpiresAt:O}");
            Console.WriteLine($"[AuthService] Current time: {DateTime.UtcNow:O}");
            Console.WriteLine($"[AuthService] Refresh token (first 10 chars): {_currentAuth.RefreshToken.Substring(0, Math.Min(10, _currentAuth.RefreshToken.Length))}...");

            var request = new RefreshTokenRequest
            {
                RefreshToken = _currentAuth.RefreshToken
            };

            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/refresh", request);

            Console.WriteLine($"[AuthService] Refresh response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    Console.WriteLine("[AuthService] ✅ Token refreshed successfully");
                    Console.WriteLine($"[AuthService] New token expires at: {authResponse.ExpiresAt:O}");
                    await StoreAuthDataAsync(authResponse);
                }
                else
                {
                    Console.WriteLine("[AuthService] ❌ Token refresh failed - empty response from server");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AuthService] ❌ Token refresh failed!");
                Console.WriteLine($"[AuthService] Status Code: {response.StatusCode}");
                Console.WriteLine($"[AuthService] Error Response: {errorContent}");
                Console.WriteLine($"[AuthService] ⚠️ LOGGING OUT USER DUE TO REFRESH FAILURE");

                // If refresh fails, log out the user
                await LogoutAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] ❌ Exception during token refresh!");
            Console.WriteLine($"[AuthService] Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"[AuthService] Exception Message: {ex.Message}");
            Console.WriteLine($"[AuthService] Stack Trace: {ex.StackTrace}");
            Console.WriteLine($"[AuthService] ⚠️ LOGGING OUT USER DUE TO EXCEPTION");

            // If refresh fails, log out the user
            await LogoutAsync();
        }
    }
}

