using System.Text.RegularExpressions;
using GolfManager.Core.Common;
using GolfManager.Core.Entities;
using GolfManager.Core.Enums;
using GolfManager.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Data.Seed;

/// <summary>
/// Imports Holy Grail v1 SQL Server backup data into GolfManager v2 SQLite database
/// </summary>
public class HolyGrailImporter(GolfManagerDbContext context, ILogger logger, IShortIdService shortIdService)
{
    private readonly GolfManagerDbContext _context = context;
    private readonly ILogger _logger = logger;
    private readonly IShortIdService _shortId = shortIdService;

    // Mapping dictionaries (old keys → new IDs)
    private readonly Dictionary<string, string> _leagueMap = new();
    private readonly Dictionary<string, string> _golferMap = new();  // Old GolferId → New LeagueGolferId
    private readonly Dictionary<string, string> _golferToGolferIdMap = new();  // Old GolferId → New GolferId (global)
    private readonly Dictionary<string, string> _courseMap = new();
    private readonly Dictionary<string, string> _teeMap = new();      // "courseKey_teeName" → new TeeId
    private readonly Dictionary<string, string> _teeIdMap = new();    // old TeeId → new TeeId
    private readonly Dictionary<string, string> _seasonMap = new();
    private readonly Dictionary<string, string> _seasonEventMap = new(); // Old SeasonEventId → New ID
    private readonly Dictionary<string, string> _teamMap = new(); // Old TeamId → New ID
    private readonly Dictionary<string, string> _roundMap = new(); // Old RoundId → New ID
    private readonly Dictionary<string, DateTime> _roundDateMap = new(); // Old RoundId → RoundDate

    public async Task<bool> ImportFromBackupAsync(string backupFilePath)
    {
        try
        {
            _logger.LogInformation("===========================================");
            _logger.LogInformation("Holy Grail Data Import Started");
            _logger.LogInformation("===========================================");
            _logger.LogInformation("Backup file: {FilePath}", backupFilePath);

            if (!File.Exists(backupFilePath))
            {
                _logger.LogError("Backup file not found: {FilePath}", backupFilePath);
                return false;
            }

            var sqlContent = await File.ReadAllTextAsync(backupFilePath);
            _logger.LogInformation("Backup file loaded: {Size} bytes", sqlContent.Length);

            // Import in dependency order
            await ImportLeaguesAsync(sqlContent);
            await ImportGolfersAsync(sqlContent);
            await ImportCoursesAsync(sqlContent);
            await ImportTeesAsync(sqlContent);
            await ImportHolesAndHoleTeesAsync(sqlContent);
            await ImportSeasonsAsync(sqlContent);
            await ImportSeasonSettingsAsync(sqlContent);
            await ImportSeasonTeamsAsync(sqlContent);
            await ImportSeasonGolfersAsync(sqlContent);
            await ImportSeasonEventsAsync(sqlContent);
            await ImportSeasonEventMatchesAsync(sqlContent);
            await ImportRoundsAsync(sqlContent);
            await ImportRoundHolesAsync(sqlContent);
            await ImportSeasonEventGolfersAsync(sqlContent);  // must run after rounds are in _roundMap; creates SeasonEventPlayerScores directly from holy-grail data
            await SetLeagueGolferActiveStatusAsync();
            await SetGolferJoinedDatesAsync();

            _logger.LogInformation("===========================================");
            _logger.LogInformation("Holy Grail Data Import Completed Successfully!");
            _logger.LogInformation("===========================================");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Holy Grail data import");
            return false;
        }
    }

    private async Task ImportLeaguesAsync(string sqlContent)
    {
        _logger.LogInformation("Importing Leagues...");

        // Pattern: INSERT INTO [dbo].[GolfLeagues] VALUES ('dkgl', 'DigiKey Golf League', '2025');
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfLeagues\] VALUES \('([^']+)',\s*'([^']+)',\s*(?:'([^']*)'|NULL)\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            var name = match.Groups[2].Value;

            // Check if league already exists
            var existingLeague = await _context.Leagues
                .FirstOrDefaultAsync(l => l.Key == key);

            string leagueId;
            if (existingLeague != null)
            {
                _logger.LogWarning("League already exists: {Key} - using existing ID", key);
                leagueId = existingLeague.Id;
                _leagueMap[key] = leagueId;
                continue;
            }

            leagueId = key;
            _leagueMap[key] = leagueId;

            var league = new League
            {
                Id = leagueId,
                Key = key,
                Name = name,
                Description = string.Empty,
                LogoUrl = key == "dkgl" ? "/img/dkgltr.png" : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Leagues.Add(league);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} leagues", matches.Count);
    }

    private async Task ImportGolfersAsync(string sqlContent)
    {
        _logger.LogInformation("Importing Golfers (Users)...");

        // Pattern: INSERT INTO [dbo].[GolfGolfers] VALUES (Id, FirstName, LastName, PinHash, IsAdmin, IsActive, UserName, NormUserName, Email, NormEmail, EmailConfirmed, PassHash, ...);
        // Note: This is complex - we'll parse values positionally
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfGolfers\] VALUES \(([^)]+)\);";
        var matches = Regex.Matches(sqlContent, pattern);

        // We need to know the league ID first - get it from the map
        if (!_leagueMap.Any())
        {
            _logger.LogWarning("No leagues imported yet - golfers will not be linked to league");
        }

        var leagueId = _leagueMap.Values.FirstOrDefault();

        foreach (Match match in matches)
        {
            try
            {
                var values = ParseSqlValues(match.Groups[1].Value);
                if (values.Count < 6) continue; // Skip invalid rows

                var oldId = values[0];
                var firstName = values[1];
                var lastName = values[2];
                var isAdmin = values[4].Equals("1") || values[4].ToLower() == "true";
                var isActive = values[5].Equals("1") || values[5].ToLower() == "true";
                var email = values.Count > 8 ? values[8] : $"{firstName}.{lastName}@imported.local".ToLower();

                var userId = oldId;
                var golferId = oldId;
                var leagueGolferId = oldId;

                _golferMap[oldId] = leagueGolferId; // Map old ID to LeagueGolferId (used in SeasonGolfers)
                _golferToGolferIdMap[oldId] = golferId; // Map old ID to GolferId (global)

                // 1. Create User (Identity/Auth)
                var user = new User
                {
                    Id = userId,
                    Email = CleanString(email),
                    FirstName = CleanString(firstName),
                    LastName = CleanString(lastName),
                    IsGlobalAdmin = isAdmin,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe123!"), // Default password
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);

                // 2. Create Golfer (Global Profile)
                var golfer = new Golfer
                {
                    Id = golferId,
                    UserId = userId,
                    DisplayName = $"{CleanString(firstName)} {CleanString(lastName)}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Golfers.Add(golfer);

                // 3. Create LeagueGolfer (League-specific Profile) - if we have a league
                if (!string.IsNullOrEmpty(leagueId))
                {
                    var leagueGolfer = new LeagueGolfer
                    {
                        Id = leagueGolferId,
                        LeagueId = leagueId,
                        GolferId = golferId,
                        DisplayName = $"{CleanString(firstName)} {CleanString(lastName)}",
                        JoinedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.LeagueGolfers.Add(leagueGolfer);

                    // 4. Create UserLeague (League Membership)
                    var userLeague = new UserLeague
                    {
                        Id = _shortId.GenerateId(),
                        UserId = userId,
                        LeagueId = leagueId,
                        LeagueGolferId = leagueGolferId,
                        IsLeagueAdmin = isAdmin, // Use same admin flag as global
                        Role = isAdmin ? LeagueMemberRole.Admin : LeagueMemberRole.Member,
                        JoinedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "system"
                    };
                    _context.UserLeagues.Add(userLeague);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse golfer: {Values}", match.Groups[1].Value.Substring(0, Math.Min(100, match.Groups[1].Value.Length)));
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} golfers/users with full profiles", _golferMap.Count);
    }
    private async Task ImportCoursesAsync(string sqlContent)
    {
        _logger.LogInformation("Importing Courses...");

        // Pattern: INSERT INTO [dbo].[GolfCourses] VALUES ('course1', 'Course Name', 'City', 'State', '18');
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfCourses\] VALUES \('([^']+)',\s*'([^']+)',\s*'([^']*)',\s*'([^']*)',\s*'(\d+)'\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            var name = match.Groups[2].Value;
            var city = match.Groups[3].Value;
            var state = match.Groups[4].Value;
            var holes = int.Parse(match.Groups[5].Value);

            var courseId = key;
            _courseMap[key] = courseId;

            var course = new Course
            {
                Id = courseId,
                Key = key,
                Name = name,
                City = city,
                State = state,
                NumberOfHoles = holes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} courses", matches.Count);
    }

    private async Task ImportTeesAsync(string sqlContent)
    {
        _logger.LogInformation("Importing Tees...");

        // Actual format: ('OldTeeId', 'courseKey', 'TeeName', 'color', 'ratingOut', 'slopeOut', 'ratingIn', 'slopeIn', 'yardsOut', 'yardsIn', 'parOut', 'parIn')
        // All 12 values are quoted strings. Old TeeId is the first field (not courseKey).
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfTees\] VALUES \('([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']*)',\s*'([0-9.]+)',\s*'([0-9.]+)',\s*'([0-9.]+)',\s*'([0-9.]+)',\s*'(\d+)',\s*'(\d+)',\s*'(\d+)',\s*'(\d+)'\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            var oldTeeId = match.Groups[1].Value;
            var courseKey = match.Groups[2].Value;
            var teeName = match.Groups[3].Value;
            var color = match.Groups[4].Value;
            var ratingOut = double.Parse(match.Groups[5].Value);
            var slopeOut = int.Parse(match.Groups[6].Value);
            var ratingIn = double.Parse(match.Groups[7].Value);
            var slopeIn = int.Parse(match.Groups[8].Value);
            var yardsOut = int.Parse(match.Groups[9].Value);
            var yardsIn = int.Parse(match.Groups[10].Value);
            var parOut = int.Parse(match.Groups[11].Value);
            var parIn = int.Parse(match.Groups[12].Value);

            if (!_courseMap.TryGetValue(courseKey, out var courseId))
            {
                _logger.LogWarning("Course not found for tee: {CourseKey}", courseKey);
                continue;
            }

            _teeMap[$"{courseKey}_{teeName}"] = oldTeeId;
            _teeIdMap[oldTeeId] = oldTeeId;

            var tee = new Tee
            {
                Id = oldTeeId,
                CourseId = courseId,
                Name = teeName,
                HtmlColorCode = color,
                RatingOut = ratingOut,
                RatingIn = ratingIn,
                SlopeOut = slopeOut,
                SlopeIn = slopeIn,
                YardsOut = yardsOut,
                YardsIn = yardsIn,
                ParOut = parOut,
                ParIn = parIn,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tees.Add(tee);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} tees", matches.Count);
    }

    private async Task ImportHolesAndHoleTeesAsync(string sqlContent)
    {
        _logger.LogInformation("Importing Holes and HoleTees...");

        // First import Holes
        var holePattern = @"INSERT INTO \[dbo\]\.\[GolfHoles\] VALUES \('([^']+)',\s*(\d+),\s*'([^']*)',\s*'([^']*)'\);";
        var holeMatches = Regex.Matches(sqlContent, holePattern);
        var holeMap = new Dictionary<string, string>(); // courseKey_holeNumber → holeId

        foreach (Match match in holeMatches)
        {
            var courseKey = match.Groups[1].Value;
            var holeNumber = int.Parse(match.Groups[2].Value);
            var holeName = match.Groups[3].Value;

            if (!_courseMap.TryGetValue(courseKey, out var courseId)) continue;

            var holeId = _shortId.GenerateId();
            holeMap[$"{courseKey}_{holeNumber}"] = holeId;

            var hole = new Hole
            {
                Id = holeId,
                CourseId = courseId,
                HoleNumber = holeNumber,
                Name = holeName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Holes.Add(hole);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} holes", holeMatches.Count);

        // Then import HoleTees
        // Actual format: ('OldTeeId', 'HoleNumber', 'Par', 'Yardage', 'Handicap') — all 5 quoted, keyed by TeeId directly
        var holeTeePattern = @"INSERT INTO \[dbo\]\.\[GolfHoleTees\] VALUES \('([^']+)',\s*'(\d+)',\s*'(\d+)',\s*'(\d+)',\s*'(\d+)'\);";
        var holeTeeMatches = Regex.Matches(sqlContent, holeTeePattern);

        foreach (Match match in holeTeeMatches)
        {
            var oldTeeId = match.Groups[1].Value;
            var holeNumber = int.Parse(match.Groups[2].Value);
            var par = int.Parse(match.Groups[3].Value);
            var yardage = int.Parse(match.Groups[4].Value);
            var handicap = int.Parse(match.Groups[5].Value);

            if (!_teeIdMap.TryGetValue(oldTeeId, out var teeId)) continue;

            var holeTee = new HoleTee
            {
                Id = _shortId.GenerateId(),
                TeeId = teeId,
                HoleNumber = holeNumber,
                Par = par,
                Yardage = yardage,
                Handicap = handicap,
                CreatedAt = DateTime.UtcNow
            };

            _context.HoleTees.Add(holeTee);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} hole tees", holeTeeMatches.Count);
    }

    private async Task ImportSeasonsAsync(string sqlContent)
    {
        _logger.LogInformation("Importing Seasons...");

        // Pattern: INSERT INTO [dbo].[GolfSeasons] VALUES ('2019', 'dkgl', '2019', '2019-05-13 00:00:00', 'True');
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfSeasons\] VALUES \('([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)'\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            var seasonKey = match.Groups[1].Value;
            var leagueKey = match.Groups[2].Value;
            var name = match.Groups[3].Value;
            var startDate = DateTime.Parse(match.Groups[4].Value);
            var isLocked = match.Groups[5].Value.Equals("True", StringComparison.OrdinalIgnoreCase);

            if (!_leagueMap.TryGetValue(leagueKey, out var leagueId))
            {
                _logger.LogWarning("League not found for season: {LeagueKey}", leagueKey);
                continue;
            }

            // Check if season already exists
            var existingSeason = await _context.Seasons
                .FirstOrDefaultAsync(s => s.LeagueId == leagueId && s.Key == seasonKey);

            string seasonId;
            if (existingSeason != null)
            {
                _logger.LogWarning("Season already exists: {LeagueKey}/{SeasonKey} - skipping", leagueKey, seasonKey);
                seasonId = existingSeason.Id;
                _seasonMap[seasonKey] = seasonId;
                continue;
            }

            seasonId = seasonKey;
            _seasonMap[seasonKey] = seasonId;

            var season = new Season
            {
                Id = seasonId,
                LeagueId = leagueId,
                Key = seasonKey, // Add the Key property!
                Name = name,
                StartDate = DateOnly.FromDateTime(startDate),
                EndDate = DateOnly.FromDateTime(startDate.AddDays(15 * 7)), // Approximate - Holy Grail v1 had 15-week seasons
                IsLocked = isLocked, // Lock historical seasons
                CreatedAt = startDate,
                UpdatedAt = startDate
            };

            _context.Seasons.Add(season);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} seasons", matches.Count);
    }

    private async Task ImportSeasonSettingsAsync(string sqlContent)
    {
        _logger.LogInformation("Importing SeasonSettings...");

        // Pattern: INSERT INTO [dbo].[GolfSeasonSettings] VALUES ('dkgl', '2024', 'Bobs', 'TwoPoint', 'MatchPoints', 'FieldAverage', 'PartialPoints', 'PlusFour', '18', NULL, '17:30:00');
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfSeasonSettings\] VALUES \('([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*(?:'([^']*)'|(\d+)|NULL),\s*(?:'([^']*)'|NULL),\s*(?:'([^']*)'|NULL)\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            try
            {
                var leagueKey = match.Groups[1].Value;
                var seasonKey = match.Groups[2].Value;
                var handicapType = match.Groups[3].Value;
                var individualScoringType = match.Groups[4].Value;
                var teamScoringType = match.Groups[5].Value;
                var missingPlayerType = match.Groups[6].Value;
                var missingTeamType = match.Groups[7].Value;
                var maxScoreForHandicap = match.Groups[8].Value;
                var maxHandicap = match.Groups[9].Success ? match.Groups[9].Value : match.Groups[10].Value;
                var defaultCourseKey = match.Groups[11].Success ? match.Groups[11].Value : null;
                var defaultStartTime = match.Groups[12].Success ? match.Groups[12].Value : null;

                if (!_seasonMap.TryGetValue(seasonKey, out var seasonId))
                {
                    _logger.LogWarning("Season not found for settings: {SeasonKey}", seasonKey);
                    continue;
                }

                if (!_leagueMap.TryGetValue(leagueKey, out var leagueId))
                {
                    _logger.LogWarning("League not found for settings: {LeagueKey}", leagueKey);
                    continue;
                }

                var settings = new SeasonSettings
                {
                    Id = _shortId.GenerateId(),
                    SeasonId = seasonId,
                    LeagueId = leagueId,
                    HandicapType = ParseEnum<HandicapType>(handicapType, HandicapType.None),
                    IndividualScoringType = ParseEnum<IndividualScoringType>(individualScoringType, IndividualScoringType.None),
                    TeamScoringType = ParseEnum<TeamScoringType>(teamScoringType, TeamScoringType.None),
                    MissingPlayerType = ParseEnum<MissingPlayerType>(missingPlayerType, MissingPlayerType.None),
                    MissingTeamType = ParseEnum<MissingTeamType>(missingTeamType, MissingTeamType.NoPoints),
                    MaxScoreForHandicap = ParseEnum<MaxScoreForHandicap>(maxScoreForHandicap, MaxScoreForHandicap.None),
                    MaxHandicap = string.IsNullOrEmpty(maxHandicap) ? null : int.Parse(maxHandicap),
                    DefaultCourseId = !string.IsNullOrEmpty(defaultCourseKey) && _courseMap.TryGetValue(defaultCourseKey, out var courseId) ? courseId : null,
                    DefaultStartTime = !string.IsNullOrEmpty(defaultStartTime) ? TimeOnly.Parse(defaultStartTime) : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SeasonSettings.Add(settings);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse season settings: {Match}", match.Value.Substring(0, Math.Min(200, match.Value.Length)));
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} season settings", matches.Count);
    }

    private async Task ImportSeasonTeamsAsync(string sqlContent)
    {
        _logger.LogInformation("Importing SeasonTeams...");

        // Actual format: ('TEAMID', 'dkgl', '2024', 'Team Name', ''|NULL, '150.5'|NULL, 'date') — seasonPoints is a quoted string
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfSeasonTeams\] VALUES \('([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*(?:'([^']*)'|NULL),\s*(?:'([0-9.]+)'|NULL),\s*'([^']+)'\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            var teamId = match.Groups[1].Value;
            var leagueKey = match.Groups[2].Value;
            var seasonKey = match.Groups[3].Value;
            var teamName = match.Groups[4].Value;
            var avatar = match.Groups[5].Success ? match.Groups[5].Value : null;
            var seasonPoints = match.Groups[6].Success ? double.Parse(match.Groups[6].Value) : (double?)null;
            var teamCreatedAt = match.Groups[7].Success && DateTime.TryParse(match.Groups[7].Value, out var parsedTeamDate) ? parsedTeamDate : DateTime.UtcNow;

            if (!_seasonMap.TryGetValue(seasonKey, out var seasonId))
            {
                _logger.LogWarning("Season not found for team: {SeasonKey}", seasonKey);
                continue;
            }

            if (!_leagueMap.TryGetValue(leagueKey, out var leagueId))
            {
                _logger.LogWarning("League not found for team: {LeagueKey}", leagueKey);
                continue;
            }

            _teamMap[teamId] = teamId;

            var team = new SeasonTeam
            {
                Id = teamId,
                SeasonId = seasonId,
                LeagueId = leagueId,
                Name = teamName,
                CreatedAt = teamCreatedAt,
                UpdatedAt = teamCreatedAt
            };

            _context.SeasonTeams.Add(team);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} season teams", matches.Count);
    }

    private async Task ImportSeasonGolfersAsync(string sqlContent)
    {
        _logger.LogInformation("Importing SeasonGolfers...");

        // Pattern: INSERT INTO [dbo].[GolfSeasonGolfers] VALUES ('dkgl', '2019', 'GOLFERID', 'TEAMID', '536', '2024-04-17 16:15:58');
        // Actual column order from backup: (LeagueKey, SeasonKey, GolferId, TeamId, ..., UpdatedAt)
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfSeasonGolfers\] VALUES \(([^)]+)\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            try
            {
                var values = ParseSqlValues(match.Groups[1].Value);
                if (values.Count < 4) continue;

                // Correct order from backup file
                var leagueKey = values[0];
                var seasonKey = values[1];
                var oldGolferId = values[2];
                var oldTeamId = values[3];

                if (!_golferMap.TryGetValue(oldGolferId, out var leagueGolferId))
                {
                    _logger.LogWarning("LeagueGolfer not found: {GolferId}", oldGolferId);
                    continue;
                }

                if (!_golferToGolferIdMap.TryGetValue(oldGolferId, out var globalGolferId))
                {
                    _logger.LogWarning("Global Golfer not found: {GolferId}", oldGolferId);
                    continue;
                }

                if (!_seasonMap.TryGetValue(seasonKey, out var seasonId))
                {
                    _logger.LogWarning("Season not found for golfer: {SeasonKey}", seasonKey);
                    continue;
                }

                if (!_leagueMap.TryGetValue(leagueKey, out var leagueId))
                {
                    _logger.LogWarning("League not found for golfer: {LeagueKey}", leagueKey);
                    continue;
                }

                string? newTeamId = null;
                if (!string.IsNullOrEmpty(oldTeamId) && _teamMap.TryGetValue(oldTeamId, out var mappedTeamId))
                    newTeamId = mappedTeamId;

                var golferDate = values.Count > 5 && DateTime.TryParse(values[5], out var parsedGolferDate) ? parsedGolferDate : DateTime.UtcNow;

                var seasonGolfer = new SeasonGolfer
                {
                    Id = _shortId.GenerateId(),
                    SeasonId = seasonId,
                    LeagueId = leagueId,
                    LeagueGolferId = leagueGolferId,
                    GolferId = globalGolferId,
                    TeamId = newTeamId,
                    CreatedAt = golferDate,
                    UpdatedAt = golferDate
                };

                _context.SeasonGolfers.Add(seasonGolfer);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse season golfer");
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} season golfers", _context.SeasonGolfers.Count());
    }

    private async Task ImportSeasonEventsAsync(string sqlContent)
    {
        _logger.LogInformation("Importing SeasonEvents...");

        // Pattern: INSERT INTO [dbo].[GolfSeasonEvents] VALUES ('00ZAW974', 'dkgl', '2021', '2021-05-24 17:40:00', 'thief-river-falls-golf-club', 'NVLMHJY2', 'Back', 'Match');
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfSeasonEvents\] VALUES \('([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)'\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            var eventKey = match.Groups[1].Value;
            var leagueKey = match.Groups[2].Value;
            var seasonKey = match.Groups[3].Value;
            var eventDate = DateTime.Parse(match.Groups[4].Value);
            var courseKey = match.Groups[5].Value;
            var teeKey = match.Groups[6].Value;
            var holesPlayed = match.Groups[7].Value; // Front, Back, Both
            var eventType = match.Groups[8].Value; // Match, Individual

            if (!_seasonMap.TryGetValue(seasonKey, out var seasonId))
            {
                _logger.LogWarning("Season not found for event: {SeasonKey}", seasonKey);
                continue;
            }

            if (!_leagueMap.TryGetValue(leagueKey, out var leagueId))
            {
                _logger.LogWarning("League not found for event: {LeagueKey}", leagueKey);
                continue;
            }

            if (!_courseMap.TryGetValue(courseKey, out var courseId))
            {
                _logger.LogWarning("Course not found for event: {CourseKey}", courseKey);
                continue;
            }

            // teeKey in the backup is the old TeeId (e.g. 'KX8H6NH9'), look it up by direct ID first
            string? teeId = null;
            if (!_teeIdMap.TryGetValue(teeKey, out teeId))
                _teeMap.TryGetValue($"{courseKey}_{teeKey}", out teeId);

            _seasonEventMap[eventKey] = eventKey;

            // Map scoring format and event type
            var isPlayoff = eventType.Equals("PlayoffNight", StringComparison.OrdinalIgnoreCase);
            var scoringFormat = (eventType.Equals("Match", StringComparison.OrdinalIgnoreCase) || isPlayoff)
                ? ScoringFormat.TwoPoint
                : ScoringFormat.StrokePlay;

            var seasonEvent = new SeasonEvent
            {
                Id = eventKey,
                SeasonId = seasonId,
                LeagueId = leagueId,
                Name = $"{seasonKey} Event - {eventDate:MMM dd}",
                EventDate = eventDate,
                EventType = isPlayoff ? SeasonEventType.Playoff : SeasonEventType.Regular,
                ScoringFormat = scoringFormat,
                CourseId = courseId,
                TeeId = teeId,
                HolesPlayed = holesPlayed.Equals("Back", StringComparison.OrdinalIgnoreCase) ? HolesPlayed.Back :
                              holesPlayed.Equals("Front", StringComparison.OrdinalIgnoreCase) ? HolesPlayed.Front : HolesPlayed.Eighteen,
                Status = EventStatus.Completed, // Historical events are completed
                IsLocked = true, // Lock historical events
                CreatedAt = eventDate,
                UpdatedAt = eventDate
            };

            _context.SeasonEvents.Add(seasonEvent);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} season events", matches.Count);
    }

    private async Task ImportSeasonEventMatchesAsync(string sqlContent)
    {
        _logger.LogInformation("Importing SeasonEventMatches...");

        // Actual format: ('MatchId', 'EventKey', NULL, 'HomeTeamId', 'AwayTeamId', 'HomePoints', 'AwayPoints', 'StartingHole', 'StartingFlight')
        // 9 fields, all quoted except the NULL. No league/season columns. Points are quoted strings.
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfSeasonEventMatches\] VALUES \('([^']+)',\s*'([^']+)',\s*(?:'[^']*'|NULL),\s*(?:'([^']*)'|NULL),\s*(?:'([^']*)'|NULL),\s*(?:'([0-9.]+)'|NULL),\s*(?:'([0-9.]+)'|NULL),\s*(?:'(\d+)'|NULL),\s*(?:'(\d+)'|NULL)\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            try
            {
                var oldMatchId = match.Groups[1].Value;
                var eventKey = match.Groups[2].Value;
                var homeTeamId = match.Groups[3].Success && !string.IsNullOrEmpty(match.Groups[3].Value) ? match.Groups[3].Value : null;
                var awayTeamId = match.Groups[4].Success && !string.IsNullOrEmpty(match.Groups[4].Value) ? match.Groups[4].Value : null;
                var homePoints = match.Groups[5].Success && !string.IsNullOrEmpty(match.Groups[5].Value) ? double.Parse(match.Groups[5].Value) : (double?)null;
                var awayPoints = match.Groups[6].Success && !string.IsNullOrEmpty(match.Groups[6].Value) ? double.Parse(match.Groups[6].Value) : (double?)null;
                var startingHole = match.Groups[7].Success && !string.IsNullOrEmpty(match.Groups[7].Value) ? int.Parse(match.Groups[7].Value) : (int?)null;
                var startingFlight = match.Groups[8].Success && !string.IsNullOrEmpty(match.Groups[8].Value) ? int.Parse(match.Groups[8].Value) : (int?)null;
                var isComplete = homePoints.HasValue && awayPoints.HasValue;

                if (!_seasonEventMap.TryGetValue(eventKey, out var seasonEventId))
                {
                    _logger.LogWarning("Season event not found for match: {EventKey}", eventKey);
                    continue;
                }

                var seasonEvent = await _context.SeasonEvents.FindAsync(seasonEventId);
                if (seasonEvent == null) continue;

                // Map team IDs
                var mappedHomeTeamId = !string.IsNullOrEmpty(homeTeamId) && _teamMap.TryGetValue(homeTeamId, out var hId) ? hId : null;
                var mappedAwayTeamId = !string.IsNullOrEmpty(awayTeamId) && _teamMap.TryGetValue(awayTeamId, out var aId) ? aId : null;

                var seasonEventMatch = new SeasonEventMatch
                {
                    Id = oldMatchId,
                    SeasonEventId = seasonEventId,
                    LeagueId = seasonEvent.LeagueId,
                    HomeTeamId = mappedHomeTeamId,
                    AwayTeamId = mappedAwayTeamId,
                    HomePoints = homePoints,
                    AwayPoints = awayPoints,
                    StartingHole = startingHole,
                    StartingFlight = startingFlight,
                    IsComplete = isComplete,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SeasonEventMatches.Add(seasonEventMatch);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse season event match");
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} season event matches", _context.SeasonEventMatches.Count());
    }

    private async Task ImportSeasonEventGolfersAsync(string sqlContent)
    {
        _logger.LogInformation("Importing SeasonEventGolfers...");

        // Column order: GolferId, SeasonEventId, RoundId, SeasonTeamId, Handicap, EventPoints, EventPosition, MissScore, MissCount
        // Numeric columns may be bare (9.28) or quoted ('9.28') depending on the backup tool used
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfSeasonEventGolfers\] VALUES \('([^']+)',\s*'([^']*)',\s*(?:'([^']*)'|NULL),\s*(?:'([^']*)'|NULL),\s*(?:'?([0-9.]+)'?|NULL),\s*(?:'?([0-9.]+)'?|NULL),\s*(?:'?(\d+)'?|NULL),\s*(?:'?([0-9.]+)'?|NULL),\s*(?:'?(\d+)'?|NULL)\);";
        var matches = Regex.Matches(sqlContent, pattern);

        // Pre-load everything needed for SeasonEventPlayerScore creation
        var seasonEvents = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .ToDictionaryAsync(e => e.Id, StringComparer.OrdinalIgnoreCase);

        var seasonGolfers = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .Include(sg => sg.LeagueGolfer)
            .ToListAsync();
        var seasonGolferByKey = seasonGolfers
            .ToDictionary(sg => $"{sg.LeagueGolferId}|{sg.SeasonId}", StringComparer.OrdinalIgnoreCase);

        var teamNames = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .ToDictionaryAsync(t => t.Id, t => t.Name, StringComparer.OrdinalIgnoreCase);

        var rounds = await _context.Rounds
            .IgnoreQueryFilters()
            .ToDictionaryAsync(r => r.Id, StringComparer.OrdinalIgnoreCase);

        var updatedCount = 0;
        var scoreRecords = new List<SeasonEventPlayerScore>();

        foreach (Match match in matches)
        {
            try
            {
                var oldGolferId = match.Groups[1].Value;
                var oldSeasonEventId = match.Groups[2].Value;
                var oldRoundId = match.Groups[3].Success && !string.IsNullOrWhiteSpace(match.Groups[3].Value) ? match.Groups[3].Value : null;
                var handicap = match.Groups[5].Success ? ParseDouble(match.Groups[5].Value) : null;
                var eventPoints = match.Groups[6].Success ? ParseDouble(match.Groups[6].Value) : null;
                var missScore = match.Groups[8].Success ? ParseDouble(match.Groups[8].Value) : null;

                if (!_seasonEventMap.TryGetValue(oldSeasonEventId, out var seasonEventId)
                    || !seasonEvents.TryGetValue(seasonEventId, out var seasonEvent))
                {
                    _logger.LogWarning("Season event not found for event golfer: {SeasonEventId}", oldSeasonEventId);
                    continue;
                }

                if (!_golferMap.TryGetValue(oldGolferId, out var leagueGolferId))
                {
                    _logger.LogWarning("LeagueGolfer not found for event golfer: {GolferId}", oldGolferId);
                    continue;
                }

                // Update the linked round (explicit RoundId, not date-based)
                Round? round = null;
                if (oldRoundId != null && _roundMap.TryGetValue(oldRoundId, out var roundId) && rounds.TryGetValue(roundId, out round))
                {
                    round.RoundDate = seasonEvent.EventDate;
                    if (string.IsNullOrWhiteSpace(round.LeagueId))
                        round.LeagueId = seasonEvent.LeagueId;
                    if (string.IsNullOrWhiteSpace(round.LeagueGolferId))
                        round.LeagueGolferId = leagueGolferId;
                    round.UpdatedAt = DateTime.UtcNow;
                    updatedCount++;
                }

                // Build SeasonEventPlayerScore using holy-grail's validated EventPoints
                var sgKey = $"{leagueGolferId}|{seasonEvent.SeasonId}";
                if (!seasonGolferByKey.TryGetValue(sgKey, out var seasonGolfer))
                {
                    _logger.LogWarning("SeasonGolfer not found: {Key}", sgKey);
                    continue;
                }

                var isMissing = missScore.HasValue;
                var rawScore = !isMissing ? round?.TotalScore : null;
                var netScore = rawScore.HasValue && handicap.HasValue ? (double?)(rawScore.Value - handicap.Value) : null;

                teamNames.TryGetValue(seasonGolfer.TeamId ?? string.Empty, out var teamName);

                scoreRecords.Add(new SeasonEventPlayerScore
                {
                    Id = _shortId.GenerateId(),
                    SeasonEventId = seasonEventId,
                    SeasonGolferId = seasonGolfer.Id,
                    LeagueId = seasonEvent.LeagueId,
                    RawScore = rawScore,
                    Handicap = handicap,
                    NetScore = netScore,
                    EventPoints = eventPoints,
                    IsMissing = isMissing,
                    MissScore = missScore,
                    DisplayName = seasonGolfer.LeagueGolfer.DisplayName,
                    TeamId = seasonGolfer.TeamId,
                    TeamName = teamName,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process season event golfer");
            }
        }

        _context.SeasonEventPlayerScores.AddRange(scoreRecords);
        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Linked {Count} rounds, created {ScoreCount} player score records", updatedCount, scoreRecords.Count);

        await BackfillHandicapsFromEventScoresAsync();
    }

    private async Task BackfillHandicapsFromEventScoresAsync()
    {
        _logger.LogInformation("Backfilling handicaps from event scores...");

        var scores = await _context.SeasonEventPlayerScores
            .IgnoreQueryFilters()
            .Where(s => s.Handicap != null)
            .Join(_context.SeasonEvents.IgnoreQueryFilters(),
                  s => s.SeasonEventId, e => e.Id,
                  (s, e) => new { s.SeasonGolferId, s.Handicap, e.EventDate })
            .ToListAsync();

        var bySeasonGolfer = scores
            .GroupBy(x => x.SeasonGolferId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.EventDate).First().Handicap);

        var seasonGolfers = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .Include(sg => sg.LeagueGolfer)
            .ToListAsync();

        foreach (var sg in seasonGolfers)
        {
            if (bySeasonGolfer.TryGetValue(sg.Id, out var hcp))
                sg.SeasonHandicap = hcp;
        }

        var byLeagueGolfer = seasonGolfers
            .Where(sg => sg.SeasonHandicap != null)
            .GroupBy(sg => sg.LeagueGolferId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(sg => sg.UpdatedAt).First());

        var leagueGolfers = await _context.LeagueGolfers.IgnoreQueryFilters().ToListAsync();
        foreach (var lg in leagueGolfers)
        {
            if (byLeagueGolfer.TryGetValue(lg.Id, out var sg) && sg.SeasonHandicap != null)
            {
                lg.LeagueHandicap = sg.SeasonHandicap;
                lg.LeagueHandicapUpdatedAt = sg.UpdatedAt;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Backfilled handicaps for {Count} league golfers", byLeagueGolfer.Count);
    }

    public async Task RepairHandicapsAsync(string sqlContent)
    {
        _logger.LogInformation("Repairing handicaps from backup...");

        var pattern = @"INSERT INTO \[dbo\]\.\[GolfSeasonEventGolfers\] VALUES \('([^']+)',\s*'([^']*)',\s*(?:'([^']*)'|NULL),\s*(?:'([^']*)'|NULL),\s*(?:'?([0-9.]+)'?|NULL),\s*(?:'?([0-9.]+)'?|NULL),\s*(?:'?(\d+)'?|NULL),\s*(?:'?([0-9.]+)'?|NULL),\s*(?:'?(\d+)'?|NULL)\);";
        var matches = Regex.Matches(sqlContent, pattern);

        var seasonEvents = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .ToDictionaryAsync(e => e.Id, StringComparer.OrdinalIgnoreCase);

        var handicapsByGolfer = new Dictionary<string, List<(double hcp, DateTime date)>>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in matches)
        {
            var leagueGolferId = match.Groups[1].Value;
            var oldEventId = match.Groups[2].Value;
            var handicap = match.Groups[5].Success ? ParseDouble(match.Groups[5].Value) : (double?)null;

            if (handicap == null) continue;
            if (!seasonEvents.TryGetValue(oldEventId, out var evt)) continue;

            if (!handicapsByGolfer.TryGetValue(leagueGolferId, out var list))
                handicapsByGolfer[leagueGolferId] = list = [];
            list.Add((handicap.Value, evt.EventDate));
        }

        var seasonGolfers = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .Include(sg => sg.LeagueGolfer)
            .ToListAsync();

        foreach (var sg in seasonGolfers)
        {
            if (!handicapsByGolfer.TryGetValue(sg.LeagueGolferId, out var entries)) continue;
            var best = entries.OrderByDescending(e => e.date).FirstOrDefault();
            if (best != default) sg.SeasonHandicap = best.hcp;
        }

        var leagueGolfers = await _context.LeagueGolfers.IgnoreQueryFilters().ToListAsync();
        foreach (var lg in leagueGolfers)
        {
            if (!handicapsByGolfer.TryGetValue(lg.Id, out var entries)) continue;
            var best = entries.OrderByDescending(e => e.date).First();
            lg.LeagueHandicap = best.hcp;
            lg.LeagueHandicapUpdatedAt = best.date;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Repaired handicaps for {Count} league golfers", handicapsByGolfer.Count);
    }

    private async Task ImportRoundsAsync(string sqlContent)
    {
        _logger.LogInformation("Importing Rounds...");

        // Pre-parse GolfSeasonEventGolfers to build a roundId → seasonEventId lookup for date resolution.
        // Column order in INSERT: GolferId (1), SeasonEventId (2), RoundId (3)
        var roundToSeasonEventLookup = new Dictionary<string, string>();
        var segPattern = @"INSERT INTO \[dbo\]\.\[GolfSeasonEventGolfers\] VALUES \('([^']+)',\s*'([^']*)',\s*(?:'([^']*)'|NULL)";
        foreach (Match m in Regex.Matches(sqlContent, segPattern))
        {
            var segRoundId = m.Groups[3].Success && !string.IsNullOrWhiteSpace(m.Groups[3].Value) ? m.Groups[3].Value : null;
            var segSeasonEventId = m.Groups[2].Value;
            if (segRoundId != null)
                roundToSeasonEventLookup[segRoundId] = segSeasonEventId;
        }

        var pattern = @"INSERT INTO \[dbo\]\.\[GolfRounds\] VALUES \(([^)]+)\);";
        var matches = Regex.Matches(sqlContent, pattern);

        int importedCount = 0;
        foreach (Match match in matches)
        {
            try
            {
                var values = ParseSqlValues(match.Groups[1].Value);
                if (values.Count < 6)
                {
                    _logger.LogWarning("Skipping GolfRounds insert with too few values: {ValueCount}", values.Count);
                    continue;
                }

                var roundId = values[0];
                string golferId;
                string courseId;
                string teeId;
                int? totalScore = null;
                int? netScore = null;
                double? handicapUsed = null;
                bool isComplete = true;
                string? notes = null;
                string? leagueGolferId = null;
                string? leagueId = null;
                DateTime roundDate = DateTime.UtcNow;
                var holesPlayed = HolesPlayed.Eighteen;

                if (values.Count == 6)
                {
                    // Older backup schema: Id, ScorecardId, GolferId, CourseKey, TeeId, RawScore
                    // No date column — look it up via GolfSeasonEventGolfers → GolfSeasonEvents
                    golferId = values[2];
                    courseId = values[3];
                    teeId = values[4];
                    totalScore = ParseInt(values[5]);

                    if (roundToSeasonEventLookup.TryGetValue(roundId, out var oldSeasonEventId) &&
                        _seasonEventMap.TryGetValue(oldSeasonEventId, out var mappedSeasonEventId))
                    {
                        var seasonEvent = await _context.SeasonEvents.FindAsync(mappedSeasonEventId);
                        if (seasonEvent != null)
                            roundDate = seasonEvent.EventDate;
                    }
                }
                else if (values.Count >= 13)
                {
                    // Newer backup schema with explicit round fields
                    golferId = values[2];
                    leagueGolferId = string.IsNullOrEmpty(values[3]) ? null : values[3];
                    leagueId = string.IsNullOrEmpty(values[4]) ? null : values[4];
                    courseId = values[5];
                    teeId = values[6];
                    roundDate = DateTime.TryParse(values[7], out var parsedDate) ? parsedDate : DateTime.UtcNow;
                    holesPlayed = (HolesPlayed)int.Parse(values[8]);
                    totalScore = ParseInt(values[9]);
                    netScore = ParseInt(values[10]);
                    handicapUsed = ParseDouble(values[11]);
                    isComplete = values[12].Equals("True", StringComparison.OrdinalIgnoreCase);
                    notes = values.Count > 13 ? values[13] : null;
                }
                else
                {
                    _logger.LogWarning("Unsupported GolfRounds insert format: {ValueCount} values", values.Count);
                    continue;
                }

                if (!_golferToGolferIdMap.TryGetValue(golferId, out var newGolferId))
                {
                    _logger.LogWarning("Golfer not found for round: {GolferId}", golferId);
                    continue;
                }

                _courseMap.TryGetValue(courseId, out var newCourseId);
                if (newCourseId == null)
                    _logger.LogWarning("Course not found for round {RoundId}, importing without course link", roundId);

                // For 6-value schema, teeId is the old TeeId directly; try _teeIdMap first, then _teeMap
                if (!_teeIdMap.TryGetValue(teeId, out var newTeeId) && !_teeMap.TryGetValue(teeId, out newTeeId))
                    _logger.LogWarning("Tee not found for round {RoundId}, importing without tee link", roundId);

                var mappedLeagueGolferId = !string.IsNullOrEmpty(leagueGolferId) && _golferMap.TryGetValue(leagueGolferId, out var lgId) ? lgId : null;
                var mappedLeagueId = !string.IsNullOrEmpty(leagueId) && _leagueMap.TryGetValue(leagueId, out var lId) ? lId : null;

                var round = new Round
                {
                    Id = roundId,
                    GolferId = newGolferId,
                    LeagueGolferId = mappedLeagueGolferId,
                    LeagueId = mappedLeagueId,
                    CourseId = newCourseId,
                    TeeId = newTeeId,
                    RoundDate = roundDate,
                    HolesPlayed = holesPlayed,
                    TotalScore = totalScore,
                    NetScore = netScore,
                    HandicapUsed = handicapUsed,
                    IsComplete = isComplete,
                    Notes = notes,
                    CreatedAt = roundDate,
                    UpdatedAt = roundDate
                };

                _context.Rounds.Add(round);
                _roundMap[roundId] = roundId;
                _roundDateMap[roundId] = roundDate;
                importedCount++;

                if (importedCount % 100 == 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogDebug("Saved {Count} rounds so far...", importedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse round");
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} rounds", importedCount);
    }

    private async Task ImportRoundHolesAsync(string sqlContent)
    {
        _logger.LogInformation("Importing RoundHoles...");

        var pattern = @"INSERT INTO \[dbo\]\.\[GolfRoundHoles\] VALUES \(([^)]+)\);";
        var matches = Regex.Matches(sqlContent, pattern);

        int importedCount = 0;
        var roundHoleNumbers = new Dictionary<string, List<int>>();
        foreach (Match match in matches)
        {
            try
            {
                var values = ParseSqlValues(match.Groups[1].Value);
                if (values.Count < 8)
                {
                    _logger.LogWarning("Skipping GolfRoundHoles insert with too few values: {ValueCount}", values.Count);
                    continue;
                }

                string roundId;
                int holeNumber;
                int? grossScore = null;
                int? netScore = null;
                int? putts = null;
                bool? fairwayHit = null;
                bool? gir = null;
                int? penalties = null;

                if (values.Count >= 13 && int.TryParse(values[1], out _))
                {
                    // Older backup schema: RoundId, HoleNumber, Par, Handicap, Yardage, TeeClubKey, TeeDistance, RawScore, Putts, Chips, PenaltyStrokes, SandShots, TeeShotPosition
                    roundId = values[0];
                    holeNumber = int.Parse(values[1]);
                    grossScore = ParseInt(values[7]);
                    putts = ParseInt(values[8]);
                    penalties = ParseInt(values[10]);
                }
                else if (values.Count >= 10)
                {
                    // Newer backup schema: RoundHoleId, RoundId, HoleNumber, GrossScore, NetScore, Putts, FairwayHit, GreenInRegulation, Penalties, Notes
                    roundId = values[1];
                    holeNumber = int.Parse(values[2]);
                    grossScore = ParseInt(values[3]);
                    netScore = ParseInt(values[4]);
                    putts = ParseInt(values[5]);
                    fairwayHit = ParseBool(values[6]);
                    gir = ParseBool(values[7]);
                    penalties = ParseInt(values[8]);
                }
                else
                {
                    _logger.LogWarning("Unsupported GolfRoundHoles insert format: {ValueCount} values", values.Count);
                    continue;
                }

                if (!_roundMap.TryGetValue(roundId, out var newRoundId))
                {
                    _logger.LogWarning("Round not found for hole: {RoundId}", roundId);
                    continue;
                }

                var holeDate = _roundDateMap.TryGetValue(roundId, out var rd) ? rd : DateTime.UtcNow;

                var roundHole = new RoundHole
                {
                    RoundId = newRoundId,
                    HoleNumber = holeNumber,
                    GrossScore = grossScore,
                    NetScore = netScore,
                    Putts = putts,
                    FairwayHit = fairwayHit,
                    GreenInRegulation = gir,
                    Penalties = penalties,
                    CreatedAt = holeDate,
                    UpdatedAt = holeDate
                };

                _context.RoundHoles.Add(roundHole);
                if (!roundHoleNumbers.TryGetValue(newRoundId, out var holeList))
                {
                    holeList = new List<int>();
                    roundHoleNumbers[newRoundId] = holeList;
                }
                holeList.Add(holeNumber);
                importedCount++;

                if (importedCount % 500 == 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogDebug("Saved {Count} round holes so far...", importedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse round hole");
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} round holes", importedCount);

        _logger.LogInformation("Updating HolesPlayed from RoundHoles data...");
        int updatedCount = 0;
        foreach (var (roundId, holeNums) in roundHoleNumbers)
        {
            var holesPlayed = holeNums.Count switch
            {
                18 => HolesPlayed.Eighteen,
                9 when holeNums.Min() == 1  => HolesPlayed.Front,
                9 when holeNums.Min() >= 10 => HolesPlayed.Back,
                > 0 => HolesPlayed.Nine,
                _ => (HolesPlayed?)null
            };
            if (holesPlayed == null) continue;

            var round = await _context.Rounds.FindAsync(roundId);
            if (round != null && round.HolesPlayed != holesPlayed.Value)
            {
                round.HolesPlayed = holesPlayed.Value;
                updatedCount++;
            }
        }
        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Updated HolesPlayed for {Count} rounds", updatedCount);
    }

    private async Task PopulateEventScoresAsync()
    {
        _logger.LogInformation("Populating event scores for locked events...");

        var lockedEvents = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .Include(e => e.Season)
            .Include(e => e.PlayerScores)
            .Include(e => e.MatchScores)
            .Where(e => e.IsLocked)
            .ToListAsync();

        var processedCount = 0;
        foreach (var seasonEvent in lockedEvents)
        {
            try
            {
                // Skip if already has scores
                if (seasonEvent.PlayerScores.Any() || seasonEvent.MatchScores.Any())
                {
                    continue;
                }

                await PopulateEventPlayerScoresAsync(seasonEvent);
                await PopulateEventMatchScoresAsync(seasonEvent);

                processedCount++;
                if (processedCount % 10 == 0)
                {
                    _logger.LogInformation("Processed {Count} events so far...", processedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to populate scores for event {EventId}", seasonEvent.Id);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Populated scores for {Count} locked events", processedCount);
    }

    private async Task SetLeagueGolferActiveStatusAsync()
    {
        _logger.LogInformation("Setting LeagueGolfer active status based on current season...");

        var currentSeason = await _context.Seasons
            .IgnoreQueryFilters()
            .Where(s => !s.IsLocked)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();

        if (currentSeason == null)
        {
            _logger.LogWarning("No active (non-locked) season found — skipping IsActive update");
            return;
        }

        _logger.LogInformation("Current season: {SeasonKey} (ID: {SeasonId})", currentSeason.Key, currentSeason.Id);

        var activeLeagueGolferIds = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .Where(sg => sg.SeasonId == currentSeason.Id)
            .Select(sg => sg.LeagueGolferId)
            .ToListAsync();

        _logger.LogInformation("Golfers in current season: {Count}", activeLeagueGolferIds.Count);

        // ExecuteUpdateAsync bypasses the change tracker and issues direct SQL UPDATE statements,
        // avoiding any stale-entity issues after a large import session.
        int activeUpdated = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .Where(lg => lg.LeagueId == currentSeason.LeagueId && activeLeagueGolferIds.Contains(lg.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(lg => lg.IsActive, true));

        int inactiveUpdated = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .Where(lg => lg.LeagueId == currentSeason.LeagueId && !activeLeagueGolferIds.Contains(lg.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(lg => lg.IsActive, false));

        _logger.LogInformation(
            "✓ Marked {Active} golfers active, {Inactive} inactive based on current season {SeasonKey}",
            activeUpdated,
            inactiveUpdated,
            currentSeason.Key);
    }

    private async Task SetGolferJoinedDatesAsync()
    {
        _logger.LogInformation("Setting golfer joined dates from first round...");

        // Find the earliest round date per global GolferId
        var earliestByGolferId = await _context.Rounds
            .IgnoreQueryFilters()
            .Where(r => r.GolferId != null)
            .GroupBy(r => r.GolferId)
            .Select(g => new { GolferId = g.Key, EarliestDate = g.Min(r => r.RoundDate) })
            .ToDictionaryAsync(x => x.GolferId, x => x.EarliestDate);

        var leagueGolfers = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .ToListAsync();

        int lgUpdated = 0;
        foreach (var lg in leagueGolfers)
        {
            if (earliestByGolferId.TryGetValue(lg.GolferId, out var earliest))
            {
                lg.JoinedAt = earliest;
                lgUpdated++;
            }
        }
        await _context.SaveChangesAsync();

        var userLeagues = await _context.UserLeagues
            .IgnoreQueryFilters()
            .Include(ul => ul.LeagueGolfer)
            .ToListAsync();

        int ulUpdated = 0;
        foreach (var ul in userLeagues)
        {
            if (ul.LeagueGolfer != null && earliestByGolferId.TryGetValue(ul.LeagueGolfer.GolferId, out var earliest))
            {
                ul.JoinedAt = earliest;
                ulUpdated++;
            }
        }
        await _context.SaveChangesAsync();

        _logger.LogInformation("✓ Set joined dates for {LgCount} league golfers, {UlCount} user leagues", lgUpdated, ulUpdated);
    }

    private async Task PopulateEventPlayerScoresAsync(SeasonEvent seasonEvent)
    {
        var seasonSettings = await _context.SeasonSettings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.SeasonId == seasonEvent.SeasonId && s.LeagueId == seasonEvent.LeagueId)
            ?? new SeasonSettings
            {
                SeasonId = seasonEvent.SeasonId,
                LeagueId = seasonEvent.LeagueId,
                IndividualScoringType = IndividualScoringType.TwoPoint,
                TeamScoringType = TeamScoringType.MatchPoints,
                MissingPlayerType = MissingPlayerType.FieldAverage,
                MissingTeamType = MissingTeamType.PartialPoints
            };

        var seasonGolfers = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .Include(sg => sg.LeagueGolfer)
            .Where(sg => sg.SeasonId == seasonEvent.SeasonId && sg.LeagueId == seasonEvent.LeagueId)
            .ToListAsync();

        var seasonTeams = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .Where(t => t.SeasonId == seasonEvent.SeasonId && t.LeagueId == seasonEvent.LeagueId)
            .ToListAsync();

        var eventDate = seasonEvent.EventDate.Date;
        var rounds = await _context.Rounds
            .IgnoreQueryFilters()
            .Include(r => r.Holes)
            .Where(r => r.LeagueId == seasonEvent.LeagueId
                && r.RoundDate >= eventDate
                && r.RoundDate < eventDate.AddDays(1)
                && (string.IsNullOrWhiteSpace(seasonEvent.CourseId) || r.CourseId == seasonEvent.CourseId)
                && (string.IsNullOrWhiteSpace(seasonEvent.TeeId) || r.TeeId == seasonEvent.TeeId))
            .ToListAsync();

        var activeHoleNumbers = GetHoleNumbersForScoring(seasonEvent.HolesPlayed);
        var holeTees = string.IsNullOrWhiteSpace(seasonEvent.TeeId)
            ? new List<HoleTee>()
            : await _context.HoleTees
                .IgnoreQueryFilters()
                .Where(ht => ht.TeeId == seasonEvent.TeeId && activeHoleNumbers.Contains(ht.HoleNumber))
                .OrderBy(ht => ht.HoleNumber)
                .ToListAsync();

        var teamLookup = seasonTeams.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
        var roundByLeagueGolferId = rounds
            .Where(r => !string.IsNullOrWhiteSpace(r.LeagueGolferId))
            .GroupBy(r => r.LeagueGolferId!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt).First(), StringComparer.OrdinalIgnoreCase);

        var parTotal = holeTees.Count > 0 ? holeTees.Sum(h => h.Par) : activeHoleNumbers.Count * 4;

        // First pass: compute raw and net scores for every golfer
        var playerData = seasonGolfers.Select(sg =>
        {
            roundByLeagueGolferId.TryGetValue(sg.LeagueGolferId, out var round);
            var handicap = sg.SeasonHandicap ?? sg.LeagueGolfer.LeagueHandicap;
            var rawScore = round?.TotalScore
                ?? (round != null ? round.Holes.Where(h => activeHoleNumbers.Contains(h.HoleNumber)).Sum(h => h.GrossScore ?? 0) : (int?)null);

            if (round != null && rawScore == 0 && !round.Holes.Any(h => activeHoleNumbers.Contains(h.HoleNumber) && h.GrossScore.HasValue))
                rawScore = null;

            var netScore = rawScore.HasValue
                ? CalculateNetRoundScore(round, handicap ?? 0, holeTees, activeHoleNumbers, seasonSettings.MaxHandicap)
                : null;

            return (sg, handicap, rawScore, netScore);
        }).ToList();

        // Field average uses only players who scored
        var fieldAvg = playerData.Where(p => p.netScore.HasValue).Select(p => p.netScore!.Value) is var scored && scored.Any()
            ? scored.Average()
            : (double?)null;

        // Second pass: assign miss scores
        var withMiss = playerData.Select(p =>
        {
            var isMissing = !p.netScore.HasValue;
            double? missScore = null;
            if (isMissing)
            {
                missScore = seasonSettings.MissingPlayerType switch
                {
                    MissingPlayerType.PlayAgainstPar => parTotal,
                    MissingPlayerType.FieldAverage => fieldAvg ?? parTotal,
                    MissingPlayerType.BlindDraw => fieldAvg ?? parTotal,
                    _ => null
                };
            }
            return (p.sg, p.handicap, p.rawScore, p.netScore, isMissing, missScore);
        }).ToList();

        // Third pass: ranking-based EventPoints (same algorithm as EventService.BuildPlayerScores)
        var ranked = withMiss
            .Where(p => (p.netScore ?? p.missScore).HasValue)
            .OrderBy(p => p.netScore ?? p.missScore ?? double.MaxValue)
            .ThenBy(p => p.sg.LeagueGolfer.DisplayName)
            .ToList();

        var eventPointsMap = new Dictionary<string, double?>(StringComparer.OrdinalIgnoreCase);
        var standingPoints = ranked.Count;

        foreach (var group in ranked
            .GroupBy(p => Math.Round((p.netScore ?? p.missScore) ?? double.MaxValue, 2))
            .OrderBy(g => g.Key))
        {
            var bucket = 0;
            for (var i = 0; i < group.Count(); i++)
                bucket += standingPoints - i;

            var pts = seasonSettings.IndividualScoringType == IndividualScoringType.None
                ? (double?)null
                : bucket / (double)group.Count();

            foreach (var p in group)
                eventPointsMap[p.sg.Id] = pts;

            standingPoints -= group.Count();
        }

        var records = withMiss.Select(p => new SeasonEventPlayerScore
        {
            Id = _shortId.GenerateId(),
            SeasonEventId = seasonEvent.Id,
            SeasonGolferId = p.sg.Id,
            LeagueId = seasonEvent.LeagueId,
            RawScore = p.rawScore,
            Handicap = p.handicap,
            NetScore = p.netScore,
            EventPoints = eventPointsMap.TryGetValue(p.sg.Id, out var pts) ? pts : null,
            IsMissing = p.isMissing,
            MissScore = p.missScore,
            DisplayName = p.sg.LeagueGolfer.DisplayName,
            TeamId = p.sg.TeamId,
            TeamName = p.sg.TeamId != null && teamLookup.TryGetValue(p.sg.TeamId, out var team) ? team.Name : null,
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.SeasonEventPlayerScores.AddRange(records);
    }

    private async Task PopulateEventMatchScoresAsync(SeasonEvent seasonEvent)
    {
        // Use the points already stored on each matchup record (imported directly from Holy Grail v1).
        var matchups = await _context.SeasonEventMatches
            .IgnoreQueryFilters()
            .Where(m => m.SeasonEventId == seasonEvent.Id && m.LeagueId == seasonEvent.LeagueId)
            .ToListAsync();

        var seasonTeams = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .Where(t => t.SeasonId == seasonEvent.SeasonId && t.LeagueId == seasonEvent.LeagueId)
            .ToListAsync();
        var teamLookup = seasonTeams.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);

        var records = matchups.Select(matchup => new SeasonEventMatchScore
        {
            Id = _shortId.GenerateId(),
            SeasonEventId = seasonEvent.Id,
            SeasonEventMatchId = matchup.Id,
            LeagueId = seasonEvent.LeagueId,
            HomeTeamId = matchup.HomeTeamId,
            HomeTeamName = matchup.HomeTeamId != null && teamLookup.TryGetValue(matchup.HomeTeamId, out var ht) ? ht.Name : null,
            HomePoints = matchup.HomePoints,
            AwayTeamId = matchup.AwayTeamId,
            AwayTeamName = matchup.AwayTeamId != null && teamLookup.TryGetValue(matchup.AwayTeamId, out var at) ? at.Name : null,
            AwayPoints = matchup.AwayPoints,
            IsComplete = matchup.IsComplete,
            StartingHole = matchup.StartingHole,
            StartingFlight = matchup.StartingFlight,
            CreatedBy = "system",
            CreatedAt = matchup.CreatedAt
        }).ToList();

        _context.SeasonEventMatchScores.AddRange(records);
    }

    private static List<int> GetHoleNumbersForScoring(HolesPlayed holesPlayed) =>
        StrokeCalculator.GetHoleNumbersForScoring(holesPlayed);

    private static double? CalculateNetRoundScore(Core.Entities.Round? round, double handicap, List<HoleTee> holeTees, List<int> activeHoleNumbers, int? maxHandicap) =>
        StrokeCalculator.CalculateNetRoundScore(round, handicap, holeTees, activeHoleNumbers, maxHandicap);

    // Helper methods
    private List<string> ParseSqlValues(string valuesStr)
    {
        var values = new List<string>();
        var current = "";
        var inString = false;

        for (int i = 0; i < valuesStr.Length; i++)
        {
            var ch = valuesStr[i];

            if (ch == '\'' && (i == 0 || valuesStr[i - 1] != '\\'))
            {
                if (inString && i + 1 < valuesStr.Length && valuesStr[i + 1] == '\'')
                {
                    // Escaped quote
                    current += '\'';
                    i++;
                    continue;
                }
                inString = !inString;
                continue;
            }

            if (!inString && ch == ',')
            {
                values.Add(current.Trim() == "NULL" ? "" : current.Trim());
                current = "";
                continue;
            }

            current += ch;
        }

        if (!string.IsNullOrWhiteSpace(current))
        {
            values.Add(current.Trim() == "NULL" ? "" : current.Trim());
        }

        return values;
    }

    private int? ParseInt(string value)
    {
        return int.TryParse(value, out var result) ? result : (int?)null;
    }

    private double? ParseDouble(string value)
    {
        return double.TryParse(value, out var result) ? result : (double?)null;
    }

    private bool? ParseBool(string value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (bool.TryParse(value, out var result))
            return result;

        if (value.Equals("True", StringComparison.OrdinalIgnoreCase))
            return true;

        if (value.Equals("False", StringComparison.OrdinalIgnoreCase))
            return false;

        return null;
    }

    private string CleanString(string value)
    {
        return value?.Replace("''", "'").Trim() ?? "";
    }

    private TEnum ParseEnum<TEnum>(string value, TEnum defaultValue) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        if (Enum.TryParse<TEnum>(value, true, out var result))
            return result;

        _logger.LogWarning("Failed to parse enum {EnumType} from value: {Value}", typeof(TEnum).Name, value);
        return defaultValue;
    }
}

