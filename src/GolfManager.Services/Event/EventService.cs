using GolfManager.Data;
using GolfManager.Core.Entities;
using GolfManager.Core.Enums;
using GolfManager.Core.Services;
using GolfManager.Services.Handicap;
using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.Handicap;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace GolfManager.Services.Event;

/// <summary>
/// Service for managing season events
/// </summary>
public class EventService : IEventService
{
    private readonly GolfManagerDbContext _context;
    private readonly IShortIdService _shortIdService;
    private readonly IHandicapService _handicapService;
    private readonly ISeasonPointsRecalculationQueue? _seasonPointsQueue;
    private readonly ILogger<EventService> _logger;

    public EventService(
        GolfManagerDbContext context,
        IShortIdService shortIdService,
        IHandicapService handicapService,
        ILogger<EventService> logger)
        : this(context, shortIdService, handicapService, null, logger)
    {
    }

    public EventService(
        GolfManagerDbContext context,
        IShortIdService shortIdService,
        IHandicapService handicapService,
        ISeasonPointsRecalculationQueue? seasonPointsQueue,
        ILogger<EventService> logger)
    {
        _context = context;
        _shortIdService = shortIdService;
        _handicapService = handicapService;
        _seasonPointsQueue = seasonPointsQueue;
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

    public async Task<EventScoreboardResponse> GetEventScoreboardAsync(string seasonId, string eventId, string leagueId)
    {
        var seasonEvent = await EnsureEventInSeasonAsync(seasonId, eventId, leagueId);
        var seasonSettings = await _context.SeasonSettings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.SeasonId == seasonId && s.LeagueId == leagueId)
            ?? new SeasonSettings
            {
                SeasonId = seasonId,
                LeagueId = leagueId,
                IndividualScoringType = IndividualScoringType.TwoPoint,
                TeamScoringType = TeamScoringType.MatchPoints,
                MissingPlayerType = MissingPlayerType.FieldAverage,
                MissingTeamType = MissingTeamType.PartialPoints
            };

        var seasonGolfers = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .Include(sg => sg.LeagueGolfer)
                .ThenInclude(lg => lg.Golfer)
                    .ThenInclude(g => g.User)
            .Where(sg => sg.SeasonId == seasonId && sg.LeagueId == leagueId)
            .OrderBy(sg => sg.LeagueGolfer.DisplayName)
            .ToListAsync();

        var seasonTeams = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .Where(t => t.SeasonId == seasonId && t.LeagueId == leagueId)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var rounds = await GetEventRoundsAsync(seasonEvent, leagueId);
        var matchups = await _context.SeasonEventMatches
            .IgnoreQueryFilters()
            .Where(m => m.SeasonEventId == eventId && m.LeagueId == leagueId)
            .OrderBy(m => m.StartingFlight ?? int.MaxValue)
            .ThenBy(m => m.StartingHole ?? int.MaxValue)
            .ThenBy(m => m.CreatedAt)
            .ToListAsync();

        var activeHoleNumbers = GetHoleNumbersForScoring(seasonEvent.HolesPlayed);
        var holeTees = await GetHoleTeesAsync(seasonEvent.TeeId, activeHoleNumbers);
        var teamLookup = seasonTeams.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
        var roundByLeagueGolferId = rounds
            .Where(r => !string.IsNullOrWhiteSpace(r.LeagueGolferId))
            .GroupBy(r => r.LeagueGolferId!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt).First(), StringComparer.OrdinalIgnoreCase);
        var roundBySeasonGolferId = seasonGolfers
            .Where(sg => roundByLeagueGolferId.ContainsKey(sg.LeagueGolferId))
            .ToDictionary(sg => sg.Id, sg => roundByLeagueGolferId[sg.LeagueGolferId], StringComparer.OrdinalIgnoreCase);

        var playerScores = BuildPlayerScores(seasonGolfers, teamLookup, roundByLeagueGolferId, seasonSettings, seasonEvent, holeTees, activeHoleNumbers);
        var playerLookup = playerScores.ToDictionary(p => p.SeasonGolferId, StringComparer.OrdinalIgnoreCase);
        var matchScores = BuildMatchScores(matchups, seasonGolfers, playerLookup, roundBySeasonGolferId, teamLookup, seasonSettings, holeTees, activeHoleNumbers);

        return new EventScoreboardResponse
        {
            EventId = seasonEvent.Id,
            SeasonId = seasonEvent.SeasonId,
            LeagueId = seasonEvent.LeagueId,
            EventName = seasonEvent.Name,
            EventDate = seasonEvent.EventDate,
            Matches = matchScores,
            Players = playerScores
                .OrderByDescending(p => p.EventPoints ?? double.MinValue)
                .ThenBy(p => p.NetScore ?? p.MissScore ?? double.MaxValue)
                .ThenBy(p => p.DisplayName)
                .ToList()
        };
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

        if (request.GameOfDayTitle != null)
        {
            seasonEvent.GameOfDayTitle = request.GameOfDayTitle;
        }

        if (request.GameOfDayWinnerSeasonGolferId != null)
        {
            seasonEvent.GameOfDayWinnerSeasonGolferId = request.GameOfDayWinnerSeasonGolferId;
        }

        if (request.GameOfDayWinnerDisplayName != null)
        {
            seasonEvent.GameOfDayWinnerDisplayName = request.GameOfDayWinnerDisplayName;
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
            var key = BuildPairKey(pair.HomeTeamId!, pair.AwayTeamId!);
            priorPairs.Add(key);
        }

        var unpaired = new List<SeasonTeam>(seasonTeams);
        var pairings = new List<(SeasonTeam Home, SeasonTeam Away)>();

        while (unpaired.Count > 1)
        {
            var home = unpaired[0];
            var awayIndex = -1;

            for (var i = 1; i < unpaired.Count; i++)
            {
                var candidate = unpaired[i];
                if (!priorPairs.Contains(BuildPairKey(home.Id, candidate.Id)))
                {
                    awayIndex = i;
                    break;
                }
            }

            if (awayIndex == -1)
            {
                awayIndex = 1;
            }

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
            scheduledEvent.Id,
            eventId,
            seasonId);

        return scheduledEvent;
    }

    public async Task<EventMatchupResponse> UpdateEventMatchupAsync(string seasonId, string eventId, string matchupId, UpdateEventMatchupRequest request, string leagueId, string userId)
    {
        await EnsureEventInSeasonAsync(seasonId, eventId, leagueId);

        var matchup = await _context.SeasonEventMatches
            .IgnoreQueryFilters()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .FirstOrDefaultAsync(m => m.Id == matchupId && m.SeasonEventId == eventId && m.LeagueId == leagueId);

        if (matchup == null)
        {
            throw new InvalidOperationException("Matchup not found");
        }

        if (request.HomeTeamId != null)
        {
            matchup.HomeTeamId = string.IsNullOrWhiteSpace(request.HomeTeamId) ? null : request.HomeTeamId;
        }

        if (request.AwayTeamId != null)
        {
            matchup.AwayTeamId = string.IsNullOrWhiteSpace(request.AwayTeamId) ? null : request.AwayTeamId;
        }

        if (request.HomeSubSeasonGolferId != null)
        {
            matchup.HomeSubSeasonGolferId = string.IsNullOrWhiteSpace(request.HomeSubSeasonGolferId)
                ? null
                : request.HomeSubSeasonGolferId;
        }

        if (request.AwaySubSeasonGolferId != null)
        {
            matchup.AwaySubSeasonGolferId = string.IsNullOrWhiteSpace(request.AwaySubSeasonGolferId)
                ? null
                : request.AwaySubSeasonGolferId;
        }

        if (request.StartingHole.HasValue)
        {
            matchup.StartingHole = request.StartingHole;
        }

        if (request.StartingFlight.HasValue)
        {
            matchup.StartingFlight = request.StartingFlight;
        }

        matchup.UpdatedAt = DateTime.UtcNow;
        matchup.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        if (_seasonPointsQueue != null)
        {
            await _seasonPointsQueue.QueueSeasonAsync(leagueId, seasonId, userId);
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
                eventId,
                seasonId);
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
            if (recalculated)
            {
                recalculatedCount++;
            }
        }

        _logger.LogInformation(
            "Recalculated handicaps for {Count} golfers for event {EventId}",
            recalculatedCount,
            eventId);

        return recalculatedCount;
    }

    private async Task<List<Core.Entities.Round>> GetEventRoundsAsync(SeasonEvent seasonEvent, string leagueId)
    {
        var eventDate = seasonEvent.EventDate.Date;
        var nextDate = eventDate.AddDays(1);

        return await _context.Rounds
            .IgnoreQueryFilters()
            .Include(r => r.Holes.OrderBy(h => h.HoleNumber))
            .Where(r => r.LeagueId == leagueId
                && r.RoundDate >= eventDate
                && r.RoundDate < nextDate
                && (string.IsNullOrWhiteSpace(seasonEvent.CourseId) || r.CourseId == seasonEvent.CourseId)
                && (string.IsNullOrWhiteSpace(seasonEvent.TeeId) || r.TeeId == seasonEvent.TeeId))
            .ToListAsync();
    }

    private async Task<List<HoleTee>> GetHoleTeesAsync(string? teeId, List<int> activeHoleNumbers)
    {
        if (string.IsNullOrWhiteSpace(teeId) || activeHoleNumbers.Count == 0)
        {
            return new List<HoleTee>();
        }

        return await _context.HoleTees
            .IgnoreQueryFilters()
            .Where(ht => ht.TeeId == teeId && activeHoleNumbers.Contains(ht.HoleNumber))
            .OrderBy(ht => ht.HoleNumber)
            .ToListAsync();
    }

    private static List<EventPlayerScoreResponse> BuildPlayerScores(
        List<SeasonGolfer> seasonGolfers,
        Dictionary<string, SeasonTeam> teamLookup,
        Dictionary<string, Core.Entities.Round> roundByLeagueGolferId,
        SeasonSettings seasonSettings,
        SeasonEvent seasonEvent,
        List<HoleTee> holeTees,
        List<int> activeHoleNumbers)
    {
        var parTotal = holeTees.Count > 0
            ? holeTees.Sum(h => h.Par)
            : activeHoleNumbers.Count * 4;

        var playerRows = seasonGolfers.Select(sg =>
        {
            roundByLeagueGolferId.TryGetValue(sg.LeagueGolferId, out var round);
            var handicap = sg.SeasonHandicap ?? sg.LeagueGolfer.LeagueHandicap;
            var rawScore = round?.TotalScore ?? round?.Holes.Where(h => activeHoleNumbers.Contains(h.HoleNumber)).Sum(h => h.GrossScore ?? 0);
            if (round != null && rawScore == 0 && !round.Holes.Any(h => activeHoleNumbers.Contains(h.HoleNumber) && h.GrossScore.HasValue))
            {
                rawScore = null;
            }

            return new EventPlayerScoreResponse
            {
                SeasonGolferId = sg.Id,
                LeagueGolferId = sg.LeagueGolferId,
                GolferId = sg.GolferId,
                DisplayName = sg.LeagueGolfer.DisplayName,
                TeamId = sg.TeamId,
                TeamName = sg.TeamId != null && teamLookup.TryGetValue(sg.TeamId, out var team) ? team.Name : null,
                RawScore = rawScore,
                Handicap = handicap,
                NetScore = rawScore.HasValue
                    ? CalculateNetRoundScore(round, handicap ?? 0, holeTees, activeHoleNumbers, seasonSettings.MaxHandicap)
                    : null
            };
        }).ToList();

        var availableNetScores = playerRows.Where(p => p.NetScore.HasValue).Select(p => p.NetScore!.Value).ToList();
        var averageNet = availableNetScores.Count > 0 ? availableNetScores.Average() : (double?)null;

        foreach (var row in playerRows.Where(p => !p.NetScore.HasValue))
        {
            row.MissCount = 1;
            row.MissScore = seasonSettings.MissingPlayerType switch
            {
                MissingPlayerType.PlayAgainstPar => parTotal,
                MissingPlayerType.FieldAverage => averageNet ?? parTotal,
                MissingPlayerType.BlindDraw => averageNet ?? parTotal,
                _ => null
            };
        }

        var rankedRows = playerRows
            .Where(p => (p.NetScore ?? p.MissScore).HasValue)
            .OrderBy(p => p.NetScore ?? p.MissScore ?? double.MaxValue)
            .ThenBy(p => p.DisplayName)
            .ToList();

        var currentStandingPoints = rankedRows.Count;
        foreach (var scoreGroup in rankedRows
            .GroupBy(p => Math.Round((p.NetScore ?? p.MissScore) ?? double.MaxValue, 2))
            .OrderBy(g => g.Key))
        {
            var pointsBucket = 0;
            for (var i = 0; i < scoreGroup.Count(); i++)
            {
                pointsBucket += currentStandingPoints - i;
            }

            var eventPosition = rankedRows.Count - currentStandingPoints + 1;
            var eventPoints = seasonSettings.IndividualScoringType == IndividualScoringType.None
                ? (double?)null
                : pointsBucket / (scoreGroup.Count() * 1.0);

            foreach (var row in scoreGroup)
            {
                row.EventPosition = eventPosition;
                row.EventPoints = eventPoints;
            }

            currentStandingPoints -= scoreGroup.Count();
        }

        return playerRows;
    }

    private static List<EventMatchScoreResponse> BuildMatchScores(
        List<SeasonEventMatch> matchups,
        List<SeasonGolfer> seasonGolfers,
        Dictionary<string, EventPlayerScoreResponse> playerLookup,
        Dictionary<string, Core.Entities.Round> roundBySeasonGolferId,
        Dictionary<string, SeasonTeam> teamLookup,
        SeasonSettings seasonSettings,
        List<HoleTee> holeTees,
        List<int> activeHoleNumbers)
    {
        var seasonGolferLookup = seasonGolfers.ToDictionary(sg => sg.Id, StringComparer.OrdinalIgnoreCase);

        return matchups.Select(matchup =>
        {
            var homeIds = ResolveMatchParticipants(seasonGolfers, matchup.HomeTeamId, matchup.HomeSubSeasonGolferId);
            var awayIds = ResolveMatchParticipants(seasonGolfers, matchup.AwayTeamId, matchup.AwaySubSeasonGolferId);

            var homeMembers = homeIds
                .Where(playerLookup.ContainsKey)
                .Select(id => ToTeamMemberScore(playerLookup[id], string.Equals(matchup.HomeSubSeasonGolferId, id, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            var awayMembers = awayIds
                .Where(playerLookup.ContainsKey)
                .Select(id => ToTeamMemberScore(playerLookup[id], string.Equals(matchup.AwaySubSeasonGolferId, id, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var calculated = seasonSettings.TeamScoringType == TeamScoringType.MatchPoints
                ? CalculateMatchPoints(homeIds, awayIds, seasonGolferLookup, roundBySeasonGolferId, holeTees, activeHoleNumbers, seasonSettings.MaxHandicap)
                : (matchup.HomePoints, matchup.AwayPoints, matchup.IsComplete);

            return new EventMatchScoreResponse
            {
                MatchupId = matchup.Id,
                HomeTeamId = matchup.HomeTeamId,
                HomeTeamName = matchup.HomeTeamId != null && teamLookup.TryGetValue(matchup.HomeTeamId, out var homeTeam) ? homeTeam.Name : null,
                HomeSubSeasonGolferId = matchup.HomeSubSeasonGolferId,
                HomeSubDisplayName = matchup.HomeSubSeasonGolferId != null && seasonGolferLookup.TryGetValue(matchup.HomeSubSeasonGolferId, out var homeSub) ? homeSub.LeagueGolfer.DisplayName : null,
                HomePoints = calculated.HomePoints,
                AwayTeamId = matchup.AwayTeamId,
                AwayTeamName = matchup.AwayTeamId != null && teamLookup.TryGetValue(matchup.AwayTeamId, out var awayTeam) ? awayTeam.Name : null,
                AwaySubSeasonGolferId = matchup.AwaySubSeasonGolferId,
                AwaySubDisplayName = matchup.AwaySubSeasonGolferId != null && seasonGolferLookup.TryGetValue(matchup.AwaySubSeasonGolferId, out var awaySub) ? awaySub.LeagueGolfer.DisplayName : null,
                AwayPoints = calculated.AwayPoints,
                StartingHole = matchup.StartingHole,
                StartingFlight = matchup.StartingFlight,
                IsComplete = calculated.IsComplete,
                HomeMembers = homeMembers,
                AwayMembers = awayMembers
            };
        }).ToList();
    }

    private static EventTeamMemberScoreResponse ToTeamMemberScore(EventPlayerScoreResponse player, bool isSubstitute)
    {
        return new EventTeamMemberScoreResponse
        {
            SeasonGolferId = player.SeasonGolferId,
            LeagueGolferId = player.LeagueGolferId,
            GolferId = player.GolferId,
            DisplayName = player.DisplayName,
            Handicap = player.Handicap,
            RawScore = player.RawScore,
            NetScore = player.NetScore ?? player.MissScore,
            IsSubstitute = isSubstitute
        };
    }

    private static List<string> ResolveMatchParticipants(List<SeasonGolfer> seasonGolfers, string? teamId, string? subSeasonGolferId)
    {
        if (!string.IsNullOrWhiteSpace(subSeasonGolferId))
        {
            return new List<string> { subSeasonGolferId };
        }

        if (string.IsNullOrWhiteSpace(teamId))
        {
            return new List<string>();
        }

        return seasonGolfers
            .Where(sg => string.Equals(sg.TeamId, teamId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(sg => sg.LeagueGolfer.DisplayName)
            .Select(sg => sg.Id)
            .ToList();
    }

    private static (double? HomePoints, double? AwayPoints, bool IsComplete) CalculateMatchPoints(
        List<string> homeIds,
        List<string> awayIds,
        Dictionary<string, SeasonGolfer> seasonGolferLookup,
        Dictionary<string, Core.Entities.Round> roundBySeasonGolferId,
        List<HoleTee> holeTees,
        List<int> activeHoleNumbers,
        int? maxHandicap)
    {
        if (homeIds.Count == 0 && awayIds.Count == 0)
        {
            return (null, null, false);
        }

        var homeComplete = homeIds.Any(id => roundBySeasonGolferId.TryGetValue(id, out var round) && round.IsComplete);
        var awayComplete = awayIds.Any(id => roundBySeasonGolferId.TryGetValue(id, out var round) && round.IsComplete);

        if (holeTees.Count == 0)
        {
            var homeFallback = homeIds
                .Where(roundBySeasonGolferId.ContainsKey)
                .Select(id => CalculateNetRoundScore(roundBySeasonGolferId[id], seasonGolferLookup[id].SeasonHandicap ?? seasonGolferLookup[id].LeagueGolfer.LeagueHandicap ?? 0, holeTees, activeHoleNumbers, maxHandicap))
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .OrderBy(x => x)
                .Take(2)
                .Sum();
            var awayFallback = awayIds
                .Where(roundBySeasonGolferId.ContainsKey)
                .Select(id => CalculateNetRoundScore(roundBySeasonGolferId[id], seasonGolferLookup[id].SeasonHandicap ?? seasonGolferLookup[id].LeagueGolfer.LeagueHandicap ?? 0, holeTees, activeHoleNumbers, maxHandicap))
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .OrderBy(x => x)
                .Take(2)
                .Sum();

            if (homeFallback == 0 && awayFallback == 0)
            {
                return (null, null, homeComplete && awayComplete);
            }

            return homeFallback == awayFallback
                ? (11, 11, homeComplete && awayComplete)
                : homeFallback < awayFallback
                    ? (22, 0, homeComplete && awayComplete)
                    : (0, 22, homeComplete && awayComplete);
        }

        var strokeAllocations = homeIds.Concat(awayIds)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(seasonGolferLookup.ContainsKey)
            .ToDictionary(
                id => id,
                id => AllocateHandicapStrokes(
                    seasonGolferLookup[id].SeasonHandicap ?? seasonGolferLookup[id].LeagueGolfer.LeagueHandicap ?? 0,
                    holeTees,
                    maxHandicap),
                StringComparer.OrdinalIgnoreCase);

        double homePoints = 0;
        double awayPoints = 0;

        foreach (var holeNumber in activeHoleNumbers)
        {
            var homeScores = homeIds
                .Select(id => GetHoleNetScore(roundBySeasonGolferId, strokeAllocations, id, holeNumber))
                .Where(score => score.HasValue)
                .Select(score => score!.Value)
                .OrderBy(score => score)
                .ToList();
            var awayScores = awayIds
                .Select(id => GetHoleNetScore(roundBySeasonGolferId, strokeAllocations, id, holeNumber))
                .Where(score => score.HasValue)
                .Select(score => score!.Value)
                .OrderBy(score => score)
                .ToList();

            if (homeScores.Count == 0 && awayScores.Count == 0)
            {
                continue;
            }

            if (homeScores.Count == 0)
            {
                homePoints += 0.5;
                awayPoints += 1.5;
                continue;
            }

            if (awayScores.Count == 0)
            {
                homePoints += 1.5;
                awayPoints += 0.5;
                continue;
            }

            var scoresUsed = Math.Max(1, Math.Min(Math.Min(homeScores.Count, awayScores.Count), 2));
            var homeNet = homeScores.Take(scoresUsed).Sum();
            var awayNet = awayScores.Take(scoresUsed).Sum();

            if (homeNet < awayNet)
            {
                homePoints += 2;
            }
            else if (awayNet < homeNet)
            {
                awayPoints += 2;
            }
            else
            {
                homePoints += 1;
                awayPoints += 1;
            }
        }

        if (homePoints > awayPoints)
        {
            homePoints += 4;
        }
        else if (awayPoints > homePoints)
        {
            awayPoints += 4;
        }
        else if (homePoints > 0 || awayPoints > 0)
        {
            homePoints += 2;
            awayPoints += 2;
        }

        return (homePoints, awayPoints, homeComplete && awayComplete);
    }

    private static Dictionary<int, int> AllocateHandicapStrokes(double handicap, List<HoleTee> holeTees, int? maxHandicap)
    {
        var cappedHandicap = Math.Abs((int)Math.Floor(handicap));
        if (maxHandicap.HasValue)
        {
            cappedHandicap = Math.Min(cappedHandicap, maxHandicap.Value);
        }

        var sign = handicap < 0 ? -1 : 1;
        var strokeMap = holeTees.ToDictionary(h => h.HoleNumber, _ => 0);
        var orderedHoles = sign > 0
            ? holeTees.OrderBy(h => h.Handicap).ToList()
            : holeTees.OrderByDescending(h => h.Handicap).ToList();

        var currentIndex = 0;
        while (cappedHandicap > 0 && orderedHoles.Count > 0)
        {
            var holeNumber = orderedHoles[currentIndex].HoleNumber;
            strokeMap[holeNumber] += sign;
            currentIndex = (currentIndex + 1) % orderedHoles.Count;
            cappedHandicap--;
        }

        return strokeMap;
    }

    private static double? GetHoleNetScore(
        Dictionary<string, Core.Entities.Round> roundBySeasonGolferId,
        Dictionary<string, Dictionary<int, int>> strokeAllocations,
        string seasonGolferId,
        int holeNumber)
    {
        if (!roundBySeasonGolferId.TryGetValue(seasonGolferId, out var round))
        {
            return null;
        }

        var hole = round.Holes.FirstOrDefault(h => h.HoleNumber == holeNumber);
        if (hole?.GrossScore == null)
        {
            return null;
        }

        var strokes = strokeAllocations.TryGetValue(seasonGolferId, out var playerStrokes)
            ? playerStrokes.GetValueOrDefault(holeNumber)
            : 0;

        return hole.GrossScore.Value - strokes;
    }

    private static double? CalculateNetRoundScore(Core.Entities.Round? round, double handicap, List<HoleTee> holeTees, List<int> activeHoleNumbers, int? maxHandicap)
    {
        if (round == null)
        {
            return null;
        }

        if (round.NetScore.HasValue)
        {
            return round.NetScore.Value;
        }

        var scoredHoles = round.Holes.Where(h => activeHoleNumbers.Contains(h.HoleNumber) && h.GrossScore.HasValue).ToList();
        if (scoredHoles.Count == 0)
        {
            return round.TotalScore.HasValue ? round.TotalScore.Value - handicap : null;
        }

        if (holeTees.Count == 0)
        {
            return scoredHoles.Sum(h => h.GrossScore ?? 0) - handicap;
        }

        var strokeMap = AllocateHandicapStrokes(handicap, holeTees, maxHandicap);
        return scoredHoles.Sum(h => (h.GrossScore ?? 0) - strokeMap.GetValueOrDefault(h.HoleNumber));
    }

    private static List<int> GetHoleNumbersForScoring(HolesPlayed holesPlayed)
    {
        return holesPlayed switch
        {
            HolesPlayed.Back => Enumerable.Range(10, 9).ToList(),
            HolesPlayed.Eighteen => Enumerable.Range(1, 18).ToList(),
            HolesPlayed.Front => Enumerable.Range(1, 9).ToList(),
            HolesPlayed.Nine => Enumerable.Range(1, 9).ToList(),
            _ => new List<int>()
        };
    }

    public async Task<bool> RecalculateEventGolferHandicapAsync(string seasonId, string eventId, string golferId, string leagueId, string userId)
    {
        await EnsureEventInSeasonAsync(seasonId, eventId, leagueId);

        var method = await ResolveCalculationMethodAsync(seasonId, leagueId);
        if (!method.HasValue)
        {
            _logger.LogInformation(
                "Skipping handicap recalculation for golfer {GolferId} in season {SeasonId}: handicap type is None",
                golferId,
                seasonId);
            return false;
        }

        var recalculated = await RecalculateGolferAsync(golferId, leagueId, seasonId, method.Value, userId);
        if (!recalculated)
        {
            throw new InvalidOperationException("Unable to recalculate handicap for golfer");
        }

        return true;
    }

    public async Task<int> RecalculateSeasonTeamStandingsAsync(string seasonId, string leagueId, string userId)
    {
        var seasonTeams = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .Where(t => t.SeasonId == seasonId && t.LeagueId == leagueId)
            .ToListAsync();

        if (seasonTeams.Count == 0)
        {
            return 0;
        }

        var eventIds = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .Where(e => e.SeasonId == seasonId && e.LeagueId == leagueId)
            .OrderBy(e => e.EventDate)
            .Select(e => e.Id)
            .ToListAsync();

        var aggregates = seasonTeams.ToDictionary(
            t => t.Id,
            _ => new TeamAggregate(),
            StringComparer.OrdinalIgnoreCase);

        foreach (var eventId in eventIds)
        {
            var scoreboard = await GetEventScoreboardAsync(seasonId, eventId, leagueId);
            foreach (var match in scoreboard.Matches)
            {
                if (string.IsNullOrWhiteSpace(match.HomeTeamId) || string.IsNullOrWhiteSpace(match.AwayTeamId))
                {
                    continue;
                }

                if (!aggregates.TryGetValue(match.HomeTeamId, out var home) || !aggregates.TryGetValue(match.AwayTeamId, out var away))
                {
                    continue;
                }

                if (match.HomePoints.HasValue)
                {
                    home.SeasonPoints += match.HomePoints.Value;
                }

                if (match.AwayPoints.HasValue)
                {
                    away.SeasonPoints += match.AwayPoints.Value;
                }

                if (!match.HomePoints.HasValue || !match.AwayPoints.HasValue)
                {
                    continue;
                }

                if (match.HomePoints.Value > match.AwayPoints.Value)
                {
                    home.Wins++;
                    away.Losses++;
                }
                else if (match.HomePoints.Value < match.AwayPoints.Value)
                {
                    home.Losses++;
                    away.Wins++;
                }
                else
                {
                    home.Ties++;
                    away.Ties++;
                }
            }
        }

        foreach (var team in seasonTeams)
        {
            var aggregate = aggregates[team.Id];
            team.Wins = aggregate.Wins;
            team.Losses = aggregate.Losses;
            team.Ties = aggregate.Ties;
            team.SeasonPoints = Math.Round(aggregate.SeasonPoints, 2);
            team.UpdatedAt = DateTime.UtcNow;
            team.UpdatedBy = userId;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Recalculated season standings for season {SeasonId}: {TeamCount} teams updated", seasonId, seasonTeams.Count);

        return seasonTeams.Count;
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
                golferId,
                seasonId,
                result.Message);
            return false;
        }

        return true;
    }

    private async Task<SeasonEvent> EnsureEventInSeasonAsync(string seasonId, string eventId, string leagueId)
    {
        var seasonEvent = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == eventId && e.SeasonId == seasonId && e.LeagueId == leagueId);

        if (seasonEvent == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        return seasonEvent;
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
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

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

    private sealed class TeamAggregate
    {
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }
        public double SeasonPoints { get; set; }
    }
}

