namespace GolfManager.Core.Enums;

/// <summary>
/// Status of an event
/// </summary>
public enum EventStatus
{
    /// <summary>
    /// Event is being created/edited (not visible to public)
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Event is published and open for registration
    /// </summary>
    Published = 1,

    /// <summary>
    /// Registration is closed (no more teams can register)
    /// </summary>
    RegistrationClosed = 2,

    /// <summary>
    /// Event is currently in progress
    /// </summary>
    InProgress = 3,

    /// <summary>
    /// Event has been completed
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Event has been cancelled
    /// </summary>
    Cancelled = 5
}

