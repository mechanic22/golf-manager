using GolfManager.Core.Entities;
using GolfManager.Core.Enums;
using GolfManager.Data;
using GolfManager.Shared.DTOs.Simulation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.Simulation;

/// <summary>
/// Service for simulating full seasons
/// </summary>
public class SeasonSimulationService : ISeasonSimulationService
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<SeasonSimulationService> _logger;
    private static readonly Random _random = new();

    // Realistic player names
    private static readonly string[] FirstNames = new[]
    {
        "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Charles",
        "Christopher", "Daniel", "Matthew", "Anthony", "Mark", "Donald", "Steven", "Paul", "Andrew", "Joshua",
        "Kenneth", "Kevin", "Brian", "George", "Timothy", "Ronald", "Edward", "Jason", "Jeffrey", "Ryan",
        "Jacob", "Gary", "Nicholas", "Eric", "Jonathan", "Stephen", "Larry", "Justin", "Scott", "Brandon",
        "Benjamin", "Samuel", "Raymond", "Gregory", "Frank", "Alexander", "Patrick", "Jack", "Dennis", "Jerry",
        "Tyler", "Aaron", "Jose", "Adam", "Nathan", "Henry", "Douglas", "Zachary", "Peter", "Kyle"
    };

    private static readonly string[] LastNames = new[]
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
        "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin",
        "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
        "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
        "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts",
        "Gomez", "Phillips", "Evans", "Turner", "Diaz", "Parker", "Cruz", "Edwards", "Collins", "Reyes"
    };

    public SeasonSimulationService(
        GolfManagerDbContext context,
        ILogger<SeasonSimulationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SimulationResult> SeedSeasonAsync(
        string leagueId,
        string seasonId,
        int playerCount = 60,
        int playersPerTeam = 3,
        int weekCount = 16)
    {
        var result = new SimulationResult();

        try
        {
            _logger.LogInformation("Starting season simulation: {PlayerCount} players, {WeekCount} weeks", 
                playerCount, weekCount);

            // Verify league and season exist
            var league = await _context.Leagues
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == leagueId);
            if (league == null)
            {
                result.Errors.Add("League not found");
                return result;
            }

            var season = await _context.Seasons
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == seasonId);
            if (season == null)
            {
                result.Errors.Add("Season not found");
                return result;
            }

            // Get default course and tee
            var settings = await _context.SeasonSettings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.SeasonId == seasonId);
            var courseId = settings?.DefaultCourseId;

            // If no default course, get the first available course
            if (string.IsNullOrEmpty(courseId))
            {
                var firstCourse = await _context.Courses
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync();
                courseId = firstCourse?.Id;
            }

            if (string.IsNullOrEmpty(courseId))
            {
                result.Errors.Add("No course available for simulation");
                return result;
            }

            var tee = await _context.Tees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.CourseId == courseId);
            if (tee == null)
            {
                result.Errors.Add("No tee found for course");
                return result;
            }

            // Create players
            var players = await CreatePlayersAsync(leagueId, playerCount);
            result.PlayersCreated = players.Count;

            // Create teams
            var teams = await CreateTeamsAsync(leagueId, seasonId, players, playersPerTeam);
            result.TeamsCreated = teams.Count;

            // Create events
            var events = await CreateEventsAsync(leagueId, seasonId, courseId, tee.Id, weekCount);
            result.EventsCreated = events.Count;

            await _context.SaveChangesAsync();

            result.Success = true;
            result.Message = $"Created {result.PlayersCreated} players, {result.TeamsCreated} teams, and {result.EventsCreated} events";
            
            _logger.LogInformation("Season simulation complete: {Message}", result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during season simulation");
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    public async Task<SimulationResult> SimulateEventScoresAsync(string leagueId, string eventId)
    {
        var result = new SimulationResult();

        try
        {
            var seasonEvent = await _context.SeasonEvents
                .IgnoreQueryFilters()
                .Include(e => e.Course)
                .Include(e => e.Tee)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.LeagueId == leagueId);

            if (seasonEvent == null)
            {
                result.Errors.Add("Event not found");
                return result;
            }

            // Get all season golfers for this season
            var seasonGolfers = await _context.SeasonGolfers
                .Include(sg => sg.LeagueGolfer)
                .Where(sg => sg.SeasonId == seasonEvent.SeasonId)
                .ToListAsync();

            // Create rounds for each player
            foreach (var seasonGolfer in seasonGolfers)
            {
                var round = await CreateSimulatedRoundAsync(seasonGolfer.LeagueGolfer, seasonEvent);
                result.RoundsCreated++;
            }

            await _context.SaveChangesAsync();

            result.Success = true;
            result.Message = $"Simulated {result.RoundsCreated} rounds for event";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error simulating event scores");
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    public async Task<SimulationResult> SimulateNextEventAsync(string leagueId, string seasonId)
    {
        // Get all events for the season
        var events = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .Where(e => e.LeagueId == leagueId && e.SeasonId == seasonId)
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        // Find first event without rounds (simple check based on event date and round date)
        SeasonEvent? nextEvent = null;
        foreach (var evt in events)
        {
            var hasRounds = await _context.Rounds
                .IgnoreQueryFilters()
                .AnyAsync(r => r.LeagueId == leagueId &&
                              r.RoundDate.Date == evt.EventDate.Date);

            if (!hasRounds)
            {
                nextEvent = evt;
                break;
            }
        }

        if (nextEvent == null)
        {
            return new SimulationResult
            {
                Success = false,
                Message = "No unplayed events found"
            };
        }

        return await SimulateEventScoresAsync(leagueId, nextEvent.Id);
    }

    public async Task<SimulationResult> SimulateAllEventsAsync(string leagueId, string seasonId)
    {
        var result = new SimulationResult();

        var events = await _context.SeasonEvents
            .IgnoreQueryFilters()
            .Where(e => e.LeagueId == leagueId && e.SeasonId == seasonId)
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        foreach (var evt in events)
        {
            var eventResult = await SimulateEventScoresAsync(leagueId, evt.Id);
            result.RoundsCreated += eventResult.RoundsCreated;
            result.Errors.AddRange(eventResult.Errors);
        }

        result.Success = result.Errors.Count == 0;
        result.Message = $"Simulated {result.RoundsCreated} rounds across {events.Count} events";

        return result;
    }

    public async Task<SimulationResult> ClearSeasonDataAsync(string leagueId, string seasonId)
    {
        var result = new SimulationResult();

        try
        {
            // Get event dates for this season
            var eventDates = await _context.SeasonEvents
                .IgnoreQueryFilters()
                .Where(e => e.SeasonId == seasonId)
                .Select(e => e.EventDate.Date)
                .ToListAsync();

            // Delete rounds that match these dates (approximate - better than nothing)
            var rounds = await _context.Rounds
                .IgnoreQueryFilters()
                .Where(r => r.LeagueId == leagueId && eventDates.Contains(r.RoundDate.Date))
                .ToListAsync();

            _context.Rounds.RemoveRange(rounds);

            // Delete events
            var events = await _context.SeasonEvents
                .IgnoreQueryFilters()
                .Where(e => e.SeasonId == seasonId)
                .ToListAsync();

            _context.SeasonEvents.RemoveRange(events);

            // Delete season golfers
            var seasonGolfers = await _context.SeasonGolfers
                .IgnoreQueryFilters()
                .Where(sg => sg.SeasonId == seasonId)
                .ToListAsync();

            _context.SeasonGolfers.RemoveRange(seasonGolfers);

            // Delete season teams
            var teams = await _context.SeasonTeams
                .Where(st => st.SeasonId == seasonId)
                .ToListAsync();

            _context.SeasonTeams.RemoveRange(teams);

            // Delete league golfers created for simulation
            var leagueGolferIds = seasonGolfers.Select(sg => sg.LeagueGolferId).ToList();
            var leagueGolfers = await _context.LeagueGolfers
                .Where(lg => leagueGolferIds.Contains(lg.Id))
                .ToListAsync();

            _context.LeagueGolfers.RemoveRange(leagueGolfers);

            await _context.SaveChangesAsync();

            result.Success = true;
            result.Message = $"Cleared {rounds.Count} rounds, {events.Count} events, {teams.Count} teams, {leagueGolfers.Count} players";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing season data");
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    // Helper methods

    private async Task<List<LeagueGolfer>> CreatePlayersAsync(string leagueId, int count)
    {
        var players = new List<LeagueGolfer>();
        var usedNames = new HashSet<string>();

        for (int i = 0; i < count; i++)
        {
            string fullName;
            string firstName;
            string lastName;
            do
            {
                firstName = FirstNames[_random.Next(FirstNames.Length)];
                lastName = LastNames[_random.Next(LastNames.Length)];
                fullName = $"{firstName} {lastName}";
            } while (usedNames.Contains(fullName));

            usedNames.Add(fullName);

            // Generate realistic handicap (0-36, weighted toward middle)
            var handicap = GenerateRealisticHandicap();

            // Create a fake user for the simulation
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}.sim{i}@simulation.local";
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = "SIMULATION_USER_NO_PASSWORD",
                IsActive = false, // Mark as inactive since it's a simulation user
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            // Create the global Golfer record
            var golfer = new Golfer
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                DisplayName = fullName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Golfers.Add(golfer);

            // Create the LeagueGolfer record
            var player = new LeagueGolfer
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = leagueId,
                GolferId = golfer.Id,
                DisplayName = fullName,
                LeagueHandicap = handicap,
                CreatedAt = DateTime.UtcNow
            };

            _context.LeagueGolfers.Add(player);
            players.Add(player);
        }

        return players;
    }

    private async Task<List<SeasonTeam>> CreateTeamsAsync(
        string leagueId,
        string seasonId,
        List<LeagueGolfer> players,
        int playersPerTeam)
    {
        var teams = new List<SeasonTeam>();
        var teamCount = players.Count / playersPerTeam;

        // Shuffle players for random team assignment
        var shuffledPlayers = players.OrderBy(x => _random.Next()).ToList();

        for (int i = 0; i < teamCount; i++)
        {
            var team = new SeasonTeam
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = leagueId,
                SeasonId = seasonId,
                Name = $"Team {i + 1}",
                CreatedAt = DateTime.UtcNow
            };

            _context.SeasonTeams.Add(team);
            teams.Add(team);

            // Assign players to team
            for (int j = 0; j < playersPerTeam && (i * playersPerTeam + j) < shuffledPlayers.Count; j++)
            {
                var player = shuffledPlayers[i * playersPerTeam + j];

                var seasonGolfer = new SeasonGolfer
                {
                    Id = Guid.NewGuid().ToString(),
                    SeasonId = seasonId,
                    LeagueId = leagueId,
                    LeagueGolferId = player.Id,
                    GolferId = player.GolferId,
                    TeamId = team.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SeasonGolfers.Add(seasonGolfer);
            }
        }

        return teams;
    }

    private async Task<List<SeasonEvent>> CreateEventsAsync(
        string leagueId,
        string seasonId,
        string courseId,
        string teeId,
        int weekCount)
    {
        var events = new List<SeasonEvent>();
        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == seasonId);
        var startDate = season!.StartDate.ToDateTime(TimeOnly.MinValue);

        for (int i = 0; i < weekCount; i++)
        {
            var eventDate = startDate.AddDays(i * 7); // Weekly events

            var seasonEvent = new SeasonEvent
            {
                Id = Guid.NewGuid().ToString(),
                SeasonId = seasonId,
                LeagueId = leagueId,
                EventDate = eventDate,
                CourseId = courseId,
                TeeId = teeId,
                HolesPlayed = HolesPlayed.Eighteen,
                EventType = SeasonEventType.Regular,
                ScoringFormat = ScoringFormat.StrokePlay,
                Name = $"Week {i + 1}",
                Description = $"Regular season event - Week {i + 1}",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _context.SeasonEvents.Add(seasonEvent);
            events.Add(seasonEvent);
        }

        return events;
    }

    private async Task<Core.Entities.Round> CreateSimulatedRoundAsync(LeagueGolfer player, SeasonEvent seasonEvent)
    {
        var handicap = player.LeagueHandicap ?? 18.0;

        var round = new Core.Entities.Round
        {
            Id = Guid.NewGuid().ToString(),
            GolferId = player.GolferId,
            LeagueGolferId = player.Id,
            LeagueId = player.LeagueId,
            CourseId = seasonEvent.CourseId!,
            TeeId = seasonEvent.TeeId!,
            RoundDate = seasonEvent.EventDate,
            HolesPlayed = seasonEvent.HolesPlayed,
            HandicapUsed = handicap,
            IsComplete = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Rounds.Add(round);

        // Generate hole scores
        var totalScore = 0;
        var holesCount = seasonEvent.HolesPlayed == HolesPlayed.Eighteen ? 18 : 9;

        for (int holeNum = 1; holeNum <= holesCount; holeNum++)
        {
            var score = GenerateRealisticScore(handicap, holeNum);
            totalScore += score;

            var roundHole = new RoundHole
            {
                RoundId = round.Id,
                HoleNumber = holeNum,
                GrossScore = score,
                CreatedAt = DateTime.UtcNow
            };

            _context.RoundHoles.Add(roundHole);
        }

        round.TotalScore = totalScore;
        round.NetScore = totalScore - (int)handicap;

        return round;
    }

    private double GenerateRealisticHandicap()
    {
        // Generate handicap with bell curve distribution (mean ~15, std dev ~8)
        var u1 = 1.0 - _random.NextDouble();
        var u2 = 1.0 - _random.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        var handicap = 15 + 8 * randStdNormal;

        // Clamp to 0-36 range
        handicap = Math.Max(0, Math.Min(36, handicap));

        return Math.Round(handicap, 1);
    }

    private int GenerateRealisticScore(double handicap, int holeNumber)
    {
        // Assume par 4 for simplicity (could be enhanced with actual hole data)
        var par = 4;

        // Expected score based on handicap
        // Handicap 18 = +1 per hole, Handicap 36 = +2 per hole
        var expectedOverPar = handicap / 18.0;

        // Add some randomness (-1 to +2 strokes)
        var randomness = _random.Next(-1, 3);

        var score = par + (int)Math.Round(expectedOverPar) + randomness;

        // Clamp to reasonable range (2-10)
        return Math.Max(2, Math.Min(10, score));
    }
}
