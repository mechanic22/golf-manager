using GolfManager.Api.Authorization;
using GolfManager.Core.Services;
using GolfManager.Services.Event;
using GolfManager.Services.Player;
using GolfManager.Services.Season;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Player;
using GolfManager.Shared.DTOs.Season;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolfManager.Api.Controllers;

/// <summary>
/// Controller for managing seasons
/// </summary>
[Route("api/v1/seasons")]
public class SeasonsController : BaseLeagueController
{
    private readonly ISeasonService _seasonService;
    private readonly ISeasonSettingsService _seasonSettingsService;
    private readonly IPlayerService _playerService;
    private readonly IEventService _eventService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SeasonsController> _logger;

    public SeasonsController(
        ISeasonService seasonService,
        ISeasonSettingsService seasonSettingsService,
        IPlayerService playerService,
        IEventService eventService,
        ICurrentUserService currentUserService,
        ILogger<SeasonsController> logger)
    {
        _seasonService = seasonService;
        _playerService = playerService;
        _seasonSettingsService = seasonSettingsService;
        _eventService = eventService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get all seasons for a league
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<SeasonResponse>>>> GetLeagueSeasons()
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<List<SeasonResponse>>.ErrorResponse("League context required"));
        }

        var seasons = await _seasonService.GetLeagueSeasonsAsync(leagueId);
        return Ok(ApiResponse<List<SeasonResponse>>.SuccessResponse(seasons));
    }

    /// <summary>
    /// Get a season by ID
    /// </summary>
    [HttpGet("{seasonId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SeasonResponse>>> GetSeasonById(string seasonId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<SeasonResponse>.ErrorResponse("League context required"));
        }

        var season = await _seasonService.GetSeasonByIdAsync(seasonId, leagueId);

        if (season == null)
        {
            return NotFound(ApiResponse<SeasonResponse>.ErrorResponse("Season not found"));
        }

        return Ok(ApiResponse<SeasonResponse>.SuccessResponse(season));
    }

    /// <summary>
    /// Get a season by key
    /// </summary>
    [HttpGet("by-key/{seasonKey}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SeasonResponse>>> GetSeasonByKey(string seasonKey)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<SeasonResponse>.ErrorResponse("League context required"));
        }

        var season = await _seasonService.GetSeasonByKeyAsync(seasonKey, leagueId);

        if (season == null)
        {
            return NotFound(ApiResponse<SeasonResponse>.ErrorResponse("Season not found"));
        }

        return Ok(ApiResponse<SeasonResponse>.SuccessResponse(season));
    }

    /// <summary>
    /// Create a new season
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<SeasonResponse>>> CreateSeason([FromBody] CreateSeasonRequest request)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<SeasonResponse>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;
        var season = await _seasonService.CreateSeasonAsync(request, leagueId, userId);

        _logger.LogInformation("Season {SeasonKey} created in league {LeagueId} by user {UserId}",
            season.Key, leagueId, userId);

        return CreatedAtAction(
            nameof(GetSeasonById),
            new { seasonId = season.Id },
            ApiResponse<SeasonResponse>.SuccessResponse(season));
    }

    /// <summary>
    /// Update a season
    /// </summary>
    [HttpPut("{seasonId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<SeasonResponse>>> UpdateSeason(
        string seasonId,
        [FromBody] UpdateSeasonRequest request)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<SeasonResponse>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;
        var season = await _seasonService.UpdateSeasonAsync(seasonId, request, leagueId, userId);

        _logger.LogInformation("Season {SeasonId} updated in league {LeagueId} by user {UserId}",
            seasonId, leagueId, userId);

        return Ok(ApiResponse<SeasonResponse>.SuccessResponse(season));
    }

    /// <summary>
    /// Delete a season
    /// </summary>
    [HttpDelete("{seasonId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSeason(string seasonId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;
        var result = await _seasonService.DeleteSeasonAsync(seasonId, leagueId, userId);

        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Season not found"));
        }

        _logger.LogInformation("Season {SeasonId} deleted from league {LeagueId} by user {UserId}",
            seasonId, leagueId, userId);

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    /// <summary>
    /// Bulk configure a season from pasted calendar and team roster text.
    /// </summary>
    [HttpPost("{seasonId}/setup")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<SeasonSetupResponse>>> SetupSeason(
        string seasonId,
        [FromBody] SeasonSetupRequest request)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<SeasonSetupResponse>.ErrorResponse("League context required"));
        }

        try
        {
            var userId = _currentUserService.UserId!;
            var result = await _seasonService.SetupSeasonAsync(seasonId, request, leagueId, userId);

            _logger.LogInformation("Season {SeasonId} bulk setup completed in league {LeagueId} by user {UserId}",
                seasonId, leagueId, userId);

            return Ok(ApiResponse<SeasonSetupResponse>.SuccessResponse(result, "Season setup completed"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SeasonSetupResponse>.ErrorResponse(ex.Message));
        }
    }

    #region Season Settings

    /// <summary>
    /// Get settings for a season
    /// </summary>
    [HttpGet("{seasonId}/settings")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<SeasonSettingsResponse>>> GetSeasonSettings(string seasonId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<SeasonSettingsResponse>.ErrorResponse("League context required"));
        }

        var settings = await _seasonSettingsService.GetSeasonSettingsAsync(seasonId, leagueId);

        if (settings == null)
        {
            return NotFound(ApiResponse<SeasonSettingsResponse>.ErrorResponse("Season settings not found"));
        }

        return Ok(ApiResponse<SeasonSettingsResponse>.SuccessResponse(settings));
    }

    /// <summary>
    /// Update season settings
    /// </summary>
    [HttpPut("{seasonId}/settings")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<SeasonSettingsResponse>>> UpdateSeasonSettings(
        string seasonId,
        [FromBody] UpdateSeasonSettingsRequest request)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<SeasonSettingsResponse>.ErrorResponse("League context required"));
        }

        try
        {
            var settings = await _seasonSettingsService.UpdateSeasonSettingsAsync(seasonId, leagueId, request);

            _logger.LogInformation("Season settings updated for season {SeasonId} in league {LeagueId}",
                seasonId, leagueId);

            return Ok(ApiResponse<SeasonSettingsResponse>.SuccessResponse(settings));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SeasonSettingsResponse>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Create default settings for a season
    /// </summary>
    [HttpPost("{seasonId}/settings/default")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<SeasonSettingsResponse>>> CreateDefaultSettings(string seasonId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<SeasonSettingsResponse>.ErrorResponse("League context required"));
        }

        try
        {
            var settings = await _seasonSettingsService.CreateDefaultSettingsAsync(seasonId, leagueId);

            _logger.LogInformation("Default settings created for season {SeasonId} in league {LeagueId}",
                seasonId, leagueId);

            return CreatedAtAction(
                nameof(GetSeasonSettings),
                new { seasonId },
                ApiResponse<SeasonSettingsResponse>.SuccessResponse(settings));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SeasonSettingsResponse>.ErrorResponse(ex.Message));
        }
    }

    #endregion

    #region Season Players

    /// <summary>
    /// Get all players participating in a season
    /// </summary>
    [HttpGet("{seasonId}/players")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<PlayerResponse>>>> GetSeasonPlayers(string seasonId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<List<PlayerResponse>>.ErrorResponse("League context required"));
        }

        var players = await _playerService.GetSeasonPlayersAsync(seasonId, leagueId);
        return Ok(ApiResponse<List<PlayerResponse>>.SuccessResponse(players, $"Retrieved {players.Count} season players"));
    }

    /// <summary>
    /// Add a player to a season (and league membership if needed)
    /// </summary>
    [HttpPost("{seasonId}/players")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<PlayerResponse>>> AddSeasonPlayer(
        string seasonId,
        [FromBody] CreatePlayerRequest request)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<PlayerResponse>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;
        var player = await _playerService.AddPlayerToSeasonAsync(seasonId, leagueId, request, userId);

        return Ok(ApiResponse<PlayerResponse>.SuccessResponse(player, "Player added to season"));
    }

    /// <summary>
    /// Remove a player from a season
    /// </summary>
    [HttpDelete("{seasonId}/players/{seasonGolferId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveSeasonPlayer(string seasonId, string seasonGolferId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<bool>.ErrorResponse("League context required"));

        var userId = _currentUserService.UserId!;
        var result = await _seasonService.RemovePlayerFromSeasonAsync(seasonId, seasonGolferId, leagueId, userId);

        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Player not found in season"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Player removed from season"));
    }

    /// <summary>
    /// Mark a season player as paid/unpaid.
    /// </summary>
    [HttpPut("{seasonId}/players/{seasonGolferId}/payment")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSeasonPlayerPayment(
        string seasonId,
        string seasonGolferId,
        [FromBody] UpdateSeasonPlayerPaymentRequest request)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<bool>.ErrorResponse("League context required"));

        try
        {
            var userId = _currentUserService.UserId!;
            await _seasonService.UpdateSeasonPlayerPaymentAsync(seasonId, seasonGolferId, request, leagueId, userId);
            var message = request.IsPaidForSeason ? "Player marked paid" : "Player marked unpaid";
            return Ok(ApiResponse<bool>.SuccessResponse(true, message));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Assign (or unassign) a season player to a team
    /// </summary>
    [HttpPut("{seasonId}/players/{seasonGolferId}/team")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> AssignPlayerToTeam(
        string seasonId,
        string seasonGolferId,
        [FromBody] AssignPlayerToTeamRequest request)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<bool>.ErrorResponse("League context required"));

        try
        {
            var userId = _currentUserService.UserId!;
            await _seasonService.AssignPlayerToTeamAsync(seasonId, seasonGolferId, request, leagueId, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Player assigned to team"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    #endregion

    #region Season Teams

    /// <summary>
    /// Get all teams in a season
    /// </summary>
    [HttpGet("{seasonId}/teams")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<SeasonTeamResponse>>>> GetSeasonTeams(string seasonId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<List<SeasonTeamResponse>>.ErrorResponse("League context required"));

        var teams = await _seasonService.GetSeasonTeamsAsync(seasonId, leagueId);
        return Ok(ApiResponse<List<SeasonTeamResponse>>.SuccessResponse(teams));
    }

    /// <summary>
    /// Create a team in a season
    /// </summary>
    [HttpPost("{seasonId}/teams")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<SeasonTeamResponse>>> CreateSeasonTeam(
        string seasonId,
        [FromBody] CreateSeasonTeamRequest request)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<SeasonTeamResponse>.ErrorResponse("League context required"));

        try
        {
            var userId = _currentUserService.UserId!;
            var team = await _seasonService.CreateSeasonTeamAsync(seasonId, request, leagueId, userId);
            return Ok(ApiResponse<SeasonTeamResponse>.SuccessResponse(team, "Team created"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SeasonTeamResponse>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Update a team in a season
    /// </summary>
    [HttpPut("{seasonId}/teams/{teamId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<SeasonTeamResponse>>> UpdateSeasonTeam(
        string seasonId,
        string teamId,
        [FromBody] UpdateSeasonTeamRequest request)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<SeasonTeamResponse>.ErrorResponse("League context required"));

        try
        {
            var userId = _currentUserService.UserId!;
            var team = await _seasonService.UpdateSeasonTeamAsync(seasonId, teamId, request, leagueId, userId);
            return Ok(ApiResponse<SeasonTeamResponse>.SuccessResponse(team, "Team updated"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SeasonTeamResponse>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Delete a team from a season
    /// </summary>
    [HttpDelete("{seasonId}/teams/{teamId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSeasonTeam(string seasonId, string teamId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<bool>.ErrorResponse("League context required"));

        var userId = _currentUserService.UserId!;
        var result = await _seasonService.DeleteSeasonTeamAsync(seasonId, teamId, leagueId, userId);

        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Team not found"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Team deleted"));
    }

    #endregion

    #region Stats

    [HttpGet("{seasonId}/golfers/{leagueGolferId}/hole-stats")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PlayerSeasonHoleStatsResponse>>> GetPlayerSeasonHoleStats(string seasonId, string leagueGolferId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<PlayerSeasonHoleStatsResponse>.ErrorResponse("League context required"));

        var result = await _seasonService.GetPlayerSeasonHoleStatsAsync(seasonId, leagueGolferId, leagueId);
        return Ok(ApiResponse<PlayerSeasonHoleStatsResponse>.SuccessResponse(result ?? new PlayerSeasonHoleStatsResponse()));
    }

    [HttpGet("golfers/{leagueGolferId}/career-hole-stats")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PlayerSeasonHoleStatsResponse>>> GetPlayerCareerHoleStats(string leagueGolferId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<PlayerSeasonHoleStatsResponse>.ErrorResponse("League context required"));

        var result = await _seasonService.GetPlayerCareerHoleStatsAsync(leagueGolferId, leagueId);
        return Ok(ApiResponse<PlayerSeasonHoleStatsResponse>.SuccessResponse(result ?? new PlayerSeasonHoleStatsResponse()));
    }

    #endregion

    #region Standings

    [HttpGet("{seasonId}/standings")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<PlayerStandingResponse>>>> GetSeasonStandings(string seasonId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<List<PlayerStandingResponse>>.ErrorResponse("League context required"));

        var playersTask = _playerService.GetSeasonPlayersAsync(seasonId, leagueId);
        var eventsTask = _eventService.GetSeasonEventsAsync(seasonId, leagueId, pageSize: 100);

        await Task.WhenAll(playersTask, eventsTask);

        var players = playersTask.Result;
        var events = eventsTask.Result.Items;

        // Load all event scoreboards in parallel (server-side, no HTTP latency)
        var scoreboardTasks = events
            .Select(e => _eventService.GetEventScoreboardAsync(seasonId, e.Id, leagueId));
        var scoreboards = await Task.WhenAll(scoreboardTasks);

        var scoreboardPlayers = scoreboards.SelectMany(s => s.Players).ToList();

        var standings = players.Select(player =>
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

        return Ok(ApiResponse<List<PlayerStandingResponse>>.SuccessResponse(standings));
    }

    #endregion
}

