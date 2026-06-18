using System.Security.Claims;
using GolfManager.Api.Authorization;
using GolfManager.Data;
using GolfManager.Services.Event;
using GolfManager.Services.League;
using GolfManager.Services.Player;
using GolfManager.Core.Entities;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.League;
using GolfManager.Shared.DTOs.Player;
using GolfManager.Shared.DTOs.Season;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfManager.Api.Controllers;

/// <summary>
/// Controller for managing leagues
/// </summary>
[ApiController]
[Route("api/v1/leagues")]
[Authorize]
public class LeaguesController : ControllerBase
{
    private readonly ILeagueService _leagueService;
    private readonly IPlayerService _playerService;
    private readonly IEventService _eventService;
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<LeaguesController> _logger;

    public LeaguesController(
        ILeagueService leagueService,
        IPlayerService playerService,
        IEventService eventService,
        GolfManagerDbContext context,
        ILogger<LeaguesController> logger)
    {
        _leagueService = leagueService;
        _playerService = playerService;
        _eventService = eventService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all leagues for the current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<LeagueResponse>>>> GetUserLeagues()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<List<LeagueResponse>>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        var leagues = await _leagueService.GetUserLeaguesAsync(userId);
        return Ok(ApiResponse<List<LeagueResponse>>.SuccessResponse(leagues, $"Retrieved {leagues.Count} leagues"));
    }

    /// <summary>
    /// Get a specific league by ID
    /// </summary>
    [HttpGet("{leagueId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<LeagueResponse>>> GetLeagueById(string leagueId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var league = await _leagueService.GetLeagueByIdAsync(leagueId, userId);

        if (league == null)
        {
            return NotFound(ApiResponse<LeagueResponse>.ErrorResponse("League not found", $"League with ID {leagueId} not found"));
        }

        return Ok(ApiResponse<LeagueResponse>.SuccessResponse(league, "League retrieved successfully"));
    }

    /// <summary>
    /// Discover publicly listed leagues (anonymous, opt-in leagues only)
    /// </summary>
    [HttpGet("discover")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<LeagueResponse>>>> DiscoverLeagues([FromQuery] string? search = null)
    {
        var leagues = await _leagueService.GetPublicLeaguesAsync(search);
        return Ok(ApiResponse<List<LeagueResponse>>.SuccessResponse(leagues, $"Found {leagues.Count} leagues"));
    }

    /// <summary>
    /// Get a specific league by key
    /// </summary>
    [HttpGet("by-key/{leagueKey}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LeagueResponse>>> GetLeagueByKey(
        string leagueKey,
        [FromQuery] string? anonymousAccessPassword = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(anonymousAccessPassword)
            && Request.Headers.TryGetValue("X-League-Anonymous-Password", out var headerPassword))
        {
            anonymousAccessPassword = headerPassword.FirstOrDefault();
        }

        var league = await _leagueService.GetLeagueByKeyAsync(leagueKey, userId, anonymousAccessPassword);

        if (league == null)
        {
            return NotFound(ApiResponse<LeagueResponse>.ErrorResponse("League not found", $"League with key '{leagueKey}' not found"));
        }

        return Ok(ApiResponse<LeagueResponse>.SuccessResponse(league, "League retrieved successfully"));
    }

    /// <summary>
    /// Get team and individual standings for the active season.
    /// Accepts both guest sessions (league from cookie claims) and regular league members
    /// (league from X-League-Context middleware).
    /// </summary>
    [HttpGet("guest/standings")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<GuestStandingsResponse>>> GetGuestStandings()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isGuest = User.FindFirst(AuthorizationConstants.Claims.IsGuest)?.Value == "true";

        string? leagueId;
        if (isGuest)
        {
            leagueId = User.FindFirst(AuthorizationConstants.Claims.LeagueId)?.Value;
        }
        else
        {
            leagueId = HttpContext.Items["LeagueId"]?.ToString();
        }

        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<GuestStandingsResponse>.ErrorResponse("No league context"));

        var league = await _context.Leagues
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == leagueId && !l.IsDeleted);

        if (league == null)
            return NotFound(ApiResponse<GuestStandingsResponse>.ErrorResponse("League not found"));

        // Prefer the league's pinned active season; fall back to the most recent season.
        var seasonId = league.ActiveSeasonId;
        if (string.IsNullOrEmpty(seasonId))
        {
            seasonId = await _context.Seasons
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(s => s.LeagueId == leagueId && !s.IsDeleted)
                .OrderByDescending(s => s.StartDate)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();
        }

        if (string.IsNullOrEmpty(seasonId))
        {
            return Ok(ApiResponse<GuestStandingsResponse>.SuccessResponse(new GuestStandingsResponse
            {
                LeagueName = league.Name,
                LogoUrl = league.LogoUrl
            }));
        }

        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == seasonId && !s.IsDeleted);

        var teamsTask = _context.SeasonTeams
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.SeasonId == seasonId && t.LeagueId == leagueId && !t.IsDeleted)
            .ToListAsync();

        // Compute all standings from live scoreboard data (not stale denormalized fields).
        var playersTask = _playerService.GetSeasonPlayersAsync(seasonId, leagueId);
        var eventsTask = _eventService.GetSeasonEventsAsync(seasonId, leagueId, pageSize: 100);

        await Task.WhenAll(teamsTask, playersTask, eventsTask);

        var players = playersTask.Result;
        var events = eventsTask.Result.Items;

        var scoreboardTasks = events
            .Select(e => _eventService.GetEventScoreboardAsync(seasonId, e.Id, leagueId));
        var scoreboards = await Task.WhenAll(scoreboardTasks);

        // Aggregate team wins/losses/ties/points from completed match results.
        var teamPts = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var teamWins = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var teamLosses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var teamTies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var match in scoreboards.SelectMany(s => s.Matches).Where(m => m.IsComplete))
        {
            var hp = match.HomePoints ?? 0;
            var ap = match.AwayPoints ?? 0;

            if (!string.IsNullOrEmpty(match.HomeTeamId))
            {
                teamPts[match.HomeTeamId] = teamPts.GetValueOrDefault(match.HomeTeamId) + hp;
                if (hp > ap) teamWins[match.HomeTeamId] = teamWins.GetValueOrDefault(match.HomeTeamId) + 1;
                else if (hp < ap) teamLosses[match.HomeTeamId] = teamLosses.GetValueOrDefault(match.HomeTeamId) + 1;
                else teamTies[match.HomeTeamId] = teamTies.GetValueOrDefault(match.HomeTeamId) + 1;
            }
            if (!string.IsNullOrEmpty(match.AwayTeamId))
            {
                teamPts[match.AwayTeamId] = teamPts.GetValueOrDefault(match.AwayTeamId) + ap;
                if (ap > hp) teamWins[match.AwayTeamId] = teamWins.GetValueOrDefault(match.AwayTeamId) + 1;
                else if (ap < hp) teamLosses[match.AwayTeamId] = teamLosses.GetValueOrDefault(match.AwayTeamId) + 1;
                else teamTies[match.AwayTeamId] = teamTies.GetValueOrDefault(match.AwayTeamId) + 1;
            }
        }

        var teamRows = teamsTask.Result
            .Select(t =>
            {
                var livePts = teamPts.GetValueOrDefault(t.Id);
                return new GuestTeamStandingRow
                {
                    Name = t.Name,
                    Wins = teamWins.GetValueOrDefault(t.Id),
                    Losses = teamLosses.GetValueOrDefault(t.Id),
                    Ties = teamTies.GetValueOrDefault(t.Id),
                    // Fall back to denormalized SeasonPoints if no live scoreboard data yet.
                    SeasonPoints = livePts > 0 ? livePts : t.SeasonPoints
                };
            })
            .OrderByDescending(t => t.SeasonPoints ?? 0)
            .ThenByDescending(t => t.Wins)
            .ThenBy(t => t.Losses)
            .ToList();
        var scoreboardPlayers = scoreboards.SelectMany(s => s.Players).ToList();

        var playerRows = players.Select(player =>
        {
            var playerScores = scoreboardPlayers
                .Where(sp => string.Equals(sp.SeasonGolferId, player.SeasonGolferId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var completedRounds = playerScores.Where(sp => sp.RawScore.HasValue).ToList();
            var hasPoints = playerScores.Any(sp => sp.EventPoints.HasValue);
            var totalPoints = hasPoints ? playerScores.Sum(sp => sp.EventPoints ?? 0) : (double?)null;

            return new PlayerStandingResponse
            {
                SeasonGolferId = player.SeasonGolferId ?? player.Id,
                LeagueGolferId = player.Id,
                DisplayName = player.DisplayName,
                LeagueHandicap = player.LeagueHandicap,
                SeasonPoints = totalPoints,
                RoundCount = completedRounds.Count,
                AverageNetScore = completedRounds.Count > 0
                    ? completedRounds.Average(sp => sp.NetScore ?? sp.RawScore ?? 0)
                    : null,
                BestRawScore = completedRounds.Count > 0
                    ? completedRounds.Min(sp => sp.RawScore ?? int.MaxValue)
                    : null
            };
        })
        .OrderByDescending(s => s.SeasonPoints ?? double.MinValue)
        .ThenBy(s => s.AverageNetScore ?? double.MaxValue)
        .ThenBy(s => s.DisplayName)
        .ToList();

        // For logged-in members, resolve their SeasonGolferId for row highlighting in the UI.
        string? currentUserSeasonGolferId = null;
        if (!isGuest && !string.IsNullOrEmpty(userId))
        {
            var golfer = await _context.Golfers
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (golfer != null)
            {
                currentUserSeasonGolferId = players
                    .FirstOrDefault(p => string.Equals(p.GolferId, golfer.Id, StringComparison.OrdinalIgnoreCase))
                    ?.SeasonGolferId;
            }
        }

        _logger.LogInformation("Standings served for league {LeagueId} (guest={IsGuest})", leagueId, isGuest);

        return Ok(ApiResponse<GuestStandingsResponse>.SuccessResponse(new GuestStandingsResponse
        {
            LeagueName = league.Name,
            LogoUrl = league.LogoUrl,
            SeasonName = season?.Name,
            Teams = teamRows,
            Players = playerRows,
            CurrentUserSeasonGolferId = currentUserSeasonGolferId
        }));
    }

    /// <summary>
    /// Get season events with matchup scores for guests and league members.
    /// </summary>
    [HttpGet("guest/events")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<GuestEventsResponse>>> GetGuestEvents()
    {
        var isGuest = User.FindFirst(AuthorizationConstants.Claims.IsGuest)?.Value == "true";
        string? leagueId = isGuest
            ? User.FindFirst(AuthorizationConstants.Claims.LeagueId)?.Value
            : HttpContext.Items["LeagueId"]?.ToString();

        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<GuestEventsResponse>.ErrorResponse("No league context"));

        var league = await _context.Leagues
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == leagueId && !l.IsDeleted);

        if (league == null)
            return NotFound(ApiResponse<GuestEventsResponse>.ErrorResponse("League not found"));

        var seasonId = league.ActiveSeasonId;
        if (string.IsNullOrEmpty(seasonId))
        {
            seasonId = await _context.Seasons
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(s => s.LeagueId == leagueId && !s.IsDeleted)
                .OrderByDescending(s => s.StartDate)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();
        }

        if (string.IsNullOrEmpty(seasonId))
        {
            return Ok(ApiResponse<GuestEventsResponse>.SuccessResponse(new GuestEventsResponse
            {
                LeagueName = league.Name
            }));
        }

        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == seasonId && !s.IsDeleted);

        var eventsResult = await _eventService.GetSeasonEventsAsync(seasonId, leagueId, pageSize: 100);
        var eventItems = eventsResult.Items.OrderByDescending(e => e.EventDate).ToList();

        var scoreboardTasks = eventItems
            .Select(e => _eventService.GetEventScoreboardAsync(seasonId, e.Id, leagueId));
        var scoreboards = await Task.WhenAll(scoreboardTasks);

        var eventRows = eventItems.Select((ev, idx) =>
        {
            var scoreboard = scoreboards[idx];
            var matchups = scoreboard.Matches.Select(m => new GuestMatchupRow
            {
                Id = m.MatchupId,
                HomeTeamName = m.HomeTeamName,
                AwayTeamName = m.AwayTeamName,
                HomePoints = m.HomePoints,
                AwayPoints = m.AwayPoints,
                IsComplete = m.IsComplete,
                HomeMembers = m.HomeMembers.Select(p => new GuestMatchupMemberRow
                {
                    DisplayName = p.DisplayName,
                    Handicap = p.Handicap,
                    RawScore = p.RawScore,
                    NetScore = p.NetScore,
                    IsSubstitute = p.IsSubstitute
                }).ToList(),
                AwayMembers = m.AwayMembers.Select(p => new GuestMatchupMemberRow
                {
                    DisplayName = p.DisplayName,
                    Handicap = p.Handicap,
                    RawScore = p.RawScore,
                    NetScore = p.NetScore,
                    IsSubstitute = p.IsSubstitute
                }).ToList()
            }).ToList();

            return new GuestEventRow
            {
                Id = ev.Id,
                Name = ev.Name,
                EventDate = ev.EventDate,
                IsComplete = ev.IsLocked,
                Matchups = matchups
            };
        }).ToList();

        return Ok(ApiResponse<GuestEventsResponse>.SuccessResponse(new GuestEventsResponse
        {
            LeagueName = league.Name,
            SeasonName = season?.Name,
            Events = eventRows
        }));
    }

    /// <summary>
    /// Get matchup details for a single event — accessible to guests and members.
    /// </summary>
    [HttpGet("guest/events/{eventId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<GuestEventRow>>> GetGuestEventDetail(string eventId)
    {
        var isGuest = User.FindFirst(AuthorizationConstants.Claims.IsGuest)?.Value == "true";
        string? leagueId = isGuest
            ? User.FindFirst(AuthorizationConstants.Claims.LeagueId)?.Value
            : HttpContext.Items["LeagueId"]?.ToString();

        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<GuestEventRow>.ErrorResponse("No league context"));

        var ev = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId && e.LeagueId == leagueId && !e.IsDeleted);

        if (ev == null)
            return NotFound(ApiResponse<GuestEventRow>.ErrorResponse("Event not found"));

        var scoreboard = await _eventService.GetEventScoreboardAsync(ev.SeasonId, eventId, leagueId);

        var matchups = scoreboard.Matches.Select(m => new GuestMatchupRow
        {
            Id = m.MatchupId,
            HomeTeamName = m.HomeTeamName,
            AwayTeamName = m.AwayTeamName,
            HomePoints = m.HomePoints,
            AwayPoints = m.AwayPoints,
            IsComplete = m.IsComplete,
            HomeMembers = m.HomeMembers.Select(p => new GuestMatchupMemberRow
            {
                DisplayName = p.DisplayName,
                Handicap = p.Handicap,
                RawScore = p.RawScore,
                NetScore = p.NetScore,
                IsSubstitute = p.IsSubstitute
            }).ToList(),
            AwayMembers = m.AwayMembers.Select(p => new GuestMatchupMemberRow
            {
                DisplayName = p.DisplayName,
                Handicap = p.Handicap,
                RawScore = p.RawScore,
                NetScore = p.NetScore,
                IsSubstitute = p.IsSubstitute
            }).ToList()
        }).ToList();

        return Ok(ApiResponse<GuestEventRow>.SuccessResponse(new GuestEventRow
        {
            Id = ev.Id,
            Name = ev.Name,
            EventDate = ev.EventDate,
            IsComplete = ev.IsLocked,
            Matchups = matchups
        }));
    }

    /// <summary>
    /// Get hole-by-hole match detail for a specific matchup — accessible to guests and members.
    /// </summary>
    [HttpGet("guest/matchups/{matchupId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<MatchDetailResponse>>> GetGuestMatchDetail(string matchupId)
    {
        var isGuest = User.FindFirst(AuthorizationConstants.Claims.IsGuest)?.Value == "true";
        string? leagueId = isGuest
            ? User.FindFirst(AuthorizationConstants.Claims.LeagueId)?.Value
            : HttpContext.Items["LeagueId"]?.ToString();

        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<MatchDetailResponse>.ErrorResponse("No league context"));

        var match = await _context.Set<SeasonEventMatch>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(m => m.SeasonEvent)
            .FirstOrDefaultAsync(m => m.Id == matchupId && m.LeagueId == leagueId && !m.IsDeleted);

        if (match == null)
            return NotFound(ApiResponse<MatchDetailResponse>.ErrorResponse("Matchup not found"));

        var detail = await _eventService.GetMatchDetailAsync(
            match.SeasonEvent.SeasonId,
            match.SeasonEventId,
            matchupId,
            leagueId);

        if (detail == null)
            return NotFound(ApiResponse<MatchDetailResponse>.ErrorResponse("Match detail not found"));

        return Ok(ApiResponse<MatchDetailResponse>.SuccessResponse(detail));
    }

    /// <summary>
    /// Verify anonymous/public access password for a league.
    /// </summary>
    [HttpPost("by-key/{leagueKey}/anonymous-access")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyAnonymousAccess(
        string leagueKey,
        [FromBody] VerifyAnonymousAccessRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Password is required"));
        }

        var isValid = await _leagueService.VerifyAnonymousAccessAsync(leagueKey, request.Password);
        if (!isValid)
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse("Invalid password"));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Anonymous access verified"));
    }

    /// <summary>
    /// Create a new league
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<LeagueResponse>>> CreateLeague([FromBody] CreateLeagueRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<LeagueResponse>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        try
        {
            var league = await _leagueService.CreateLeagueAsync(request, userId);
            _logger.LogInformation("League {LeagueKey} created by user {UserId}", league.Key, userId);

            return CreatedAtAction(
                nameof(GetLeagueById),
                new { leagueId = league.Id },
                ApiResponse<LeagueResponse>.SuccessResponse(league, "League created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LeagueResponse>.ErrorResponse("Invalid operation", ex.Message));
        }
    }

    /// <summary>
    /// Update a league
    /// </summary>
    [HttpPut("{leagueId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<LeagueResponse>>> UpdateLeague(string leagueId, [FromBody] UpdateLeagueRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<LeagueResponse>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        try
        {
            var league = await _leagueService.UpdateLeagueAsync(leagueId, request, userId);
            _logger.LogInformation("League {LeagueId} updated by user {UserId}", leagueId, userId);

            return Ok(ApiResponse<LeagueResponse>.SuccessResponse(league, "League updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LeagueResponse>.ErrorResponse("League not found", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LeagueResponse>.ErrorResponse("Invalid operation", ex.Message));
        }
    }

    /// <summary>
    /// Verify a league custom domain
    /// </summary>
    [HttpPost("{leagueId}/custom-domain/verify")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<LeagueResponse>>> VerifyLeagueCustomDomain(string leagueId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<LeagueResponse>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        try
        {
            var league = await _leagueService.VerifyCustomDomainAsync(leagueId, userId);
            return Ok(ApiResponse<LeagueResponse>.SuccessResponse(league, "Custom domain verified successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LeagueResponse>.ErrorResponse("League not found", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LeagueResponse>.ErrorResponse("Domain verification failed", ex.Message));
        }
    }

    /// <summary>
    /// Delete a league
    /// </summary>
    [HttpDelete("{leagueId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteLeague(string leagueId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        var result = await _leagueService.DeleteLeagueAsync(leagueId, userId);

        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("League not found", $"League with ID {leagueId} not found"));
        }

        _logger.LogInformation("League {LeagueId} deleted by user {UserId}", leagueId, userId);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "League deleted successfully"));
    }

    /// <summary>
    /// Get all members of a league
    /// </summary>
    [HttpGet("{leagueId}/members")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<List<LeagueMemberResponse>>>> GetLeagueMembers(string leagueId)
    {
        var members = await _leagueService.GetLeagueMembersAsync(leagueId);
        return Ok(ApiResponse<List<LeagueMemberResponse>>.SuccessResponse(members, $"Retrieved {members.Count} members"));
    }

    /// <summary>
    /// Add a member to a league
    /// </summary>
    [HttpPost("{leagueId}/members")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<LeagueMemberResponse>>> AddLeagueMember(string leagueId, [FromBody] AddLeagueMemberRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<LeagueMemberResponse>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        try
        {
            var member = await _leagueService.AddLeagueMemberAsync(leagueId, request, userId);
            _logger.LogInformation("User {Email} added to league {LeagueId} by {UserId}", request.Email, leagueId, userId);
            return Ok(ApiResponse<LeagueMemberResponse>.SuccessResponse(member, "Member added successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LeagueMemberResponse>.ErrorResponse("Not found", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LeagueMemberResponse>.ErrorResponse("Invalid operation", ex.Message));
        }
    }

    /// <summary>
    /// Remove a member from a league
    /// </summary>
    [HttpDelete("{leagueId}/members/{memberId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveLeagueMember(string leagueId, string memberId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        try
        {
            var result = await _leagueService.RemoveLeagueMemberAsync(leagueId, memberId, userId);

            if (!result)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Member not found", $"Member {memberId} not found in league {leagueId}"));
            }

            _logger.LogInformation("User {MemberId} removed from league {LeagueId} by {UserId}", memberId, leagueId, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Member removed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid operation", ex.Message));
        }
    }

    /// <summary>
    /// Update a league member's role
    /// </summary>
    [HttpPut("{leagueId}/members/{memberId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<LeagueMemberResponse>>> UpdateLeagueMember(string leagueId, string memberId, [FromBody] UpdateLeagueMemberRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<LeagueMemberResponse>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        try
        {
            var member = await _leagueService.UpdateLeagueMemberAsync(leagueId, memberId, request, userId);
            _logger.LogInformation("User {MemberId} role updated in league {LeagueId} by {UserId}", memberId, leagueId, userId);
            return Ok(ApiResponse<LeagueMemberResponse>.SuccessResponse(member, "Member role updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LeagueMemberResponse>.ErrorResponse("Not found", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LeagueMemberResponse>.ErrorResponse("Invalid operation", ex.Message));
        }
    }
}

