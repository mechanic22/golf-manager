using GolfManager.Core.Enums;
using GolfManager.Data;
using GolfManager.Shared.DTOs.OneTimeEvent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.OneTimeEvent;

/// <summary>
/// Service for managing team registrations for one-time events
/// </summary>
public class TeamRegistrationService : ITeamRegistrationService
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<TeamRegistrationService> _logger;

    public TeamRegistrationService(GolfManagerDbContext context, ILogger<TeamRegistrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<OneTimeEventTeamResponse>> GetEventTeamsAsync(string eventId)
    {
        var teams = await _context.OneTimeEventTeams
            .Include(t => t.Players)
            .Where(t => t.EventId == eventId && t.IsActive)
            .OrderBy(t => t.TeamNumber)
            .ToListAsync();

        return teams.Select(MapToResponse).ToList();
    }

    public async Task<OneTimeEventTeamResponse?> GetTeamByIdAsync(string teamId)
    {
        var team = await _context.OneTimeEventTeams
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == teamId && t.IsActive);

        return team == null ? null : MapToResponse(team);
    }

    public async Task<OneTimeEventTeamResponse> RegisterTeamAsync(string eventId, RegisterTeamRequest request, string? userId = null)
    {
        // Get the event
        var eventEntity = await _context.OneTimeEvents
            .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive);

        if (eventEntity == null)
        {
            throw new KeyNotFoundException($"Event with ID {eventId} not found");
        }

        // Validate event is published
        if (eventEntity.Status != EventStatus.Published)
        {
            throw new InvalidOperationException("Cannot register for an event that is not published");
        }

        // Validate registration deadline
        if (eventEntity.RegistrationDeadline.HasValue && DateTime.UtcNow > eventEntity.RegistrationDeadline.Value)
        {
            throw new InvalidOperationException("Registration deadline has passed");
        }

        // Validate registration code for private events
        if (eventEntity.AccessType == EventAccessType.Private)
        {
            if (string.IsNullOrWhiteSpace(request.RegistrationCode) ||
                request.RegistrationCode != eventEntity.RegistrationCode)
            {
                throw new ArgumentException("Invalid registration code");
            }
        }

        // Check if event is full
        var currentTeamCount = await _context.OneTimeEventTeams
            .CountAsync(t => t.EventId == eventId && t.IsActive);

        if (eventEntity.MaxTeams.HasValue && currentTeamCount >= eventEntity.MaxTeams.Value)
        {
            throw new InvalidOperationException("Event is full");
        }

        // Validate team size
        if (request.Players.Count > eventEntity.TeamSize)
        {
            throw new InvalidOperationException($"Team size cannot exceed {eventEntity.TeamSize} players");
        }

        // Get next team number
        var maxTeamNumber = await _context.OneTimeEventTeams
            .Where(t => t.EventId == eventId)
            .MaxAsync(t => (int?)t.TeamNumber) ?? 0;

        // Create team
        var team = new Core.Entities.OneTimeEventTeam
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventId,
            TeamName = request.TeamName,
            TeamNumber = maxTeamNumber + 1,
            CaptainUserId = userId,
            CaptainName = request.CaptainName,
            CaptainEmail = request.CaptainEmail,
            CaptainPhone = request.CaptainPhone,
            RegisteredAt = DateTime.UtcNow,
            IsCheckedIn = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId ?? "anonymous"
        };

        _context.OneTimeEventTeams.Add(team);

        // Add players
        int playerNumber = 1;
        foreach (var playerInfo in request.Players)
        {
            var player = new Core.Entities.OneTimeEventPlayer
            {
                Id = Guid.NewGuid().ToString(),
                TeamId = team.Id,
                EventId = eventId,
                PlayerName = playerInfo.PlayerName,
                Email = playerInfo.Email,
                Handicap = playerInfo.Handicap,
                PlayerNumber = playerNumber++,
                IsCaptain = playerInfo.IsCaptain,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId ?? "anonymous"
            };

            _context.OneTimeEventPlayers.Add(player);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Registered team {TeamName} ({TeamId}) for event {EventId}",
            team.TeamName, team.Id, eventId);

        return MapToResponse(team);
    }

    public async Task<OneTimeEventTeamResponse> UpdateTeamAsync(string teamId, UpdateTeamRequest request, string userId)
    {
        var team = await _context.OneTimeEventTeams
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == teamId && t.IsActive);

        if (team == null)
        {
            throw new KeyNotFoundException($"Team with ID {teamId} not found");
        }

        // Only captain or event organizer can update
        if (team.CaptainUserId != userId && team.Event?.OrganizerId != userId)
        {
            throw new UnauthorizedAccessException("Only the team captain or event organizer can update this team");
        }

        // Cannot update if event is locked
        if (team.Event?.IsLocked == true)
        {
            throw new InvalidOperationException("Cannot update team for a locked event");
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.TeamName))
            team.TeamName = request.TeamName;

        if (!string.IsNullOrWhiteSpace(request.CaptainName))
            team.CaptainName = request.CaptainName;

        if (!string.IsNullOrWhiteSpace(request.CaptainEmail))
            team.CaptainEmail = request.CaptainEmail;

        if (request.CaptainPhone != null)
            team.CaptainPhone = request.CaptainPhone;

        team.UpdatedAt = DateTime.UtcNow;
        team.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated team {TeamId} by user {UserId}", teamId, userId);

        return MapToResponse(team);
    }

    public async Task<bool> RemoveTeamAsync(string teamId, string userId)
    {
        var team = await _context.OneTimeEventTeams
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == teamId && t.IsActive);

        if (team == null)
        {
            return false;
        }

        // Only captain or event organizer can remove
        if (team.CaptainUserId != userId && team.Event?.OrganizerId != userId)
        {
            throw new UnauthorizedAccessException("Only the team captain or event organizer can remove this team");
        }

        // Cannot remove if event is locked
        if (team.Event?.IsLocked == true)
        {
            throw new InvalidOperationException("Cannot remove team from a locked event");
        }

        // Soft delete team and players
        team.IsActive = false;
        team.UpdatedAt = DateTime.UtcNow;
        team.UpdatedBy = userId;

        var players = await _context.OneTimeEventPlayers
            .Where(p => p.TeamId == teamId && p.IsActive)
            .ToListAsync();

        foreach (var player in players)
        {
            player.IsActive = false;
            player.UpdatedAt = DateTime.UtcNow;
            player.UpdatedBy = userId;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed team {TeamId} by user {UserId}", teamId, userId);

        return true;
    }

    public async Task<OneTimeEventTeamResponse> CheckInTeamAsync(string teamId, string userId)
    {
        var team = await _context.OneTimeEventTeams
            .Include(t => t.Event)
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == teamId && t.IsActive);

        if (team == null)
        {
            throw new KeyNotFoundException($"Team with ID {teamId} not found");
        }

        // Only event organizer can check in teams
        if (team.Event?.OrganizerId != userId)
        {
            throw new UnauthorizedAccessException("Only the event organizer can check in teams");
        }

        // Validate event is in progress
        if (team.Event?.Status != EventStatus.InProgress && team.Event?.Status != EventStatus.Published)
        {
            throw new InvalidOperationException("Can only check in teams for published or in-progress events");
        }

        team.IsCheckedIn = true;
        team.CheckedInAt = DateTime.UtcNow;
        team.UpdatedAt = DateTime.UtcNow;
        team.UpdatedBy = userId;

        // Update event status to InProgress if it's the first check-in
        if (team.Event.Status == EventStatus.Published)
        {
            team.Event.Status = EventStatus.InProgress;
            team.Event.UpdatedAt = DateTime.UtcNow;
            team.Event.UpdatedBy = userId;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Checked in team {TeamId} for event {EventId} by user {UserId}",
            teamId, team.EventId, userId);

        return MapToResponse(team);
    }

    public async Task<OneTimeEventPlayerResponse> AddPlayerAsync(string teamId, AddPlayerRequest request, string userId)
    {
        var team = await _context.OneTimeEventTeams
            .Include(t => t.Event)
            .Include(t => t.Players.Where(p => p.IsActive))
            .FirstOrDefaultAsync(t => t.Id == teamId && t.IsActive);

        if (team == null)
        {
            throw new KeyNotFoundException($"Team with ID {teamId} not found");
        }

        // Only captain or event organizer can add players
        if (team.CaptainUserId != userId && team.Event?.OrganizerId != userId)
        {
            throw new UnauthorizedAccessException("Only the team captain or event organizer can add players");
        }

        // Cannot add if event is locked
        if (team.Event?.IsLocked == true)
        {
            throw new InvalidOperationException("Cannot add players to a locked event");
        }

        // Validate team size
        var currentPlayerCount = team.Players?.Count ?? 0;
        if (team.Event != null && currentPlayerCount >= team.Event.TeamSize)
        {
            throw new InvalidOperationException($"Team is full (max {team.Event.TeamSize} players)");
        }

        // Get next player number
        var maxPlayerNumber = team.Players?.Max(p => p.PlayerNumber) ?? 0;

        var player = new Core.Entities.OneTimeEventPlayer
        {
            Id = Guid.NewGuid().ToString(),
            TeamId = teamId,
            EventId = team.EventId,
            PlayerName = request.PlayerName,
            Email = request.Email,
            Handicap = request.Handicap,
            PlayerNumber = maxPlayerNumber + 1,
            IsCaptain = request.IsCaptain,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.OneTimeEventPlayers.Add(player);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added player {PlayerName} ({PlayerId}) to team {TeamId}",
            player.PlayerName, player.Id, teamId);

        return MapToPlayerResponse(player);
    }

    public async Task<OneTimeEventPlayerResponse> UpdatePlayerAsync(string playerId, UpdatePlayerRequest request, string userId)
    {
        var player = await _context.OneTimeEventPlayers
            .Include(p => p.Team)
                .ThenInclude(t => t!.Event)
            .FirstOrDefaultAsync(p => p.Id == playerId && p.IsActive);

        if (player == null)
        {
            throw new KeyNotFoundException($"Player with ID {playerId} not found");
        }

        // Only captain or event organizer can update
        if (player.Team?.CaptainUserId != userId && player.Team?.Event?.OrganizerId != userId)
        {
            throw new UnauthorizedAccessException("Only the team captain or event organizer can update players");
        }

        // Cannot update if event is locked
        if (player.Team?.Event?.IsLocked == true)
        {
            throw new InvalidOperationException("Cannot update players for a locked event");
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.PlayerName))
            player.PlayerName = request.PlayerName;

        if (request.Email != null)
            player.Email = request.Email;

        if (request.Handicap != null)
            player.Handicap = request.Handicap;

        player.UpdatedAt = DateTime.UtcNow;
        player.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated player {PlayerId} by user {UserId}", playerId, userId);

        return MapToPlayerResponse(player);
    }

    public async Task<bool> RemovePlayerAsync(string playerId, string userId)
    {
        var player = await _context.OneTimeEventPlayers
            .Include(p => p.Team)
                .ThenInclude(t => t!.Event)
            .FirstOrDefaultAsync(p => p.Id == playerId && p.IsActive);

        if (player == null)
        {
            return false;
        }

        // Only captain or event organizer can remove
        if (player.Team?.CaptainUserId != userId && player.Team?.Event?.OrganizerId != userId)
        {
            throw new UnauthorizedAccessException("Only the team captain or event organizer can remove players");
        }

        // Cannot remove if event is locked
        if (player.Team?.Event?.IsLocked == true)
        {
            throw new InvalidOperationException("Cannot remove players from a locked event");
        }

        // Soft delete
        player.IsActive = false;
        player.UpdatedAt = DateTime.UtcNow;
        player.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed player {PlayerId} by user {UserId}", playerId, userId);

        return true;
    }

    public async Task<bool> IsTeamCaptainAsync(string teamId, string userId)
    {
        return await _context.OneTimeEventTeams
            .AnyAsync(t => t.Id == teamId && t.CaptainUserId == userId && t.IsActive);
    }

    // Mapping methods
    private OneTimeEventTeamResponse MapToResponse(Core.Entities.OneTimeEventTeam team)
    {
        var players = team.Players?.Where(p => p.IsActive).OrderBy(p => p.PlayerNumber).ToList() ?? new List<Core.Entities.OneTimeEventPlayer>();

        return new OneTimeEventTeamResponse
        {
            Id = team.Id,
            EventId = team.EventId,
            TeamName = team.TeamName,
            TeamNumber = team.TeamNumber,
            CaptainUserId = team.CaptainUserId,
            CaptainName = team.CaptainName,
            CaptainEmail = team.CaptainEmail,
            CaptainPhone = team.CaptainPhone,
            RegisteredAt = team.RegisteredAt,
            IsCheckedIn = team.IsCheckedIn,
            CheckedInAt = team.CheckedInAt,
            TotalScore = team.TotalScore,
            NetScore = team.NetScore,
            Position = team.Position,
            Players = players.Select(MapToPlayerResponse).ToList()
        };
    }

    private static OneTimeEventPlayerResponse MapToPlayerResponse(Core.Entities.OneTimeEventPlayer player)
    {
        return new OneTimeEventPlayerResponse
        {
            Id = player.Id,
            TeamId = player.TeamId,
            EventId = player.EventId,
            UserId = player.UserId,
            PlayerName = player.PlayerName,
            Email = player.Email,
            Handicap = player.Handicap,
            PlayerNumber = player.PlayerNumber,
            IsCaptain = player.IsCaptain
        };
    }
}
