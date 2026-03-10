using GolfManager.Data;
using GolfManager.Core.Entities;
using GolfManager.Shared.DTOs.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.Event;

/// <summary>
/// Service for managing season events
/// </summary>
public class EventService : IEventService
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<EventService> _logger;

    public EventService(GolfManagerDbContext context, ILogger<EventService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<EventResponse>> GetSeasonEventsAsync(string seasonId, string leagueId)
    {
        return await _context.SeasonEvents
            .IgnoreQueryFilters()
            .Where(e => e.SeasonId == seasonId && e.LeagueId == leagueId)
            .OrderBy(e => e.EventDate)
            .Select(e => MapToResponse(e))
            .ToListAsync();
    }

    public async Task<EventResponse?> GetEventByIdAsync(string eventId, string leagueId)
    {
        var seasonEvent = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .Where(e => e.Id == eventId && e.LeagueId == leagueId)
            .FirstOrDefaultAsync();

        return seasonEvent == null ? null : MapToResponse(seasonEvent);
    }

    public async Task<EventResponse> CreateEventAsync(CreateEventRequest request, string seasonId, string leagueId, string userId)
    {
        // Verify season exists
        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == seasonId && s.LeagueId == leagueId);

        if (season == null)
        {
            throw new InvalidOperationException("Season not found");
        }

        // Check if season is locked
        if (season.IsLocked)
        {
            throw new InvalidOperationException("Cannot add events to a locked season");
        }

        var seasonEvent = new SeasonEvent
        {
            Id = Guid.NewGuid().ToString(),
            SeasonId = seasonId,
            LeagueId = leagueId,
            EventDate = request.EventDate,
            CourseId = request.CourseId,
            TeeId = request.TeeId,
            HolesPlayed = request.HolesPlayed,
            EventType = request.EventType,
            ScoringFormat = request.ScoringFormat,
            Name = request.Name,
            Description = request.Description,
            IsLocked = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.SeasonEvents.Add(seasonEvent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created event {EventId} in season {SeasonId} by user {UserId}",
            seasonEvent.Id, seasonId, userId);

        return MapToResponse(seasonEvent);
    }

    public async Task<EventResponse> UpdateEventAsync(string eventId, UpdateEventRequest request, string leagueId, string userId)
    {
        var seasonEvent = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == eventId && e.LeagueId == leagueId);

        if (seasonEvent == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        // Update fields if provided
        if (request.EventDate.HasValue)
        {
            seasonEvent.EventDate = request.EventDate.Value;
        }

        if (request.CourseId != null)
        {
            seasonEvent.CourseId = request.CourseId;
        }

        if (request.TeeId != null)
        {
            seasonEvent.TeeId = request.TeeId;
        }

        if (request.HolesPlayed.HasValue)
        {
            seasonEvent.HolesPlayed = request.HolesPlayed.Value;
        }

        if (request.EventType.HasValue)
        {
            seasonEvent.EventType = request.EventType.Value;
        }

        if (request.ScoringFormat.HasValue)
        {
            seasonEvent.ScoringFormat = request.ScoringFormat.Value;
        }

        if (request.Name != null)
        {
            seasonEvent.Name = request.Name;
        }

        if (request.Description != null)
        {
            seasonEvent.Description = request.Description;
        }

        if (request.IsLocked.HasValue)
        {
            seasonEvent.IsLocked = request.IsLocked.Value;
        }

        seasonEvent.UpdatedAt = DateTime.UtcNow;
        seasonEvent.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated event {EventId} in league {LeagueId} by user {UserId}",
            eventId, leagueId, userId);

        return MapToResponse(seasonEvent);
    }

    public async Task<bool> DeleteEventAsync(string eventId, string leagueId, string userId)
    {
        var seasonEvent = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == eventId && e.LeagueId == leagueId);

        if (seasonEvent == null)
        {
            return false;
        }

        _context.SeasonEvents.Remove(seasonEvent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted event {EventId} from league {LeagueId} by user {UserId}",
            eventId, leagueId, userId);

        return true;
    }

    private static EventResponse MapToResponse(SeasonEvent seasonEvent)
    {
        return new EventResponse
        {
            Id = seasonEvent.Id,
            SeasonId = seasonEvent.SeasonId,
            LeagueId = seasonEvent.LeagueId,
            EventDate = seasonEvent.EventDate,
            CourseId = seasonEvent.CourseId,
            TeeId = seasonEvent.TeeId,
            HolesPlayed = seasonEvent.HolesPlayed,
            EventType = seasonEvent.EventType,
            ScoringFormat = seasonEvent.ScoringFormat,
            Name = seasonEvent.Name,
            Description = seasonEvent.Description,
            IsLocked = seasonEvent.IsLocked,
            CreatedAt = seasonEvent.CreatedAt,
            UpdatedAt = seasonEvent.UpdatedAt
        };
    }
}

