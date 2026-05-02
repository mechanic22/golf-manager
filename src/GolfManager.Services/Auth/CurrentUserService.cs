using System.Security.Claims;
using GolfManager.Core.Services;
using Microsoft.AspNetCore.Http;

namespace GolfManager.Services.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? FirstName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.GivenName)?.Value;

    public string? LastName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Surname)?.Value;

    public bool IsGlobalAdmin
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("is_global_admin")?.Value;
            return claim != null && bool.Parse(claim);
        }
    }

    public List<string> LeagueIds
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User?.FindAll("league_id");
            return claims?.Select(c => c.Value).ToList() ?? new List<string>();
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}

