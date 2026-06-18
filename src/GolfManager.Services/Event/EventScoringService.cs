using GolfManager.Core.Common;
using GolfManager.Core.Entities;
using GolfManager.Core.Enums;
using GolfManager.Core.Services;
using GolfManager.Data;
using GolfManager.Shared.DTOs.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.Event;

public class EventScoringService : IEventScoringService
{
    private readonly GolfManagerDbContext _context;
    private readonly IShortIdService _shortIdService;
    private readonly ILogger<EventScoringService> _logger;

    public EventScoringService(
        GolfManagerDbContext context,
        IShortIdService shortIdService,
        ILogger<EventScoringService> logger)
    {
        _context = context;
        _shortIdService = shortIdService;
        _logger = logger;
    }

    public async Task<EventScoreboardResponse> BuildEventScoreboardAsync(SeasonEvent seasonEvent, string leagueId)
    {
        var seasonSettings = await _context.SeasonSettings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.SeasonId == seasonEvent.SeasonId && s.LeagueId == leagueId)
            ?? new SeasonSettings
            {
                SeasonId = seasonEvent.SeasonId,
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
            .Where(sg => sg.SeasonId == seasonEvent.SeasonId && sg.LeagueId == leagueId)
            .OrderBy(sg => sg.LeagueGolfer.DisplayName)
            .ToListAsync();

        var seasonTeams = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .Where(t => t.SeasonId == seasonEvent.SeasonId && t.LeagueId == leagueId)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var rounds = await GetEventRoundsAsync(seasonEvent, leagueId);
        var matchups = await _context.SeasonEventMatches
            .IgnoreQueryFilters()
            .Where(m => m.SeasonEventId == seasonEvent.Id && m.LeagueId == leagueId)
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

        if (playerScores.Any(p => p.MissCount.HasValue))
        {
            var priorMissCounts = await GetCumulativeMissCountsAsync(
                seasonEvent.SeasonId, leagueId, seasonEvent.EventDate, excludeEventId: seasonEvent.Id);
            foreach (var player in playerScores.Where(p => p.MissCount.HasValue))
            {
                var prior = priorMissCounts.TryGetValue(player.SeasonGolferId, out var c) ? c : 0;
                player.MissCount = prior + 1;
            }
        }

        var subSeasonGolferIds = matchups
            .SelectMany(m => new[] { m.HomeSubSeasonGolferId, m.AwaySubSeasonGolferId })
            .Where(id => id != null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase)!;
        foreach (var p in playerScores.Where(p => subSeasonGolferIds.Contains(p.SeasonGolferId)))
            p.IsSubstitute = true;

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

    public async Task PersistEventScoreboardAsync(string eventId, string leagueId, EventScoreboardResponse scoreboard)
    {
        var existingPlayers = await _context.SeasonEventPlayerScores
            .IgnoreQueryFilters()
            .Where(ps => ps.SeasonEventId == eventId && ps.LeagueId == leagueId)
            .ToListAsync();
        if (existingPlayers.Count > 0)
            _context.SeasonEventPlayerScores.RemoveRange(existingPlayers);

        var existingMatches = await _context.SeasonEventMatchScores
            .IgnoreQueryFilters()
            .Where(ms => ms.SeasonEventId == eventId && ms.LeagueId == leagueId)
            .ToListAsync();
        if (existingMatches.Count > 0)
            _context.SeasonEventMatchScores.RemoveRange(existingMatches);

        foreach (var player in scoreboard.Players)
        {
            _context.SeasonEventPlayerScores.Add(new SeasonEventPlayerScore
            {
                Id = _shortIdService.GenerateId(),
                SeasonEventId = eventId,
                SeasonGolferId = player.SeasonGolferId,
                LeagueId = leagueId,
                RawScore = player.RawScore,
                Handicap = player.Handicap,
                NetScore = player.NetScore,
                EventPoints = player.EventPoints,
                IsMissing = player.MissCount.HasValue && player.MissCount > 0,
                MissScore = player.MissScore,
                DisplayName = player.DisplayName,
                TeamId = player.TeamId,
                TeamName = player.TeamName
            });
        }

        foreach (var match in scoreboard.Matches)
        {
            _context.SeasonEventMatchScores.Add(new SeasonEventMatchScore
            {
                Id = _shortIdService.GenerateId(),
                SeasonEventId = eventId,
                SeasonEventMatchId = match.MatchupId,
                LeagueId = leagueId,
                HomeTeamId = match.HomeTeamId,
                HomeTeamName = match.HomeTeamName,
                HomePoints = match.HomePoints,
                AwayTeamId = match.AwayTeamId,
                AwayTeamName = match.AwayTeamName,
                AwayPoints = match.AwayPoints,
                IsComplete = match.IsComplete,
                StartingHole = match.StartingHole,
                StartingFlight = match.StartingFlight
            });
        }

        // Sync round NetScore to match the definitive event scoring
        var leagueGolferIds = scoreboard.Players
            .Where(p => !string.IsNullOrEmpty(p.LeagueGolferId) && p.Handicap.HasValue && p.RawScore.HasValue)
            .Select(p => p.LeagueGolferId!)
            .ToList();

        if (leagueGolferIds.Any())
        {
            var evt = await _context.SeasonEvents
                .IgnoreQueryFilters()
                .Where(e => e.Id == eventId && e.LeagueId == leagueId)
                .Select(e => new { e.EventDate, e.CourseId })
                .FirstOrDefaultAsync();

            if (evt != null)
            {
                var eventDate = evt.EventDate.Date;
                var eventRounds = await _context.Rounds
                    .IgnoreQueryFilters()
                    .Where(r => r.LeagueId == leagueId
                             && r.LeagueGolferId != null
                             && leagueGolferIds.Contains(r.LeagueGolferId)
                             && r.CourseId == evt.CourseId
                             && r.RoundDate.Date == eventDate)
                    .ToListAsync();

                var scoredByLeagueGolferId = scoreboard.Players
                    .Where(p => !string.IsNullOrEmpty(p.LeagueGolferId) && p.Handicap.HasValue)
                    .ToDictionary(p => p.LeagueGolferId!, p => p, StringComparer.OrdinalIgnoreCase);

                foreach (var round in eventRounds)
                {
                    if (scoredByLeagueGolferId.TryGetValue(round.LeagueGolferId!, out var scored))
                    {
                        round.NetScore = scored.NetScore.HasValue ? (int)Math.Round(scored.NetScore.Value) : null;
                    }
                }
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Persisted scoreboard for event {EventId}: {Players} players, {Matches} matches",
            eventId, scoreboard.Players.Count, scoreboard.Matches.Count);
    }

    public async Task<EventScoreboardResponse?> TryBuildFromStoredAsync(SeasonEvent seasonEvent, string leagueId)
    {
        var storedPlayers = await _context.SeasonEventPlayerScores
            .IgnoreQueryFilters()
            .Include(ps => ps.SeasonGolfer)
                .ThenInclude(sg => sg!.LeagueGolfer)
            .Where(ps => ps.SeasonEventId == seasonEvent.Id && ps.LeagueId == leagueId)
            .ToListAsync();

        if (storedPlayers.Count == 0)
            return null;

        var storedMatches = await _context.SeasonEventMatchScores
            .IgnoreQueryFilters()
            .Where(ms => ms.SeasonEventId == seasonEvent.Id && ms.LeagueId == leagueId)
            .ToListAsync();

        var matchups = await _context.SeasonEventMatches
            .IgnoreQueryFilters()
            .Where(m => m.SeasonEventId == seasonEvent.Id && m.LeagueId == leagueId)
            .OrderBy(m => m.StartingFlight ?? int.MaxValue)
            .ThenBy(m => m.StartingHole ?? int.MaxValue)
            .ThenBy(m => m.CreatedAt)
            .ToListAsync();

        var matchScoreLookup = storedMatches
            .ToDictionary(ms => ms.SeasonEventMatchId, StringComparer.OrdinalIgnoreCase);

        var ranked = storedPlayers
            .Where(ps => (ps.NetScore ?? ps.MissScore).HasValue)
            .OrderBy(ps => ps.NetScore ?? ps.MissScore ?? double.MaxValue)
            .ThenBy(ps => ps.DisplayName)
            .ToList();

        var positions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var standingPoints = ranked.Count;
        foreach (var group in ranked
            .GroupBy(ps => Math.Round((ps.NetScore ?? ps.MissScore) ?? double.MaxValue, 2))
            .OrderBy(g => g.Key))
        {
            var pos = ranked.Count - standingPoints + 1;
            foreach (var ps in group)
                positions[ps.SeasonGolferId] = pos;
            standingPoints -= group.Count();
        }

        var cumulativeMissCounts = await GetCumulativeMissCountsAsync(
            seasonEvent.SeasonId, leagueId, seasonEvent.EventDate);

        var playerResponses = storedPlayers
            .Select(ps =>
            {
                var sg = ps.SeasonGolfer;
                return new EventPlayerScoreResponse
                {
                    SeasonGolferId = ps.SeasonGolferId,
                    LeagueGolferId = sg?.LeagueGolferId ?? string.Empty,
                    GolferId = sg?.GolferId ?? string.Empty,
                    DisplayName = ps.DisplayName,
                    TeamId = ps.TeamId,
                    TeamName = ps.TeamName,
                    RawScore = ps.RawScore,
                    Handicap = ps.Handicap,
                    NetScore = ps.NetScore,
                    EventPoints = ps.EventPoints,
                    EventPosition = positions.TryGetValue(ps.SeasonGolferId, out var pos) ? pos : null,
                    MissCount = ps.IsMissing
                        ? (cumulativeMissCounts.TryGetValue(ps.SeasonGolferId, out var mc) ? mc : 1)
                        : null,
                    MissScore = ps.MissScore
                };
            })
            .OrderByDescending(p => p.EventPoints ?? double.MinValue)
            .ThenBy(p => p.NetScore ?? p.MissScore ?? double.MaxValue)
            .ThenBy(p => p.DisplayName)
            .ToList();

        var storedSubIds = matchups
            .SelectMany(m => new[] { m.HomeSubSeasonGolferId, m.AwaySubSeasonGolferId })
            .Where(id => id != null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase)!;
        foreach (var p in playerResponses.Where(p => storedSubIds.Contains(p.SeasonGolferId)))
            p.IsSubstitute = true;

        var playerLookup = playerResponses.ToDictionary(p => p.SeasonGolferId, StringComparer.OrdinalIgnoreCase);

        var seasonGolfers = storedPlayers
            .Where(ps => ps.SeasonGolfer != null)
            .Select(ps => ps.SeasonGolfer!)
            .ToList();

        var matchResponses = matchups.Select(matchup =>
        {
            matchScoreLookup.TryGetValue(matchup.Id, out var ms);

            var homeIds = ResolveMatchParticipants(seasonGolfers, matchup.HomeTeamId, matchup.HomeSubSeasonGolferId);
            var awayIds = ResolveMatchParticipants(seasonGolfers, matchup.AwayTeamId, matchup.AwaySubSeasonGolferId);

            var homeMembers = homeIds
                .Where(playerLookup.ContainsKey)
                .Select(id => ToTeamMemberScore(playerLookup[id],
                    string.Equals(matchup.HomeSubSeasonGolferId, id, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            var awayMembers = awayIds
                .Where(playerLookup.ContainsKey)
                .Select(id => ToTeamMemberScore(playerLookup[id],
                    string.Equals(matchup.AwaySubSeasonGolferId, id, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return new EventMatchScoreResponse
            {
                MatchupId = matchup.Id,
                HomeTeamId = ms?.HomeTeamId ?? matchup.HomeTeamId,
                HomeTeamName = ms?.HomeTeamName,
                HomeSubSeasonGolferId = matchup.HomeSubSeasonGolferId,
                HomeSubDisplayName = matchup.HomeSubSeasonGolferId != null
                    && playerLookup.TryGetValue(matchup.HomeSubSeasonGolferId, out var hSub)
                    ? hSub.DisplayName : null,
                HomePoints = ms?.HomePoints ?? matchup.HomePoints,
                AwayTeamId = ms?.AwayTeamId ?? matchup.AwayTeamId,
                AwayTeamName = ms?.AwayTeamName,
                AwaySubSeasonGolferId = matchup.AwaySubSeasonGolferId,
                AwaySubDisplayName = matchup.AwaySubSeasonGolferId != null
                    && playerLookup.TryGetValue(matchup.AwaySubSeasonGolferId, out var aSub)
                    ? aSub.DisplayName : null,
                AwayPoints = ms?.AwayPoints ?? matchup.AwayPoints,
                StartingHole = ms?.StartingHole ?? matchup.StartingHole,
                StartingFlight = ms?.StartingFlight ?? matchup.StartingFlight,
                IsComplete = ms?.IsComplete ?? matchup.IsComplete,
                HomeMembers = homeMembers,
                AwayMembers = awayMembers
            };
        }).ToList();

        return new EventScoreboardResponse
        {
            EventId = seasonEvent.Id,
            SeasonId = seasonEvent.SeasonId,
            LeagueId = seasonEvent.LeagueId,
            EventName = seasonEvent.Name,
            EventDate = seasonEvent.EventDate,
            Matches = matchResponses,
            Players = playerResponses
        };
    }

    public async Task<int> RecalculateSeasonTeamStandingsAsync(string seasonId, string leagueId, string userId)
    {
        var seasonTeams = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .Where(t => t.SeasonId == seasonId && t.LeagueId == leagueId)
            .ToListAsync();

        if (seasonTeams.Count == 0) return 0;

        var seasonEvents = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .Where(e => e.SeasonId == seasonId && e.LeagueId == leagueId)
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        if (seasonEvents.Count == 0) return 0;

        var eventIds = seasonEvents.Select(e => e.Id).ToList();

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
            .ToListAsync();

        var allMatchups = await _context.SeasonEventMatches
            .IgnoreQueryFilters()
            .Where(m => eventIds.Contains(m.SeasonEventId) && m.LeagueId == leagueId)
            .ToListAsync();
        var matchupsByEventId = allMatchups
            .GroupBy(m => m.SeasonEventId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var minDate = seasonEvents.Min(e => e.EventDate.Date);
        var maxDate = seasonEvents.Max(e => e.EventDate.Date).AddDays(1);
        var allRounds = await _context.Rounds
            .IgnoreQueryFilters()
            .Include(r => r.Holes.OrderBy(h => h.HoleNumber))
            .Where(r => r.LeagueId == leagueId && r.RoundDate >= minDate && r.RoundDate < maxDate)
            .ToListAsync();

        var teeIds = seasonEvents
            .Where(e => !string.IsNullOrWhiteSpace(e.TeeId))
            .Select(e => e.TeeId!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var allHoleTees = teeIds.Count > 0
            ? await _context.HoleTees.IgnoreQueryFilters().Where(ht => teeIds.Contains(ht.TeeId)).ToListAsync()
            : new List<HoleTee>();
        var holeTeesByTeeId = allHoleTees
            .GroupBy(ht => ht.TeeId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var teamLookup = seasonTeams.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
        var aggregates = seasonTeams.ToDictionary(t => t.Id, _ => new TeamAggregate(), StringComparer.OrdinalIgnoreCase);

        foreach (var seasonEvent in seasonEvents)
        {
            if (seasonEvent.EventType == SeasonEventType.Playoff) continue;

            var eventMatchups = matchupsByEventId.GetValueOrDefault(seasonEvent.Id, new List<SeasonEventMatch>());
            if (eventMatchups.Count == 0) continue;

            var eventDate = seasonEvent.EventDate.Date;
            var nextDate = eventDate.AddDays(1);
            var eventRounds = allRounds.Where(r =>
                r.RoundDate >= eventDate && r.RoundDate < nextDate
                && (string.IsNullOrWhiteSpace(seasonEvent.CourseId) || string.Equals(r.CourseId, seasonEvent.CourseId, StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrWhiteSpace(seasonEvent.TeeId) || string.Equals(r.TeeId, seasonEvent.TeeId, StringComparison.OrdinalIgnoreCase))).ToList();

            var activeHoleNumbers = GetHoleNumbersForScoring(seasonEvent.HolesPlayed);
            var holeTees = seasonEvent.TeeId != null && holeTeesByTeeId.TryGetValue(seasonEvent.TeeId, out var ht)
                ? ht.Where(h => activeHoleNumbers.Contains(h.HoleNumber)).ToList()
                : new List<HoleTee>();

            var roundByLeagueGolferId = eventRounds
                .Where(r => !string.IsNullOrWhiteSpace(r.LeagueGolferId))
                .GroupBy(r => r.LeagueGolferId!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt).First(), StringComparer.OrdinalIgnoreCase);
            var roundBySeasonGolferId = seasonGolfers
                .Where(sg => roundByLeagueGolferId.ContainsKey(sg.LeagueGolferId))
                .ToDictionary(sg => sg.Id, sg => roundByLeagueGolferId[sg.LeagueGolferId], StringComparer.OrdinalIgnoreCase);

            var playerScores = BuildPlayerScores(seasonGolfers, teamLookup, roundByLeagueGolferId, seasonSettings, seasonEvent, holeTees, activeHoleNumbers);
            var playerLookup = playerScores.ToDictionary(p => p.SeasonGolferId, StringComparer.OrdinalIgnoreCase);
            var matches = BuildMatchScores(eventMatchups, seasonGolfers, playerLookup, roundBySeasonGolferId, teamLookup, seasonSettings, holeTees, activeHoleNumbers);

            foreach (var match in matches)
            {
                if (string.IsNullOrWhiteSpace(match.HomeTeamId) || string.IsNullOrWhiteSpace(match.AwayTeamId)) continue;
                if (!aggregates.TryGetValue(match.HomeTeamId, out var home) || !aggregates.TryGetValue(match.AwayTeamId, out var away)) continue;

                if (match.HomePoints.HasValue) home.SeasonPoints += match.HomePoints.Value;
                if (match.AwayPoints.HasValue) away.SeasonPoints += match.AwayPoints.Value;

                if (!match.HomePoints.HasValue || !match.AwayPoints.HasValue) continue;

                if (match.HomePoints.Value > match.AwayPoints.Value) { home.Wins++; away.Losses++; }
                else if (match.HomePoints.Value < match.AwayPoints.Value) { home.Losses++; away.Wins++; }
                else { home.Ties++; away.Ties++; }
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

    public async Task<MatchDetailResponse?> BuildMatchDetailAsync(string matchupId, SeasonEvent seasonEvent, string leagueId)
    {
        var matchup = await _context.SeasonEventMatches
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == matchupId && m.SeasonEventId == seasonEvent.Id && m.LeagueId == leagueId);

        if (matchup == null) return null;

        var seasonGolfers = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .Include(sg => sg.LeagueGolfer)
            .Where(sg => sg.SeasonId == seasonEvent.SeasonId && sg.LeagueId == leagueId)
            .ToListAsync();

        var seasonTeams = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .Where(t => t.SeasonId == seasonEvent.SeasonId && t.LeagueId == leagueId)
            .ToListAsync();

        var teamLookup = seasonTeams.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);

        var rounds = await GetEventRoundsAsync(seasonEvent, leagueId);
        var roundByLeagueGolferId = rounds
            .Where(r => !string.IsNullOrWhiteSpace(r.LeagueGolferId))
            .GroupBy(r => r.LeagueGolferId!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt).First(), StringComparer.OrdinalIgnoreCase);
        var roundBySeasonGolferId = seasonGolfers
            .Where(sg => roundByLeagueGolferId.ContainsKey(sg.LeagueGolferId))
            .ToDictionary(sg => sg.Id, sg => roundByLeagueGolferId[sg.LeagueGolferId], StringComparer.OrdinalIgnoreCase);

        var homeIds = ResolveMatchParticipants(seasonGolfers, matchup.HomeTeamId, matchup.HomeSubSeasonGolferId);
        var awayIds = ResolveMatchParticipants(seasonGolfers, matchup.AwayTeamId, matchup.AwaySubSeasonGolferId);

        // Derive active holes from what participants actually played — handles shotgun starts where
        // the event-level HolesPlayed doesn't reflect a specific match's starting half.
        var participantHoles = homeIds.Concat(awayIds)
            .Where(roundBySeasonGolferId.ContainsKey)
            .SelectMany(id => roundBySeasonGolferId[id].Holes.Select(h => h.HoleNumber))
            .Distinct()
            .OrderBy(h => h)
            .ToList();
        var activeHoleNumbers = participantHoles.Count > 0
            ? participantHoles
            : GetHoleNumbersForScoring(seasonEvent.HolesPlayed);
        var holeTees = await GetHoleTeesAsync(seasonEvent.TeeId, activeHoleNumbers);

        var seasonGolferLookup = seasonGolfers.ToDictionary(sg => sg.Id, StringComparer.OrdinalIgnoreCase);

        var seasonSettings = await _context.SeasonSettings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.SeasonId == seasonEvent.SeasonId && s.LeagueId == leagueId)
            ?? new SeasonSettings { SeasonId = seasonEvent.SeasonId, LeagueId = leagueId };

        var strokeAllocations = homeIds.Concat(awayIds)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(seasonGolferLookup.ContainsKey)
            .ToDictionary(
                id => id,
                id => AllocateHandicapStrokes(
                    seasonGolferLookup[id].SeasonHandicap ?? seasonGolferLookup[id].LeagueGolfer.LeagueHandicap ?? 0,
                    holeTees,
                    seasonSettings.MaxHandicap),
                StringComparer.OrdinalIgnoreCase);

        var (homeTotal, awayTotal, _) = CalculateMatchPoints(homeIds, awayIds, seasonGolferLookup, roundBySeasonGolferId, holeTees, activeHoleNumbers, seasonSettings.MaxHandicap);

        var holeDetails = new List<MatchDetailHoleResponse>();
        double homeRunning = 0;
        double awayRunning = 0;

        foreach (var holeNumber in activeHoleNumbers)
        {
            var holeTee = holeTees.FirstOrDefault(h => h.HoleNumber == holeNumber);

            var homeGolferScores = homeIds
                .Where(seasonGolferLookup.ContainsKey)
                .Select(id =>
                {
                    roundBySeasonGolferId.TryGetValue(id, out var round);
                    var hole = round?.Holes.FirstOrDefault(h => h.HoleNumber == holeNumber);
                    var strokes = strokeAllocations.TryGetValue(id, out var alloc) ? alloc.GetValueOrDefault(holeNumber) : 0;
                    var net = hole?.GrossScore.HasValue == true ? (double?)(hole.GrossScore.Value - strokes) : null;
                    return (id, gross: hole?.GrossScore, strokes, net);
                })
                .OrderBy(x => x.net ?? double.MaxValue)
                .ToList();

            var awayGolferScores = awayIds
                .Where(seasonGolferLookup.ContainsKey)
                .Select(id =>
                {
                    roundBySeasonGolferId.TryGetValue(id, out var round);
                    var hole = round?.Holes.FirstOrDefault(h => h.HoleNumber == holeNumber);
                    var strokes = strokeAllocations.TryGetValue(id, out var alloc) ? alloc.GetValueOrDefault(holeNumber) : 0;
                    var net = hole?.GrossScore.HasValue == true ? (double?)(hole.GrossScore.Value - strokes) : null;
                    return (id, gross: hole?.GrossScore, strokes, net);
                })
                .OrderBy(x => x.net ?? double.MaxValue)
                .ToList();

            var homeValidNets = homeGolferScores.Where(x => x.net.HasValue).Select(x => x.net!.Value).ToList();
            var awayValidNets = awayGolferScores.Where(x => x.net.HasValue).Select(x => x.net!.Value).ToList();

            var scoresUsed = homeValidNets.Count > 0 && awayValidNets.Count > 0
                ? Math.Max(1, Math.Min(Math.Min(homeValidNets.Count, awayValidNets.Count), 2))
                : 0;

            var homeNetUsed = scoresUsed > 0 ? homeValidNets.Take(scoresUsed).Sum() : (double?)null;
            var awayNetUsed = scoresUsed > 0 ? awayValidNets.Take(scoresUsed).Sum() : (double?)null;

            string winner = "None";
            double homeHolePoints = 0;
            double awayHolePoints = 0;

            if (homeNetUsed.HasValue && awayNetUsed.HasValue)
            {
                if (homeNetUsed < awayNetUsed) { winner = "Home"; homeHolePoints = 2; }
                else if (awayNetUsed < homeNetUsed) { winner = "Away"; awayHolePoints = 2; }
                else { winner = "Tie"; homeHolePoints = 1; awayHolePoints = 1; }
            }
            else if (homeValidNets.Count == 0 && awayValidNets.Count > 0) { winner = "Away"; awayHolePoints = 2; }
            else if (awayValidNets.Count == 0 && homeValidNets.Count > 0) { winner = "Home"; homeHolePoints = 2; }

            homeRunning += homeHolePoints;
            awayRunning += awayHolePoints;

            var homeScoresList = homeGolferScores
                .Select((x, idx) => new MatchDetailGolferScore
                {
                    SeasonGolferId = x.id,
                    GrossScore = x.gross,
                    StrokesReceived = x.strokes,
                    NetScore = x.net,
                    IsUsed = x.net.HasValue && idx < scoresUsed
                })
                .ToList();

            var awayScoresList = awayGolferScores
                .Select((x, idx) => new MatchDetailGolferScore
                {
                    SeasonGolferId = x.id,
                    GrossScore = x.gross,
                    StrokesReceived = x.strokes,
                    NetScore = x.net,
                    IsUsed = x.net.HasValue && idx < scoresUsed
                })
                .ToList();

            holeDetails.Add(new MatchDetailHoleResponse
            {
                HoleNumber = holeNumber,
                Par = holeTee?.Par ?? 4,
                Yardage = holeTee?.Yardage ?? 0,
                HoleHandicap = holeTee?.Handicap ?? 0,
                HomeNetUsed = homeNetUsed,
                AwayNetUsed = awayNetUsed,
                HoleWinner = winner,
                HomeHolePoints = homeHolePoints,
                AwayHolePoints = awayHolePoints,
                HomeScores = homeScoresList,
                AwayScores = awayScoresList
            });
        }

        var homeMembers = homeIds
            .Where(seasonGolferLookup.ContainsKey)
            .Select(id => new MatchDetailMemberResponse
            {
                SeasonGolferId = id,
                DisplayName = seasonGolferLookup[id].LeagueGolfer.DisplayName,
                Handicap = seasonGolferLookup[id].SeasonHandicap ?? seasonGolferLookup[id].LeagueGolfer.LeagueHandicap
            })
            .ToList();

        var awayMembers = awayIds
            .Where(seasonGolferLookup.ContainsKey)
            .Select(id => new MatchDetailMemberResponse
            {
                SeasonGolferId = id,
                DisplayName = seasonGolferLookup[id].LeagueGolfer.DisplayName,
                Handicap = seasonGolferLookup[id].SeasonHandicap ?? seasonGolferLookup[id].LeagueGolfer.LeagueHandicap
            })
            .ToList();

        return new MatchDetailResponse
        {
            MatchupId = matchupId,
            StartingHole = matchup.StartingHole,
            StartingFlight = matchup.StartingFlight,
            HomeTeamName = matchup.HomeTeamId != null && teamLookup.TryGetValue(matchup.HomeTeamId, out var ht) ? ht.Name : null,
            AwayTeamName = matchup.AwayTeamId != null && teamLookup.TryGetValue(matchup.AwayTeamId, out var at) ? at.Name : null,
            HomePoints = homeTotal,
            AwayPoints = awayTotal,
            HomeMembers = homeMembers,
            AwayMembers = awayMembers,
            Holes = holeDetails
        };
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
                ? (ScoringConstants.FallbackMatchTie, ScoringConstants.FallbackMatchTie, homeComplete && awayComplete)
                : homeFallback < awayFallback
                    ? (ScoringConstants.FallbackMatchWin, 0.0, homeComplete && awayComplete)
                    : (0.0, ScoringConstants.FallbackMatchWin, homeComplete && awayComplete);
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
                homePoints += ScoringConstants.HoleForfeitLosePoints;
                awayPoints += ScoringConstants.HoleForfeitWinPoints;
                continue;
            }

            if (awayScores.Count == 0)
            {
                homePoints += ScoringConstants.HoleForfeitWinPoints;
                awayPoints += ScoringConstants.HoleForfeitLosePoints;
                continue;
            }

            var scoresUsed = Math.Max(1, Math.Min(Math.Min(homeScores.Count, awayScores.Count), 2));
            var homeNet = homeScores.Take(scoresUsed).Sum();
            var awayNet = awayScores.Take(scoresUsed).Sum();

            if (homeNet < awayNet)
            {
                homePoints += ScoringConstants.HoleWinPoints;
            }
            else if (awayNet < homeNet)
            {
                awayPoints += ScoringConstants.HoleWinPoints;
            }
            else
            {
                homePoints += ScoringConstants.HoleHalvePoints;
                awayPoints += ScoringConstants.HoleHalvePoints;
            }
        }

        if (homePoints > awayPoints)
        {
            homePoints += ScoringConstants.MatchBonusWin;
        }
        else if (awayPoints > homePoints)
        {
            awayPoints += ScoringConstants.MatchBonusWin;
        }
        else if (homePoints > 0 || awayPoints > 0)
        {
            homePoints += ScoringConstants.MatchBonusTie;
            awayPoints += ScoringConstants.MatchBonusTie;
        }

        return (homePoints, awayPoints, homeComplete && awayComplete);
    }

    private static Dictionary<int, int> AllocateHandicapStrokes(double handicap, List<HoleTee> holeTees, int? maxHandicap) =>
        StrokeCalculator.AllocateHandicapStrokes(handicap, holeTees, maxHandicap);

    private static double? GetHoleNetScore(
        Dictionary<string, Core.Entities.Round> roundBySeasonGolferId,
        Dictionary<string, Dictionary<int, int>> strokeAllocations,
        string seasonGolferId,
        int holeNumber)
    {
        if (!roundBySeasonGolferId.TryGetValue(seasonGolferId, out var round))
            return null;

        var hole = round.Holes.FirstOrDefault(h => h.HoleNumber == holeNumber);
        if (hole?.GrossScore == null)
            return null;

        var strokes = strokeAllocations.TryGetValue(seasonGolferId, out var playerStrokes)
            ? playerStrokes.GetValueOrDefault(holeNumber)
            : 0;

        return hole.GrossScore.Value - strokes;
    }

    private static double? CalculateNetRoundScore(Core.Entities.Round? round, double handicap, List<HoleTee> holeTees, List<int> activeHoleNumbers, int? maxHandicap) =>
        StrokeCalculator.CalculateNetRoundScore(round, handicap, holeTees, activeHoleNumbers, maxHandicap);

    private static List<int> GetHoleNumbersForScoring(HolesPlayed holesPlayed) =>
        StrokeCalculator.GetHoleNumbersForScoring(holesPlayed);

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

    private async Task<Dictionary<string, int>> GetCumulativeMissCountsAsync(
        string seasonId,
        string leagueId,
        DateTime upToEventDate,
        string? excludeEventId = null)
    {
        return await _context.SeasonEventPlayerScores
            .IgnoreQueryFilters()
            .Where(ps => ps.SeasonEvent!.SeasonId == seasonId
                         && ps.LeagueId == leagueId
                         && ps.SeasonEvent.EventDate <= upToEventDate
                         && (excludeEventId == null || ps.SeasonEventId != excludeEventId)
                         && ps.IsMissing)
            .GroupBy(ps => ps.SeasonGolferId)
            .Select(g => new { SeasonGolferId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SeasonGolferId, x => x.Count,
                StringComparer.OrdinalIgnoreCase);
    }

    private sealed class TeamAggregate
    {
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }
        public double SeasonPoints { get; set; }
    }
}
