using GolfManager.Shared.DTOs.League;

namespace GolfManager.Mobile.Services;

public interface ILeagueService
{
    Task<List<LeagueResponse>?> GetLeaguesAsync();
}
