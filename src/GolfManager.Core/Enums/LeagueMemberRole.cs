namespace GolfManager.Core.Enums;

/// <summary>
/// Role assigned to a user within a league.
/// </summary>
public enum LeagueMemberRole
{
    /// <summary>
    /// League owner with full administrative control.
    /// </summary>
    Owner = 0,

    /// <summary>
    /// League administrator who can manage league data and members.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Standard league member.
    /// </summary>
    Member = 2,

    /// <summary>
    /// View-only league access.
    /// </summary>
    Viewer = 3
}
