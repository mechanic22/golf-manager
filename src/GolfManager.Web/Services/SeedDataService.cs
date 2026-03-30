namespace GolfManager.Web.Services;

/// <summary>
/// Service for generating comprehensive demo/seed data for development
/// </summary>
public class SeedDataService
{
    // User Data
    public List<SeedUser> Users { get; private set; } = new();
    
    // League Data
    public List<SeedLeague> Leagues { get; private set; } = new();
    
    // Season Data
    public List<SeedSeason> Seasons { get; private set; } = new();
    
    // Event Data
    public List<SeedEvent> Events { get; private set; } = new();
    
    // Round/Score Data
    public List<SeedRound> Rounds { get; private set; } = new();

    public SeedDataService()
    {
        GenerateSeedData();
    }

    private void GenerateSeedData()
    {
        GenerateUsers();
        GenerateLeagues();
        GenerateSeasons();
        GenerateEvents();
        GenerateRounds();
    }

    private void GenerateUsers()
    {
        var random = new Random(42); // Fixed seed for consistent data
        
        // Admin users
        Users.Add(new SeedUser
        {
            Id = "user-1",
            FirstName = "Sarah",
            LastName = "Admin",
            Email = "admin@pinzo.pro",
            Password = "Admin123!",
            IsGlobalAdmin = true,
            Handicap = 12.5m,
            JoinedDate = DateTime.Now.AddYears(-2)
        });

        Users.Add(new SeedUser
        {
            Id = "user-2",
            FirstName = "Mike",
            LastName = "Commissioner",
            Email = "league.admin@pinzo.pro",
            Password = "League123!",
            IsGlobalAdmin = false,
            Handicap = 8.3m,
            JoinedDate = DateTime.Now.AddYears(-1).AddMonths(-6)
        });

        // Regular players
        var firstNames = new[] { "John", "Jane", "Bob", "Alice", "Tom", "Emma", "David", "Lisa", "Chris", "Maria",
                                 "James", "Jennifer", "Robert", "Linda", "Michael", "Patricia", "William", "Barbara",
                                 "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen",
                                 "Daniel", "Nancy", "Matthew", "Betty", "Anthony", "Margaret", "Mark", "Sandra",
                                 "Donald", "Ashley", "Steven", "Kimberly", "Paul", "Emily", "Andrew", "Donna",
                                 "Joshua", "Michelle", "Kenneth", "Carol", "Kevin", "Amanda", "Brian", "Melissa" };
        
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
                               "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
                               "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Thompson", "White",
                               "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young",
                               "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", "Green" };

        int userId = 3;
        for (int i = 0; i < 50; i++)
        {
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}{(i > 25 ? i.ToString() : "")}@example.com";
            
            Users.Add(new SeedUser
            {
                Id = $"user-{userId++}",
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = "Player123!",
                IsGlobalAdmin = false,
                Handicap = Math.Round((decimal)(random.NextDouble() * 30 + 5), 1), // 5-35 handicap
                JoinedDate = DateTime.Now.AddDays(-random.Next(30, 730)) // Joined within last 2 years
            });
        }
    }

    private void GenerateLeagues()
    {
        var random = new Random(42);
        
        var leagueNames = new[]
        {
            ("Sunset Golf League", "sunset-golf", "A friendly evening league for golfers of all skill levels"),
            ("Championship Tour", "championship-tour", "Competitive league for serious golfers"),
            ("Weekend Warriors", "weekend-warriors", "Casual weekend golf for busy professionals"),
            ("Senior Golf Association", "senior-golf", "League for experienced golfers 55+"),
            ("Ladies Golf Club", "ladies-golf", "Empowering women through golf"),
            ("Corporate Challenge", "corporate-challenge", "Inter-company golf competition"),
            ("Junior Development League", "junior-dev", "Youth golf development program"),
            ("Twilight League", "twilight", "After-work golf league"),
        };

        int leagueId = 1;
        foreach (var (name, key, description) in leagueNames)
        {
            var adminUser = Users[random.Next(2, Math.Min(15, Users.Count))]; // Random user as admin
            
            Leagues.Add(new SeedLeague
            {
                Id = $"league-{leagueId}",
                Key = key,
                Name = name,
                Description = description,
                AdminUserId = adminUser.Id,
                IsPublic = random.Next(100) > 30, // 70% public
                CreatedDate = DateTime.Now.AddMonths(-random.Next(6, 24)),
                MemberCount = random.Next(8, 35)
            });
            
            leagueId++;
        }
    }

    private void GenerateSeasons()
    {
        var random = new Random(42);
        var seasonId = 1;

        foreach (var league in Leagues)
        {
            // Each league has 2-4 seasons
            int seasonCount = random.Next(2, 5);
            
            for (int i = 0; i < seasonCount; i++)
            {
                var year = DateTime.Now.Year - (seasonCount - i - 1);
                var seasonNames = new[] { "Spring", "Summer", "Fall", "Winter" };
                var seasonName = $"{seasonNames[i % 4]} {year}";

                var startMonth = (i % 4) * 3 + 1; // Spring=1, Summer=4, Fall=7, Winter=10
                var startDate = new DateTime(year, startMonth, 1);
                var endDate = startDate.AddMonths(3).AddDays(-1);

                Seasons.Add(new SeedSeason
                {
                    Id = $"season-{seasonId}",
                    Key = $"{seasonNames[i % 4].ToLower()}-{year}",
                    Name = seasonName,
                    LeagueId = league.Id,
                    LeagueKey = league.Key,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsActive = i == seasonCount - 1, // Last season is active
                    PlayerCount = random.Next(6, league.MemberCount)
                });

                seasonId++;
            }
        }
    }

    private void GenerateEvents()
    {
        var random = new Random(42);
        var eventId = 1;

        var courseNames = new[]
        {
            "Pebble Beach Golf Links", "Augusta National", "St Andrews Old Course",
            "Pinehurst No. 2", "Oakmont Country Club", "Shinnecock Hills",
            "Bethpage Black", "TPC Sawgrass", "Torrey Pines", "Whistling Straits",
            "Kiawah Island Ocean Course", "Chambers Bay", "Bandon Dunes",
            "Shadow Creek", "Pine Valley", "Cypress Point"
        };

        foreach (var season in Seasons)
        {
            // Each season has 6-12 events
            int eventCount = random.Next(6, 13);
            var seasonStart = season.StartDate;
            var seasonEnd = season.EndDate;
            var daysBetweenEvents = (seasonEnd - seasonStart).Days / eventCount;

            for (int i = 0; i < eventCount; i++)
            {
                var eventDate = seasonStart.AddDays(i * daysBetweenEvents + random.Next(0, 7));
                var courseName = courseNames[random.Next(courseNames.Length)];

                Events.Add(new SeedEvent
                {
                    Id = $"event-{eventId}",
                    Key = $"event-{eventId}",
                    Name = $"Round {i + 1} - {courseName}",
                    SeasonId = season.Id,
                    SeasonKey = season.Key,
                    LeagueId = season.LeagueId,
                    LeagueKey = season.LeagueKey,
                    Date = eventDate,
                    CourseName = courseName,
                    ParticipantCount = random.Next(4, season.PlayerCount),
                    IsCompleted = eventDate < DateTime.Now
                });

                eventId++;
            }
        }
    }

    private void GenerateRounds()
    {
        var random = new Random(42);
        var roundId = 1;

        foreach (var evt in Events.Where(e => e.IsCompleted))
        {
            // Get random players for this event
            var playerCount = evt.ParticipantCount;
            var eventPlayers = Users
                .Where(u => !u.IsGlobalAdmin)
                .OrderBy(x => random.Next())
                .Take(playerCount)
                .ToList();

            foreach (var player in eventPlayers)
            {
                // Generate realistic scores based on handicap
                var expectedScore = 72 + (int)player.Handicap; // Par 72 + handicap
                var actualScore = expectedScore + random.Next(-5, 6); // +/- 5 strokes variance

                // Generate hole-by-hole scores
                var holeScores = new List<int>();
                var remainingStrokes = actualScore;

                for (int hole = 1; hole <= 18; hole++)
                {
                    var par = hole <= 4 || (hole >= 10 && hole <= 13) ? 4 : (hole == 5 || hole == 14 ? 5 : 3);
                    var holeHandicap = (int)(player.Handicap / 18.0m);
                    var expectedHoleScore = par + (holeHandicap > 0 ? 1 : 0);

                    // Add some variance
                    var holeScore = expectedHoleScore + random.Next(-1, 3);
                    holeScore = Math.Max(par - 1, Math.Min(par + 3, holeScore)); // Keep realistic

                    holeScores.Add(holeScore);
                    remainingStrokes -= holeScore;
                }

                // Adjust last few holes to match total
                if (remainingStrokes != 0)
                {
                    for (int i = 17; i >= 15 && remainingStrokes != 0; i--)
                    {
                        if (remainingStrokes > 0 && holeScores[i] > 3)
                        {
                            holeScores[i]--;
                            remainingStrokes--;
                        }
                        else if (remainingStrokes < 0 && holeScores[i] < 7)
                        {
                            holeScores[i]++;
                            remainingStrokes++;
                        }
                    }
                }

                Rounds.Add(new SeedRound
                {
                    Id = $"round-{roundId}",
                    EventId = evt.Id,
                    EventKey = evt.Key,
                    PlayerId = player.Id,
                    PlayerName = $"{player.FirstName} {player.LastName}",
                    TotalScore = holeScores.Sum(),
                    HoleScores = holeScores,
                    Handicap = player.Handicap,
                    NetScore = holeScores.Sum() - (int)player.Handicap,
                    DatePlayed = evt.Date
                });

                roundId++;
            }
        }
    }

    // Helper method to get users for a specific league
    public List<SeedUser> GetLeagueMembers(string leagueId)
    {
        var random = new Random(leagueId.GetHashCode());
        var league = Leagues.FirstOrDefault(l => l.Id == leagueId);
        if (league == null) return new List<SeedUser>();

        return Users
            .Where(u => !u.IsGlobalAdmin)
            .OrderBy(x => random.Next())
            .Take(league.MemberCount)
            .ToList();
    }

    // Helper method to get rounds for a specific player
    public List<SeedRound> GetPlayerRounds(string playerId)
    {
        return Rounds.Where(r => r.PlayerId == playerId).OrderByDescending(r => r.DatePlayed).ToList();
    }

    // Helper method to get standings for a season
    public List<SeasonStanding> GetSeasonStandings(string seasonId)
    {
        var seasonEvents = Events.Where(e => e.SeasonId == seasonId && e.IsCompleted).ToList();
        var seasonRounds = Rounds.Where(r => seasonEvents.Any(e => e.Id == r.EventId)).ToList();

        var standings = seasonRounds
            .GroupBy(r => r.PlayerId)
            .Select(g => new SeasonStanding
            {
                PlayerId = g.Key,
                PlayerName = g.First().PlayerName,
                RoundsPlayed = g.Count(),
                TotalStrokes = g.Sum(r => r.TotalScore),
                AverageScore = g.Average(r => r.TotalScore),
                BestScore = g.Min(r => r.TotalScore),
                Handicap = g.First().Handicap
            })
            .OrderBy(s => s.AverageScore)
            .ToList();

        // Assign ranks
        for (int i = 0; i < standings.Count; i++)
        {
            standings[i].Rank = i + 1;
        }

        return standings;
    }
}

// Data Models
public class SeedUser
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsGlobalAdmin { get; set; }
    public decimal Handicap { get; set; }
    public DateTime JoinedDate { get; set; }
}

public class SeedLeague
{
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AdminUserId { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime CreatedDate { get; set; }
    public int MemberCount { get; set; }
}

public class SeedSeason
{
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LeagueId { get; set; } = string.Empty;
    public string LeagueKey { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int PlayerCount { get; set; }
}

public class SeedEvent
{
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SeasonId { get; set; } = string.Empty;
    public string SeasonKey { get; set; } = string.Empty;
    public string LeagueId { get; set; } = string.Empty;
    public string LeagueKey { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public bool IsCompleted { get; set; }
}

public class SeedRound
{
    public string Id { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string EventKey { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public List<int> HoleScores { get; set; } = new();
    public decimal Handicap { get; set; }
    public int NetScore { get; set; }
    public DateTime DatePlayed { get; set; }
}

public class SeasonStanding
{
    public int Rank { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int RoundsPlayed { get; set; }
    public int TotalStrokes { get; set; }
    public double AverageScore { get; set; }
    public int BestScore { get; set; }
    public decimal Handicap { get; set; }
}

