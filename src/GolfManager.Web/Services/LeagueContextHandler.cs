namespace GolfManager.Web.Services;

/// <summary>
/// HTTP message handler that adds league context header to all API requests
/// </summary>
public class LeagueContextHandler : DelegatingHandler
{
    private readonly AppState _appState;
    private readonly ILogger<LeagueContextHandler> _logger;
    
    public LeagueContextHandler(AppState appState, ILogger<LeagueContextHandler> logger)
    {
        _appState = appState;
        _logger = logger;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Add league context header if user has selected a league
        if (!string.IsNullOrEmpty(_appState.CurrentLeagueKey))
        {
            request.Headers.Add("X-League-Context", _appState.CurrentLeagueKey);
            _logger.LogDebug("Adding X-League-Context header: {LeagueKey}", _appState.CurrentLeagueKey);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}
