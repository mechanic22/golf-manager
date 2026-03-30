using GolfManager.Shared.DTOs.OneTimeEvent;

namespace GolfManager.Services.OneTimeEvent;

/// <summary>
/// Service for managing team registrations for one-time events
/// </summary>
public interface ITeamRegistrationService
{
    /// <summary>
    /// Get all teams for an event
    /// </summary>
    Task<List<OneTimeEventTeamResponse>> GetEventTeamsAsync(string eventId);

    /// <summary>
    /// Get a specific team by ID
    /// </summary>
    Task<OneTimeEventTeamResponse?> GetTeamByIdAsync(string teamId);

    /// <summary>
    /// Register a new team for an event
    /// </summary>
    Task<OneTimeEventTeamResponse> RegisterTeamAsync(string eventId, RegisterTeamRequest request, string? userId = null);

    /// <summary>
    /// Update team information
    /// </summary>
    Task<OneTimeEventTeamResponse> UpdateTeamAsync(string teamId, UpdateTeamRequest request, string userId);

    /// <summary>
    /// Remove a team from an event
    /// </summary>
    Task<bool> RemoveTeamAsync(string teamId, string userId);

    /// <summary>
    /// Check in a team on event day
    /// </summary>
    Task<OneTimeEventTeamResponse> CheckInTeamAsync(string teamId, string userId);

    /// <summary>
    /// Add a player to a team
    /// </summary>
    Task<OneTimeEventPlayerResponse> AddPlayerAsync(string teamId, AddPlayerRequest request, string userId);

    /// <summary>
    /// Update a player on a team
    /// </summary>
    Task<OneTimeEventPlayerResponse> UpdatePlayerAsync(string playerId, UpdatePlayerRequest request, string userId);

    /// <summary>
    /// Remove a player from a team
    /// </summary>
    Task<bool> RemovePlayerAsync(string playerId, string userId);

    /// <summary>
    /// Check if a user is the captain of a team
    /// </summary>
    Task<bool> IsTeamCaptainAsync(string teamId, string userId);
}

