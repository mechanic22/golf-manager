using GolfManager.Core.Services;
using GolfManager.Data;
using GolfManager.Shared.DTOs.Season;
using GolfManager.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.Season;

/// <summary>
/// Service for managing seasons
/// </summary>
public class SeasonService : ISeasonService
{
    private readonly GolfManagerDbContext _context;
    private readonly IShortIdService _shortIdService;
    private readonly ILogger<SeasonService> _logger;

    public SeasonService(
        GolfManagerDbContext context,
        IShortIdService shortIdService,
        ILogger<SeasonService> logger)
    {
        _context = context;
        _shortIdService = shortIdService;
        _logger = logger;
    }

    public async Task<List<SeasonResponse>> GetLeagueSeasonsAsync(string leagueId)
    {
        var seasons = await _context.Seasons
            .IgnoreQueryFilters()
            .Include(s => s.Events)
            .Include(s => s.SeasonGolfers)
            .Where(s => s.LeagueId == leagueId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();

        return seasons.Select(MapToResponse).ToList();
    }

    public async Task<SeasonResponse?> GetSeasonByIdAsync(string seasonId, string leagueId)
    {
        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .Include(s => s.Events)
            .Include(s => s.SeasonGolfers)
            .Where(s => s.Id == seasonId && s.LeagueId == leagueId)
            .FirstOrDefaultAsync();

        return season == null ? null : MapToResponse(season);
    }

    public async Task<SeasonResponse?> GetSeasonByKeyAsync(string seasonKey, string leagueId)
    {
        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .Include(s => s.Events)
            .Include(s => s.SeasonGolfers)
            .Where(s => s.Key == seasonKey && s.LeagueId == leagueId)
            .FirstOrDefaultAsync();

        return season == null ? null : MapToResponse(season);
    }

    public async Task<SeasonResponse> CreateSeasonAsync(CreateSeasonRequest request, string leagueId, string userId)
    {
        // Auto-generate key from name if not provided
        var seasonKey = string.IsNullOrWhiteSpace(request.Key)
            ? request.Name.ToSlug()
            : request.Key;

        // Ensure key is not empty
        if (string.IsNullOrWhiteSpace(seasonKey))
        {
            throw new InvalidOperationException("Season key cannot be empty");
        }

        // Check for duplicate key
        var existingSeason = await _context.Seasons
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Key == seasonKey && s.LeagueId == leagueId);

        if (existingSeason != null)
        {
            throw new InvalidOperationException($"Season with key '{seasonKey}' already exists in this league");
        }

        // Validate dates
        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
        {
            throw new InvalidOperationException("End date cannot be before start date");
        }

        var season = new Core.Entities.Season
        {
            Id = _shortIdService.GenerateId(),
            LeagueId = leagueId,
            Key = seasonKey,
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsLocked = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Seasons.Add(season);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created season {SeasonKey} ({SeasonId}) in league {LeagueId} by user {UserId}",
            season.Key, season.Id, leagueId, userId);

        return MapToResponse(season);
    }

    public async Task<SeasonResponse> UpdateSeasonAsync(string seasonId, UpdateSeasonRequest request, string leagueId, string userId)
    {
        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == seasonId && s.LeagueId == leagueId);

        if (season == null)
        {
            throw new InvalidOperationException("Season not found");
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            season.Name = request.Name;
        }

        if (request.StartDate.HasValue)
        {
            season.StartDate = request.StartDate.Value;
        }

        if (request.EndDate.HasValue)
        {
            season.EndDate = request.EndDate.Value;
        }

        if (request.IsLocked.HasValue)
        {
            season.IsLocked = request.IsLocked.Value;
        }

        // Validate dates
        if (season.EndDate.HasValue && season.EndDate.Value < season.StartDate)
        {
            throw new InvalidOperationException("End date cannot be before start date");
        }

        season.UpdatedAt = DateTime.UtcNow;
        season.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated season {SeasonId} in league {LeagueId} by user {UserId}",
            seasonId, leagueId, userId);

        return MapToResponse(season);
    }

    public async Task<bool> DeleteSeasonAsync(string seasonId, string leagueId, string userId)
    {
        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == seasonId && s.LeagueId == leagueId);

        if (season == null)
        {
            return false;
        }

        _context.Seasons.Remove(season);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted season {SeasonId} in league {LeagueId} by user {UserId}",
            seasonId, leagueId, userId);

        return true;
    }

    private static SeasonResponse MapToResponse(Core.Entities.Season season)
    {
        return new SeasonResponse
        {
            Id = season.Id,
            LeagueId = season.LeagueId,
            Key = season.Key,
            Name = season.Name,
            StartDate = season.StartDate,
            EndDate = season.EndDate,
            IsLocked = season.IsLocked,
            EventCount = season.Events?.Count ?? 0,
            GolferCount = season.SeasonGolfers?.Count ?? 0,
            CreatedAt = season.CreatedAt,
            UpdatedAt = season.UpdatedAt
        };
    }
}

