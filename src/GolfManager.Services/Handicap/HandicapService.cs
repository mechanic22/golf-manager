using GolfManager.Core.Entities;
using GolfManager.Data;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Handicap;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.Handicap;

/// <summary>
/// Service for managing golfer handicaps
/// </summary>
public class HandicapService : IHandicapService
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<HandicapService> _logger;

    public HandicapService(GolfManagerDbContext context, ILogger<HandicapService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<HandicapHistoryResponse>>> GetHandicapHistoryAsync(
        string golferId, 
        string? leagueId = null, 
        string? seasonId = null,
        int limit = 50)
    {
        try
        {
            var query = _context.HandicapHistories
                .Where(h => h.GolferId == golferId && !h.IsDeleted);

            // Filter by scope
            if (seasonId != null)
            {
                // Season-specific handicap
                query = query.Where(h => h.SeasonId == seasonId);
            }
            else if (leagueId != null)
            {
                // League-specific handicap (but not season-specific)
                query = query.Where(h => h.LeagueId == leagueId && h.SeasonId == null);
            }
            else
            {
                // Global handicap only
                query = query.Where(h => h.LeagueId == null && h.SeasonId == null);
            }

            var history = await query
                .OrderByDescending(h => h.EffectiveDate)
                .Take(limit)
                .Select(h => new HandicapHistoryResponse
                {
                    Id = h.Id,
                    GolferId = h.GolferId,
                    LeagueId = h.LeagueId,
                    SeasonId = h.SeasonId,
                    HandicapIndex = h.HandicapIndex,
                    EffectiveDate = h.EffectiveDate,
                    CalculationMethod = h.CalculationMethod,
                    RoundsUsed = h.RoundsUsed,
                    Notes = h.Notes,
                    CreatedAt = h.CreatedAt,
                    CreatedBy = h.CreatedBy
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} handicap history records for golfer {GolferId}", 
                history.Count, golferId);

            return ApiResponse<List<HandicapHistoryResponse>>.SuccessResponse(
                history, 
                $"Retrieved {history.Count} handicap history records");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving handicap history for golfer {GolferId}", golferId);
            return ApiResponse<List<HandicapHistoryResponse>>.ErrorResponse(
                "Failed to retrieve handicap history", 
                ex.Message);
        }
    }

    public async Task<ApiResponse<HandicapHistoryResponse>> CreateHandicapAsync(
        string golferId, 
        CreateHandicapRequest request,
        string currentUserId)
    {
        try
        {
            // Verify golfer exists
            var golfer = await _context.Golfers
                .FirstOrDefaultAsync(g => g.Id == golferId && !g.IsDeleted);

            if (golfer == null)
            {
                return ApiResponse<HandicapHistoryResponse>.ErrorResponse("Golfer not found");
            }

            var effectiveDate = request.EffectiveDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            // Create handicap history entry
            var history = new HandicapHistory
            {
                Id = Guid.NewGuid().ToString(),
                GolferId = golferId,
                LeagueId = request.LeagueId,
                SeasonId = request.SeasonId,
                HandicapIndex = request.HandicapIndex,
                EffectiveDate = effectiveDate,
                CalculationMethod = request.CalculationMethod ?? "Manual",
                RoundsUsed = request.RoundsUsed,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            _context.HandicapHistories.Add(history);

            // Update the current handicap in the appropriate entity
            await UpdateCurrentHandicapAsync(golferId, request.LeagueId, request.SeasonId, 
                request.HandicapIndex, currentUserId);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Created handicap {Handicap} for golfer {GolferId} (League: {LeagueId}, Season: {SeasonId})",
                request.HandicapIndex, golferId, request.LeagueId ?? "null", request.SeasonId ?? "null");

            var response = new HandicapHistoryResponse
            {
                Id = history.Id,
                GolferId = history.GolferId,
                LeagueId = history.LeagueId,
                SeasonId = history.SeasonId,
                HandicapIndex = history.HandicapIndex,
                EffectiveDate = history.EffectiveDate,
                CalculationMethod = history.CalculationMethod,
                RoundsUsed = history.RoundsUsed,
                Notes = history.Notes,
                CreatedAt = history.CreatedAt,
                CreatedBy = history.CreatedBy
            };

            return ApiResponse<HandicapHistoryResponse>.SuccessResponse(
                response, 
                "Handicap created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating handicap for golfer {GolferId}", golferId);
            return ApiResponse<HandicapHistoryResponse>.ErrorResponse(
                "Failed to create handicap", 
                ex.Message);
        }
    }

    public async Task<double?> GetCurrentHandicapAsync(
        string golferId,
        string? leagueId = null,
        string? seasonId = null)
    {
        try
        {
            if (seasonId != null)
            {
                // Get season handicap
                var seasonGolfer = await _context.SeasonGolfers
                    .FirstOrDefaultAsync(sg => sg.GolferId == golferId && sg.SeasonId == seasonId && !sg.IsDeleted);
                return seasonGolfer?.SeasonHandicap;
            }
            else if (leagueId != null)
            {
                // Get league handicap
                var leagueGolfer = await _context.LeagueGolfers
                    .FirstOrDefaultAsync(lg => lg.GolferId == golferId && lg.LeagueId == leagueId && !lg.IsDeleted);
                return leagueGolfer?.LeagueHandicap;
            }
            else
            {
                // Get global handicap
                var golfer = await _context.Golfers
                    .FirstOrDefaultAsync(g => g.Id == golferId && !g.IsDeleted);
                return golfer?.GlobalHandicap;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current handicap for golfer {GolferId}", golferId);
            return null;
        }
    }

    /// <summary>
    /// Update the current handicap in the appropriate entity (Golfer, LeagueGolfer, or SeasonGolfer)
    /// </summary>
    private async Task UpdateCurrentHandicapAsync(
        string golferId,
        string? leagueId,
        string? seasonId,
        double handicapIndex,
        string currentUserId)
    {
        var now = DateTime.UtcNow;

        if (seasonId != null)
        {
            // Update season handicap
            var seasonGolfer = await _context.SeasonGolfers
                .FirstOrDefaultAsync(sg => sg.GolferId == golferId && sg.SeasonId == seasonId && !sg.IsDeleted);

            if (seasonGolfer != null)
            {
                seasonGolfer.SeasonHandicap = handicapIndex;
                seasonGolfer.UpdatedAt = now;
                seasonGolfer.UpdatedBy = currentUserId;
            }
        }
        else if (leagueId != null)
        {
            // Update league handicap
            var leagueGolfer = await _context.LeagueGolfers
                .FirstOrDefaultAsync(lg => lg.GolferId == golferId && lg.LeagueId == leagueId && !lg.IsDeleted);

            if (leagueGolfer != null)
            {
                leagueGolfer.LeagueHandicap = handicapIndex;
                leagueGolfer.LeagueHandicapUpdatedAt = now;
                leagueGolfer.UpdatedAt = now;
                leagueGolfer.UpdatedBy = currentUserId;
            }
        }
        else
        {
            // Update global handicap
            var golfer = await _context.Golfers
                .FirstOrDefaultAsync(g => g.Id == golferId && !g.IsDeleted);

            if (golfer != null)
            {
                golfer.GlobalHandicap = handicapIndex;
                golfer.GlobalHandicapUpdatedAt = now;
                golfer.UpdatedAt = now;
                golfer.UpdatedBy = currentUserId;
            }
        }
    }
}
