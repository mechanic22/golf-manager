namespace GolfManager.Shared.DTOs.League;

/// <summary>
/// Password payload for validating anonymous/public league access.
/// </summary>
public class VerifyAnonymousAccessRequest
{
    /// <summary>
    /// Plain-text password entered by the anonymous/public viewer.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
