using GolfManager.Api.Authorization;
using GolfManager.Services.Auth;
using GolfManager.Services.Season;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Season;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolfManager.Api.Controllers;

/// <summary>
/// Controller for managing seasons
/// </summary>
[ApiController]
[Route("api/v1/leagues/{leagueId}/seasons")]
[Authorize]
public class SeasonsController : ControllerBase
{
    private readonly ISeasonService _seasonService;
    private readonly ISeasonSettingsService _seasonSettingsService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SeasonsController> _logger;

    public SeasonsController(
        ISeasonService seasonService,
        ISeasonSettingsService seasonSettingsService,
        ICurrentUserService currentUserService,
        ILogger<SeasonsController> logger)
    {
        _seasonService = seasonService;
        _seasonSettingsService = seasonSettingsService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get all seasons for a league
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<List<SeasonResponse>>>> GetLeagueSeasons(string leagueId)
    {
        var seasons = await _seasonService.GetLeagueSeasonsAsync(leagueId);
        return Ok(ApiResponse<List<SeasonResponse>>.SuccessResponse(seasons));
    }

    /// <summary>
    /// Get a season by ID
    /// </summary>
    [HttpGet("{seasonId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<SeasonResponse>>> GetSeasonById(string leagueId, string seasonId)
    {
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
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<SeasonResponse>>> GetSeasonByKey(string leagueId, string seasonKey)
    {
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
    public async Task<ActionResult<ApiResponse<SeasonResponse>>> CreateSeason(string leagueId, [FromBody] CreateSeasonRequest request)
    {
        var userId = _currentUserService.UserId!;
        var season = await _seasonService.CreateSeasonAsync(request, leagueId, userId);

        _logger.LogInformation("Season {SeasonKey} created in league {LeagueId} by user {UserId}",
            season.Key, leagueId, userId);

        return CreatedAtAction(
            nameof(GetSeasonById),
            new { leagueId, seasonId = season.Id },
            ApiResponse<SeasonResponse>.SuccessResponse(season));
    }

    /// <summary>
    /// Update a season
    /// </summary>
    [HttpPut("{seasonId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<SeasonResponse>>> UpdateSeason(
        string leagueId,
        string seasonId,
        [FromBody] UpdateSeasonRequest request)
    {
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
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSeason(string leagueId, string seasonId)
    {
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

    #region Season Settings

    /// <summary>
    /// Get settings for a season
    /// </summary>
    [HttpGet("{seasonId}/settings")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<SeasonSettingsResponse>>> GetSeasonSettings(string leagueId, string seasonId)
    {
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
        string leagueId,
        string seasonId,
        [FromBody] UpdateSeasonSettingsRequest request)
    {
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
    public async Task<ActionResult<ApiResponse<SeasonSettingsResponse>>> CreateDefaultSettings(string leagueId, string seasonId)
    {
        try
        {
            var settings = await _seasonSettingsService.CreateDefaultSettingsAsync(seasonId, leagueId);

            _logger.LogInformation("Default settings created for season {SeasonId} in league {LeagueId}",
                seasonId, leagueId);

            return CreatedAtAction(
                nameof(GetSeasonSettings),
                new { leagueId, seasonId },
                ApiResponse<SeasonSettingsResponse>.SuccessResponse(settings));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SeasonSettingsResponse>.ErrorResponse(ex.Message));
        }
    }

    #endregion
}

