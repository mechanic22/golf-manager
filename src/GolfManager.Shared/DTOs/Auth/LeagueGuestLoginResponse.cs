namespace GolfManager.Shared.DTOs.Auth;

public class LeagueGuestLoginResponse
{
    public string LeagueKey { get; set; } = string.Empty;
    public string LeagueId { get; set; } = string.Empty;
    public string LeagueName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsGuest { get; set; } = true;
}
