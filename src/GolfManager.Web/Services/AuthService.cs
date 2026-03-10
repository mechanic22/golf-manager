using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using GolfManager.Shared.DTOs.Auth;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for handling authentication operations
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private const string TokenKey = "authToken";
    private const string UserEmailKey = "userEmail";
    private const string UserIdKey = "userId";

    private AuthResponse? _currentAuth;

    public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public bool IsAuthenticated => _currentAuth != null;
    public string? UserEmail => _currentAuth?.Email;
    public string? UserId => _currentAuth?.UserId;
    public string? AccessToken => _currentAuth?.AccessToken;

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
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", request);
            
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

    public async Task LogoutAsync()
    {
        _currentAuth = null;
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserEmailKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserIdKey);
    }

    public async Task InitializeAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            var email = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", UserEmailKey);
            var userId = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", UserIdKey);

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(userId))
            {
                _currentAuth = new AuthResponse
                {
                    AccessToken = token,
                    Email = email,
                    UserId = userId
                };
            }
        }
        catch
        {
            // LocalStorage not available yet
        }
    }

    private async Task StoreAuthDataAsync(AuthResponse authResponse)
    {
        _currentAuth = authResponse;
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, authResponse.AccessToken);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserEmailKey, authResponse.Email);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserIdKey, authResponse.UserId);
    }
}

