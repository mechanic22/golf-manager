using GolfManager.Shared.DTOs.League;

namespace GolfManager.Mobile.Services;

public class LeagueService : ILeagueService
{
    private readonly IApiService _api;

    public LeagueService(IApiService api) => _api = api;

    public Task<List<LeagueResponse>?> GetLeaguesAsync()
        => _api.GetAsync<List<LeagueResponse>>("/api/v1/leagues");
}
