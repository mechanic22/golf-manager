namespace GolfManager.Core.Enums;

/// <summary>
/// Access control type for events
/// </summary>
public enum EventAccessType
{
    /// <summary>
    /// Anyone can view and register for the event
    /// </summary>
    Public = 0,

    /// <summary>
    /// Event requires a registration code to view and register
    /// </summary>
    Private = 1,

    /// <summary>
    /// Event is invite-only (organizer must invite teams)
    /// </summary>
    InviteOnly = 2
}

