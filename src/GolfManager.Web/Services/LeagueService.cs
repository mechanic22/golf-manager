using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.League;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for handling league operations
/// </summary>
public class LeagueService : ILeagueService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public LeagueService(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<ApiResponse<LeagueResponse>?> CreateLeagueAsync(CreateLeagueRequest request)
    {
        try
        {
            AddAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/v1/leagues", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<List<LeagueResponse>>?> GetUserLeaguesAsync()
    {
        try
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync("api/v1/leagues");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<LeagueResponse>>>();
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<LeagueResponse>?> GetLeagueByKeyAsync(string key)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/leagues/by-key/{key}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    private void AddAuthHeader()
    {
        if (_authService.IsAuthenticated && !string.IsNullOrEmpty(_authService.AccessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.AccessToken);
        }
    }
}

