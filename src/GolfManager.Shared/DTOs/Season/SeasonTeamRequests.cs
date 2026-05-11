using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.Season;

public class CreateSeasonTeamRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

public class UpdateSeasonTeamRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

/// <summary>
/// Assign (or remove) a season player from a team. Set TeamId to null to remove from all teams.
/// </summary>
public class AssignPlayerToTeamRequest
{
    public string? TeamId { get; set; }
}
