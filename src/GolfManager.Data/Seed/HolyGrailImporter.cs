using System.Text.RegularExpressions;
using GolfManager.Core.Entities;
using GolfManager.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Data.Seed;

/// <summary>
/// Imports Holy Grail v1 SQL Server backup data into GolfManager v2 SQLite database
/// </summary>
public class HolyGrailImporter(GolfManagerDbContext context, ILogger logger)
{
    private readonly GolfManagerDbContext _context = context;
    private readonly ILogger _logger = logger;
    
    // Mapping dictionaries (old keys → new IDs)
    private readonly Dictionary<string, string> _leagueMap = new();
    private readonly Dictionary<string, string> _golferMap = new();  // Old GolferId → New LeagueGolferId
    private readonly Dictionary<string, string> _golferToGolferIdMap = new();  // Old GolferId → New GolferId (global)
    private readonly Dictionary<string, string> _courseMap = new();
    private readonly Dictionary<string, string> _teeMap = new();
    private readonly Dictionary<string, string> _teeIdMap = new(); // Old TeeId -> New TeeId
    private readonly Dictionary<string, string> _seasonMap = new();
    private readonly Dictionary<string, string> _seasonEventMap = new(); // Old SeasonEventId → New ID
    private readonly Dictionary<string, string> _teamMap = new(); // Old TeamId → New ID
    private readonly Dictionary<string, string> _roundMap = new(); // Old RoundId -> New RoundId

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
            await ImportRoundsAndRoundHolesAsync(sqlContent);
            await EnsureImportedLeagueHas2026SeasonAsync();

            LogImportSummary();

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

            leagueId = Guid.NewGuid().ToString();
            _leagueMap[key] = leagueId;

            var league = new League
            {
                Id = leagueId,
                Key = key,
                Name = name,
                Description = $"Imported from Holy Grail on {DateTime.Now:yyyy-MM-dd}",
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

                var userId = Guid.NewGuid().ToString();
                var golferId = Guid.NewGuid().ToString();
                var leagueGolferId = Guid.NewGuid().ToString();

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
                        Id = Guid.NewGuid().ToString(),
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

            var courseId = Guid.NewGuid().ToString();
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

        // Current backup format:
        // (Id, CourseKey, Name, HtmlColorCode, RatingOut, SlopeOut, RatingIn, SlopeIn, YardsOut, YardsIn, ParOut, ParIn)
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfTees\] VALUES \(([^)]+)\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            try
            {
                var values = ParseSqlValues(match.Groups[1].Value);
                if (values.Count < 12)
                {
                    _logger.LogWarning("Skipping tee row with unexpected value count: {Count}", values.Count);
                    continue;
                }

                var oldTeeId = values[0];
                var courseKey = values[1];
                var teeName = values[2];
                var color = values[3];
                var ratingOut = double.Parse(values[4]);
                var slopeOut = int.Parse(values[5]);
                var ratingIn = double.Parse(values[6]);
                var slopeIn = int.Parse(values[7]);
                var yardsOut = int.Parse(values[8]);
                var yardsIn = int.Parse(values[9]);
                var parOut = int.Parse(values[10]);
                var parIn = int.Parse(values[11]);

                if (!_courseMap.TryGetValue(courseKey, out var courseId))
                {
                    _logger.LogWarning("Course not found for tee: {CourseKey}", courseKey);
                    continue;
                }

                var teeId = Guid.NewGuid().ToString();
                _teeMap[$"{courseKey}_{teeName}"] = teeId;
                _teeIdMap[oldTeeId] = teeId;

                var tee = new Tee
                {
                    Id = teeId,
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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                };

                _context.Tees.Add(tee);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse tee row: {Row}", match.Value.Substring(0, Math.Min(200, match.Value.Length)));
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} tees", matches.Count);
    }

    private async Task ImportHolesAndHoleTeesAsync(string sqlContent)
    {
        _logger.LogInformation("Importing Holes and HoleTees...");

        // First import Holes
        // Holes are uniquely constrained by (CourseId, HoleNumber).
        // Keep batch persistence to limit change tracker growth on large imports.
        var holePattern = @"INSERT INTO \[dbo\]\.\[GolfHoles\] VALUES \(([^)]+)\);";
        var holeMatches = Regex.Matches(sqlContent, holePattern);

        string? currentCourseKey = null;
        int holeImportCount = 0;

        foreach (Match match in holeMatches)
        {
            try
            {
                var values = ParseSqlValues(match.Groups[1].Value);
                if (values.Count < 3)
                {
                    _logger.LogWarning("Skipping hole row with unexpected value count: {Count}", values.Count);
                    continue;
                }

                var courseKey = values[0];
                var holeNumber = int.Parse(values[1]);
                var holeName = values[2];

                if (!_courseMap.TryGetValue(courseKey, out var courseId))
                {
                    continue;
                }

                // Save + clear whenever the course changes to avoid alternate-key conflicts
                if (currentCourseKey != null && currentCourseKey != courseKey)
                {
                    await _context.SaveChangesAsync();
                    _context.ChangeTracker.Clear();
                }
                currentCourseKey = courseKey;

                var hole = new Hole
                {
                    Id = Guid.NewGuid().ToString(),
                    CourseId = courseId,
                    HoleNumber = holeNumber,
                    Name = holeName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                };

                _context.Holes.Add(hole);
                holeImportCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse hole row: {Row}", match.Value.Substring(0, Math.Min(200, match.Value.Length)));
            }
        }

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        _logger.LogInformation("✓ Imported {Count} holes", holeImportCount);

        // Then import HoleTees
        var holeTeePattern = @"INSERT INTO \[dbo\]\.\[GolfHoleTees\] VALUES \(([^)]+)\);";
        var holeTeeMatches = Regex.Matches(sqlContent, holeTeePattern);

        foreach (Match match in holeTeeMatches)
        {
            try
            {
                var values = ParseSqlValues(match.Groups[1].Value);
                if (values.Count < 5)
                {
                    _logger.LogWarning("Skipping hole tee row with unexpected value count: {Count}", values.Count);
                    continue;
                }

                string? teeId = null;
                int holeNumber;
                int par;
                int yardage;
                int handicap;

                // Current format: (TeeId, HoleNumber, Par, Yardage, Handicap)
                if (_teeIdMap.TryGetValue(values[0], out var mappedByOldId))
                {
                    teeId = mappedByOldId;
                    holeNumber = int.Parse(values[1]);
                    par = int.Parse(values[2]);
                    yardage = int.Parse(values[3]);
                    handicap = int.Parse(values[4]);
                }
                // Legacy fallback: (CourseKey, TeeName, HoleNumber, Par, Yardage, Handicap, ...)
                else if (values.Count >= 6 && _teeMap.TryGetValue($"{values[0]}_{values[1]}", out var mappedByName))
                {
                    teeId = mappedByName;
                    holeNumber = int.Parse(values[2]);
                    par = int.Parse(values[3]);
                    yardage = int.Parse(values[4]);
                    handicap = int.Parse(values[5]);
                }
                else
                {
                    continue;
                }

                var holeTee = new HoleTee
                {
                    Id = Guid.NewGuid().ToString(),
                    TeeId = teeId,
                    HoleNumber = holeNumber,
                    Par = par,
                    Yardage = yardage,
                    Handicap = handicap,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                };

                _context.HoleTees.Add(holeTee);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse hole tee row: {Row}", match.Value.Substring(0, Math.Min(200, match.Value.Length)));
            }
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

            seasonId = Guid.NewGuid().ToString();
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Seasons.Add(season);
        }

        await _context.SaveChangesAsync();

        // Set each league's active season after all seasons are imported.
        // Prefer the most recent unlocked season; fall back to most recent by start date.
        foreach (var leagueId in _leagueMap.Values.Distinct())
        {
            var activeSeason = await _context.Seasons
                .IgnoreQueryFilters()
                .Where(s => s.LeagueId == leagueId && s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.IsLocked)
                .ThenByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (activeSeason == null)
            {
                continue;
            }

            var league = await _context.Leagues
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == leagueId);

            if (league != null)
            {
                league.ActiveSeasonId = activeSeason.Id;
                league.UpdatedAt = DateTime.UtcNow;
            }
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
                    Id = Guid.NewGuid().ToString(),
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

        // Pattern example: INSERT INTO [dbo].[GolfSeasonTeams] VALUES ('TEAMID', 'dkgl', '2024', 'Team Name', '', '145', '2024-05-06 07:59:32');
        // Note: season points are quoted in the backup.
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfSeasonTeams\] VALUES \(([^)]+)\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            try
            {
                var values = ParseSqlValues(match.Groups[1].Value);
                if (values.Count < 4)
                {
                    _logger.LogWarning("Skipping season team row with unexpected value count: {Count}", values.Count);
                    continue;
                }

                var teamId = values[0];
                var leagueKey = values[1];
                var seasonKey = values[2];
                var teamName = values[3];

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

                var newTeamId = Guid.NewGuid().ToString();
                _teamMap[teamId] = newTeamId; // Store in class-level map for later use

                var team = new SeasonTeam
                {
                    Id = newTeamId,
                    SeasonId = seasonId,
                    LeagueId = leagueId,
                    Name = teamName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SeasonTeams.Add(team);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse season team");
            }
        }

        await _context.SaveChangesAsync();
        var seasonTeamCount = await _context.SeasonTeams.IgnoreQueryFilters().CountAsync();
        _logger.LogInformation("✓ Imported {Count} season teams", seasonTeamCount);
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
                if (values.Count < 3) continue;

                // Correct order from backup file
                var leagueKey = values[0];
                var seasonKey = values[1];
                var oldGolferId = values[2];
                var oldTeamId = values.Count > 3 ? values[3] : null;

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

                var seasonGolfer = new SeasonGolfer
                {
                    Id = Guid.NewGuid().ToString(),
                    SeasonId = seasonId,
                    LeagueId = leagueId,
                    LeagueGolferId = leagueGolferId, // Correct: LeagueGolfer ID
                    GolferId = globalGolferId, // Correct: Global Golfer ID
                    TeamId = !string.IsNullOrWhiteSpace(oldTeamId) && _teamMap.TryGetValue(oldTeamId, out var mappedTeamId)
                        ? mappedTeamId
                        : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SeasonGolfers.Add(seasonGolfer);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse season golfer");
            }
        }

        await _context.SaveChangesAsync();
        var seasonGolferCount = await _context.SeasonGolfers.IgnoreQueryFilters().CountAsync();
        _logger.LogInformation("✓ Imported {Count} season golfers", seasonGolferCount);
    }

    private async Task EnsureImportedLeagueHas2026SeasonAsync()
    {
        // Holy Grail import includes seasons through 2025. Ensure 2025 is closed and 2026 exists.
        foreach (var leagueId in _leagueMap.Values.Distinct())
        {
            var season2025 = await _context.Seasons
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.LeagueId == leagueId && s.Key == "2025" && s.IsActive && !s.IsDeleted);

            if (season2025 != null && !season2025.IsLocked)
            {
                season2025.IsLocked = true;
                season2025.UpdatedAt = DateTime.UtcNow;
                season2025.UpdatedBy = "system";
            }

            var season2026 = await _context.Seasons
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.LeagueId == leagueId && s.Key == "2026" && s.IsActive && !s.IsDeleted);

            if (season2026 == null)
            {
                var startDate = season2025 != null
                    ? season2025.StartDate.AddYears(1)
                    : DateOnly.FromDateTime(new DateTime(DateTime.UtcNow.Year, 5, 1));

                season2026 = new Season
                {
                    Id = Guid.NewGuid().ToString(),
                    LeagueId = leagueId,
                    Key = "2026",
                    Name = "2026",
                    StartDate = startDate,
                    EndDate = null,
                    IsLocked = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                };

                _context.Seasons.Add(season2026);
                await _context.SaveChangesAsync();
            }

            if (season2025 != null)
            {
                await CloneSeasonTeamsAndPlayersAsync(leagueId, season2025.Id, season2026.Id);
            }

            var league = await _context.Leagues
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == leagueId);

            if (league != null)
            {
                league.ActiveSeasonId = season2026.Id;
                league.UpdatedAt = DateTime.UtcNow;
                league.UpdatedBy = "system";
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task CloneSeasonTeamsAndPlayersAsync(string leagueId, string fromSeasonId, string toSeasonId)
    {
        var existingTeamCount = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .CountAsync(t => t.LeagueId == leagueId && t.SeasonId == toSeasonId && t.IsActive && !t.IsDeleted);

        var targetTeamsByName = new Dictionary<string, SeasonTeam>(StringComparer.OrdinalIgnoreCase);

        if (existingTeamCount == 0)
        {
            var sourceTeams = await _context.SeasonTeams
                .IgnoreQueryFilters()
                .Where(t => t.LeagueId == leagueId && t.SeasonId == fromSeasonId && t.IsActive && !t.IsDeleted)
                .OrderBy(t => t.Name)
                .ToListAsync();

            foreach (var sourceTeam in sourceTeams)
            {
                var clone = new SeasonTeam
                {
                    Id = Guid.NewGuid().ToString(),
                    LeagueId = leagueId,
                    SeasonId = toSeasonId,
                    Name = sourceTeam.Name,
                    AvatarUrl = sourceTeam.AvatarUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                };

                _context.SeasonTeams.Add(clone);
                targetTeamsByName[sourceTeam.Name] = clone;
            }

            await _context.SaveChangesAsync();
        }
        else
        {
            var existingTeams = await _context.SeasonTeams
                .IgnoreQueryFilters()
                .Where(t => t.LeagueId == leagueId && t.SeasonId == toSeasonId && t.IsActive && !t.IsDeleted)
                .ToListAsync();

            foreach (var team in existingTeams)
            {
                targetTeamsByName[team.Name] = team;
            }
        }

        var existingLeagueGolferIds = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .Where(sg => sg.LeagueId == leagueId && sg.SeasonId == toSeasonId && sg.IsActive && !sg.IsDeleted)
            .Select(sg => sg.LeagueGolferId)
            .ToListAsync();

        var existingLeagueGolferSet = new HashSet<string>(existingLeagueGolferIds, StringComparer.OrdinalIgnoreCase);

        var sourceGolfers = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .Include(sg => sg.Team)
            .Where(sg => sg.LeagueId == leagueId && sg.SeasonId == fromSeasonId && sg.IsActive && !sg.IsDeleted)
            .ToListAsync();

        foreach (var sourceGolfer in sourceGolfers)
        {
            if (existingLeagueGolferSet.Contains(sourceGolfer.LeagueGolferId))
            {
                continue;
            }

            string? targetTeamId = null;
            var sourceTeamName = sourceGolfer.Team?.Name;
            if (!string.IsNullOrWhiteSpace(sourceTeamName)
                && targetTeamsByName.TryGetValue(sourceTeamName, out var targetTeam))
            {
                targetTeamId = targetTeam.Id;
            }

            var clone = new SeasonGolfer
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = leagueId,
                SeasonId = toSeasonId,
                LeagueGolferId = sourceGolfer.LeagueGolferId,
                GolferId = sourceGolfer.GolferId,
                TeamId = targetTeamId,
                SeasonHandicap = sourceGolfer.SeasonHandicap,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _context.SeasonGolfers.Add(clone);
            existingLeagueGolferSet.Add(sourceGolfer.LeagueGolferId);
        }

        await _context.SaveChangesAsync();
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

            // Find the tee for this course
            var teeId = _teeMap.TryGetValue($"{courseKey}_{teeKey}", out var foundTeeId) ? foundTeeId : _teeMap.Values.FirstOrDefault();

            // Preserve the legacy event key as the v2 ID so deep links remain stable across reseeds.
            var seasonEventId = eventKey;
            _seasonEventMap[eventKey] = seasonEventId; // Store for match import

            // Map scoring format
            var scoringFormat = eventType.Equals("Match", StringComparison.OrdinalIgnoreCase) ? ScoringFormat.TwoPoint : ScoringFormat.StrokePlay;

            var seasonEvent = new SeasonEvent
            {
                Id = seasonEventId,
                SeasonId = seasonId,
                LeagueId = leagueId,
                Name = $"{seasonKey} Event - {eventDate:MMM dd}",
                EventDate = eventDate,
                EventType = SeasonEventType.Regular, // All imported events are regular season events
                ScoringFormat = scoringFormat,
                CourseId = courseId,
                TeeId = teeId,
                HolesPlayed = holesPlayed.Equals("Back", StringComparison.OrdinalIgnoreCase) ? HolesPlayed.Back :
                              holesPlayed.Equals("Front", StringComparison.OrdinalIgnoreCase) ? HolesPlayed.Front : HolesPlayed.Eighteen,
                Status = EventStatus.Completed, // Historical events are completed
                IsLocked = true, // Lock historical events
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SeasonEvents.Add(seasonEvent);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} season events", matches.Count);
    }

    private async Task ImportSeasonEventMatchesAsync(string sqlContent)
    {
        _logger.LogInformation("Importing SeasonEventMatches...");

        // Pattern example: INSERT INTO [dbo].[GolfSeasonEventMatches] VALUES ('EVENTKEY', 'SCORECARDID', NULL, 'HOMETEAMID', 'AWAYTEAMID', '16', '6', '10', '1');
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfSeasonEventMatches\] VALUES \(([^)]+)\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            try
            {
                var values = ParseSqlValues(match.Groups[1].Value);
                if (values.Count < 9)
                {
                    _logger.LogWarning("Skipping season event match row with unexpected value count: {Count}", values.Count);
                    continue;
                }

                // Legacy row shape:
                // (MatchId, SeasonEventKey, ScorecardId, HomeTeamId, AwayTeamId, HomePoints, AwayPoints, StartingHole, StartingFlight)
                var eventKey = values[1];
                var scorecardId = string.IsNullOrWhiteSpace(values[2]) ? null : values[2];
                var homeTeamId = string.IsNullOrWhiteSpace(values[3]) ? null : values[3];
                var awayTeamId = string.IsNullOrWhiteSpace(values[4]) ? null : values[4];
                var homePoints = !string.IsNullOrWhiteSpace(values[5]) ? double.Parse(values[5]) : (double?)null;
                var awayPoints = !string.IsNullOrWhiteSpace(values[6]) ? double.Parse(values[6]) : (double?)null;
                var startingHole = !string.IsNullOrWhiteSpace(values[7]) ? int.Parse(values[7]) : (int?)null;
                var startingFlight = !string.IsNullOrWhiteSpace(values[8]) ? int.Parse(values[8]) : (int?)null;

                // Historical imports are completed matches
                var isComplete = true;

                if (!_seasonEventMap.TryGetValue(eventKey, out var seasonEventId))
                {
                    _logger.LogWarning("Season event not found for match: {EventKey}", eventKey);
                    continue;
                }

                var leagueId = _leagueMap.Values.FirstOrDefault();
                if (string.IsNullOrEmpty(leagueId))
                {
                    _logger.LogWarning("League not found for season event match");
                    continue;
                }

                // Map team IDs
                var mappedHomeTeamId = !string.IsNullOrEmpty(homeTeamId) && _teamMap.TryGetValue(homeTeamId, out var hId) ? hId : null;
                var mappedAwayTeamId = !string.IsNullOrEmpty(awayTeamId) && _teamMap.TryGetValue(awayTeamId, out var aId) ? aId : null;

                var seasonEventMatch = new SeasonEventMatch
                {
                    Id = Guid.NewGuid().ToString(),
                    SeasonEventId = seasonEventId,
                    LeagueId = leagueId,
                    ScorecardId = scorecardId, // Note: This may not exist in v2 yet
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
        var seasonEventMatchCount = await _context.SeasonEventMatches.IgnoreQueryFilters().CountAsync();
        _logger.LogInformation("✓ Imported {Count} season event matches", seasonEventMatchCount);
    }

    private async Task ImportRoundsAndRoundHolesAsync(string sqlContent)
    {
        _logger.LogInformation("Importing Rounds and RoundHoles...");

        var roundDateByOldRoundId = await BuildRoundDateMapAsync(sqlContent);
        var roundByNewId = new Dictionary<string, Round>();

        var roundPattern = @"INSERT INTO \[dbo\]\.\[GolfRounds\] VALUES \(([^)]+)\);";
        var roundMatches = Regex.Matches(sqlContent, roundPattern);

        foreach (Match match in roundMatches)
        {
            try
            {
                var values = ParseSqlValues(match.Groups[1].Value);
                if (values.Count < 6)
                {
                    _logger.LogWarning("Skipping round row with unexpected value count: {Count}", values.Count);
                    continue;
                }

                var oldRoundId = values[0];
                var oldScorecardId = values[1];
                var oldGolferId = values[2];
                var courseKey = values[3];
                var oldTeeId = values[4];
                var rawScore = string.IsNullOrWhiteSpace(values[5]) ? (int?)null : int.Parse(values[5]);

                if (!_golferToGolferIdMap.TryGetValue(oldGolferId, out var golferId))
                {
                    _logger.LogWarning("Golfer not found for round: {GolferId}", oldGolferId);
                    continue;
                }

                _golferMap.TryGetValue(oldGolferId, out var leagueGolferId);

                if (!_courseMap.TryGetValue(courseKey, out var courseId))
                {
                    _logger.LogWarning("Course not found for round: {CourseKey}", courseKey);
                    continue;
                }

                if (!_teeIdMap.TryGetValue(oldTeeId, out var teeId))
                {
                    _logger.LogWarning("Tee not found for round: {TeeId}", oldTeeId);
                    continue;
                }

                var roundId = Guid.NewGuid().ToString();
                _roundMap[oldRoundId] = roundId;

                var roundDate = roundDateByOldRoundId.TryGetValue(oldRoundId, out var historicalDate)
                    ? historicalDate
                    : DateTime.UtcNow;

                var round = new Round
                {
                    Id = roundId,
                    GolferId = golferId,
                    LeagueGolferId = leagueGolferId,
                    LeagueId = _leagueMap.Values.FirstOrDefault(),
                    CourseId = courseId,
                    TeeId = teeId,
                    RoundDate = roundDate,
                    HolesPlayed = HolesPlayed.Eighteen,
                    TotalScore = rawScore,
                    IsComplete = true,
                    Notes = "Imported from Holy Grail v1",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                };

                _context.Rounds.Add(round);
                roundByNewId[roundId] = round;

                if (!string.IsNullOrWhiteSpace(oldScorecardId))
                {
                    var scorecard = new Scorecard
                    {
                        Id = Guid.NewGuid().ToString(),
                        RoundId = roundId,
                        Notes = $"Imported from Holy Grail v1 scorecard {oldScorecardId}",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "system"
                    };

                    _context.Scorecards.Add(scorecard);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse round row: {Row}", match.Value.Substring(0, Math.Min(200, match.Value.Length)));
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} rounds", _roundMap.Count);

        var roundHolePattern = @"INSERT INTO \[dbo\]\.\[GolfRoundHoles\] VALUES \(([^)]+)\);";
        var roundHoleMatches = Regex.Matches(sqlContent, roundHolePattern);
        var holeRangeByRound = new Dictionary<string, (bool hasFront, bool hasBack)>();

        foreach (Match match in roundHoleMatches)
        {
            try
            {
                var values = ParseSqlValues(match.Groups[1].Value);
                if (values.Count < 8)
                {
                    _logger.LogWarning("Skipping round hole row with unexpected value count: {Count}", values.Count);
                    continue;
                }

                var oldRoundId = values[0];
                if (!_roundMap.TryGetValue(oldRoundId, out var roundId))
                {
                    continue;
                }

                var holeNumber = int.Parse(values[1]);
                var rawScore = string.IsNullOrWhiteSpace(values[7]) ? (int?)null : int.Parse(values[7]);
                var putts = values.Count > 8 && !string.IsNullOrWhiteSpace(values[8]) ? int.Parse(values[8]) : (int?)null;
                var penalties = values.Count > 10 && !string.IsNullOrWhiteSpace(values[10]) ? int.Parse(values[10]) : (int?)null;

                var roundHole = new RoundHole
                {
                    Id = Guid.NewGuid().ToString(),
                    RoundId = roundId,
                    HoleNumber = holeNumber,
                    GrossScore = rawScore,
                    Putts = putts,
                    Penalties = penalties,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                };

                _context.RoundHoles.Add(roundHole);

                var (hasFront, hasBack) = holeRangeByRound.TryGetValue(roundId, out var tracked)
                    ? tracked
                    : (false, false);
                if (holeNumber <= 9)
                {
                    hasFront = true;
                }
                if (holeNumber >= 10)
                {
                    hasBack = true;
                }
                holeRangeByRound[roundId] = (hasFront, hasBack);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse round hole row: {Row}", match.Value.Substring(0, Math.Min(200, match.Value.Length)));
            }
        }

        await _context.SaveChangesAsync();

        foreach (var kvp in holeRangeByRound)
        {
            if (!roundByNewId.TryGetValue(kvp.Key, out var round))
            {
                continue;
            }

            round.HolesPlayed = kvp.Value switch
            {
                (true, false) => HolesPlayed.Front,
                (false, true) => HolesPlayed.Back,
                _ => HolesPlayed.Eighteen
            };
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Imported {Count} round holes", _context.RoundHoles.Count());

        await RecalculateLeagueGolferStatsAsync();
    }

    private async Task RecalculateLeagueGolferStatsAsync()
    {
        _logger.LogInformation("Recalculating LeagueGolfer stats from imported rounds...");

        var leagueGolfers = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .ToListAsync();

        var roundStatsByLeagueGolfer = await _context.Rounds
            .IgnoreQueryFilters()
            .Where(r => r.LeagueGolferId != null && r.TotalScore != null)
            .GroupBy(r => r.LeagueGolferId!)
            .Select(g => new
            {
                LeagueGolferId = g.Key,
                Count = g.Count(),
                Average = g.Average(r => (double)r.TotalScore!.Value),
                Best = g.Min(r => r.TotalScore!.Value)
            })
            .ToDictionaryAsync(x => x.LeagueGolferId);

        foreach (var lg in leagueGolfers)
        {
            if (roundStatsByLeagueGolfer.TryGetValue(lg.Id, out var stats))
            {
                lg.TotalRounds = stats.Count;
                lg.AverageScore = stats.Average;
                lg.BestScore = stats.Best;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Recalculated stats for {Count} league golfers", leagueGolfers.Count);
    }

    private async Task<Dictionary<string, DateTime>> BuildRoundDateMapAsync(string sqlContent)
    {
        var roundDateByOldRoundId = new Dictionary<string, DateTime>();

        var newEventDates = await _context.SeasonEvents
            .AsNoTracking()
            .ToDictionaryAsync(e => e.Id, e => e.EventDate);

        var seasonEventGolferPattern = @"INSERT INTO \[dbo\]\.\[GolfSeasonEventGolfers\] VALUES \(([^)]+)\);";
        var matches = Regex.Matches(sqlContent, seasonEventGolferPattern);

        foreach (Match match in matches)
        {
            try
            {
                var values = ParseSqlValues(match.Groups[1].Value);
                if (values.Count < 3)
                {
                    continue;
                }

                var oldSeasonEventId = values[1];
                var oldRoundId = values[2];

                if (string.IsNullOrWhiteSpace(oldRoundId) || !_seasonEventMap.TryGetValue(oldSeasonEventId, out var newSeasonEventId))
                {
                    continue;
                }

                if (newEventDates.TryGetValue(newSeasonEventId, out var eventDate))
                {
                    roundDateByOldRoundId[oldRoundId] = eventDate;
                }
            }
            catch
            {
                // Ignore malformed rows; rounds can still import with fallback dates.
            }
        }

        return roundDateByOldRoundId;
    }

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

    private void LogImportSummary()
    {
        _logger.LogInformation("===========================================");
        _logger.LogInformation("Import Summary");
        _logger.LogInformation("===========================================");
        _logger.LogInformation("  Leagues:       {Count}", _leagueMap.Count);
        _logger.LogInformation("  Golfers:       {Count} (global), {Count2} (league members)",
            _golferToGolferIdMap.Count, _golferMap.Count);
        _logger.LogInformation("  Courses:       {Count}", _courseMap.Count);
        _logger.LogInformation("  Tees:          {Count}", _teeIdMap.Count);
        _logger.LogInformation("  Seasons:       {Count}", _seasonMap.Count);
        _logger.LogInformation("  Season Events: {Count}", _seasonEventMap.Count);
        _logger.LogInformation("  Season Teams:  {Count}", _teamMap.Count);
        _logger.LogInformation("  Rounds:        {Count}", _roundMap.Count);
        _logger.LogInformation("===========================================");

        // Spot-check DB counts for verification
        Task.Run(async () =>
        {
            try
            {
                var leagues        = await _context.Leagues.IgnoreQueryFilters().CountAsync();
                var golfers        = await _context.Golfers.IgnoreQueryFilters().CountAsync();
                var leagueGolfers  = await _context.LeagueGolfers.IgnoreQueryFilters().CountAsync();
                var courses        = await _context.Courses.IgnoreQueryFilters().CountAsync();
                var tees           = await _context.Tees.IgnoreQueryFilters().CountAsync();
                var seasons        = await _context.Seasons.IgnoreQueryFilters().CountAsync();
                var events         = await _context.SeasonEvents.IgnoreQueryFilters().CountAsync();
                var rounds         = await _context.Rounds.IgnoreQueryFilters().CountAsync();
                var roundHoles     = await _context.RoundHoles.IgnoreQueryFilters().CountAsync();

                _logger.LogInformation("=== DB Record Counts Post-Import ===");
                _logger.LogInformation("  Leagues:       {Count}", leagues);
                _logger.LogInformation("  Golfers:       {Golfers} global / {LG} league members", golfers, leagueGolfers);
                _logger.LogInformation("  Courses:       {Count}", courses);
                _logger.LogInformation("  Tees:          {Count}", tees);
                _logger.LogInformation("  Seasons:       {Count}", seasons);
                _logger.LogInformation("  Season Events: {Count}", events);
                _logger.LogInformation("  Rounds:        {Count}", rounds);
                _logger.LogInformation("  Round Holes:   {Count}", roundHoles);
                _logger.LogInformation("=====================================");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not retrieve post-import DB counts");
            }
        }).GetAwaiter().GetResult();
    }
}
