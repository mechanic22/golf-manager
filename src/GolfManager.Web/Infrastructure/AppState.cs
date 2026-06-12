using GolfManager.Shared.DTOs.Auth;

namespace GolfManager.Web.Infrastructure;

/// <summary>
/// Application state service for managing league context
/// </summary>
public class AppState
{
    /// <summary>
    /// Event fired when league context changes
    /// </summary>
    public event Action? OnChange;
    
    /// <summary>
    /// User's league memberships with domain mappings
    /// </summary>
    public List<LeagueMappingResponse> UserLeagues { get; private set; } = new();
    
    /// <summary>
    /// Domain to league key mapping (for current user)
    /// </summary>
    public Dictionary<string, string> DomainToLeagueMap { get; private set; } = new();
    
    /// <summary>
    /// Current league key (set based on domain or user selection)
    /// </summary>
    public string? CurrentLeagueKey { get; private set; }
    
    /// <summary>
    /// Current league ID
    /// </summary>
    public string? CurrentLeagueId { get; private set; }
    
    /// <summary>
    /// Current league name
    /// </summary>
    public string? CurrentLeagueName { get; private set; }
    
    /// <summary>
    /// Is user a league admin for current league
    /// </summary>
    public bool IsCurrentLeagueAdmin { get; private set; }
    
    /// <summary>
    /// Set league mappings (called after login)
    /// </summary>
    public void SetLeagueMappings(List<LeagueMappingResponse> mappings)
    {
        UserLeagues = mappings;
        DomainToLeagueMap.Clear();
        
        foreach (var mapping in mappings)
        {
            if (!string.IsNullOrWhiteSpace(mapping.CustomDomain))
            {
                var normalizedDomain = mapping.CustomDomain.Trim().ToLowerInvariant();
                DomainToLeagueMap[normalizedDomain] = mapping.LeagueKey;
            }
        }
        
        NotifyStateChanged();
    }
    
    /// <summary>
    /// Set current league context from a custom domain host
    /// </summary>
    public bool TrySetCurrentLeagueByDomain(string? domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return false;
        }

        var normalizedDomain = domain.Trim().ToLowerInvariant();
        if (DomainToLeagueMap.TryGetValue(normalizedDomain, out var leagueKey))
        {
            SetCurrentLeague(leagueKey);
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Set current league context
    /// </summary>
    public void SetCurrentLeague(string? leagueKey)
    {
        var normalizedInput = string.IsNullOrWhiteSpace(leagueKey)
            ? null
            : leagueKey.Trim().ToLowerInvariant();

        if (normalizedInput == CurrentLeagueKey)
            return;
            
        CurrentLeagueKey = normalizedInput;
        
        if (!string.IsNullOrEmpty(normalizedInput))
        {
                var league = UserLeagues.FirstOrDefault(l => 
                l.LeagueKey.ToLowerInvariant() == normalizedInput);
            if (league != null)
            {
                CurrentLeagueId = league.LeagueId;
                CurrentLeagueName = league.LeagueName;
                IsCurrentLeagueAdmin = league.IsLeagueAdmin;
            }
        }
        else
        {
            CurrentLeagueId = null;
            CurrentLeagueName = null;
            IsCurrentLeagueAdmin = false;
        }
        
        NotifyStateChanged();
    }
    
    /// <summary>
    /// Get league key from domain
    /// </summary>
    public string? GetLeagueKeyFromDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return null;
        }

        return DomainToLeagueMap.TryGetValue(domain.Trim().ToLowerInvariant(), out var leagueKey) ? leagueKey : null;
    }
    
    /// <summary>
    /// Clear all state (on logout)
    /// </summary>
    public void Clear()
    {
        UserLeagues.Clear();
        DomainToLeagueMap.Clear();
        CurrentLeagueKey = null;
        CurrentLeagueId = null;
        CurrentLeagueName = null;
        IsCurrentLeagueAdmin = false;
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}
