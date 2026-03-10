namespace GolfManager.Data.Services;

/// <summary>
/// Service for accessing the current tenant (league) context
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets the current league ID (tenant ID) from the HTTP context
    /// Returns null if no tenant context is available
    /// </summary>
    string? GetCurrentLeagueId();

    /// <summary>
    /// Sets the current league ID for the request
    /// </summary>
    void SetCurrentLeagueId(string? leagueId);
}

