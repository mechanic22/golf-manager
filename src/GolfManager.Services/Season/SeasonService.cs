using GolfManager.Core.Services;
using GolfManager.Data;
using GolfManager.Core.Entities;
using GolfManager.Core.Enums;
using GolfManager.Shared.DTOs.Season;
using GolfManager.Shared.Extensions;
using GolfManager.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GolfManager.Services.Season;

/// <summary>
/// Service for managing seasons
/// </summary>
public class SeasonService : ISeasonService
{
    private readonly GolfManagerDbContext _context;
    private readonly IShortIdService _shortIdService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<SeasonService> _logger;

    public SeasonService(
        GolfManagerDbContext context,
        IShortIdService shortIdService,
        IPasswordHasher passwordHasher,
        ILogger<SeasonService> logger)
    {
        _context = context;
        _shortIdService = shortIdService;
        _passwordHasher = passwordHasher;
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

    public async Task<SeasonSetupResponse> SetupSeasonAsync(string seasonId, SeasonSetupRequest request, string leagueId, string userId)
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
            throw new InvalidOperationException("Cannot configure a locked season");
        }

        var calendarEntries = ParseCalendar(request.CalendarText);
        if (calendarEntries.Count == 0)
        {
            throw new InvalidOperationException("No calendar entries could be parsed");
        }

        var teamEntries = ParseTeams(request.TeamsText);
        if (teamEntries.Count == 0)
        {
            throw new InvalidOperationException("No team entries could be parsed");
        }

        var hasExistingSetup = await _context.SeasonEvents.IgnoreQueryFilters().AnyAsync(e => e.SeasonId == seasonId && e.LeagueId == leagueId)
            || await _context.SeasonTeams.IgnoreQueryFilters().AnyAsync(t => t.SeasonId == seasonId && t.LeagueId == leagueId)
            || await _context.SeasonGolfers.IgnoreQueryFilters().AnyAsync(g => g.SeasonId == seasonId && g.LeagueId == leagueId);

        if (hasExistingSetup && !request.ReplaceExistingData)
        {
            throw new InvalidOperationException("This season already has setup data. Enable replace to rebuild it.");
        }

        if (request.ReplaceExistingData)
        {
            var existingEventIds = await _context.SeasonEvents.IgnoreQueryFilters()
                .Where(e => e.SeasonId == seasonId && e.LeagueId == leagueId)
                .Select(e => e.Id)
                .ToListAsync();

            var existingMatches = await _context.SeasonEventMatches.IgnoreQueryFilters()
                .Where(m => m.LeagueId == leagueId && existingEventIds.Contains(m.SeasonEventId))
                .ToListAsync();
            if (existingMatches.Count > 0)
            {
                _context.SeasonEventMatches.RemoveRange(existingMatches);
            }

            var existingEvents = await _context.SeasonEvents.IgnoreQueryFilters()
                .Where(e => e.SeasonId == seasonId && e.LeagueId == leagueId)
                .ToListAsync();
            if (existingEvents.Count > 0)
            {
                _context.SeasonEvents.RemoveRange(existingEvents);
            }

            var existingGolfers = await _context.SeasonGolfers.IgnoreQueryFilters()
                .Where(g => g.SeasonId == seasonId && g.LeagueId == leagueId)
                .ToListAsync();
            if (existingGolfers.Count > 0)
            {
                _context.SeasonGolfers.RemoveRange(existingGolfers);
            }

            var existingTeams = await _context.SeasonTeams.IgnoreQueryFilters()
                .Where(t => t.SeasonId == seasonId && t.LeagueId == leagueId)
                .ToListAsync();
            if (existingTeams.Count > 0)
            {
                _context.SeasonTeams.RemoveRange(existingTeams);
            }
        }

        var leagueGolfers = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .Include(lg => lg.Golfer)
                .ThenInclude(g => g.User)
            .Where(lg => lg.LeagueId == leagueId && lg.IsActive)
            .ToListAsync();

        var golferByName = leagueGolfers
            .GroupBy(lg => NormalizeName(lg.DisplayName))
            .ToDictionary(g => g.Key, g => g.First());

        var priorSeason = await _context.Seasons
            .IgnoreQueryFilters()
            .Where(s => s.LeagueId == leagueId && s.Id != seasonId && s.StartDate < season.StartDate)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();

        var templateEvents = priorSeason == null
            ? new List<SeasonEvent>()
            : await _context.SeasonEvents
                .IgnoreQueryFilters()
                .Where(e => e.SeasonId == priorSeason.Id && e.LeagueId == leagueId)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

        var response = new SeasonSetupResponse
        {
            CalendarWeeksParsed = calendarEntries.Count,
            SkippedWeeks = calendarEntries.Count(entry => entry.IsNoPlay)
        };

        var seasonGolfersByLeagueGolferId = new Dictionary<string, SeasonGolfer>(StringComparer.OrdinalIgnoreCase);

        foreach (var teamEntry in teamEntries)
        {
            var seasonTeam = new SeasonTeam
            {
                Id = _shortIdService.GenerateId(),
                SeasonId = seasonId,
                LeagueId = leagueId,
                Name = $"Team {teamEntry.Number}",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.SeasonTeams.Add(seasonTeam);
            response.TeamsCreated++;

            foreach (var playerName in teamEntry.Players)
            {
                var normalizedName = NormalizeName(playerName);
                if (!golferByName.TryGetValue(normalizedName, out var leagueGolfer))
                {
                    leagueGolfer = await CreateLeagueGolferForImportedNameAsync(
                        cleanedDisplayName: CleanName(playerName),
                        normalizedName: normalizedName,
                        leagueId: leagueId,
                        userId: userId);

                    golferByName[normalizedName] = leagueGolfer;
                    response.Warnings.Add($"Created league golfer for missing roster name: {leagueGolfer.DisplayName}");
                }

                if (!seasonGolfersByLeagueGolferId.TryGetValue(leagueGolfer.Id, out var seasonGolfer))
                {
                    seasonGolfer = new SeasonGolfer
                    {
                        Id = _shortIdService.GenerateId(),
                        SeasonId = seasonId,
                        LeagueId = leagueId,
                        LeagueGolferId = leagueGolfer.Id,
                        GolferId = leagueGolfer.GolferId,
                        TeamId = seasonTeam.Id,
                        SeasonHandicap = leagueGolfer.LeagueHandicap,
                        JoinedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    seasonGolfersByLeagueGolferId.Add(leagueGolfer.Id, seasonGolfer);
                    _context.SeasonGolfers.Add(seasonGolfer);
                    response.PlayersAssigned++;
                }
                else
                {
                    seasonGolfer.TeamId = seasonTeam.Id;
                    seasonGolfer.UpdatedAt = DateTime.UtcNow;
                    seasonGolfer.UpdatedBy = userId;
                }
            }
        }

        var playableEntries = calendarEntries.Where(entry => !entry.IsNoPlay).OrderBy(entry => entry.Week).ToList();
        for (var i = 0; i < playableEntries.Count; i++)
        {
            var entry = playableEntries[i];
            var template = templateEvents.Count == 0 ? null : templateEvents[Math.Min(i, templateEvents.Count - 1)];

            var seasonEvent = new SeasonEvent
            {
                Id = _shortIdService.GenerateId(),
                SeasonId = seasonId,
                LeagueId = leagueId,
                EventDate = entry.Date,
                CourseId = template?.CourseId,
                TeeId = template?.TeeId,
                HolesPlayed = template?.HolesPlayed ?? HolesPlayed.Nine,
                EventType = GetEventType(entry.Note),
                ScoringFormat = template?.ScoringFormat ?? ScoringFormat.MatchPlay,
                Name = GetEventName(entry),
                Description = entry.Note,
                TeamSize = template?.TeamSize ?? 3,
                UseHandicaps = template?.UseHandicaps ?? true,
                Status = EventStatus.Draft,
                IsLocked = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.SeasonEvents.Add(seasonEvent);
            response.EventsCreated++;
        }

        if (response.MissingPlayers.Count > 0)
        {
            response.MissingPlayers = response.MissingPlayers
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (templateEvents.Count == 0)
        {
            response.Warnings.Add("No prior season events were available, so default event templates were used.");
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Configured season {SeasonId} in league {LeagueId}: {Events} events, {Teams} teams, {Players} players assigned",
            seasonId,
            leagueId,
            response.EventsCreated,
            response.TeamsCreated,
            response.PlayersAssigned);

        return response;
    }

    private async Task<LeagueGolfer> CreateLeagueGolferForImportedNameAsync(
        string cleanedDisplayName,
        string normalizedName,
        string leagueId,
        string userId)
    {
        if (string.IsNullOrWhiteSpace(cleanedDisplayName))
        {
            throw new InvalidOperationException("Player name could not be parsed.");
        }

        var existingLeagueGolfer = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .Include(lg => lg.Golfer)
            .Where(lg => lg.LeagueId == leagueId && lg.IsActive)
            .FirstOrDefaultAsync(lg => NormalizeName(lg.DisplayName) == normalizedName);

        if (existingLeagueGolfer != null)
        {
            return existingLeagueGolfer;
        }

        var emailLocal = Regex.Replace(normalizedName, "[^a-z0-9]+", ".").Trim('.');
        if (string.IsNullOrWhiteSpace(emailLocal))
        {
            emailLocal = "golfer";
        }

        var candidateEmail = $"{emailLocal}@{leagueId}.imported.local";
        var suffix = 1;
        while (await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == candidateEmail))
        {
            candidateEmail = $"{emailLocal}.{suffix}@{leagueId}.imported.local";
            suffix++;
        }

        var nameParts = cleanedDisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : "League";
        var lastName = nameParts.Length > 1 ? string.Join(' ', nameParts.Skip(1)) : "Golfer";

        var newUser = new User
        {
            Id = _shortIdService.GenerateId(),
            Email = candidateEmail,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = _passwordHasher.HashPassword("ChangeMe123!"),
            IsGlobalAdmin = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        var newGolfer = new Golfer
        {
            Id = _shortIdService.GenerateId(),
            UserId = newUser.Id,
            DisplayName = cleanedDisplayName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        var newLeagueGolfer = new LeagueGolfer
        {
            Id = _shortIdService.GenerateId(),
            LeagueId = leagueId,
            GolferId = newGolfer.Id,
            DisplayName = cleanedDisplayName,
            JoinedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        var newMembership = new UserLeague
        {
            Id = _shortIdService.GenerateId(),
            UserId = newUser.Id,
            LeagueId = leagueId,
            LeagueGolferId = newLeagueGolfer.Id,
            Role = LeagueMemberRole.Member,
            IsLeagueAdmin = false,
            JoinedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Users.Add(newUser);
        _context.Golfers.Add(newGolfer);
        _context.LeagueGolfers.Add(newLeagueGolfer);
        _context.UserLeagues.Add(newMembership);

        return newLeagueGolfer;
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

    private static List<CalendarEntry> ParseCalendar(string calendarText)
    {
        var cleanedLines = calendarText
            .Replace("\r", string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !string.Equals(line, "Week", StringComparison.OrdinalIgnoreCase))
            .Where(line => !string.Equals(line, "Date", StringComparison.OrdinalIgnoreCase))
            .Where(line => !string.Equals(line, "Notes", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var entries = new List<CalendarEntry>();
        for (var i = 0; i + 2 < cleanedLines.Count;)
        {
            if (!int.TryParse(cleanedLines[i], out var weekNumber))
            {
                i++;
                continue;
            }

            if (!DateTime.TryParse(cleanedLines[i + 1], CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var eventDate))
            {
                i++;
                continue;
            }

            var note = cleanedLines[i + 2];
            entries.Add(new CalendarEntry(weekNumber, eventDate, note));
            i += 3;
        }

        return entries;
    }

    private static List<TeamEntry> ParseTeams(string teamsText)
    {
        var entries = new List<TeamEntry>();
        var lines = teamsText.Replace("\r", string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            var tabParts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (tabParts.Length >= 4 && int.TryParse(tabParts[0], out var teamNumber))
            {
                entries.Add(new TeamEntry(teamNumber, tabParts.Skip(1).Take(3).Select(CleanName).ToList()));
                continue;
            }

            var match = Regex.Match(line, @"^(\d+)\s+(.+)$");
            if (!match.Success)
            {
                continue;
            }

            var names = Regex.Split(match.Groups[2].Value.Trim(), @"\s{3,}")
                .Select(CleanName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            if (names.Count >= 3 && int.TryParse(match.Groups[1].Value, out teamNumber))
            {
                entries.Add(new TeamEntry(teamNumber, names.Take(3).ToList()));
            }
        }

        return entries;
    }

    private static string NormalizeName(string value)
    {
        var compact = Regex.Replace(value ?? string.Empty, @"\s+", " ").Trim();
        return compact.ToLowerInvariant();
    }

    private static string CleanName(string value)
    {
        return Regex.Replace(value ?? string.Empty, @"\s+", " ").Trim();
    }

    private static SeasonEventType GetEventType(string note)
    {
        if (note.Contains("championship", StringComparison.OrdinalIgnoreCase))
        {
            return SeasonEventType.Championship;
        }

        if (note.Contains("play off", StringComparison.OrdinalIgnoreCase)
            || note.Contains("semi final", StringComparison.OrdinalIgnoreCase)
            || note.Contains("toilet bowl", StringComparison.OrdinalIgnoreCase))
        {
            return SeasonEventType.Playoff;
        }

        if (!note.Contains("regular", StringComparison.OrdinalIgnoreCase))
        {
            return SeasonEventType.Special;
        }

        return SeasonEventType.Regular;
    }

    private static string GetEventName(CalendarEntry entry)
    {
        if (entry.Note.Contains("regular", StringComparison.OrdinalIgnoreCase))
        {
            return $"Week {entry.Week}";
        }

        return entry.Note;
    }

    private sealed record CalendarEntry(int Week, DateTime Date, string Note)
    {
        public bool IsNoPlay => Note.Contains("no league play", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record TeamEntry(int Number, List<string> Players);

    // ── Teams ────────────────────────────────────────────────────────────────

    public async Task<List<SeasonTeamResponse>> GetSeasonTeamsAsync(string seasonId, string leagueId)
    {
        var teams = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .Include(t => t.Members)
                .ThenInclude(m => m.LeagueGolfer)
            .Where(t => t.SeasonId == seasonId && t.LeagueId == leagueId)
            .OrderBy(t => t.Name)
            .ToListAsync();

        return teams.Select(MapTeamToResponse).ToList();
    }

    public async Task<SeasonTeamResponse> CreateSeasonTeamAsync(string seasonId, CreateSeasonTeamRequest request, string leagueId, string userId)
    {
        var season = await _context.Seasons.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == seasonId && s.LeagueId == leagueId)
            ?? throw new InvalidOperationException("Season not found");

        var team = new SeasonTeam
        {
            Id = _shortIdService.GenerateId(),
            SeasonId = seasonId,
            LeagueId = leagueId,
            Name = request.Name,
            AvatarUrl = request.AvatarUrl,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.SeasonTeams.Add(team);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created team {TeamId} '{Name}' in season {SeasonId}", team.Id, team.Name, seasonId);

        return MapTeamToResponse(team);
    }

    public async Task<SeasonTeamResponse> UpdateSeasonTeamAsync(string seasonId, string teamId, UpdateSeasonTeamRequest request, string leagueId, string userId)
    {
        var team = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .Include(t => t.Members)
                .ThenInclude(m => m.LeagueGolfer)
            .FirstOrDefaultAsync(t => t.Id == teamId && t.SeasonId == seasonId && t.LeagueId == leagueId)
            ?? throw new InvalidOperationException("Team not found");

        team.Name = request.Name;
        team.AvatarUrl = request.AvatarUrl;
        team.UpdatedAt = DateTime.UtcNow;
        team.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        return MapTeamToResponse(team);
    }

    public async Task<bool> DeleteSeasonTeamAsync(string seasonId, string teamId, string leagueId, string userId)
    {
        var team = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == teamId && t.SeasonId == seasonId && t.LeagueId == leagueId);

        if (team == null) return false;

        // Unassign all season golfers from this team first
        var golfers = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .Where(g => g.TeamId == teamId && g.SeasonId == seasonId)
            .ToListAsync();

        foreach (var g in golfers)
        {
            g.TeamId = null;
            g.UpdatedAt = DateTime.UtcNow;
            g.UpdatedBy = userId;
        }

        _context.SeasonTeams.Remove(team);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted team {TeamId} from season {SeasonId}", teamId, seasonId);
        return true;
    }

    public async Task AssignPlayerToTeamAsync(string seasonId, string seasonGolferId, AssignPlayerToTeamRequest request, string leagueId, string userId)
    {
        var golfer = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(g => g.Id == seasonGolferId && g.SeasonId == seasonId && g.LeagueId == leagueId)
            ?? throw new InvalidOperationException("Season golfer not found");

        if (request.TeamId != null)
        {
            var teamExists = await _context.SeasonTeams
                .IgnoreQueryFilters()
                .AnyAsync(t => t.Id == request.TeamId && t.SeasonId == seasonId && t.LeagueId == leagueId);

            if (!teamExists) throw new InvalidOperationException("Team not found in this season");
        }

        golfer.TeamId = request.TeamId;
        golfer.UpdatedAt = DateTime.UtcNow;
        golfer.UpdatedBy = userId;

        await _context.SaveChangesAsync();
    }

    // ── Players ──────────────────────────────────────────────────────────────

    public async Task<bool> RemovePlayerFromSeasonAsync(string seasonId, string seasonGolferId, string leagueId, string userId)
    {
        var golfer = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(g => g.Id == seasonGolferId && g.SeasonId == seasonId && g.LeagueId == leagueId);

        if (golfer == null) return false;

        _context.SeasonGolfers.Remove(golfer);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed season golfer {GolferId} from season {SeasonId}", seasonGolferId, seasonId);
        return true;
    }

    public async Task UpdateSeasonPlayerPaymentAsync(string seasonId, string seasonGolferId, UpdateSeasonPlayerPaymentRequest request, string leagueId, string userId)
    {
        var golfer = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(g => g.Id == seasonGolferId && g.SeasonId == seasonId && g.LeagueId == leagueId)
            ?? throw new InvalidOperationException("Player not found in season");

        golfer.IsPaidForSeason = request.IsPaidForSeason;
        golfer.PaidAt = request.IsPaidForSeason ? DateTime.UtcNow : null;
        golfer.UpdatedAt = DateTime.UtcNow;
        golfer.UpdatedBy = userId;

        await _context.SaveChangesAsync();
    }

    private static SeasonTeamResponse MapTeamToResponse(SeasonTeam team)
    {
        return new SeasonTeamResponse
        {
            Id = team.Id,
            SeasonId = team.SeasonId,
            Name = team.Name,
            AvatarUrl = team.AvatarUrl,
            Wins = team.Wins,
            Losses = team.Losses,
            Ties = team.Ties,
            SeasonPoints = team.SeasonPoints,
            Members = team.Members?.Select(m => new SeasonTeamMemberResponse
            {
                SeasonGolferId = m.Id,
                DisplayName = m.LeagueGolfer?.DisplayName ?? "Unknown",
                LeagueHandicap = m.LeagueGolfer?.LeagueHandicap
            }).ToList() ?? new()
        };
    }
}

