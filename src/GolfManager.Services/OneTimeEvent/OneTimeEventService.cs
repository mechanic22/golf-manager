using GolfManager.Core.Enums;
using GolfManager.Data;
using GolfManager.Shared.DTOs.OneTimeEvent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.OneTimeEvent;

/// <summary>
/// Service for managing one-time events
/// </summary>
public class OneTimeEventService : IOneTimeEventService
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<OneTimeEventService> _logger;

    public OneTimeEventService(GolfManagerDbContext context, ILogger<OneTimeEventService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<OneTimeEventListResponse>> GetEventsAsync(
        bool? publicOnly = null,
        bool? upcomingOnly = null,
        string? organizerId = null)
    {
        var query = _context.OneTimeEvents
            .Include(e => e.Organizer)
            .Include(e => e.Course)
            .Include(e => e.Teams)
            .Where(e => e.IsActive);

        // Filter by access type
        if (publicOnly == true)
        {
            query = query.Where(e => e.AccessType == EventAccessType.Public);
        }

        // Filter by date
        if (upcomingOnly == true)
        {
            var today = DateTime.UtcNow.Date;
            query = query.Where(e => e.EventDate >= today);
        }

        // Filter by organizer
        if (!string.IsNullOrEmpty(organizerId))
        {
            query = query.Where(e => e.OrganizerId == organizerId);
        }

        var events = await query
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        return events.Select(MapToListResponse).ToList();
    }

    public async Task<OneTimeEventResponse?> GetEventByIdAsync(string eventId)
    {
        var eventEntity = await _context.OneTimeEvents
            .Include(e => e.Organizer)
            .Include(e => e.Course)
            .Include(e => e.Tee)
            .Include(e => e.Teams)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive);

        return eventEntity == null ? null : await MapToResponseAsync(eventEntity);
    }

    public async Task<OneTimeEventResponse?> GetEventByKeyAsync(string eventKey)
    {
        var eventEntity = await _context.OneTimeEvents
            .Include(e => e.Organizer)
            .Include(e => e.Course)
            .Include(e => e.Tee)
            .Include(e => e.Teams)
            .FirstOrDefaultAsync(e => e.Key == eventKey && e.IsActive);

        return eventEntity == null ? null : await MapToResponseAsync(eventEntity);
    }

    public async Task<OneTimeEventResponse> CreateEventAsync(CreateOneTimeEventRequest request, string userId)
    {
        // Check for duplicate key
        var existingEvent = await _context.OneTimeEvents
            .FirstOrDefaultAsync(e => e.Key == request.Key);

        if (existingEvent != null)
        {
            throw new InvalidOperationException($"Event with key '{request.Key}' already exists");
        }

        // Validate event date
        if (request.EventDate < DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("Event date cannot be in the past");
        }

        // Validate registration deadline
        if (request.RegistrationDeadline.HasValue && request.RegistrationDeadline.Value > request.EventDate)
        {
            throw new InvalidOperationException("Registration deadline cannot be after event date");
        }

        // Validate private event has registration code
        if (request.AccessType == EventAccessType.Private && string.IsNullOrWhiteSpace(request.RegistrationCode))
        {
            throw new InvalidOperationException("Private events must have a registration code");
        }

        var eventEntity = new Core.Entities.OneTimeEvent
        {
            Id = Guid.NewGuid().ToString(),
            Key = request.Key,
            Name = request.Name,
            Description = request.Description,
            EventDate = request.EventDate,
            OrganizerId = userId,
            OrganizationName = request.OrganizationName,
            OrganizerEmail = request.OrganizerEmail,
            OrganizerPhone = request.OrganizerPhone,
            CourseId = request.CourseId,
            TeeId = request.TeeId,
            HolesPlayed = request.HolesPlayed,
            Format = request.Format,
            TeamSize = request.TeamSize,
            UseHandicaps = request.UseHandicaps,
            MaxTeams = request.MaxTeams,
            TotalRounds = request.TotalRounds,
            AccessType = request.AccessType,
            RegistrationCode = request.RegistrationCode,
            RegistrationDeadline = request.RegistrationDeadline,
            Status = EventStatus.Draft,
            IsLocked = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.OneTimeEvents.Add(eventEntity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created one-time event {EventKey} ({EventId}) by user {UserId}",
            eventEntity.Key, eventEntity.Id, userId);

        return await MapToResponseAsync(eventEntity);
    }

    public async Task<OneTimeEventResponse> UpdateEventAsync(string eventId, UpdateOneTimeEventRequest request, string userId)
    {
        var eventEntity = await _context.OneTimeEvents
            .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive);

        if (eventEntity == null)
        {
            throw new KeyNotFoundException($"Event with ID {eventId} not found");
        }

        // Only organizer can update
        if (eventEntity.OrganizerId != userId)
        {
            throw new UnauthorizedAccessException("Only the event organizer can update this event");
        }

        // Cannot update locked events
        if (eventEntity.IsLocked)
        {
            throw new InvalidOperationException("Cannot update a locked event");
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
            eventEntity.Name = request.Name;

        if (request.Description != null)
            eventEntity.Description = request.Description;

        if (request.EventDate.HasValue)
        {
            if (request.EventDate.Value < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Event date cannot be in the past");
            }
            eventEntity.EventDate = request.EventDate.Value;
        }

        if (request.OrganizationName != null)
            eventEntity.OrganizationName = request.OrganizationName;

        if (request.OrganizerEmail != null)
            eventEntity.OrganizerEmail = request.OrganizerEmail;

        if (request.OrganizerPhone != null)
            eventEntity.OrganizerPhone = request.OrganizerPhone;

        if (request.CourseId != null)
            eventEntity.CourseId = request.CourseId;

        if (request.TeeId != null)
            eventEntity.TeeId = request.TeeId;

        if (request.HolesPlayed.HasValue)
            eventEntity.HolesPlayed = request.HolesPlayed.Value;

        if (request.Format.HasValue)
            eventEntity.Format = request.Format.Value;

        if (request.TeamSize.HasValue)
            eventEntity.TeamSize = request.TeamSize.Value;

        if (request.UseHandicaps.HasValue)
            eventEntity.UseHandicaps = request.UseHandicaps.Value;

        if (request.MaxTeams.HasValue)
            eventEntity.MaxTeams = request.MaxTeams.Value;

        if (request.TotalRounds.HasValue)
            eventEntity.TotalRounds = request.TotalRounds.Value;

        if (request.AccessType.HasValue)
            eventEntity.AccessType = request.AccessType.Value;

        if (request.RegistrationCode != null)
            eventEntity.RegistrationCode = request.RegistrationCode;

        if (request.RegistrationDeadline.HasValue)
            eventEntity.RegistrationDeadline = request.RegistrationDeadline;

        eventEntity.UpdatedAt = DateTime.UtcNow;
        eventEntity.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated one-time event {EventId} by user {UserId}", eventId, userId);

        return await MapToResponseAsync(eventEntity);
    }

    public async Task<OneTimeEventResponse> PublishEventAsync(string eventId, string userId)
    {
        var eventEntity = await _context.OneTimeEvents
            .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive);

        if (eventEntity == null)
        {
            throw new KeyNotFoundException($"Event with ID {eventId} not found");
        }

        // Only organizer can publish
        if (eventEntity.OrganizerId != userId)
        {
            throw new UnauthorizedAccessException("Only the event organizer can publish this event");
        }

        // Can only publish draft events
        if (eventEntity.Status != EventStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot publish event with status {eventEntity.Status}");
        }

        eventEntity.Status = EventStatus.Published;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        eventEntity.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Published one-time event {EventId} by user {UserId}", eventId, userId);

        return await MapToResponseAsync(eventEntity);
    }

    public async Task<OneTimeEventResponse> CancelEventAsync(string eventId, string userId)
    {
        var eventEntity = await _context.OneTimeEvents
            .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive);

        if (eventEntity == null)
        {
            throw new KeyNotFoundException($"Event with ID {eventId} not found");
        }

        // Only organizer can cancel
        if (eventEntity.OrganizerId != userId)
        {
            throw new UnauthorizedAccessException("Only the event organizer can cancel this event");
        }

        // Cannot cancel completed events
        if (eventEntity.Status == EventStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a completed event");
        }

        eventEntity.Status = EventStatus.Cancelled;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        eventEntity.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Cancelled one-time event {EventId} by user {UserId}", eventId, userId);

        return await MapToResponseAsync(eventEntity);
    }

    public async Task<bool> DeleteEventAsync(string eventId, string userId)
    {
        var eventEntity = await _context.OneTimeEvents
            .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive);

        if (eventEntity == null)
        {
            return false;
        }

        // Only organizer can delete
        if (eventEntity.OrganizerId != userId)
        {
            throw new UnauthorizedAccessException("Only the event organizer can delete this event");
        }

        // Soft delete
        eventEntity.IsActive = false;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        eventEntity.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted one-time event {EventId} by user {UserId}", eventId, userId);

        return true;
    }

    public async Task<bool> IsOrganizerAsync(string eventId, string userId)
    {
        return await _context.OneTimeEvents
            .AnyAsync(e => e.Id == eventId && e.OrganizerId == userId && e.IsActive);
    }

    // Mapping methods
    private static OneTimeEventListResponse MapToListResponse(Core.Entities.OneTimeEvent eventEntity)
    {
        var registeredCount = eventEntity.Teams?.Count(t => t.IsActive) ?? 0;
        int? spotsRemaining = eventEntity.MaxTeams.HasValue
            ? Math.Max(0, eventEntity.MaxTeams.Value - registeredCount)
            : null;

        var isRegistrationOpen = eventEntity.Status == EventStatus.Published &&
                                 (!eventEntity.RegistrationDeadline.HasValue || eventEntity.RegistrationDeadline.Value >= DateTime.UtcNow) &&
                                 (!eventEntity.MaxTeams.HasValue || registeredCount < eventEntity.MaxTeams.Value);

        return new OneTimeEventListResponse
        {
            Id = eventEntity.Id,
            Key = eventEntity.Key,
            Name = eventEntity.Name,
            EventDate = eventEntity.EventDate,
            OrganizerName = $"{eventEntity.Organizer?.FirstName} {eventEntity.Organizer?.LastName}".Trim(),
            OrganizationName = eventEntity.OrganizationName,
            CourseName = eventEntity.Course?.Name,
            Format = eventEntity.Format,
            TeamSize = eventEntity.TeamSize,
            TotalRounds = eventEntity.TotalRounds,
            AccessType = eventEntity.AccessType,
            Status = eventEntity.Status,
            RegisteredTeamsCount = registeredCount,
            MaxTeams = eventEntity.MaxTeams,
            SpotsRemaining = spotsRemaining,
            RegistrationDeadline = eventEntity.RegistrationDeadline,
            IsRegistrationOpen = isRegistrationOpen
        };
    }

    private async Task<OneTimeEventResponse> MapToResponseAsync(Core.Entities.OneTimeEvent eventEntity)
    {
        // Load related entities if not already loaded
        if (eventEntity.Organizer == null)
        {
            await _context.Entry(eventEntity).Reference(e => e.Organizer).LoadAsync();
        }
        if (eventEntity.Course == null && eventEntity.CourseId != null)
        {
            await _context.Entry(eventEntity).Reference(e => e.Course).LoadAsync();
        }
        if (eventEntity.Tee == null && eventEntity.TeeId != null)
        {
            await _context.Entry(eventEntity).Reference(e => e.Tee).LoadAsync();
        }
        if (eventEntity.Teams == null)
        {
            await _context.Entry(eventEntity).Collection(e => e.Teams).LoadAsync();
        }

        var registeredCount = eventEntity.Teams?.Count(t => t.IsActive) ?? 0;
        var checkedInCount = eventEntity.Teams?.Count(t => t.IsActive && t.IsCheckedIn) ?? 0;
        int? spotsRemaining = eventEntity.MaxTeams.HasValue
            ? Math.Max(0, eventEntity.MaxTeams.Value - registeredCount)
            : null;

        return new OneTimeEventResponse
        {
            Id = eventEntity.Id,
            Key = eventEntity.Key,
            Name = eventEntity.Name,
            Description = eventEntity.Description,
            EventDate = eventEntity.EventDate,
            OrganizerId = eventEntity.OrganizerId,
            OrganizerName = $"{eventEntity.Organizer?.FirstName} {eventEntity.Organizer?.LastName}".Trim(),
            OrganizationName = eventEntity.OrganizationName,
            OrganizerEmail = eventEntity.OrganizerEmail,
            OrganizerPhone = eventEntity.OrganizerPhone,
            CourseId = eventEntity.CourseId,
            CourseName = eventEntity.Course?.Name,
            TeeId = eventEntity.TeeId,
            TeeName = eventEntity.Tee?.Name,
            HolesPlayed = eventEntity.HolesPlayed,
            Format = eventEntity.Format,
            TeamSize = eventEntity.TeamSize,
            UseHandicaps = eventEntity.UseHandicaps,
            MaxTeams = eventEntity.MaxTeams,
            TotalRounds = eventEntity.TotalRounds,
            AccessType = eventEntity.AccessType,
            RegistrationDeadline = eventEntity.RegistrationDeadline,
            Status = eventEntity.Status,
            IsLocked = eventEntity.IsLocked,
            RegisteredTeamsCount = registeredCount,
            CheckedInTeamsCount = checkedInCount,
            SpotsRemaining = spotsRemaining,
            CreatedAt = eventEntity.CreatedAt,
            UpdatedAt = eventEntity.UpdatedAt
        };
    }
}
