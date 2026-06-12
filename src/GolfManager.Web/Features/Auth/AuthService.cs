using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Auth;
using GolfManager.Shared.DTOs.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GolfManager.Web.Features.Auth;

/// <summary>
/// Local-user authentication backed by an HttpOnly server cookie.
/// Supports both regular user login and guest league access.
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly NavigationManager _navigationManager;
    private readonly AppState _appState;

    private AuthResponse? _currentAuth;
    private bool _initialized;
    private bool _isGuest;
    private string? _guestLeagueKey;
    private string? _guestLeagueId;

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
    public string? UserFirstName => _currentAuth?.FirstName;
    public string? UserLastName => _currentAuth?.LastName;
    public string? UserId => _currentAuth?.UserId;
    public string? AccessToken => _currentAuth?.AccessToken;
    public bool IsGlobalAdmin => _currentAuth?.IsGlobalAdmin ?? false;
    public bool IsGuest => _isGuest;
    public string? GuestLeagueKey => _guestLeagueKey;
    public string? GuestLeagueId => _guestLeagueId;

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

            var wrappedResponse = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
            var authResponse = wrappedResponse?.Data;
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

    public async Task<bool> LoginAsGuestAsync(string leagueKey, string password)
    {
        try
        {
            Console.WriteLine($"[AuthService] Attempting guest login for league: {leagueKey}");
            var request = new LeagueGuestLoginRequest { LeagueKey = leagueKey, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/league-guest-login", request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[AuthService] Guest login failed. Status: {response.StatusCode}");
                return false;
            }

            var wrappedResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LeagueGuestLoginResponse>>();
            if (wrappedResponse?.Data == null)
            {
                return false;
            }

            _isGuest = true;
            _guestLeagueKey = leagueKey;
            _guestLeagueId = wrappedResponse.Data.LeagueId;
            _appState.SetCurrentLeague(leagueKey);

            Console.WriteLine($"[AuthService] Guest login successful for league: {leagueKey}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Exception during guest login: {ex.Message}");
            return false;
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
        _isGuest = false;
        _guestLeagueKey = null;
        _guestLeagueId = null;
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
            var wrappedResponse = await _httpClient.GetFromJsonAsync<ApiResponse<AuthResponse>>("api/v1/auth/me");
            var authResponse = wrappedResponse?.Data;

            if (authResponse?.IsGuest == true)
            {
                _isGuest = true;
                _guestLeagueKey = authResponse.GuestLeagueKey;
                _guestLeagueId = authResponse.GuestLeagueId;
                Console.WriteLine($"[AuthService] Initialized as guest for league: {_guestLeagueKey}");
            }
            else if (authResponse != null)
            {
                _isGuest = false;
                _guestLeagueKey = null;
                _guestLeagueId = null;
            }

            ApplyAuthResponse(authResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] No active local auth cookie: {ex.Message}");
            _currentAuth = null;
            _isGuest = false;
            _guestLeagueKey = null;
            _guestLeagueId = null;
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
