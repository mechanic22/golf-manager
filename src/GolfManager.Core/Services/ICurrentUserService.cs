namespace GolfManager.Core.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? FirstName { get; }
    string? LastName { get; }
    bool IsGlobalAdmin { get; }
    List<string> LeagueIds { get; }
    bool IsAuthenticated { get; }
}