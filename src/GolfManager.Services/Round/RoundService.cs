using GolfManager.Data;
using GolfManager.Core.Services;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Round;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.Round;

/// <summary>
/// Service for round management
/// </summary>
public class RoundService : IRoundService
{
    private sealed record SeasonEventLookup(string Id, DateTime EventDate, string? CourseId, string? TeeId);

    private readonly GolfManagerDbContext _context;
    private readonly ILogger<RoundService> _logger;
    private readonly IHandicapRecalculationQueue? _handicapQueue;
    private readonly ISeasonPointsRecalculationQueue? _seasonPointsQueue;

    public RoundService(GolfManagerDbContext context, ILogger<RoundService> logger)
        : this(context, logger, null, null)
    {
    }

    public RoundService(
        GolfManagerDbContext context,
        ILogger<RoundService> logger,
        IHandicapRecalculationQueue? handicapQueue,
        ISeasonPointsRecalculationQueue? seasonPointsQueue)
    {
        _context = context;
        _logger = logger;
        _handicapQueue = handicapQueue;
        _seasonPointsQueue = seasonPointsQueue;
    }

    public async Task<PagedResponse<RoundResponse>> GetLeagueGolferRoundsAsync(string leagueGolferId, string leagueId, int page = 1, int pageSize = 25)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var baseQuery = _context.Rounds
            .IgnoreQueryFilters()
            .Where(r => r.LeagueGolferId == leagueGolferId && r.LeagueId == leagueId);

        var totalCount = await baseQuery.CountAsync();
        var rounds = await baseQuery
            .Include(r => r.Course)
            .Include(r => r.Tee)
            .Include(r => r.Holes.OrderBy(h => h.HoleNumber))
            .OrderByDescending(r => r.RoundDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var seasonEvents = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .Where(e => e.LeagueId == leagueId)
            .Select(e => new SeasonEventLookup(e.Id, e.EventDate, e.CourseId, e.TeeId))
            .ToListAsync();

        var items = rounds.Select(r =>
        {
            var evt = seasonEvents.FirstOrDefault(e =>
                e.EventDate.Date == r.RoundDate.Date &&
                e.CourseId == r.CourseId &&
                e.TeeId == r.TeeId);
            return MapToResponse(r, evt?.Id, evt?.EventDate);
        }).ToList();
        return PagedResponse<RoundResponse>.From(items, page, pageSize, totalCount);
    }

    public async Task<RoundResponse?> GetRoundByIdAsync(string roundId, string leagueId)
    {
        var round = await _context.Rounds
            .IgnoreQueryFilters()
            .Include(r => r.Course)
            .Include(r => r.Tee)
            .Include(r => r.Holes.OrderBy(h => h.HoleNumber))
            .Where(r => r.Id == roundId && r.LeagueId == leagueId)
            .FirstOrDefaultAsync();

        if (round == null)
        {
            return null;
        }

        var seasonEvent = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .Where(e => e.LeagueId == leagueId &&
                        e.EventDate.Date == round.RoundDate.Date &&
                        e.CourseId == round.CourseId &&
                        e.TeeId == round.TeeId)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        return MapToResponse(round, seasonEvent);
    }

    public async Task<List<RoundResponse>> GetEventRoundsAsync(string seasonEventId, string leagueId)
    {
        // First, verify the event exists and belongs to the league
        var seasonEvent = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .Where(e => e.Id == seasonEventId && e.LeagueId == leagueId)
            .FirstOrDefaultAsync();

        if (seasonEvent == null)
        {
            throw new InvalidOperationException($"Event {seasonEventId} not found in league {leagueId}");
        }

        // Filter rounds by event date/course/tee.
        var rounds = await _context.Rounds
            .IgnoreQueryFilters()
            .Include(r => r.Course)
            .Include(r => r.Tee)
            .Include(r => r.Holes.OrderBy(h => h.HoleNumber))
            .Include(r => r.LeagueGolfer)
            .Where(r => r.LeagueId == leagueId &&
                        r.RoundDate.Date == seasonEvent.EventDate.Date &&
                        r.CourseId == seasonEvent.CourseId &&
                        r.TeeId == seasonEvent.TeeId)
            .OrderByDescending(r => r.RoundDate)
            .ToListAsync();

        return rounds.Select(r => MapToResponse(r, seasonEventId)).ToList();
    }

    public async Task<RoundResponse> CreateRoundAsync(CreateRoundRequest request, string leagueId, string userId)
    {
        // Verify the league golfer exists and belongs to the league
        var leagueGolfer = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .Include(lg => lg.Golfer)
            .Where(lg => lg.Id == request.LeagueGolferId && lg.LeagueId == leagueId)
            .FirstOrDefaultAsync();

        if (leagueGolfer == null)
        {
            throw new InvalidOperationException($"League golfer {request.LeagueGolferId} not found in league {leagueId}");
        }

        // Verify the course and tee exist (only when IDs are provided)
        var courseId = string.IsNullOrWhiteSpace(request.CourseId) ? null : request.CourseId;
        var teeId = string.IsNullOrWhiteSpace(request.TeeId) ? null : request.TeeId;

        if (courseId != null)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                throw new InvalidOperationException($"Course {courseId} not found");
        }

        if (teeId != null)
        {
            var tee = await _context.Tees.FindAsync(teeId);
            if (tee == null)
                throw new InvalidOperationException($"Tee {teeId} not found");
        }

        // Create the round
        var round = new Core.Entities.Round
        {
            Id = Guid.NewGuid().ToString(),
            GolferId = leagueGolfer.GolferId,
            LeagueGolferId = request.LeagueGolferId,
            LeagueId = leagueId,
            CourseId = courseId,
            TeeId = teeId,
            RoundDate = request.RoundDate,
            HolesPlayed = request.HolesPlayed,
            HandicapUsed = request.HandicapUsed,
            Notes = request.Notes,
            IsComplete = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Rounds.Add(round);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created round {RoundId} for golfer {GolferId} in league {LeagueId} by user {UserId}",
            round.Id, leagueGolfer.GolferId, leagueId, userId);

        // Reload with navigation properties
        return (await GetRoundByIdAsync(round.Id, leagueId))!;
    }

    public async Task<RoundResponse> UpdateRoundAsync(string roundId, UpdateRoundRequest request, string leagueId, string userId)
    {
        var round = await _context.Rounds
            .IgnoreQueryFilters()
            .Where(r => r.Id == roundId && r.LeagueId == leagueId)
            .FirstOrDefaultAsync();

        if (round == null)
        {
            throw new InvalidOperationException($"Round {roundId} not found in league {leagueId}");
        }

        // Update fields
        if (request.HandicapUsed.HasValue)
        {
            round.HandicapUsed = request.HandicapUsed.Value;
        }

        if (request.Notes != null)
        {
            round.Notes = request.Notes;
        }

        if (request.IsComplete.HasValue)
        {
            round.IsComplete = request.IsComplete.Value;
        }

        round.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated round {RoundId} in league {LeagueId} by user {UserId}",
            roundId, leagueId, userId);

        return (await GetRoundByIdAsync(roundId, leagueId))!;
    }

    public async Task DeleteRoundAsync(string roundId, string leagueId, string userId)
    {
        var round = await _context.Rounds
            .IgnoreQueryFilters()
            .Include(r => r.Holes)
            .Where(r => r.Id == roundId && r.LeagueId == leagueId)
            .FirstOrDefaultAsync();

        if (round == null)
        {
            throw new InvalidOperationException($"Round {roundId} not found in league {leagueId}");
        }

        // Delete all hole scores first
        _context.RoundHoles.RemoveRange(round.Holes);

        // Delete the round
        _context.Rounds.Remove(round);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted round {RoundId} from league {LeagueId} by user {UserId}",
            roundId, leagueId, userId);
    }

    public async Task<RoundResponse> RecordHoleScoreAsync(string roundId, RecordHoleScoreRequest request, string leagueId, string userId)
    {
        var round = await _context.Rounds
            .IgnoreQueryFilters()
            .Include(r => r.Holes)
            .Where(r => r.Id == roundId && r.LeagueId == leagueId)
            .FirstOrDefaultAsync();

        if (round == null)
        {
            throw new InvalidOperationException($"Round {roundId} not found in league {leagueId}");
        }

        // Find or create the hole score
        var holeScore = round.Holes.FirstOrDefault(h => h.HoleNumber == request.HoleNumber);

        if (holeScore == null)
        {
            holeScore = new Core.Entities.RoundHole
            {
                RoundId = roundId,
                HoleNumber = request.HoleNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.RoundHoles.Add(holeScore);
        }

        // Update the hole score
        holeScore.GrossScore = request.GrossScore;
        holeScore.Putts = request.Putts;
        holeScore.FairwayHit = request.FairwayHit;
        holeScore.GreenInRegulation = request.GreenInRegulation;
        holeScore.Penalties = request.Penalties;
        holeScore.Notes = request.Notes;
        holeScore.UpdatedAt = DateTime.UtcNow;

        // Update the round's updated timestamp
        round.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Recorded score for hole {HoleNumber} in round {RoundId} by user {UserId}",
            request.HoleNumber, roundId, userId);

        // Recalculate round totals
        var updatedRound = await CalculateRoundTotalsAsync(roundId, leagueId);

        if (!string.IsNullOrWhiteSpace(updatedRound.SeasonEventId))
        {
            var seasonEvent = await _context.SeasonEvents
                .IgnoreQueryFilters()
                .Where(e => e.Id == updatedRound.SeasonEventId && e.LeagueId == leagueId)
                .Select(e => new { e.SeasonId, e.Id })
                .FirstOrDefaultAsync();

            if (seasonEvent != null)
            {
                if (_handicapQueue != null)
                {
                    await _handicapQueue.QueueGolferAsync(leagueId, seasonEvent.SeasonId, seasonEvent.Id, round.GolferId, userId);
                }

                if (_seasonPointsQueue != null)
                {
                    await _seasonPointsQueue.QueueSeasonAsync(leagueId, seasonEvent.SeasonId, userId);
                }
            }
        }

        return updatedRound;
    }

    public async Task<RoundResponse> CalculateRoundTotalsAsync(string roundId, string leagueId)
    {
        var round = await _context.Rounds
            .IgnoreQueryFilters()
            .Include(r => r.Holes)
            .Where(r => r.Id == roundId && r.LeagueId == leagueId)
            .FirstOrDefaultAsync();

        if (round == null)
        {
            throw new InvalidOperationException($"Round {roundId} not found in league {leagueId}");
        }

        // Calculate total score (sum of all gross scores)
        var holesWithScores = round.Holes.Where(h => h.GrossScore.HasValue).ToList();

        if (holesWithScores.Any())
        {
            round.TotalScore = holesWithScores.Sum(h => h.GrossScore!.Value);

            // Calculate net score if handicap is available
            if (round.HandicapUsed.HasValue)
            {
                round.NetScore = round.TotalScore - (int)Math.Round(round.HandicapUsed.Value);
            }
        }
        else
        {
            round.TotalScore = null;
            round.NetScore = null;
        }

        round.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Calculated totals for round {RoundId}: Total={TotalScore}, Net={NetScore}",
            roundId, round.TotalScore, round.NetScore);

        return (await GetRoundByIdAsync(roundId, leagueId))!;
    }

    private static string? ResolveSeasonEventId(
        Core.Entities.Round round,
        IEnumerable<SeasonEventLookup> seasonEvents)
    {
        return seasonEvents
            .FirstOrDefault(e =>
                e.EventDate.Date == round.RoundDate.Date &&
                e.CourseId == round.CourseId &&
                e.TeeId == round.TeeId)
            ?.Id;
    }

    private RoundResponse MapToResponse(Core.Entities.Round round, string? seasonEventId = null, DateTime? fallbackEventDate = null)
    {
        return new RoundResponse
        {
            Id = round.Id,
            GolferId = round.GolferId,
            LeagueGolferId = round.LeagueGolferId,
            LeagueId = round.LeagueId,
            SeasonEventId = seasonEventId,
            CourseId = round.CourseId,
            CourseName = round.Course?.Name ?? string.Empty,
            TeeId = round.TeeId,
            TeeName = round.Tee?.Name ?? string.Empty,
            RoundDate = round.RoundDate == default && fallbackEventDate.HasValue
                ? fallbackEventDate.Value
                : round.RoundDate,
            EventDate = fallbackEventDate,
            HolesPlayed = round.HolesPlayed,
            TotalScore = round.TotalScore,
            NetScore = round.NetScore,
            HandicapUsed = round.HandicapUsed,
            IsComplete = round.IsComplete,
            Notes = round.Notes,
            Holes = round.Holes.Select(h => new RoundHoleResponse
            {
                HoleNumber = h.HoleNumber,
                GrossScore = h.GrossScore,
                NetScore = h.NetScore,
                Putts = h.Putts,
                FairwayHit = h.FairwayHit,
                GreenInRegulation = h.GreenInRegulation,
                Penalties = h.Penalties,
                Notes = h.Notes
            }).ToList(),
            CreatedAt = round.CreatedAt,
            UpdatedAt = round.UpdatedAt
        };
    }
}
