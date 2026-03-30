using GolfManager.Core.Entities;
using GolfManager.Core.Enums;
using GolfManager.Data;
using GolfManager.Shared.DTOs.Season;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.Season;

/// <summary>
/// Service for managing season settings
/// </summary>
public class SeasonSettingsService : ISeasonSettingsService
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<SeasonSettingsService> _logger;

    public SeasonSettingsService(GolfManagerDbContext context, ILogger<SeasonSettingsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SeasonSettingsResponse?> GetSeasonSettingsAsync(string seasonId, string leagueId)
    {
        var settings = await _context.SeasonSettings
            .IgnoreQueryFilters()
            .Include(s => s.DefaultCourse)
            .Where(s => s.SeasonId == seasonId && s.LeagueId == leagueId)
            .FirstOrDefaultAsync();

        return settings == null ? null : MapToResponse(settings);
    }

    public async Task<SeasonSettingsResponse> UpdateSeasonSettingsAsync(
        string seasonId,
        string leagueId,
        UpdateSeasonSettingsRequest request)
    {
        // Verify season exists and belongs to league
        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == seasonId && s.LeagueId == leagueId);

        if (season == null)
        {
            throw new InvalidOperationException($"Season {seasonId} not found in league {leagueId}");
        }

        // Check if season is locked
        if (season.IsLocked)
        {
            throw new InvalidOperationException("Cannot modify settings for a locked season");
        }

        // Get or create settings
        var settings = await _context.SeasonSettings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.SeasonId == seasonId && s.LeagueId == leagueId);

        if (settings == null)
        {
            // Create new settings
            settings = new SeasonSettings
            {
                Id = Guid.NewGuid().ToString(),
                SeasonId = seasonId,
                LeagueId = leagueId
            };
            _context.SeasonSettings.Add(settings);
        }

        // Update settings
        if (request.HandicapType.HasValue)
            settings.HandicapType = request.HandicapType.Value;

        if (request.MaxHandicap.HasValue)
            settings.MaxHandicap = request.MaxHandicap.Value;

        if (request.MaxScoreForHandicap.HasValue)
            settings.MaxScoreForHandicap = request.MaxScoreForHandicap.Value;

        if (request.IndividualScoringType.HasValue)
            settings.IndividualScoringType = request.IndividualScoringType.Value;

        if (request.TeamScoringType.HasValue)
            settings.TeamScoringType = request.TeamScoringType.Value;

        if (request.MissingPlayerType.HasValue)
            settings.MissingPlayerType = request.MissingPlayerType.Value;

        if (request.MissingTeamType.HasValue)
            settings.MissingTeamType = request.MissingTeamType.Value;

        if (request.DefaultCourseId != null)
            settings.DefaultCourseId = request.DefaultCourseId;

        if (request.DefaultStartTime.HasValue)
            settings.DefaultStartTime = request.DefaultStartTime.Value;

        await _context.SaveChangesAsync();

        // Reload with course info
        await _context.Entry(settings).Reference(s => s.DefaultCourse).LoadAsync();

        return MapToResponse(settings);
    }

    public async Task<SeasonSettingsResponse> CreateDefaultSettingsAsync(string seasonId, string leagueId)
    {
        // Verify season exists
        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == seasonId && s.LeagueId == leagueId);

        if (season == null)
        {
            throw new InvalidOperationException($"Season {seasonId} not found in league {leagueId}");
        }

        // Check if settings already exist
        var existing = await _context.SeasonSettings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.SeasonId == seasonId);

        if (existing != null)
        {
            throw new InvalidOperationException($"Settings already exist for season {seasonId}");
        }

        // Create default settings
        var settings = new SeasonSettings
        {
            Id = Guid.NewGuid().ToString(),
            SeasonId = seasonId,
            LeagueId = leagueId,
            HandicapType = HandicapType.Bobs,
            MaxHandicap = 18,
            MaxScoreForHandicap = MaxScoreForHandicap.PlusFour,
            IndividualScoringType = IndividualScoringType.TwoPoint,
            TeamScoringType = TeamScoringType.MatchPoints,
            MissingPlayerType = MissingPlayerType.FieldAverage,
            MissingTeamType = MissingTeamType.PartialPoints,
            DefaultStartTime = new TimeOnly(17, 30) // 5:30 PM
        };

        _context.SeasonSettings.Add(settings);
        await _context.SaveChangesAsync();

        return MapToResponse(settings);
    }

    private static SeasonSettingsResponse MapToResponse(SeasonSettings settings)
    {
        return new SeasonSettingsResponse
        {
            Id = settings.Id,
            SeasonId = settings.SeasonId,
            LeagueId = settings.LeagueId,
            HandicapType = settings.HandicapType,
            MaxHandicap = settings.MaxHandicap,
            MaxScoreForHandicap = settings.MaxScoreForHandicap,
            IndividualScoringType = settings.IndividualScoringType,
            TeamScoringType = settings.TeamScoringType,
            MissingPlayerType = settings.MissingPlayerType,
            MissingTeamType = settings.MissingTeamType,
            DefaultCourseId = settings.DefaultCourseId,
            DefaultCourseName = settings.DefaultCourse?.Name,
            DefaultStartTime = settings.DefaultStartTime,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
    }
}

