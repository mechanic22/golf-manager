using GolfManager.Shared.DTOs.Simulation;

namespace GolfManager.Services.Simulation;

/// <summary>
/// Service for simulating full seasons with players, teams, events, and scores
/// </summary>
public interface ISeasonSimulationService
{
    /// <summary>
    /// Seed a season with players, teams, and events
    /// </summary>
    /// <param name="leagueId">League ID</param>
    /// <param name="seasonId">Season ID</param>
    /// <param name="playerCount">Number of players to create (default: 60)</param>
    /// <param name="playersPerTeam">Players per team (default: 3)</param>
    /// <param name="weekCount">Number of weeks/events (default: 16)</param>
    Task<SimulationResult> SeedSeasonAsync(
        string leagueId, 
        string seasonId, 
        int playerCount = 60, 
        int playersPerTeam = 3, 
        int weekCount = 16);

    /// <summary>
    /// Simulate scores for a specific event
    /// </summary>
    /// <param name="leagueId">League ID</param>
    /// <param name="eventId">Event ID</param>
    Task<SimulationResult> SimulateEventScoresAsync(string leagueId, string eventId);

    /// <summary>
    /// Simulate scores for the next unplayed event
    /// </summary>
    /// <param name="leagueId">League ID</param>
    /// <param name="seasonId">Season ID</param>
    Task<SimulationResult> SimulateNextEventAsync(string leagueId, string seasonId);

    /// <summary>
    /// Simulate all remaining events in a season
    /// </summary>
    /// <param name="leagueId">League ID</param>
    /// <param name="seasonId">Season ID</param>
    Task<SimulationResult> SimulateAllEventsAsync(string leagueId, string seasonId);

    /// <summary>
    /// Clear all simulation data for a season
    /// </summary>
    /// <param name="leagueId">League ID</param>
    /// <param name="seasonId">Season ID</param>
    Task<SimulationResult> ClearSeasonDataAsync(string leagueId, string seasonId);
}

