namespace GolfManager.Mobile.Services;

public interface IApiService
{
    Task<T?> GetAsync<T>(string url, string? leagueKey = null);
    Task<T?> PostAsync<T>(string url, object body, string? leagueKey = null);
}
