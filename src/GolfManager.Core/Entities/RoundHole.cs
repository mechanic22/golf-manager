namespace GolfManager.Core.Entities;

public class RoundHole
{
    public string RoundId { get; set; } = string.Empty;
    public int HoleNumber { get; set; }
    public int? GrossScore { get; set; }
    public int? NetScore { get; set; }
    public int? Putts { get; set; }
    public bool? FairwayHit { get; set; }
    public bool? GreenInRegulation { get; set; }
    public int? Penalties { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Round Round { get; set; } = null!;
}
