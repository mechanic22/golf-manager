using System.Security.Cryptography;
using GolfManager.Core.Entities;
using GolfManager.Core.Services;
using GolfManager.Data;
using GolfManager.Services.Auth;
using GolfManager.Shared.DTOs.Player;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.Player;

/// <summary>
/// Service for managing players within leagues
/// </summary>
public class PlayerService : IPlayerService
{
    private readonly GolfManagerDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IShortIdService _shortIdService;
    private readonly ILogger<PlayerService> _logger;

    public PlayerService(
        GolfManagerDbContext context,
        IPasswordHasher passwordHasher,
        IShortIdService shortIdService,
        ILogger<PlayerService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _shortIdService = shortIdService;
        _logger = logger;
    }

    public async Task<List<PlayerResponse>> GetLeaguePlayersAsync(string leagueId)
    {
        // Use IgnoreQueryFilters() because we're explicitly filtering by leagueId
        var players = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .Include(lg => lg.Golfer)
                .ThenInclude(g => g.User)
            .Where(lg => lg.LeagueId == leagueId && lg.IsActive)
            .OrderBy(lg => lg.DisplayName)
            .ToListAsync();

        return players.Select(MapToResponse).ToList();
    }

    public async Task<PlayerResponse?> GetPlayerAsync(string leagueId, string playerId)
    {
        // Use IgnoreQueryFilters() because we're explicitly filtering by leagueId
        var player = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .Include(lg => lg.Golfer)
                .ThenInclude(g => g.User)
            .Where(lg => lg.Id == playerId && lg.LeagueId == leagueId && lg.IsActive)
            .FirstOrDefaultAsync();

        return player == null ? null : MapToResponse(player);
    }

    public async Task<PlayerResponse> AddPlayerToLeagueAsync(string leagueId, CreatePlayerRequest request)
    {
        // Verify league exists
        var league = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Id == leagueId && l.IsActive);

        if (league == null)
        {
            throw new KeyNotFoundException($"League with ID {leagueId} not found");
        }

        string userId;
        string golferId;

        // Check if we're adding an existing user or creating a new one
        if (!string.IsNullOrEmpty(request.UserId))
        {
            // Adding existing user
            userId = request.UserId;

            // Check if user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Check if user already has a golfer profile
            var existingGolfer = await _context.Golfers
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (existingGolfer != null)
            {
                golferId = existingGolfer.Id;
            }
            else
            {
                // Create golfer profile for existing user
                var golfer = new Golfer
                {
                    Id = _shortIdService.GenerateId(),
                    UserId = userId,
                    DisplayName = request.DisplayName,
                    Nickname = request.Nickname,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    IsActive = true
                };

                _context.Golfers.Add(golfer);
                golferId = golfer.Id;
            }
        }
        else
        {
            // Creating new user and golfer
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
            {
                throw new ArgumentException("Email, FirstName, and LastName are required when creating a new user");
            }

            // Check if email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email {request.Email} already exists");
            }

            // Generate random password
            var randomPassword = GenerateRandomPassword();

            // Create new user
            var newUser = new User
            {
                Id = _shortIdService.GenerateId(),
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(randomPassword),
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsGlobalAdmin = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                IsActive = true
            };

            _context.Users.Add(newUser);
            userId = newUser.Id;

            // Create golfer profile
            var golfer = new Golfer
            {
                Id = _shortIdService.GenerateId(),
                UserId = userId,
                DisplayName = request.DisplayName,
                Nickname = request.Nickname,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsActive = true
            };

            _context.Golfers.Add(golfer);
            golferId = golfer.Id;

            _logger.LogInformation("Created new user {Email} with temporary password", request.Email);
        }

        // Check if golfer is already in the league
        var existingLeagueGolfer = await _context.LeagueGolfers
            .FirstOrDefaultAsync(lg => lg.GolferId == golferId && lg.LeagueId == leagueId);

        if (existingLeagueGolfer != null)
        {
            if (existingLeagueGolfer.IsActive)
            {
                throw new InvalidOperationException("Player is already a member of this league");
            }
            else
            {
                // Reactivate the player
                existingLeagueGolfer.IsActive = true;
                existingLeagueGolfer.UpdatedAt = DateTime.UtcNow;
                existingLeagueGolfer.UpdatedBy = userId;
            }
        }
        else
        {
            // Create league golfer profile
            var leagueGolfer = new LeagueGolfer
            {
                Id = _shortIdService.GenerateId(),
                GolferId = golferId,
                LeagueId = leagueId,
                DisplayName = request.DisplayName,
                Nickname = request.Nickname,
                LeagueHandicap = request.LeagueHandicap,
                LeagueHandicapUpdatedAt = request.LeagueHandicap.HasValue ? DateTime.UtcNow : null,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsActive = true
            };

            _context.LeagueGolfers.Add(leagueGolfer);
            existingLeagueGolfer = leagueGolfer;
        }

        await _context.SaveChangesAsync();

        // Reload with navigation properties
        // Use IgnoreQueryFilters() because the tenant context might not be set yet
        var result = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .Include(lg => lg.Golfer)
                .ThenInclude(g => g.User)
            .FirstAsync(lg => lg.Id == existingLeagueGolfer.Id);

        _logger.LogInformation("Added player {DisplayName} to league {LeagueId}", request.DisplayName, leagueId);

        return MapToResponse(result);
    }

    public async Task<PlayerResponse> AddPlayerToSeasonAsync(string seasonId, string leagueId, CreatePlayerRequest request, string userId)
    {
        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == seasonId && s.LeagueId == leagueId);

        if (season == null)
        {
            throw new KeyNotFoundException($"Season with ID {seasonId} not found");
        }

        if (season.IsLocked)
        {
            throw new InvalidOperationException("Cannot add players to a locked season");
        }

        var leaguePlayer = await AddPlayerToLeagueAsync(leagueId, request);

        var existingSeasonGolfer = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(sg =>
                sg.SeasonId == seasonId
                && sg.LeagueId == leagueId
                && sg.LeagueGolferId == leaguePlayer.Id);

        if (existingSeasonGolfer == null)
        {
            var seasonGolfer = new SeasonGolfer
            {
                Id = _shortIdService.GenerateId(),
                SeasonId = seasonId,
                LeagueId = leagueId,
                LeagueGolferId = leaguePlayer.Id,
                GolferId = leaguePlayer.GolferId,
                SeasonHandicap = leaguePlayer.LeagueHandicap,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsActive = true
            };

            _context.SeasonGolfers.Add(seasonGolfer);
        }
        else if (!existingSeasonGolfer.IsActive)
        {
            existingSeasonGolfer.IsActive = true;
            existingSeasonGolfer.UpdatedAt = DateTime.UtcNow;
            existingSeasonGolfer.UpdatedBy = userId;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Added player {PlayerId} to season {SeasonId} in league {LeagueId}",
            leaguePlayer.Id,
            seasonId,
            leagueId);

        return leaguePlayer;
    }

    public async Task<PlayerResponse> UpdatePlayerAsync(string leagueId, string playerId, UpdatePlayerRequest request)
    {
        // Use IgnoreQueryFilters() because we're explicitly filtering by leagueId
        var player = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .Include(lg => lg.Golfer)
                .ThenInclude(g => g.User)
            .FirstOrDefaultAsync(lg => lg.Id == playerId && lg.LeagueId == leagueId && lg.IsActive);

        if (player == null)
        {
            throw new KeyNotFoundException($"Player with ID {playerId} not found in league {leagueId}");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.DisplayName))
        {
            player.DisplayName = request.DisplayName;
        }

        if (request.Nickname != null)
        {
            player.Nickname = request.Nickname;
        }

        if (request.LeagueHandicap.HasValue)
        {
            player.LeagueHandicap = request.LeagueHandicap;
            player.LeagueHandicapUpdatedAt = DateTime.UtcNow;
        }

        player.UpdatedAt = DateTime.UtcNow;
        player.UpdatedBy = player.Golfer.UserId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated player {PlayerId} in league {LeagueId}", playerId, leagueId);

        return MapToResponse(player);
    }

    public async Task<bool> RemovePlayerFromLeagueAsync(string leagueId, string playerId)
    {
        // Use IgnoreQueryFilters() because we're explicitly filtering by leagueId
        var player = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(lg => lg.Id == playerId && lg.LeagueId == leagueId && lg.IsActive);

        if (player == null)
        {
            return false;
        }

        // Soft delete
        player.IsActive = false;
        player.UpdatedAt = DateTime.UtcNow;
        player.UpdatedBy = "System";

        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed player {PlayerId} from league {LeagueId}", playerId, leagueId);

        return true;
    }

    public async Task<List<PlayerResponse>> GetSeasonPlayersAsync(string seasonId, string leagueId)
    {
        // Get all SeasonGolfer records for this season, with their LeagueGolfer/Golfer/User navigation
        var seasonPlayers = await _context.SeasonGolfers
            .IgnoreQueryFilters()
            .Include(sg => sg.LeagueGolfer)
                .ThenInclude(lg => lg.Golfer)
                    .ThenInclude(g => g.User)
            .Where(sg => sg.SeasonId == seasonId && sg.LeagueId == leagueId)
            .OrderBy(sg => sg.LeagueGolfer.DisplayName)
            .ToListAsync();

        return seasonPlayers.Select(sg =>
        {
            var r = MapToResponse(sg.LeagueGolfer);
            r.SeasonGolferId = sg.Id;
            r.TeamId = sg.TeamId;
            r.IsPaidForSeason = sg.IsPaidForSeason;
            r.PaidAt = sg.PaidAt;
            return r;
        }).ToList();
    }

    private static PlayerResponse MapToResponse(LeagueGolfer leagueGolfer)
    {
        return new PlayerResponse
        {
            Id = leagueGolfer.Id,
            GolferId = leagueGolfer.GolferId,
            UserId = leagueGolfer.Golfer.UserId,
            LeagueId = leagueGolfer.LeagueId,
            DisplayName = leagueGolfer.DisplayName,
            Nickname = leagueGolfer.Nickname,
            Email = leagueGolfer.Golfer.User.Email,
            LeagueHandicap = leagueGolfer.LeagueHandicap,
            LeagueHandicapUpdatedAt = leagueGolfer.LeagueHandicapUpdatedAt,
            TotalRounds = leagueGolfer.TotalRounds,
            AverageScore = leagueGolfer.AverageScore,
            BestScore = leagueGolfer.BestScore,
            JoinedAt = leagueGolfer.JoinedAt,
            CreatedAt = leagueGolfer.CreatedAt,
            UpdatedAt = leagueGolfer.UpdatedAt
        };
    }

    private static string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
        var randomBytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var password = new char[16];
        for (int i = 0; i < 16; i++)
        {
            password[i] = chars[randomBytes[i] % chars.Length];
        }

        return new string(password);
    }
}

