using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Infrastructure;

/// <summary>
/// HTTP message handler that adds league context header to all API requests
/// </summary>
public class LeagueContextHandler : DelegatingHandler
{
    private readonly AppState _appState;
    private readonly NavigationManager _navigation;
    private readonly ILogger<LeagueContextHandler> _logger;
    
    public LeagueContextHandler(AppState appState, NavigationManager navigation, ILogger<LeagueContextHandler> logger)
    {
        _appState = appState;
        _navigation = navigation;
        _logger = logger;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Auth endpoints must not be tenant-scoped; a stale league context can
        // cause false 403s during login/logout/session checks.
        var requestPath = request.RequestUri?.AbsolutePath ?? string.Empty;
        if (requestPath.StartsWith("/api/v1/auth", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.Remove("X-League-Context");
            return await base.SendAsync(request, cancellationToken);
        }

        // Add league context header from app state, with URL fallback for deep links.
        var leagueKey = _appState.CurrentLeagueKey;

        if (string.IsNullOrEmpty(leagueKey))
        {
            var path = _navigation.ToBaseRelativePath(_navigation.Uri).Trim('/');
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < parts.Length - 1; i++)
            {
                if (!parts[i].Equals("league", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                leagueKey = parts[i + 1].Trim().ToLowerInvariant();
                break;
            }
        }

        if (string.IsNullOrEmpty(leagueKey) && _appState.UserLeagues.Count == 1)
        {
            leagueKey = _appState.UserLeagues[0].LeagueKey;
        }

        if (!string.IsNullOrEmpty(leagueKey))
        {
            request.Headers.Remove("X-League-Context");
            request.Headers.Add("X-League-Context", leagueKey);
            _logger.LogDebug("Adding X-League-Context header: {LeagueKey}", leagueKey);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}
