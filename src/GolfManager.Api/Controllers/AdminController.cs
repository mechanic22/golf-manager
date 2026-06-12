using GolfManager.Api.Authorization;
using GolfManager.Data;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.League;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfManager.Api.Controllers;

/// <summary>
/// Admin utility endpoints (Global Admin only)
/// </summary>
[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = AuthorizationConstants.Policies.GlobalAdmin)]
public class AdminController : ControllerBase
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(GolfManagerDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Recalculate TotalRounds, AverageScore, and BestScore for all LeagueGolfers
    /// from actual Round data. Useful after importing historical data.
    /// </summary>
    [HttpPost("recalculate-player-stats")]
    public async Task<ActionResult<ApiResponse<object>>> RecalculatePlayerStats()
    {
        _logger.LogInformation("Admin: Recalculating player stats from rounds...");

        var roundStats = await _context.Rounds
            .IgnoreQueryFilters()
            .Where(r => r.LeagueGolferId != null && r.TotalScore != null)
            .GroupBy(r => r.LeagueGolferId!)
            .Select(g => new
            {
                LeagueGolferId = g.Key,
                Count = g.Count(),
                Average = g.Average(r => (double)r.TotalScore!.Value),
                Best = g.Min(r => r.TotalScore!.Value)
            })
            .ToDictionaryAsync(x => x.LeagueGolferId);

        var leagueGolfers = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .ToListAsync();

        int updated = 0;
        foreach (var lg in leagueGolfers)
        {
            if (roundStats.TryGetValue(lg.Id, out var stats))
            {
                lg.TotalRounds = stats.Count;
                lg.AverageScore = stats.Average;
                lg.BestScore = stats.Best;
                updated++;
            }
            else
            {
                lg.TotalRounds = 0;
                lg.AverageScore = null;
                lg.BestScore = null;
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin: Updated stats for {Updated}/{Total} players", updated, leagueGolfers.Count);

        return Ok(ApiResponse<object>.SuccessResponse(
            new { Updated = updated, Total = leagueGolfers.Count },
            $"Recalculated stats for {updated} of {leagueGolfers.Count} players"));
    }

    /// <summary>
    /// Get all leagues on the platform (Global Admin only)
    /// </summary>
    [HttpGet("leagues")]
    public async Task<ActionResult<ApiResponse<List<LeagueResponse>>>> GetAllLeagues([FromQuery] string? search = null)
    {
        var query = _context.Leagues.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(l => l.Name.ToLower().Contains(lower) || l.Key.ToLower().Contains(lower));
        }

        var leagues = await query
            .OrderBy(l => l.Name)
            .Select(l => new LeagueResponse
            {
                Id = l.Id,
                Key = l.Key,
                Name = l.Name,
                Description = l.Description,
                LogoUrl = l.LogoUrl,
                ActiveSeasonId = l.ActiveSeasonId,
                MemberCount = _context.UserLeagues.Count(ul => ul.LeagueId == l.Id && !ul.IsDeleted),
                SeasonCount = _context.Seasons.Count(s => s.LeagueId == l.Id && !s.IsDeleted),
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<LeagueResponse>>.SuccessResponse(leagues, $"Found {leagues.Count} leagues"));
    }
}
