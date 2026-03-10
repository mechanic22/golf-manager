using Microsoft.AspNetCore.Http;

namespace GolfManager.Data.Services;

/// <summary>
/// Implementation of ITenantService that extracts tenant ID from HTTP context
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string? _currentLeagueId;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCurrentLeagueId()
    {
        // If explicitly set, use that
        if (_currentLeagueId != null)
            return _currentLeagueId;

        // Try to get from HTTP context claims (JWT token)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            // Look for league_id claim in JWT
            var leagueIdClaim = httpContext.User.FindFirst("league_id");
            if (leagueIdClaim != null)
                return leagueIdClaim.Value;

            // Alternative: Look for custom header (for API calls)
            if (httpContext.Request.Headers.TryGetValue("X-League-Id", out var leagueIdHeader))
                return leagueIdHeader.FirstOrDefault();
        }

        return null;
    }

    public void SetCurrentLeagueId(string? leagueId)
    {
        _currentLeagueId = leagueId;
    }
}

