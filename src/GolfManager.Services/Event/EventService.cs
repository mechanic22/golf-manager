using GolfManager.Core.Entities;
using GolfManager.Core.Enums;
using GolfManager.Core.Services;
using GolfManager.Data;
using GolfManager.Services.Handicap;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.Handicap;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace GolfManager.Services.Event;

public class EventService : IEventService
{
    private readonly GolfManagerDbContext _context;
    private readonly IShortIdService _shortIdService;
    private readonly IHandicapService _handicapService;
    private readonly IEventScoringService _scoringService;
    private readonly ISeasonPointsRecalculationQueue? _seasonPointsQueue;
    private readonly ILogger<EventService> _logger;

    public EventService(
        GolfManagerDbContext context,
        IShortIdService shortIdService,
        IHandicapService handicapService,
        IEventScoringService scoringService,
        ILogger<EventService> logger)
        : this(context, shortIdService, handicapService, scoringService, null, logger)
    {
    }

    public EventService(
        GolfManagerDbContext context,
        IShortIdService shortIdService,
        IHandicapService handicapService,
        IEventScoringService scoringService,
        ISeasonPointsRecalculationQueue? seasonPointsQueue,
        ILogger<EventService> logger)
    {
        _context = context;
        _shortIdService = shortIdService;
        _handicapService = handicapService;
        _scoringService = scoringService;
        _seasonPointsQueue = seasonPointsQueue;
        _logger = logger;
    }

    public async Task<PagedResponse<EventResponse>> GetSeasonEventsAsync(string seasonId, string leagueId, int page = 1, int pageSize = 25)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var baseQuery = _context.SeasonEvents
            .IgnoreQueryFilters()
            .Where(e => e.SeasonId == seasonId && e.LeagueId == leagueId);

        var totalCount = await baseQuery.CountAsync();
        var events = await baseQuery
            .OrderBy(e => e.EventDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => MapToResponse(e))
            .ToListAsync();

        return PagedResponse<EventResponse>.From(events, page, pageSize, totalCount);
    }

    public async Task<EventResponse?> GetEventByIdAsync(string eventId, string leagueId)
    {
        var seasonEvent = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .Where(e => e.Id == eventId && e.LeagueId == leagueId)
            .FirstOrDefaultAsync();

        return seasonEvent == null ? null : MapToResponse(seasonEvent);
    }

    public async Task<EventScoreboardResponse> GetEventScoreboardAsync(string seasonId, string eventId, string leagueId)
    {
        var seasonEvent = await EnsureEventInSeasonAsync(seasonId, eventId, leagueId);

        if (seasonEvent.IsLocked || seasonEvent.Season?.IsLocked == true)
        {
            var stored = await _scoringService.TryBuildFromStoredAsync(seasonEvent, leagueId);
            if (stored != null) return stored;

            // Lazy-populate: compute once and persist for future requests
            var scoreboard = await _scoringService.BuildEventScoreboardAsync(seasonEvent, leagueId);
            await _scoringService.PersistEventScoreboardAsync(seasonEvent.Id, leagueId, scoreboard);
            return scoreboard;
        }

        return await _scoringService.BuildEventScoreboardAsync(seasonEvent, leagueId);
    }

    public async Task<EventResponse> CreateEventAsync(CreateEventRequest request, string seasonId, string leagueId, string userId)
    {
        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == seasonId && s.LeagueId == leagueId);

        if (season == null)
        {
            throw new InvalidOperationException("Season not found");
        }

        if (season.IsLocked)
        {
            throw new InvalidOperationException("Cannot add events to a locked season");
        }

        var seasonEvent = new SeasonEvent
        {
            Id = _shortIdService.GenerateId(),
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
            GameOfDayTitle = request.GameOfDayTitle,
            GameOfDayWinnerSeasonGolferId = request.GameOfDayWinnerSeasonGolferId,
            GameOfDayWinnerDisplayName = request.GameOfDayWinnerDisplayName,
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
            .Include(e => e.Season)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.LeagueId == leagueId);

        if (seasonEvent == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        if (request.EventDate.HasValue) seasonEvent.EventDate = request.EventDate.Value;
        if (request.CourseId != null) seasonEvent.CourseId = request.CourseId;
        if (request.TeeId != null) seasonEvent.TeeId = request.TeeId;
        if (request.HolesPlayed.HasValue) seasonEvent.HolesPlayed = request.HolesPlayed.Value;
        if (request.EventType.HasValue) seasonEvent.EventType = request.EventType.Value;
        if (request.ScoringFormat.HasValue) seasonEvent.ScoringFormat = request.ScoringFormat.Value;
        if (request.Name != null) seasonEvent.Name = request.Name;
        if (request.Description != null) seasonEvent.Description = request.Description;
        if (request.GameOfDayTitle != null) seasonEvent.GameOfDayTitle = request.GameOfDayTitle;
        if (request.GameOfDayWinnerSeasonGolferId != null) seasonEvent.GameOfDayWinnerSeasonGolferId = request.GameOfDayWinnerSeasonGolferId;
        if (request.GameOfDayWinnerDisplayName != null) seasonEvent.GameOfDayWinnerDisplayName = request.GameOfDayWinnerDisplayName;

        var wasLocked = seasonEvent.IsLocked;
        if (request.IsLocked.HasValue) seasonEvent.IsLocked = request.IsLocked.Value;

        seasonEvent.UpdatedAt = DateTime.UtcNow;
        seasonEvent.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated event {EventId} in league {LeagueId} by user {UserId}",
            eventId, leagueId, userId);

        if (!wasLocked && seasonEvent.IsLocked)
        {
            var scoreboard = await _scoringService.BuildEventScoreboardAsync(seasonEvent, leagueId);
            await _scoringService.PersistEventScoreboardAsync(seasonEvent.Id, leagueId, scoreboard);
        }

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

    public async Task<List<EventMatchupResponse>> GetEventMatchupsAsync(string seasonId, string eventId, string leagueId)
    {
        await EnsureEventInSeasonAsync(seasonId, eventId, leagueId);

        var matches = await _context.SeasonEventMatches
            .IgnoreQueryFilters()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m => m.SeasonEventId == eventId && m.LeagueId == leagueId)
            .OrderBy(m => m.StartingFlight ?? int.MaxValue)
            .ThenBy(m => m.StartingHole ?? int.MaxValue)
            .ThenBy(m => m.CreatedAt)
            .ToListAsync();

        return matches.Select(MapToMatchupResponse).ToList();
    }

    public async Task<List<EventMatchupResponse>> AutoSetupEventMatchupsFromStandingsAsync(string seasonId, string eventId, string leagueId, string userId)
    {
        var seasonEvent = await EnsureEventInSeasonAsync(seasonId, eventId, leagueId);

        var seasonTeams = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .Where(t => t.SeasonId == seasonId && t.LeagueId == leagueId)
            .OrderByDescending(t => t.SeasonPoints ?? 0)
            .ThenByDescending(t => t.Wins)
            .ThenBy(t => t.Losses)
            .ThenBy(t => t.Name)
            .ToListAsync();

        if (seasonTeams.Count < 2)
        {
            throw new InvalidOperationException("At least two teams are required to generate matchups");
        }

        var previousMatchPairs = await _context.SeasonEventMatches
            .IgnoreQueryFilters()
            .Include(m => m.SeasonEvent)
            .Where(m => m.LeagueId == leagueId
                && m.SeasonEvent.SeasonId == seasonId
                && m.SeasonEventId != eventId
                && m.HomeTeamId != null
                && m.AwayTeamId != null)
            .Select(m => new { m.HomeTeamId, m.AwayTeamId })
            .ToListAsync();

        var priorPairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in previousMatchPairs)
        {
            priorPairs.Add(BuildPairKey(pair.HomeTeamId!, pair.AwayTeamId!));
        }

        var unpaired = new List<SeasonTeam>(seasonTeams);
        var pairings = new List<(SeasonTeam Home, SeasonTeam Away)>();

        while (unpaired.Count > 1)
        {
            var home = unpaired[0];
            var awayIndex = -1;

            for (var i = 1; i < unpaired.Count; i++)
            {
                if (!priorPairs.Contains(BuildPairKey(home.Id, unpaired[i].Id)))
                {
                    awayIndex = i;
                    break;
                }
            }

            if (awayIndex == -1) awayIndex = 1;

            var away = unpaired[awayIndex];
            pairings.Add((home, away));
            unpaired.RemoveAt(awayIndex);
            unpaired.RemoveAt(0);
        }

        if (unpaired.Count == 1)
        {
            pairings.Add((unpaired[0], unpaired[0]));
        }

        var existingMatches = await _context.SeasonEventMatches
            .IgnoreQueryFilters()
            .Where(m => m.LeagueId == leagueId && m.SeasonEventId == eventId)
            .ToListAsync();
        if (existingMatches.Count > 0)
        {
            _context.SeasonEventMatches.RemoveRange(existingMatches);
        }

        var flight = 1;
        foreach (var pairing in pairings)
        {
            _context.SeasonEventMatches.Add(new SeasonEventMatch
            {
                Id = _shortIdService.GenerateId(),
                SeasonEventId = eventId,
                LeagueId = leagueId,
                HomeTeamId = pairing.Home.Id,
                AwayTeamId = pairing.Away.Id,
                StartingFlight = flight,
                StartingHole = 1,
                IsComplete = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });

            flight++;
        }

        seasonEvent.UpdatedAt = DateTime.UtcNow;
        seasonEvent.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        if (_seasonPointsQueue != null)
        {
            await _seasonPointsQueue.QueueSeasonAsync(leagueId, seasonId, userId);
        }

        _logger.LogInformation("Auto-generated {Count} matchups for event {EventId} in season {SeasonId}",
            pairings.Count, eventId, seasonId);

        return await GetEventMatchupsAsync(seasonId, eventId, leagueId);
    }

    public async Task<EventResponse> ScheduleNextWeekFromEventAsync(string seasonId, string eventId, string leagueId, string userId)
    {
        var sourceEvent = await EnsureEventInSeasonAsync(seasonId, eventId, leagueId);
        var nextEventDate = sourceEvent.EventDate.Date.AddDays(7);

        var nextEvent = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .Where(e => e.SeasonId == seasonId
                && e.LeagueId == leagueId
                && e.EventDate.Date == nextEventDate)
            .OrderBy(e => e.CreatedAt)
            .FirstOrDefaultAsync();

        EventResponse scheduledEvent;
        if (nextEvent == null)
        {
            var existingEventNames = await _context.SeasonEvents
                .IgnoreQueryFilters()
                .Where(e => e.SeasonId == seasonId && e.LeagueId == leagueId && !string.IsNullOrWhiteSpace(e.Name))
                .Select(e => e.Name!)
                .ToListAsync();

            var createRequest = new CreateEventRequest
            {
                EventDate = nextEventDate,
                CourseId = sourceEvent.CourseId,
                TeeId = sourceEvent.TeeId,
                HolesPlayed = sourceEvent.HolesPlayed,
                EventType = sourceEvent.EventType,
                ScoringFormat = sourceEvent.ScoringFormat,
                Name = BuildNextWeekName(sourceEvent.Name, existingEventNames),
                Description = sourceEvent.Description
            };

            scheduledEvent = await CreateEventAsync(createRequest, seasonId, leagueId, userId);
        }
        else
        {
            scheduledEvent = MapToResponse(nextEvent);
        }

        await AutoSetupEventMatchupsFromStandingsAsync(seasonId, scheduledEvent.Id, leagueId, userId);

        _logger.LogInformation(
            "Scheduled next week event {ScheduledEventId} from source event {SourceEventId} in season {SeasonId}",
            scheduledEvent.Id, eventId, seasonId);

        return scheduledEvent;
    }

    public async Task<EventMatchupResponse> UpdateEventMatchupAsync(string seasonId, string eventId, string matchupId, UpdateEventMatchupRequest request, string leagueId, string userId)
    {
        await EnsureEventInSeasonAsync(seasonId, eventId, leagueId);

        var matchup = await _context.SeasonEventMatches
            .IgnoreQueryFilters()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.SeasonEvent)
                .ThenInclude(e => e.Season)
            .FirstOrDefaultAsync(m => m.Id == matchupId && m.SeasonEventId == eventId && m.LeagueId == leagueId);

        if (matchup == null)
        {
            throw new InvalidOperationException("Matchup not found");
        }

        if (request.HomeTeamId != null) matchup.HomeTeamId = string.IsNullOrWhiteSpace(request.HomeTeamId) ? null : request.HomeTeamId;
        if (request.AwayTeamId != null) matchup.AwayTeamId = string.IsNullOrWhiteSpace(request.AwayTeamId) ? null : request.AwayTeamId;
        if (request.HomeSubSeasonGolferId != null) matchup.HomeSubSeasonGolferId = string.IsNullOrWhiteSpace(request.HomeSubSeasonGolferId) ? null : request.HomeSubSeasonGolferId;
        if (request.AwaySubSeasonGolferId != null) matchup.AwaySubSeasonGolferId = string.IsNullOrWhiteSpace(request.AwaySubSeasonGolferId) ? null : request.AwaySubSeasonGolferId;
        if (request.StartingHole.HasValue) matchup.StartingHole = request.StartingHole;
        if (request.StartingFlight.HasValue) matchup.StartingFlight = request.StartingFlight;

        matchup.UpdatedAt = DateTime.UtcNow;
        matchup.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        if (_seasonPointsQueue != null)
        {
            await _seasonPointsQueue.QueueSeasonAsync(leagueId, seasonId, userId);
        }

        if (matchup.SeasonEvent.IsLocked || matchup.SeasonEvent.Season?.IsLocked == true)
        {
            var scoreboard = await _scoringService.BuildEventScoreboardAsync(matchup.SeasonEvent, leagueId);
            await _scoringService.PersistEventScoreboardAsync(matchup.SeasonEvent.Id, leagueId, scoreboard);
        }

        await _context.Entry(matchup).Reference(m => m.HomeTeam).LoadAsync();
        await _context.Entry(matchup).Reference(m => m.AwayTeam).LoadAsync();

        return MapToMatchupResponse(matchup);
    }

    public async Task<int> RecalculateEventHandicapsAsync(string seasonId, string eventId, string leagueId, string userId)
    {
        var seasonEvent = await EnsureEventInSeasonAsync(seasonId, eventId, leagueId);

        var method = await ResolveCalculationMethodAsync(seasonId, leagueId);
        if (!method.HasValue)
        {
            _logger.LogInformation(
                "Skipping handicap recalculation for event {EventId} in season {SeasonId}: handicap type is None",
                eventId, seasonId);
            return 0;
        }

        var eventDate = seasonEvent.EventDate.Date;
        var nextDate = eventDate.AddDays(1);

        var golferIds = await _context.Rounds
            .IgnoreQueryFilters()
            .Where(r => r.LeagueId == leagueId
                && r.IsComplete
                && r.RoundDate >= eventDate
                && r.RoundDate < nextDate
                && (string.IsNullOrWhiteSpace(seasonEvent.CourseId) || r.CourseId == seasonEvent.CourseId)
                && (string.IsNullOrWhiteSpace(seasonEvent.TeeId) || r.TeeId == seasonEvent.TeeId))
            .Select(r => r.GolferId)
            .Distinct()
            .ToListAsync();

        var recalculatedCount = 0;
        foreach (var golferId in golferIds)
        {
            var recalculated = await RecalculateGolferAsync(golferId, leagueId, seasonId, method.Value, userId);
            if (recalculated) recalculatedCount++;
        }

        _logger.LogInformation("Recalculated handicaps for {Count} golfers for event {EventId}",
            recalculatedCount, eventId);

        return recalculatedCount;
    }

    public async Task<bool> RecalculateEventGolferHandicapAsync(string seasonId, string eventId, string golferId, string leagueId, string userId)
    {
        await EnsureEventInSeasonAsync(seasonId, eventId, leagueId);

        var method = await ResolveCalculationMethodAsync(seasonId, leagueId);
        if (!method.HasValue)
        {
            _logger.LogInformation(
                "Skipping handicap recalculation for golfer {GolferId} in season {SeasonId}: handicap type is None",
                golferId, seasonId);
            return false;
        }

        var recalculated = await RecalculateGolferAsync(golferId, leagueId, seasonId, method.Value, userId);
        if (!recalculated)
        {
            throw new InvalidOperationException("Unable to recalculate handicap for golfer");
        }

        return true;
    }

    public async Task<MatchDetailResponse?> GetMatchDetailAsync(string seasonId, string eventId, string matchupId, string leagueId)
    {
        var seasonEvent = await EnsureEventInSeasonAsync(seasonId, eventId, leagueId);
        return await _scoringService.BuildMatchDetailAsync(matchupId, seasonEvent, leagueId);
    }

    private async Task<SeasonEvent> EnsureEventInSeasonAsync(string seasonId, string eventId, string leagueId)
    {
        var seasonEvent = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .Include(e => e.Season)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.SeasonId == seasonId && e.LeagueId == leagueId);

        if (seasonEvent == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        return seasonEvent;
    }

    private async Task<HandicapCalculationMethod?> ResolveCalculationMethodAsync(string seasonId, string leagueId)
    {
        var handicapType = await _context.SeasonSettings
            .IgnoreQueryFilters()
            .Where(s => s.SeasonId == seasonId && s.LeagueId == leagueId)
            .Select(s => (HandicapType?)s.HandicapType)
            .FirstOrDefaultAsync();

        if (!handicapType.HasValue)
        {
            return HandicapCalculationMethod.BobsLeague;
        }

        return handicapType.Value switch
        {
            HandicapType.Bobs => HandicapCalculationMethod.BobsLeague,
            HandicapType.USGA => HandicapCalculationMethod.WorldHandicapSystem,
            HandicapType.Scratch => HandicapCalculationMethod.Scratch,
            HandicapType.None => null,
            _ => HandicapCalculationMethod.BobsLeague
        };
    }

    private async Task<bool> RecalculateGolferAsync(
        string golferId,
        string leagueId,
        string seasonId,
        HandicapCalculationMethod method,
        string userId)
    {
        var request = new CalculateHandicapRequest
        {
            LeagueId = leagueId,
            SeasonId = seasonId,
            Method = method,
            Persist = true
        };

        var result = await _handicapService.CalculateHandicapAsync(golferId, request, userId);
        if (!result.Success)
        {
            _logger.LogWarning(
                "Handicap recalculation failed for golfer {GolferId} in season {SeasonId}: {Message}",
                golferId, seasonId, result.Message);
            return false;
        }

        return true;
    }

    private static string BuildPairKey(string teamAId, string teamBId)
    {
        return string.CompareOrdinal(teamAId, teamBId) <= 0
            ? $"{teamAId}:{teamBId}"
            : $"{teamBId}:{teamAId}";
    }

    private static string BuildNextWeekName(string? sourceName, List<string> existingNames)
    {
        const string weekPrefix = "Week ";
        var weekRegex = new Regex(@"^Week\s+(\d+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        if (!string.IsNullOrWhiteSpace(sourceName))
        {
            var sourceMatch = weekRegex.Match(sourceName.Trim());
            if (sourceMatch.Success && int.TryParse(sourceMatch.Groups[1].Value, out var sourceWeek))
            {
                return $"{weekPrefix}{sourceWeek + 1}";
            }
        }

        var maxExistingWeek = 0;
        foreach (var name in existingNames)
        {
            if (string.IsNullOrWhiteSpace(name)) continue;

            var match = weekRegex.Match(name.Trim());
            if (match.Success && int.TryParse(match.Groups[1].Value, out var week))
            {
                maxExistingWeek = Math.Max(maxExistingWeek, week);
            }
        }

        return maxExistingWeek > 0 ? $"{weekPrefix}{maxExistingWeek + 1}" : string.Empty;
    }

    private static EventMatchupResponse MapToMatchupResponse(SeasonEventMatch matchup)
    {
        return new EventMatchupResponse
        {
            Id = matchup.Id,
            SeasonEventId = matchup.SeasonEventId,
            HomeTeamId = matchup.HomeTeamId,
            HomeTeamName = matchup.HomeTeam?.Name,
            AwayTeamId = matchup.AwayTeamId,
            AwayTeamName = matchup.AwayTeam?.Name,
            HomeSubSeasonGolferId = matchup.HomeSubSeasonGolferId,
            AwaySubSeasonGolferId = matchup.AwaySubSeasonGolferId,
            HomePoints = matchup.HomePoints,
            AwayPoints = matchup.AwayPoints,
            StartingHole = matchup.StartingHole,
            StartingFlight = matchup.StartingFlight,
            IsComplete = matchup.IsComplete
        };
    }

    public async Task<MyMatchupResponse?> GetMyMatchupForEventAsync(string seasonId, string eventId, string leagueId, string userId)
    {
        var golfer = await _context.Golfers
            .FirstOrDefaultAsync(g => g.UserId == userId);

        if (golfer == null) return null;

        var seasonGolfer = await _context.SeasonGolfers
            .FirstOrDefaultAsync(sg => sg.GolferId == golfer.Id && sg.SeasonId == seasonId);

        if (seasonGolfer?.TeamId == null) return null;

        var teamId = seasonGolfer.TeamId;

        var match = await _context.SeasonEventMatches
            .IgnoreQueryFilters()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .FirstOrDefaultAsync(m =>
                m.SeasonEventId == eventId &&
                m.LeagueId == leagueId &&
                (m.HomeTeamId == teamId || m.AwayTeamId == teamId));

        if (match == null) return null;

        bool isHome = match.HomeTeamId == teamId;
        return new MyMatchupResponse
        {
            StartingHole = match.StartingHole,
            StartingFlight = match.StartingFlight,
            MyTeamName = isHome ? match.HomeTeam?.Name : match.AwayTeam?.Name,
            OpponentName = isHome ? match.AwayTeam?.Name : match.HomeTeam?.Name
        };
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
            GameOfDayTitle = seasonEvent.GameOfDayTitle,
            GameOfDayWinnerSeasonGolferId = seasonEvent.GameOfDayWinnerSeasonGolferId,
            GameOfDayWinnerDisplayName = seasonEvent.GameOfDayWinnerDisplayName,
            IsLocked = seasonEvent.IsLocked,
            CreatedAt = seasonEvent.CreatedAt,
            UpdatedAt = seasonEvent.UpdatedAt
        };
    }
}
