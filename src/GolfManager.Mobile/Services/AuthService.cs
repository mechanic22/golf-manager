using CommunityToolkit.Mvvm.ComponentModel;
using GolfManager.Mobile.Configuration;
using GolfManager.Shared.DTOs.Auth;
using GolfManager.Shared.DTOs.Common;
using System.Net.Http.Json;

namespace GolfManager.Mobile.Services;

public partial class AuthService : ObservableObject, IAuthService
{
    private const string AccessTokenKey = "gm_access_token";
    private const string RefreshTokenKey = "gm_refresh_token";
    private const string ExpiresAtKey = "gm_expires_at";

    private const string OAuthCallbackScheme = "dkgolf://oauth-callback";

    private readonly HttpClient _http;
    private readonly AppSettings _settings;

    [ObservableProperty]
    private bool _isAuthenticated;

    public string? AccessToken { get; private set; }

    public AuthService(HttpClient http, AppSettings settings)
    {
        _http = http;
        _settings = settings;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var body = new LoginRequest { Email = email, Password = password };
        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync("/api/v1/auth/login", body);
        }
        catch (HttpRequestException)
        {
            return false; // caller shows the error message
        }

        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        if (result?.Data == null) return false;

        await PersistTokensAsync(result.Data);
        IsAuthenticated = true;
        return true;
    }

    public async Task<bool> LoginWithOAuthAsync(string provider)
    {
        try
        {
            var baseUrl = _settings.ApiBaseUrl.TrimEnd('/');
            var authUrl = $"{baseUrl}/api/v1/auth/mobile/oauth/login?provider={Uri.EscapeDataString(provider)}";

            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(authUrl),
                new Uri(OAuthCallbackScheme));

            if (authResult.Properties.TryGetValue("error", out _))
                return false;

            if (!authResult.Properties.TryGetValue("accessToken", out var accessToken) ||
                !authResult.Properties.TryGetValue("refreshToken", out var refreshToken) ||
                !authResult.Properties.TryGetValue("expiresAt", out var expiresAtStr))
                return false;

            var expiresAt = long.TryParse(expiresAtStr, out var unixSecs)
                ? DateTimeOffset.FromUnixTimeSeconds(unixSecs).UtcDateTime
                : DateTime.UtcNow.AddHours(1);

            await PersistTokensAsync(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt
            });
            IsAuthenticated = true;
            return true;
        }
        catch (TaskCanceledException)
        {
            return false; // user cancelled
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        var accessToken = await SecureStorage.GetAsync(AccessTokenKey);
        var refreshToken = await SecureStorage.GetAsync(RefreshTokenKey);
        var expiresAtStr = await SecureStorage.GetAsync(ExpiresAtKey);

        if (string.IsNullOrEmpty(refreshToken)) return false;

        if (!string.IsNullOrEmpty(accessToken)
            && DateTime.TryParse(expiresAtStr, out var expiresAt)
            && expiresAt > DateTime.UtcNow.AddMinutes(5))
        {
            AccessToken = accessToken;
            IsAuthenticated = true;
            return true;
        }

        return await RefreshAsync(refreshToken);
    }

    private async Task<bool> RefreshAsync(string refreshToken)
    {
        try
        {
            var body = new RefreshTokenRequest { RefreshToken = refreshToken };
            var response = await _http.PostAsJsonAsync("/api/v1/auth/refresh", body);
            if (!response.IsSuccessStatusCode) { await LogoutAsync(); return false; }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
            if (result?.Data == null) return false;

            await PersistTokensAsync(result.Data);
            return true;
        }
        catch
        {
            await LogoutAsync();
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        AccessToken = null;
        IsAuthenticated = false;
        SecureStorage.Remove(AccessTokenKey);
        SecureStorage.Remove(RefreshTokenKey);
        SecureStorage.Remove(ExpiresAtKey);
        await Task.CompletedTask;
    }

    private async Task PersistTokensAsync(AuthResponse auth)
    {
        AccessToken = auth.AccessToken;
        await SecureStorage.SetAsync(AccessTokenKey, auth.AccessToken);
        await SecureStorage.SetAsync(RefreshTokenKey, auth.RefreshToken);
        await SecureStorage.SetAsync(ExpiresAtKey, auth.ExpiresAt.ToString("O"));
    }
}
