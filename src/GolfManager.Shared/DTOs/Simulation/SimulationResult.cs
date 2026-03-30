namespace GolfManager.Shared.DTOs.Simulation;

/// <summary>
/// Result of a simulation operation
/// </summary>
public class SimulationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int PlayersCreated { get; set; }
    public int TeamsCreated { get; set; }
    public int EventsCreated { get; set; }
    public int RoundsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
}

