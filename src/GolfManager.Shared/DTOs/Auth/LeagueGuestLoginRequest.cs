namespace GolfManager.Shared.DTOs.Auth;

public class LeagueGuestLoginRequest
{
    public string LeagueKey { get; set; } = string.Empty;
    public string? Password { get; set; }
}
