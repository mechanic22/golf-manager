using GolfManager.Shared.DTOs.League;

namespace GolfManager.Web.Infrastructure;

/// <summary>
/// Service for checking user permissions and authorization
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IAuthService _authService;
    private readonly ILeagueService _leagueService;
    private readonly ILogger<AuthorizationService> _logger;

    private List<LeagueResponse> _userLeagues = new();
    private bool _initialized = false;

    public AuthorizationService(
        IAuthService authService,
        ILeagueService leagueService,
        ILogger<AuthorizationService> logger)
    {
        _authService = authService;
        _leagueService = leagueService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        if (_initialized || !_authService.IsAuthenticated)
        {
            return;
        }

        try
        {
            _logger.LogInformation("[AuthorizationService] Initializing - loading user leagues");
            
            var response = await _leagueService.GetUserLeaguesAsync();
            if (response?.Data != null)
            {
                _userLeagues = response.Data;
                _logger.LogInformation("[AuthorizationService] Loaded {Count} leagues for user", _userLeagues.Count);
            }

            _initialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AuthorizationService] Failed to initialize");
            _initialized = true; // Mark as initialized to prevent retry loops
        }
    }

    public bool IsGlobalAdmin()
    {
        return _authService.IsGlobalAdmin;
    }

    public bool IsLeagueAdmin(string leagueId)
    {
        if (!_initialized || string.IsNullOrEmpty(leagueId))
        {
            return false;
        }

        var league = _userLeagues.FirstOrDefault(l => l.Id == leagueId);
        return league?.IsCurrentUserAdmin ?? false;
    }

    public bool IsLeagueMember(string leagueId)
    {
        if (!_initialized || string.IsNullOrEmpty(leagueId))
        {
            return false;
        }

        return _userLeagues.Any(l => l.Id == leagueId);
    }

    public bool CanManageLeague(string leagueId)
    {
        return IsGlobalAdmin() || IsLeagueAdmin(leagueId);
    }

    public bool CanViewLeague(string leagueId)
    {
        return IsGlobalAdmin() || IsLeagueMember(leagueId);
    }

    public bool CanManageSeason(string leagueId, string seasonId)
    {
        // For now: Global Admin OR League Admin
        // Future: Add Season Admin role check
        return IsGlobalAdmin() || IsLeagueAdmin(leagueId);
    }

    public bool CanEnterScores(string leagueId, string seasonId)
    {
        // For now: Global Admin OR League Admin
        // Future: Add Season Admin and Moderator role checks
        return IsGlobalAdmin() || IsLeagueAdmin(leagueId);
    }

    public bool CanViewSeason(string leagueId, string seasonId)
    {
        return IsGlobalAdmin() || IsLeagueMember(leagueId);
    }

    public List<string> GetUserLeagueIds()
    {
        if (!_initialized)
        {
            return new List<string>();
        }

        return _userLeagues.Select(l => l.Id).ToList();
    }

    public List<string> GetUserAdminLeagueIds()
    {
        if (!_initialized)
        {
            return new List<string>();
        }

        return _userLeagues
            .Where(l => l.IsCurrentUserAdmin)
            .Select(l => l.Id)
            .ToList();
    }
}

