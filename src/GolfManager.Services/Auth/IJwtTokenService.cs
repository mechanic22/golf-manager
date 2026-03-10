using GolfManager.Core.Entities;

namespace GolfManager.Services.Auth;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, List<string>? leagueIds = null);
    string GenerateRefreshToken();
    string? ValidateToken(string token);
}

