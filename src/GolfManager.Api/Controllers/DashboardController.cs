using System.Security.Claims;
using GolfManager.Core.Services;
using GolfManager.Services.Event;
using GolfManager.Services.League;
using GolfManager.Services.Season;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolfManager.Api.Controllers;

[Route("api/v1/dashboard")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ILeagueService _leagueService;
    private readonly ISeasonService _seasonService;
    private readonly IEventService _eventService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ILeagueService leagueService,
        ISeasonService seasonService,
        IEventService eventService,
        ICurrentUserService currentUserService,
        ILogger<DashboardController> logger)
    {
        _leagueService = leagueService;
        _seasonService = seasonService;
        _eventService = eventService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardResponse>>> GetDashboard()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<DashboardResponse>.ErrorResponse("User not authenticated"));

        var leagues = await _leagueService.GetUserLeaguesAsync(userId);

        var seasonByLeagueId = new Dictionary<string, (GolfManager.Shared.DTOs.Season.SeasonResponse? Season, GolfManager.Shared.DTOs.Event.EventResponse? NextEvent)>();
        foreach (var l in leagues.Where(l => !string.IsNullOrEmpty(l.ActiveSeasonId)))
        {
            var season = await _seasonService.GetSeasonByIdAsync(l.ActiveSeasonId!, l.Id);
            if (season == null) { seasonByLeagueId[l.Id] = (null, null); continue; }

            var eventsPage = await _eventService.GetSeasonEventsAsync(l.ActiveSeasonId!, l.Id, pageSize: 100);
            var nextEvent = eventsPage.Items
                .Where(e => !e.IsLocked && e.EventDate.Date >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .FirstOrDefault();

            seasonByLeagueId[l.Id] = (season, nextEvent);
        }

        var leagueItems = leagues.Select(league =>
        {
            seasonByLeagueId.TryGetValue(league.Id, out var data);

            return new DashboardResponse.DashboardLeagueItem
            {
                Id = league.Id,
                Name = league.Name,
                Key = league.Key,
                MemberCount = league.MemberCount,
                IsAdmin = league.IsCurrentUserAdmin,
                ActiveSeason = data.Season == null ? null : new DashboardResponse.ActiveSeasonSummary
                {
                    Id = data.Season.Id,
                    Key = data.Season.Key,
                    Name = data.Season.Name,
                    NextEvent = data.NextEvent == null ? null : new DashboardResponse.NextEventSummary
                    {
                        Id = data.NextEvent.Id,
                        Name = data.NextEvent.Name,
                        EventDate = data.NextEvent.EventDate,
                        NeedsScores = league.IsCurrentUserAdmin && data.NextEvent.EventDate.Date <= DateTime.Today
                    }
                }
            };
        }).ToList();

        var firstName = _currentUserService.FirstName
            ?? _currentUserService.Email?.Split('@').FirstOrDefault()
            ?? "Player";

        var response = new DashboardResponse
        {
            User = new DashboardResponse.UserSummary { FirstName = firstName },
            Leagues = leagueItems
        };

        _logger.LogInformation("Dashboard loaded for user {UserId}: {LeagueCount} leagues", userId, leagues.Count);
        return Ok(ApiResponse<DashboardResponse>.SuccessResponse(response));
    }
}
