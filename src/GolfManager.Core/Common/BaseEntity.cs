namespace GolfManager.Core.Common;

/// <summary>
/// Base entity with common properties for all entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Primary key - GUID as string
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID who created this entity
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when entity was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User ID who last updated this entity
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete flag - if true, entity is deleted but kept for referential integrity
    /// Deleted entities should be filtered out of all queries automatically
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// When entity was soft deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User ID who deleted this entity
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Active/Inactive flag - business logic state (e.g., active member vs inactive member)
    /// Inactive entities can still be shown in some contexts and can be reactivated
    /// </summary>
    public bool IsActive { get; set; } = true;
}

