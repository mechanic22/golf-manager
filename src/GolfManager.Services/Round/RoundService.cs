using GolfManager.Data;
using GolfManager.Shared.DTOs.Round;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.Round;

/// <summary>
/// Service for round management
/// </summary>
public class RoundService : IRoundService
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<RoundService> _logger;

    public RoundService(GolfManagerDbContext context, ILogger<RoundService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<RoundResponse>> GetLeagueGolferRoundsAsync(string leagueGolferId, string leagueId)
    {
        var rounds = await _context.Rounds
            .IgnoreQueryFilters()
            .Include(r => r.Course)
            .Include(r => r.Tee)
            .Include(r => r.Holes.OrderBy(h => h.HoleNumber))
            .Where(r => r.LeagueGolferId == leagueGolferId && r.LeagueId == leagueId)
            .OrderByDescending(r => r.RoundDate)
            .ToListAsync();

        return rounds.Select(MapToResponse).ToList();
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

        return round == null ? null : MapToResponse(round);
    }

    public async Task<List<RoundResponse>> GetEventRoundsAsync(string seasonEventId, string leagueId)
    {
        // First, verify the event exists and belongs to the league
        var eventExists = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .AnyAsync(e => e.Id == seasonEventId && e.LeagueId == leagueId);

        if (!eventExists)
        {
            throw new InvalidOperationException($"Event {seasonEventId} not found in league {leagueId}");
        }

        // Get all rounds for this event
        var rounds = await _context.Rounds
            .IgnoreQueryFilters()
            .Include(r => r.Course)
            .Include(r => r.Tee)
            .Include(r => r.Holes.OrderBy(h => h.HoleNumber))
            .Include(r => r.LeagueGolfer)
            .Where(r => r.LeagueId == leagueId)
            .ToListAsync();

        // Filter by SeasonEventId (this would require a link table in production)
        // For now, we'll return all rounds for the league
        return rounds.Select(MapToResponse).ToList();
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

        // Verify the course and tee exist
        var course = await _context.Courses.FindAsync(request.CourseId);
        if (course == null)
        {
            throw new InvalidOperationException($"Course {request.CourseId} not found");
        }

        var tee = await _context.Tees.FindAsync(request.TeeId);
        if (tee == null)
        {
            throw new InvalidOperationException($"Tee {request.TeeId} not found");
        }

        // Create the round
        var round = new Core.Entities.Round
        {
            Id = Guid.NewGuid().ToString(),
            GolferId = leagueGolfer.GolferId,
            LeagueGolferId = request.LeagueGolferId,
            LeagueId = leagueId,
            CourseId = request.CourseId,
            TeeId = request.TeeId,
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
                Id = Guid.NewGuid().ToString(),
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
        return await CalculateRoundTotalsAsync(roundId, leagueId);
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

    private RoundResponse MapToResponse(Core.Entities.Round round)
    {
        return new RoundResponse
        {
            Id = round.Id,
            GolferId = round.GolferId,
            LeagueGolferId = round.LeagueGolferId,
            LeagueId = round.LeagueId,
            CourseId = round.CourseId,
            CourseName = round.Course?.Name ?? string.Empty,
            TeeId = round.TeeId,
            TeeName = round.Tee?.Name ?? string.Empty,
            RoundDate = round.RoundDate,
            HolesPlayed = round.HolesPlayed,
            TotalScore = round.TotalScore,
            NetScore = round.NetScore,
            HandicapUsed = round.HandicapUsed,
            IsComplete = round.IsComplete,
            Notes = round.Notes,
            Holes = round.Holes.Select(h => new RoundHoleResponse
            {
                Id = h.Id,
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
