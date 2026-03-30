using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.OneTimeEvent;

/// <summary>
/// Request to update a team registration
/// </summary>
public class UpdateTeamRequest
{
    /// <summary>
    /// Team name
    /// </summary>
    [StringLength(100, MinimumLength = 2)]
    public string? TeamName { get; set; }

    /// <summary>
    /// Captain name
    /// </summary>
    [StringLength(100, MinimumLength = 2)]
    public string? CaptainName { get; set; }

    /// <summary>
    /// Captain email
    /// </summary>
    [EmailAddress]
    [StringLength(100)]
    public string? CaptainEmail { get; set; }

    /// <summary>
    /// Captain phone
    /// </summary>
    [Phone]
    [StringLength(20)]
    public string? CaptainPhone { get; set; }
}

