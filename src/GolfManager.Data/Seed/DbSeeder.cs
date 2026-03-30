using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    /// <summary>
    /// Seeds demo data if the database is empty
    /// </summary>
    public async Task SeedAsync()
    {
        // Check if we already have users
        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Database already contains data. Skipping seed.");
            return;
        }

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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Leagues.Add(demoLeague);
        await _context.SaveChangesAsync();

        // Add Global Admin as league admin
        _context.UserLeagues.Add(new UserLeague
        {
            UserId = adminUser.Id,
            LeagueId = demoLeague.Id,
            IsLeagueAdmin = true,
            JoinedAt = DateTime.UtcNow
        });

        // Add League Admin as league admin
        _context.UserLeagues.Add(new UserLeague
        {
            UserId = leagueAdminUser.Id,
            LeagueId = demoLeague.Id,
            IsLeagueAdmin = true,
            JoinedAt = DateTime.UtcNow
        });

        // Add Player 1 as regular member
        _context.UserLeagues.Add(new UserLeague
        {
            UserId = player1.Id,
            LeagueId = demoLeague.Id,
            IsLeagueAdmin = false,
            JoinedAt = DateTime.UtcNow
        });

        // Add Player 2 as regular member
        _context.UserLeagues.Add(new UserLeague
        {
            UserId = player2.Id,
            LeagueId = demoLeague.Id,
            IsLeagueAdmin = false,
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

        _logger.LogInformation("Database seeded successfully!");
        _logger.LogInformation("=== Test Users ===");
        _logger.LogInformation("Global Admin: admin@pinzo.pro / Admin123!");
        _logger.LogInformation("League Admin: league.admin@pinzo.pro / League123!");
        _logger.LogInformation("Player 1: john.player@pinzo.pro / Player123!");
        _logger.LogInformation("Player 2: jane.golfer@pinzo.pro / Player123!");
        _logger.LogInformation("Demo League: pinzo-demo");
        _logger.LogInformation("Demo Course: demo-course (18 holes, Par 72)");
    }
}

