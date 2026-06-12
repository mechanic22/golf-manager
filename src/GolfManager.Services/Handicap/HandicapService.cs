using GolfManager.Core.Common;
using GolfManager.Core.Entities;
using GolfManager.Data;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Handicap;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoundEntity = GolfManager.Core.Entities.Round;

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
        var query = _context.HandicapHistories
            .Where(h => h.GolferId == golferId);

        if (seasonId != null)
            query = query.Where(h => h.SeasonId == seasonId);
        else if (leagueId != null)
            query = query.Where(h => h.LeagueId == leagueId && h.SeasonId == null);
        else
            query = query.Where(h => h.LeagueId == null && h.SeasonId == null);

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

    public async Task<ApiResponse<HandicapHistoryResponse>> CreateHandicapAsync(
        string golferId,
        CreateHandicapRequest request,
        string currentUserId)
    {
        var golfer = await _context.Golfers.FirstOrDefaultAsync(g => g.Id == golferId);
        if (golfer == null)
            return ApiResponse<HandicapHistoryResponse>.ErrorResponse("Golfer not found");

        var effectiveDate = request.EffectiveDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

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
        await UpdateCurrentHandicapAsync(golferId, request.LeagueId, request.SeasonId, request.HandicapIndex, currentUserId);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created handicap {Handicap} for golfer {GolferId} (League: {LeagueId}, Season: {SeasonId})",
            request.HandicapIndex, golferId, request.LeagueId ?? "null", request.SeasonId ?? "null");

        return ApiResponse<HandicapHistoryResponse>.SuccessResponse(new HandicapHistoryResponse
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
        }, "Handicap created successfully");
    }

    public async Task<double?> GetCurrentHandicapAsync(
        string golferId,
        string? leagueId = null,
        string? seasonId = null)
    {
        if (seasonId != null)
        {
            var seasonGolfer = await _context.SeasonGolfers
                .FirstOrDefaultAsync(sg => sg.GolferId == golferId && sg.SeasonId == seasonId);
            return seasonGolfer?.SeasonHandicap;
        }

        if (leagueId != null)
        {
            var leagueGolfer = await _context.LeagueGolfers
                .FirstOrDefaultAsync(lg => lg.GolferId == golferId && lg.LeagueId == leagueId);
            return leagueGolfer?.LeagueHandicap;
        }

        var golfer = await _context.Golfers.FirstOrDefaultAsync(g => g.Id == golferId);
        return golfer?.GlobalHandicap;
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
                .FirstOrDefaultAsync(sg => sg.GolferId == golferId && sg.SeasonId == seasonId);

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
                .FirstOrDefaultAsync(lg => lg.GolferId == golferId && lg.LeagueId == leagueId);

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
                .FirstOrDefaultAsync(g => g.Id == golferId);

            if (golfer != null)
            {
                golfer.GlobalHandicap = handicapIndex;
                golfer.GlobalHandicapUpdatedAt = now;
                golfer.UpdatedAt = now;
                golfer.UpdatedBy = currentUserId;
            }
        }
    }

    // ── Handicap calculation ─────────────────────────────────────────────────

    public async Task<ApiResponse<HandicapCalculationResponse>> CalculateHandicapAsync(
        string golferId,
        CalculateHandicapRequest request,
        string currentUserId)
    {
        if (!await _context.Golfers.AnyAsync(g => g.Id == golferId))
            return ApiResponse<HandicapCalculationResponse>.ErrorResponse("Golfer not found");

        if (request.Method == HandicapCalculationMethod.Scratch)
        {
            var scratchResult = new HandicapCalculationResponse
            {
                GolferId = golferId,
                HandicapIndex = 0,
                Method = HandicapCalculationMethod.Scratch,
                RoundsUsed = 0,
                RoundsConsidered = 0,
                Notes = "Scratch golfer — handicap index is 0",
                Persisted = false
            };

            if (request.Persist)
            {
                await PersistCalculatedHandicapAsync(golferId, 0, request, currentUserId, 0);
                scratchResult.Persisted = true;
            }

            return ApiResponse<HandicapCalculationResponse>.SuccessResponse(scratchResult);
        }

        var roundQuery = _context.Rounds
            .Include(r => r.Course)
            .Include(r => r.Tee)
            .Where(r => r.GolferId == golferId && r.IsComplete && r.TotalScore.HasValue);

        if (request.SeasonId != null)
        {
            var season = await _context.Seasons.FirstOrDefaultAsync(s => s.Id == request.SeasonId);
            if (season == null)
                return ApiResponse<HandicapCalculationResponse>.ErrorResponse("Season not found");

            var seasonStart = season.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var seasonEnd = season.EndDate.HasValue
                ? season.EndDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc)
                : DateTime.UtcNow;

            roundQuery = roundQuery.Where(r =>
                r.LeagueId == request.LeagueId &&
                r.RoundDate >= seasonStart &&
                r.RoundDate <= seasonEnd);
        }
        else if (request.LeagueId != null)
        {
            roundQuery = roundQuery.Where(r => r.LeagueId == request.LeagueId);
        }

        var rounds = await roundQuery
            .OrderByDescending(r => r.RoundDate)
            .Take(20)
            .ToListAsync();

        if (rounds.Count == 0)
            return ApiResponse<HandicapCalculationResponse>.ErrorResponse(
                "No completed rounds found for this golfer in the specified scope");

        HandicapCalculationResponse result = request.Method switch
        {
            HandicapCalculationMethod.WorldHandicapSystem => CalculateWhs(golferId, rounds),
            HandicapCalculationMethod.BobsLeague => CalculateBobsLeague(golferId, rounds),
            HandicapCalculationMethod.Scratch => CalculateScratch(golferId, rounds),
            _ => CalculateWhs(golferId, rounds)
        };

        result.Method = request.Method;

        if (request.Persist)
        {
            await PersistCalculatedHandicapAsync(golferId, result.HandicapIndex, request, currentUserId, result.RoundsUsed);
            result.Persisted = true;
        }

        _logger.LogInformation(
            "Calculated {Method} handicap {Index:F1} for golfer {GolferId} using {Rounds} rounds",
            request.Method, result.HandicapIndex, golferId, result.RoundsUsed);

        return ApiResponse<HandicapCalculationResponse>.SuccessResponse(result,
            $"Handicap index calculated: {result.HandicapIndex:F1}");
    }

    /// <summary>
    /// USGA World Handicap System:
    /// Score differential = (Gross Score - Course Rating) × 113 / Slope Rating
    /// Use the best N differentials of the last 20 rounds:
    ///   3-4 rounds  → best 1; 5-6 → best 2; 7-8 → best 3; 9-10 → best 4
    ///   11-12 → best 5; 13-14 → best 6; 15-16 → best 7; 17-18 → best 8
    ///   19 → best 9; 20 → best 10
    /// Handicap Index = average of best N × 0.96
    /// Max Handicap Index: 54.0
    /// </summary>
    private static HandicapCalculationResponse CalculateWhs(string golferId, List<RoundEntity> rounds)
    {
        var details = new List<ScoreDifferentialDetail>();

        foreach (var round in rounds)
        {
            var tee = round.Tee;
            if (tee == null || round.TotalScore == null) continue;

            // Choose rating/slope based on holes played
            double courseRating;
            int slopeRating;

            if (round.HolesPlayed == Core.Enums.HolesPlayed.Front)
            {
                courseRating = tee.RatingOut;
                slopeRating = tee.SlopeOut;
            }
            else if (round.HolesPlayed == Core.Enums.HolesPlayed.Back)
            {
                courseRating = tee.RatingIn;
                slopeRating = tee.SlopeIn;
            }
            else
            {
                courseRating = tee.TotalRating;
                slopeRating = tee.AverageSlope;
            }

            if (slopeRating <= 0) slopeRating = HandicapConstants.StandardSlopeRating;

            var differential = (round.TotalScore.Value - courseRating) * HandicapConstants.StandardSlopeRating / slopeRating;

            details.Add(new ScoreDifferentialDetail
            {
                RoundId = round.Id,
                RoundDate = round.RoundDate,
                CourseName = round.Course?.Name ?? "Unknown",
                TeeName = tee.Name,
                GrossScore = round.TotalScore.Value,
                CourseRating = courseRating,
                SlopeRating = slopeRating,
                Differential = Math.Round(differential, 1),
                UsedInCalculation = false
            });
        }

        // Sort differentials ascending; select best N
        var sorted = details.OrderBy(d => d.Differential).ToList();
        int n = CountToUse(details.Count);
        for (int i = 0; i < n && i < sorted.Count; i++)
            sorted[i].UsedInCalculation = true;

        var bestN = sorted.Take(n).ToList();
        double avg = bestN.Count > 0 ? bestN.Average(d => d.Differential) : 0;
        double index = Math.Min(Math.Round(avg * HandicapConstants.WhsMultiplier, 1), HandicapConstants.WhsMaxIndex);

        return new HandicapCalculationResponse
        {
            GolferId = golferId,
            HandicapIndex = index,
            RoundsConsidered = details.Count,
            RoundsUsed = n,
            Differentials = details,
            Notes = $"WHS: best {n} of {details.Count} differentials × {HandicapConstants.WhsMultiplier}"
        };
    }

    /// <summary>
    /// Bob's League Handicap (common informal league method):
    /// Handicap = (Average Gross Score - Course Par) × 0.80
    /// Uses full 18-hole par (typically 72 if not available from tee data).
    /// All available completed rounds are used.
    /// Result capped at 36.
    /// </summary>
    private static HandicapCalculationResponse CalculateBobsLeague(string golferId, List<RoundEntity> rounds)
    {
        var details = new List<ScoreDifferentialDetail>();

        foreach (var round in rounds)
        {
            if (round.TotalScore == null) continue;

            var tee = round.Tee;
            int par = tee != null ? (tee.ParOut + tee.ParIn) : HandicapConstants.DefaultPar;
            if (round.HolesPlayed == Core.Enums.HolesPlayed.Front && tee != null) par = tee.ParOut;
            if (round.HolesPlayed == Core.Enums.HolesPlayed.Back && tee != null) par = tee.ParIn;

            double courseRating = tee?.TotalRating ?? par;
            int slope = tee?.AverageSlope ?? HandicapConstants.StandardSlopeRating;

            details.Add(new ScoreDifferentialDetail
            {
                RoundId = round.Id,
                RoundDate = round.RoundDate,
                CourseName = round.Course?.Name ?? "Unknown",
                TeeName = tee?.Name ?? "Unknown",
                GrossScore = round.TotalScore.Value,
                CourseRating = courseRating,
                SlopeRating = slope,
                // For Bob's we store gross - par as "differential" for display
                Differential = round.TotalScore.Value - par,
                UsedInCalculation = true
            });
        }

        double avgDiff = details.Count > 0 ? details.Average(d => d.Differential) : 0;
        double index = Math.Min(Math.Round(avgDiff * HandicapConstants.BobsMultiplier, 1), HandicapConstants.BobsMaxIndex);
        index = Math.Max(index, 0);

        return new HandicapCalculationResponse
        {
            GolferId = golferId,
            HandicapIndex = index,
            RoundsConsidered = details.Count,
            RoundsUsed = details.Count,
            Differentials = details,
            Notes = $"Bob's League: avg({details.Count} rounds over par) × {HandicapConstants.BobsMultiplier}"
        };
    }

    private static HandicapCalculationResponse CalculateScratch(string golferId, List<RoundEntity> rounds)
    {
        return new HandicapCalculationResponse
        {
            GolferId = golferId,
            HandicapIndex = 0,
            RoundsConsidered = rounds.Count,
            RoundsUsed = 0,
            Differentials = new List<ScoreDifferentialDetail>(),
            Notes = "Scratch handicap: index is always 0"
        };
    }

    private static int CountToUse(int totalRounds) => totalRounds switch
    {
        <= 2  => 0,
        3 or 4  => 1,
        5 or 6  => 2,
        7 or 8  => 3,
        9 or 10 => 4,
        11 or 12 => 5,
        13 or 14 => 6,
        15 or 16 => 7,
        17 or 18 => 8,
        19       => 9,
        _        => 10
    };

    private async Task PersistCalculatedHandicapAsync(
        string golferId, double handicapIndex,
        CalculateHandicapRequest request, string currentUserId, int roundsUsed)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var methodName = request.Method.ToString();

        var history = new HandicapHistory
        {
            Id = Guid.NewGuid().ToString(),
            GolferId = golferId,
            LeagueId = request.LeagueId,
            SeasonId = request.SeasonId,
            HandicapIndex = handicapIndex,
            EffectiveDate = today,
            CalculationMethod = methodName,
            RoundsUsed = roundsUsed,
            Notes = $"Auto-calculated via {methodName}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        _context.HandicapHistories.Add(history);
        await UpdateCurrentHandicapAsync(golferId, request.LeagueId, request.SeasonId, handicapIndex, currentUserId);
        await _context.SaveChangesAsync();
    }
}
