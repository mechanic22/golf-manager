namespace GolfManager.Mobile.Services;

public class RoundService : IRoundService
{
    private readonly IApiService _api;

    public RoundService(IApiService api) => _api = api;
}
