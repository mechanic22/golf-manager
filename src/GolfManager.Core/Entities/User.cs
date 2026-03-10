using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// User entity - Global authentication and identity
/// Not tenant-specific - users can belong to multiple leagues
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Email address - unique login identifier
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password for authentication
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Global admin flag - can manage all leagues
    /// </summary>
    public bool IsGlobalAdmin { get; set; }

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // Navigation Properties

    /// <summary>
    /// Optional golfer profile - user may not be a golfer (admin, scorekeeper, etc.)
    /// </summary>
    public Golfer? Golfer { get; set; }

    /// <summary>
    /// League memberships for this user
    /// </summary>
    public ICollection<UserLeague> UserLeagues { get; set; } = new List<UserLeague>();

    /// <summary>
    /// Refresh tokens for authentication
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

