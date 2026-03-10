using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// RefreshToken - JWT refresh token for authentication
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The refresh token value
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// When the token expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Is the token revoked?
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// When the token was revoked
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// IP address that created the token
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// IP address that revoked the token
    /// </summary>
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// Replacement token (if rotated)
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Is the token still valid?
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Is the token active (not expired and not revoked)?
    /// </summary>
    public new bool IsActive => !IsRevoked && !IsExpired;

    // Navigation Properties

    /// <summary>
    /// Associated user
    /// </summary>
    public User User { get; set; } = null!;
}

