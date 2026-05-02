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
    private readonly Dictionary<string, string> _seasonMap = new();
    private readonly Dictionary<string, string> _seasonEventMap = new(); // Old SeasonEventId → New ID
    private readonly Dictionary<string, string> _teamMap = new(); // Old TeamId → New ID

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
            // Note: Rounds and RoundHoles have a different structure in v2 (linked via Scorecard)
            // Importing them would require creating Scorecards first
            // Skipping for now - can be added if needed
            _logger.LogInformation("⚠️ Rounds/RoundHoles import skipped - v2 uses different structure (Scorecard-based)");

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

        // Pattern: INSERT INTO [dbo].[GolfTees] VALUES ('courseKey', 'TeeName', '#COLOR', rating, slope, yards, par, ...);
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfTees\] VALUES \('([^']+)',\s*'([^']+)',\s*'([^']*)',\s*([0-9.]+),\s*([0-9.]+),\s*(\d+),\s*(\d+),\s*(\d+),\s*([0-9.]+),\s*([0-9.]+),\s*(\d+),\s*(\d+),\s*(\d+),\s*'([^']*)'\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            var courseKey = match.Groups[1].Value;
            var teeName = match.Groups[2].Value;
            var color = match.Groups[3].Value;
            var ratingOut = double.Parse(match.Groups[4].Value);
            var ratingIn = double.Parse(match.Groups[5].Value);
            var slopeOut = int.Parse(match.Groups[6].Value);
            var slopeIn = int.Parse(match.Groups[7].Value);
            var yardsOut = int.Parse(match.Groups[8].Value);
            var yardsIn = int.Parse(match.Groups[10].Value);
            var parOut = int.Parse(match.Groups[11].Value);
            var parIn = int.Parse(match.Groups[12].Value);

            if (!_courseMap.TryGetValue(courseKey, out var courseId))
            {
                _logger.LogWarning("Course not found for tee: {CourseKey}", courseKey);
                continue;
            }

            var teeId = Guid.NewGuid().ToString();
            _teeMap[$"{courseKey}_{teeName}"] = teeId;

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

            var holeId = Guid.NewGuid().ToString();
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
        var holeTeePattern = @"INSERT INTO \[dbo\]\.\[GolfHoleTees\] VALUES \('([^']+)',\s*'([^']+)',\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*'([^']*)'\);";
        var holeTeeMatches = Regex.Matches(sqlContent, holeTeePattern);

        foreach (Match match in holeTeeMatches)
        {
            var courseKey = match.Groups[1].Value;
            var teeName = match.Groups[2].Value;
            var holeNumber = int.Parse(match.Groups[3].Value);
            var par = int.Parse(match.Groups[4].Value);
            var yardage = int.Parse(match.Groups[5].Value);
            var handicap = int.Parse(match.Groups[6].Value);

            if (!_teeMap.TryGetValue($"{courseKey}_{teeName}", out var teeId)) continue;

            var holeTee = new HoleTee
            {
                Id = Guid.NewGuid().ToString(),
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

        // Pattern: INSERT INTO [dbo].[GolfSeasonTeams] VALUES ('TEAMID', 'dkgl', '2024', 'Team Name', NULL, 150.5, '2024-05-01 00:00:00');
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfSeasonTeams\] VALUES \('([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*(?:'([^']*)'|NULL),\s*(?:([0-9.]+)|NULL),\s*'([^']+)'\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            var teamId = match.Groups[1].Value;
            var leagueKey = match.Groups[2].Value;
            var seasonKey = match.Groups[3].Value;
            var teamName = match.Groups[4].Value;
            var avatar = match.Groups[5].Success ? match.Groups[5].Value : null;
            var seasonPoints = match.Groups[6].Success ? double.Parse(match.Groups[6].Value) : (double?)null;

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
                if (values.Count < 3) continue;

                // Correct order from backup file
                var leagueKey = values[0];
                var seasonKey = values[1];
                var oldGolferId = values[2];

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

            // Find the tee for this course
            var teeId = _teeMap.TryGetValue($"{courseKey}_{teeKey}", out var foundTeeId) ? foundTeeId : _teeMap.Values.FirstOrDefault();

            var seasonEventId = Guid.NewGuid().ToString();
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

        // Pattern: INSERT INTO [dbo].[GolfSeasonEventMatches] VALUES ('EVENTID', 'dkgl', '2024', NULL, 'HOMETEAMID', 'AWAYTEAMID', 1.5, 1.5, 1, 1, 'True');
        var pattern = @"INSERT INTO \[dbo\]\.\[GolfSeasonEventMatches\] VALUES \('([^']+)',\s*'([^']+)',\s*'([^']+)',\s*(?:'([^']*)'|NULL),\s*(?:'([^']*)'|NULL),\s*(?:'([^']*)'|NULL),\s*(?:([0-9.]+)|NULL),\s*(?:([0-9.]+)|NULL),\s*(?:(\d+)|NULL),\s*(?:(\d+)|NULL),\s*'([^']+)'\);";
        var matches = Regex.Matches(sqlContent, pattern);

        foreach (Match match in matches)
        {
            try
            {
                var eventKey = match.Groups[1].Value;
                var leagueKey = match.Groups[2].Value;
                var seasonKey = match.Groups[3].Value;
                var scorecardId = match.Groups[4].Success ? match.Groups[4].Value : null;
                var homeTeamId = match.Groups[5].Success ? match.Groups[5].Value : null;
                var awayTeamId = match.Groups[6].Success ? match.Groups[6].Value : null;
                var homePoints = match.Groups[7].Success ? double.Parse(match.Groups[7].Value) : (double?)null;
                var awayPoints = match.Groups[8].Success ? double.Parse(match.Groups[8].Value) : (double?)null;
                var startingHole = match.Groups[9].Success ? int.Parse(match.Groups[9].Value) : (int?)null;
                var startingFlight = match.Groups[10].Success ? int.Parse(match.Groups[10].Value) : (int?)null;
                var isComplete = match.Groups[11].Value.Equals("True", StringComparison.OrdinalIgnoreCase);

                if (!_seasonEventMap.TryGetValue(eventKey, out var seasonEventId))
                {
                    _logger.LogWarning("Season event not found for match: {EventKey}", eventKey);
                    continue;
                }

                if (!_leagueMap.TryGetValue(leagueKey, out var leagueId))
                {
                    _logger.LogWarning("League not found for match: {LeagueKey}", leagueKey);
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
        _logger.LogInformation("✓ Imported {Count} season event matches", _context.SeasonEventMatches.Count());
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
}
