namespace GolfManager.Core.Common;

/// <summary>
/// Interface for entities that belong to a specific league (tenant)
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// League ID for multi-tenant isolation
    /// </summary>
    string LeagueId { get; set; }
}

