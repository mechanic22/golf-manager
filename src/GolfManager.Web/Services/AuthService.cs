using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GolfManager.Web.Services;

/// <summary>
/// Local-user authentication backed by an HttpOnly server cookie.
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly NavigationManager _navigationManager;
    private readonly AppState _appState;

    private AuthResponse? _currentAuth;
    private bool _initialized;

    public AuthService(
        HttpClient httpClient,
        IJSRuntime jsRuntime,
        NavigationManager navigationManager,
        AppState appState)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
        _navigationManager = navigationManager;
        _appState = appState;
    }

    public bool IsAuthenticated => _currentAuth != null;
    public string? UserEmail => _currentAuth?.Email;
    public string? UserId => _currentAuth?.UserId;
    public string? AccessToken => _currentAuth?.AccessToken;
    public bool IsGlobalAdmin => _currentAuth?.IsGlobalAdmin ?? false;

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/register", request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        ApplyAuthResponse(authResponse);
        return authResponse;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            Console.WriteLine($"[AuthService] Attempting login for: {request.Email}");
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", request);
            Console.WriteLine($"[AuthService] Response Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AuthService] Login failed. Status: {response.StatusCode}, Error: {errorContent}");
                return null;
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            ApplyAuthResponse(authResponse);
            return authResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Exception during login: {ex.GetType().Name}");
            Console.WriteLine($"[AuthService] Exception message: {ex.Message}");
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _httpClient.PostAsync("api/v1/auth/logout", null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Logout request failed: {ex.Message}");
        }

        _currentAuth = null;
        _appState.Clear();
        await ClearLegacyTokenStorageAsync();
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await ClearLegacyTokenStorageAsync();

        try
        {
            var authResponse = await _httpClient.GetFromJsonAsync<AuthResponse>("api/v1/auth/me");
            ApplyAuthResponse(authResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] No active local auth cookie: {ex.Message}");
            _currentAuth = null;
            _appState.Clear();
        }
    }

    private void ApplyAuthResponse(AuthResponse? authResponse)
    {
        _currentAuth = authResponse;

        if (authResponse == null)
        {
            _appState.Clear();
            return;
        }

        _appState.SetLeagueMappings(authResponse.LeagueMappings);

        if (string.IsNullOrEmpty(_appState.CurrentLeagueKey))
        {
            _appState.TrySetCurrentLeagueByDomain(new Uri(_navigationManager.Uri).Host);
        }
    }

    private async Task ClearLegacyTokenStorageAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "expiresAt");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userEmail");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userId");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "firstName");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "lastName");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "isGlobalAdmin");
        }
        catch
        {
            // JS interop may be unavailable during very early startup or prerender-like flows.
        }
    }
}
