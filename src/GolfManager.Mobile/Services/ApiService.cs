using GolfManager.Shared.DTOs.Common;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GolfManager.Mobile.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _http;
    private readonly IAuthService _auth;

    public ApiService(HttpClient http, IAuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<T?> GetAsync<T>(string url, string? leagueKey = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddHeaders(request, leagueKey);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        return wrapper is { Success: true } ? wrapper.Data : default;
    }

    public async Task<T?> PostAsync<T>(string url, object body, string? leagueKey = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body)
        };
        AddHeaders(request, leagueKey);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        return wrapper is { Success: true } ? wrapper.Data : default;
    }

    private void AddHeaders(HttpRequestMessage request, string? leagueKey)
    {
        if (!string.IsNullOrEmpty(_auth.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.AccessToken);

        if (!string.IsNullOrEmpty(leagueKey))
            request.Headers.Add("X-League-Context", leagueKey);
    }
}
