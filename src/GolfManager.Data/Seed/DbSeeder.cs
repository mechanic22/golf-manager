using GolfManager.Core.Entities;
using GolfManager.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace GolfManager.Data.Seed;

/// <summary>
/// Seeds the database with initial demo data
/// </summary>
public class DbSeeder
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(GolfManagerDbContext context, ILogger<DbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Hash password using BCrypt (matches the IPasswordHasher implementation)
    /// </summary>
    private static string HashPassword(string password)
    {
        // Use BCrypt to match the IPasswordHasher service
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    private static string Slugify(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        normalized = Regex.Replace(normalized, @"[^a-z0-9]+", ".");
        normalized = Regex.Replace(normalized, @"\.+", ".").Trim('.');
        return normalized;
    }

    private static (string FirstName, string LastName) SplitName(string displayName)
    {
        var parts = displayName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return ("Player", "Unknown");
        }

        if (parts.Length == 1)
        {
            return (parts[0], "Player");
        }

        return (parts[0], string.Join(" ", parts.Skip(1)));
    }

    private static string GetLastName(string displayName)
    {
        var parts = displayName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return "Unknown";
        }

        return parts[^1];
    }

    private static string BuildUniqueEmail(string displayName, HashSet<string> usedEmails)
    {
        var slug = Slugify(displayName);
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = $"player.{Guid.NewGuid():N}";
        }

        var baseEmail = $"{slug}@dkgl.local";
        var email = baseEmail;
        var counter = 2;

        while (!usedEmails.Add(email))
        {
            email = $"{slug}.{counter}@dkgl.local";
            counter++;
        }

        return email;
    }

    private async Task SeedDigiKey2026LeagueAsync(User ownerUser, Course defaultCourse, Tee defaultTee)
    {
        _logger.LogInformation("Seeding DigiKey Golf League 2026 data...");

        var dkglLeague = new League
        {
            Id = Guid.NewGuid().ToString(),
            Key = "dkgl",
            Name = "DigiKey Golf League",
            Description = "2026 DigiKey golf league seed data",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Leagues.Add(dkglLeague);
        await _context.SaveChangesAsync();

        _context.UserLeagues.Add(new UserLeague
        {
            UserId = ownerUser.Id,
            LeagueId = dkglLeague.Id,
            IsLeagueAdmin = true,
            Role = LeagueMemberRole.Owner,
            JoinedAt = DateTime.UtcNow
        });

        var teamRosters = new List<string[]>
        {
            new[] { "Kyle Beger", "Riley Bakken", "Reilly Stoltman" },
            new[] { "Tony Gilbert", "Cameron Carlson", "Matt McDermott" },
            new[] { "Paul Stauffacher", "Tony Fletcher", "Mike Swanson" },
            new[] { "Kyle Nelson", "Paul Kamrud", "Lee Peterson" },
            new[] { "Jared Hofstad", "Ryan Rogalla", "Bob Ballard" },
            new[] { "Jake Peterson", "Sam Harger", "Aaron Jacobson" },
            new[] { "Craig Mattson", "Jason Naslund", "Wayne Nomeland" },
            new[] { "Blake Hoglo", "Ron Ness", "Jeremy Williams" },
            new[] { "Rachel Grove", "Jim Hellie", "Charlie Wenker" },
            new[] { "Keith Woodruff", "Bryan Keefe", "Teri Vetsch" },
            new[] { "Brian Fay", "Chris Kramer", "Tim Maas" },
            new[] { "Travis Dazell", "Tyler Strand", "James Hurst" },
            new[] { "Sara Nelson", "Keary Petschl", "Bryn Nelson" },
            new[] { "Tyler Radniecki", "Evan Dondelinger", "Murphy Fellman" },
            new[] { "Zach Torell", "Brandon Johnsrud", "Aaron Munter" },
            new[] { "Jeremy Larson", "Brian Larson", "Travis Jurgens" },
            new[] { "Sue Kvick", "Terrell Verdell", "Vinny Sabandit" },
            new[] { "Darin Teie", "Al Erickson", "Mike Anderson" },
            new[] { "Rob Rogalla", "Andrew Curry", "Brian Metelak" },
            new[] { "Jason Langseth", "Shane Myhre", "Tom Olson" },
            new[] { "Nick Olson", "Paul Hejlik", "Chris Hultgren" },
            new[] { "Quinn Sullivan", "Adam Brillhart", "Matt Scott" },
            new[] { "Dylan Dunkel", "Garth Berg", "Eric Martin" },
            new[] { "Wayne Weyers", "Allie Weyers", "Jake Moen" },
            new[] { "Mark Schmitke", "Andrew Gallagher", "Jenny Brien" },
            new[] { "Jerry Blix", "Nick Wheat", "David Klemz" },
            new[] { "Camryn Schwab", "Karli Johnson", "Kris Petrich" },
            new[] { "Dan Mapes", "Nick Radeke", "Brandon Huewe" }
        };

        var subs = new[]
        {
            "Alex Dicken", "Josh Lawrence",
            "Cory Smith", "Chris Dahlen",
            "Parker Holt", "Steven Crummy",
            "Nick Haven", "Chris Swenson",
            "Kathy Dahlen", "Scott Daniels",
            "Brad Peterson", "Chris Jaeger",
            "Kaden Bakken"
        };

        var rosterNames = teamRosters
            .SelectMany(r => r)
            .Concat(subs)
            .Select(n => Regex.Replace(n, @"\s+", " ").Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var usedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existingEmails = await _context.Users
            .IgnoreQueryFilters()
            .Select(u => u.Email)
            .ToListAsync();
        foreach (var existing in existingEmails)
        {
            usedEmails.Add(existing);
        }

        var leagueGolfersByName = new Dictionary<string, LeagueGolfer>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in rosterNames)
        {
            var email = BuildUniqueEmail(name, usedEmails);
            var split = SplitName(name);

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                FirstName = split.FirstName,
                LastName = split.LastName,
                IsGlobalAdmin = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = HashPassword("Player123!")
            };

            var golfer = new Golfer
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                DisplayName = name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var leagueGolfer = new LeagueGolfer
            {
                Id = Guid.NewGuid().ToString(),
                GolferId = golfer.Id,
                LeagueId = dkglLeague.Id,
                DisplayName = name,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.Golfers.Add(golfer);
            _context.LeagueGolfers.Add(leagueGolfer);
            _context.UserLeagues.Add(new UserLeague
            {
                UserId = user.Id,
                LeagueId = dkglLeague.Id,
                LeagueGolferId = leagueGolfer.Id,
                IsLeagueAdmin = false,
                Role = LeagueMemberRole.Member,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            leagueGolfersByName[name] = leagueGolfer;
        }

        await _context.SaveChangesAsync();

        var season2026 = new Season
        {
            Id = Guid.NewGuid().ToString(),
            LeagueId = dkglLeague.Id,
            Key = "2026",
            Name = "2026",
            StartDate = new DateOnly(2026, 5, 4),
            EndDate = new DateOnly(2026, 8, 31),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Seasons.Add(season2026);
        await _context.SaveChangesAsync();

        dkglLeague.ActiveSeasonId = season2026.Id;
        dkglLeague.UpdatedAt = DateTime.UtcNow;

        var seasonTeams = new List<SeasonTeam>();
        for (int i = 0; i < teamRosters.Count; i++)
        {
            var teamName = string.Join(" / ", teamRosters[i].Select(GetLastName));
            var team = new SeasonTeam
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = dkglLeague.Id,
                SeasonId = season2026.Id,
                Name = teamName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            seasonTeams.Add(team);
            _context.SeasonTeams.Add(team);
        }

        await _context.SaveChangesAsync();

        for (int i = 0; i < teamRosters.Count; i++)
        {
            var team = seasonTeams[i];
            foreach (var rawName in teamRosters[i])
            {
                var name = Regex.Replace(rawName, @"\s+", " ").Trim();
                if (!leagueGolfersByName.TryGetValue(name, out var leagueGolfer))
                {
                    continue;
                }

                _context.SeasonGolfers.Add(new SeasonGolfer
                {
                    Id = Guid.NewGuid().ToString(),
                    LeagueId = dkglLeague.Id,
                    SeasonId = season2026.Id,
                    LeagueGolferId = leagueGolfer.Id,
                    GolferId = leagueGolfer.GolferId,
                    TeamId = team.Id,
                    JoinedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        foreach (var rawSub in subs)
        {
            var subName = Regex.Replace(rawSub, @"\s+", " ").Trim();
            if (!leagueGolfersByName.TryGetValue(subName, out var leagueGolfer))
            {
                continue;
            }

            _context.SeasonGolfers.Add(new SeasonGolfer
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = dkglLeague.Id,
                SeasonId = season2026.Id,
                LeagueGolferId = leagueGolfer.Id,
                GolferId = leagueGolfer.GolferId,
                TeamId = null,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        var weeks = new (int Week, DateOnly Date, string Notes)[]
        {
            (1,  new DateOnly(2026, 5,  4), "Regular Season"),
            (2,  new DateOnly(2026, 5, 11), "Regular Season"),
            (3,  new DateOnly(2026, 5, 18), "Regular Season"),
            (4,  new DateOnly(2026, 5, 25), "No League Play"),
            (5,  new DateOnly(2026, 6,  1), "Regular Season"),
            (6,  new DateOnly(2026, 6,  8), "Regular Season"),
            (7,  new DateOnly(2026, 6, 15), "Unlimited Mulligans from Tee"),
            (8,  new DateOnly(2026, 6, 22), "Regular Season"),
            (9,  new DateOnly(2026, 6, 29), "Regular Season"),
            (10, new DateOnly(2026, 7,  6), "Regular Play"),
            (11, new DateOnly(2026, 7, 13), "Regular Season"),
            (12, new DateOnly(2026, 7, 20), "Pre-Pay $5 for 3 redo's"),
            (13, new DateOnly(2026, 7, 27), "Regular Season"),
            (14, new DateOnly(2026, 8,  3), "Regular Season"),
            (15, new DateOnly(2026, 8, 10), "Regular Season"),
            (16, new DateOnly(2026, 8, 17), "Play Offs Begin"),
            (17, new DateOnly(2026, 8, 24), "Semi Finals and Toilet Bowl"),
            (18, new DateOnly(2026, 8, 31), "Championship Play")
        };

        foreach (var week in weeks)
        {
            var notesLower = week.Notes.ToLowerInvariant();
            var isNoPlay = notesLower.Contains("no league play");

            var eventType = SeasonEventType.Regular;
            if (notesLower.Contains("championship"))
            {
                eventType = SeasonEventType.Championship;
            }
            else if (notesLower.Contains("play off") || notesLower.Contains("semi finals"))
            {
                eventType = SeasonEventType.Playoff;
            }
            else if (!notesLower.Contains("regular"))
            {
                eventType = SeasonEventType.Special;
            }

            _context.SeasonEvents.Add(new SeasonEvent
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = dkglLeague.Id,
                SeasonId = season2026.Id,
                Name = $"Week {week.Week:D2}",
                Description = week.Notes,
                EventDate = week.Date.ToDateTime(new TimeOnly(17, 30)),
                CourseId = defaultCourse.Id,
                TeeId = defaultTee.Id,
                EventType = isNoPlay ? SeasonEventType.Special : eventType,
                Status = isNoPlay ? EventStatus.Cancelled : EventStatus.Published,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "DigiKey 2026 seeded: {Teams} teams, {RosterPlayers} roster players, {Subs} subs, {Weeks} weeks.",
            teamRosters.Count,
            teamRosters.Sum(r => r.Length),
            subs.Length,
            weeks.Length);
    }

    /// <summary>
    /// If rounds exist but all LeagueGolfer stats are zero, recalculate from actual round data.
    /// This handles databases imported before the auto-recalculation was added.
    /// </summary>
    private async Task RecalculateStatsIfNeededAsync()
    {
        var roundCount = await _context.Rounds.IgnoreQueryFilters().CountAsync();
        if (roundCount == 0) return;

        var anyWithStats = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .AnyAsync(lg => lg.TotalRounds > 0);

        if (anyWithStats)
        {
            _logger.LogInformation("Player stats already populated — skipping recalculation.");
            return;
        }

        _logger.LogInformation("Rounds found but player stats are zero — recalculating...");

        var roundStats = await _context.Rounds
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

        var leagueGolfers = await _context.LeagueGolfers.IgnoreQueryFilters().ToListAsync();

        int updated = 0;
        foreach (var lg in leagueGolfers)
        {
            if (roundStats.TryGetValue(lg.Id, out var stats))
            {
                lg.TotalRounds = stats.Count;
                lg.AverageScore = stats.Average;
                lg.BestScore = stats.Best;
                updated++;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✅ Recalculated stats for {Updated} players from {Rounds} rounds.", updated, roundCount);
    }

    /// <summary>
    /// Seeds demo data if the database is empty
    /// </summary>
    public async Task SeedAsync()
    {
        // Check if we already have users
        if (await _context.Users.AnyAsync())
        {
            // Detect partial Holy Grail import state (common after an interrupted import)
            // where users/league exist but seasons were never imported.
            var dkglLeague = await _context.Leagues
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Key.ToLower() == "dkgl" && !l.IsDeleted);

            if (dkglLeague != null)
            {
                var dkglSeasonCount = await _context.Seasons
                    .IgnoreQueryFilters()
                    .CountAsync(s => s.LeagueId == dkglLeague.Id && s.IsActive && !s.IsDeleted);

                if (dkglSeasonCount == 0)
                {
                    _logger.LogWarning("Partial Holy Grail import detected: league 'dkgl' exists with 0 seasons. Run reset-and-import to rebuild the database.");
                }
            }

            _logger.LogInformation("Database already contains data. Skipping seed.");
            return;
        }

        // Optional import path: disabled by default so reset/migrate behavior uses baseline seed data.
        var enableHolyGrailImport = Environment.GetEnvironmentVariable("ENABLE_HOLY_GRAIL_IMPORT")
            ?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

        if (enableHolyGrailImport)
        {
            var possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DkGolf_Backup_202604270946.sql"),
                Path.Combine(Directory.GetCurrentDirectory(), "DkGolf_Backup_202604270946.sql"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "DkGolf_Backup_202604270946.sql"),
                "/Users/tonygilbert/Projects/code/5thbox/dkgolf/golf-manager/src/GolfManager.Api/DkGolf_Backup_202604270946.sql"
            };

            string? backupPath = null;
            foreach (var path in possiblePaths)
            {
                var normalizedPath = Path.GetFullPath(path);
                if (File.Exists(normalizedPath))
                {
                    backupPath = normalizedPath;
                    break;
                }
            }

            if (backupPath != null)
            {
                _logger.LogInformation("Holy Grail backup file found: {Path}", backupPath);
                _logger.LogInformation("Importing historical data from Holy Grail v1...");

                var importer = new HolyGrailImporter(_context, _logger);
                var success = await importer.ImportFromBackupAsync(backupPath);

                if (success)
                {
                    _logger.LogInformation("✅ Holy Grail data imported successfully!");
                    _logger.LogInformation("Default password for all imported users: ChangeMe123!");
                    await RecalculateStatsIfNeededAsync();
                    return;
                }

                _logger.LogWarning("⚠️ Holy Grail import failed. Falling back to baseline seed data.");
            }
            else
            {
                _logger.LogWarning("ENABLE_HOLY_GRAIL_IMPORT=true but backup file was not found. Using baseline seed data.");
            }
        }
        else
        {
            _logger.LogInformation("Holy Grail import disabled. Using baseline seed data.");
        }

        // Fallback to demo data if no backup or import failed
        _logger.LogInformation("Seeding database with demo data...");

        // Create Global Admin
        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@pinzo.pro",
            FirstName = "Sarah",
            LastName = "Admin",
            IsGlobalAdmin = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        adminUser.PasswordHash = HashPassword("Admin123!");
        _context.Users.Add(adminUser);

        // Create League Admin (not global admin)
        var leagueAdminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "league.admin@pinzo.pro",
            FirstName = "Mike",
            LastName = "Commissioner",
            IsGlobalAdmin = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        leagueAdminUser.PasswordHash = HashPassword("League123!");
        _context.Users.Add(leagueAdminUser);

        // Create Regular Player 1
        var player1 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "john.player@pinzo.pro",
            FirstName = "John",
            LastName = "Player",
            IsGlobalAdmin = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        player1.PasswordHash = HashPassword("Player123!");
        _context.Users.Add(player1);

        // Create Regular Player 2
        var player2 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "jane.golfer@pinzo.pro",
            FirstName = "Jane",
            LastName = "Golfer",
            IsGlobalAdmin = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        player2.PasswordHash = HashPassword("Player123!");
        _context.Users.Add(player2);

        await _context.SaveChangesAsync();

        // Create demo league
        var demoLeague = new League
        {
            Id = Guid.NewGuid().ToString(),
            Key = "pinzo-demo",
            Name = "Pinzo Demo League",
            Description = "A demo league showcasing Pinzo's golf league management features",
            CustomDomain = "localhost:5001", // For local testing
            UseCustomDomain = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Leagues.Add(demoLeague);

        // Add a second demo league for multi-tenant testing
        var riverValleyLeague = new League
        {
            Id = Guid.NewGuid().ToString(),
            Key = "river-valley",
            Name = "River Valley Golf Club",
            Description = "A scenic golf club nestled in the river valley",
            CustomDomain = "localhost:5002", // For local testing
            UseCustomDomain = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Leagues.Add(riverValleyLeague);
        await _context.SaveChangesAsync();

        // Add Global Admin as league admin to both leagues
        _context.UserLeagues.Add(new UserLeague
        {
            UserId = adminUser.Id,
            LeagueId = demoLeague.Id,
            IsLeagueAdmin = true,
            Role = LeagueMemberRole.Owner,
            JoinedAt = DateTime.UtcNow
        });

        // Add Global Admin to River Valley league
        _context.UserLeagues.Add(new UserLeague
        {
            UserId = adminUser.Id,
            LeagueId = riverValleyLeague.Id,
            IsLeagueAdmin = true,
            Role = LeagueMemberRole.Owner,
            JoinedAt = DateTime.UtcNow
        });

        // Add League Admin as league admin (Pinzo Demo only)
        _context.UserLeagues.Add(new UserLeague
        {
            UserId = leagueAdminUser.Id,
            LeagueId = demoLeague.Id,
            IsLeagueAdmin = true,
            Role = LeagueMemberRole.Admin,
            JoinedAt = DateTime.UtcNow
        });

        // Add Player 1 as regular member
        _context.UserLeagues.Add(new UserLeague
        {
            UserId = player1.Id,
            LeagueId = demoLeague.Id,
            IsLeagueAdmin = false,
            Role = LeagueMemberRole.Member,
            JoinedAt = DateTime.UtcNow
        });

        // Add Player 2 as regular member
        _context.UserLeagues.Add(new UserLeague
        {
            UserId = player2.Id,
            LeagueId = demoLeague.Id,
            IsLeagueAdmin = false,
            Role = LeagueMemberRole.Member,
            JoinedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Create demo course
        var demoCourse = new Course
        {
            Id = Guid.NewGuid().ToString(),
            Key = "demo-course",
            Name = "Demo Golf Course",
            City = "Minneapolis",
            State = "MN",
            NumberOfHoles = 18,
            CreatedAt = DateTime.UtcNow
        };

        _context.Courses.Add(demoCourse);
        await _context.SaveChangesAsync();

        // Create holes for the course
        for (int i = 1; i <= 18; i++)
        {
            var hole = new Hole
            {
                Id = Guid.NewGuid().ToString(),
                CourseId = demoCourse.Id,
                HoleNumber = i,
                Name = $"Hole {i}",
                CreatedAt = DateTime.UtcNow
            };
            _context.Holes.Add(hole);
        }

        await _context.SaveChangesAsync();

        // Create a demo tee (Blue tees)
        // Par pattern: 4,3,5,4,4,3,4,5,4 (front 9 = 36) + 4,3,5,4,4,3,4,5,4 (back 9 = 36) = Par 72
        var demoTee = new Tee
        {
            Id = Guid.NewGuid().ToString(),
            CourseId = demoCourse.Id,
            Name = "Blue",
            HtmlColorCode = "#0000FF",
            RatingOut = 36.2,
            RatingIn = 36.3,
            SlopeOut = 130,
            SlopeIn = 130,
            YardsOut = 3275, // Front 9 total
            YardsIn = 3280,  // Back 9 total
            ParOut = 36,
            ParIn = 36,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tees.Add(demoTee);
        await _context.SaveChangesAsync();

        // Create hole tees with realistic pars and yardages
        var parPattern = new[] { 4, 3, 5, 4, 4, 3, 4, 5, 4, 4, 3, 5, 4, 4, 3, 4, 5, 4 }; // Par 72
        var yardagePattern = new[] { 380, 165, 520, 410, 395, 180, 425, 545, 400, 390, 175, 530, 405, 385, 170, 420, 550, 395 };

        for (int i = 1; i <= 18; i++)
        {
            var holeTee = new HoleTee
            {
                Id = Guid.NewGuid().ToString(),
                TeeId = demoTee.Id,
                HoleNumber = i,
                Par = parPattern[i - 1],
                Yardage = yardagePattern[i - 1],
                Handicap = i, // Simple 1-18 handicap
                CreatedAt = DateTime.UtcNow
            };
            _context.HoleTees.Add(holeTee);
        }

        await _context.SaveChangesAsync();

        await SeedDigiKey2026LeagueAsync(adminUser, demoCourse, demoTee);

        _logger.LogInformation("Database seeded successfully!");
        _logger.LogInformation("=== Test Users ===");
        _logger.LogInformation("Global Admin: admin@pinzo.pro / Admin123!");
        _logger.LogInformation("League Admin: league.admin@pinzo.pro / League123!");
        _logger.LogInformation("Player 1: john.player@pinzo.pro / Player123!");
        _logger.LogInformation("Player 2: jane.golfer@pinzo.pro / Player123!");
        _logger.LogInformation("Demo League: pinzo-demo");
        _logger.LogInformation("DigiKey League: dkgl (Season 2026)");
        _logger.LogInformation("Demo Course: demo-course (18 holes, Par 72)");
    }
}
