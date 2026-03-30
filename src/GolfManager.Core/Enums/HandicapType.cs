namespace GolfManager.Core.Enums;

/// <summary>
/// Type of handicap system used
/// </summary>
public enum HandicapType
{
    /// <summary>
    /// No handicap system
    /// </summary>
    None = 0,

    /// <summary>
    /// Bob's Famous Method (custom league handicap)
    /// </summary>
    Bobs = 1,

    /// <summary>
    /// Scratch (no handicap adjustments)
    /// </summary>
    Scratch = 2,

    /// <summary>
    /// USGA Handicap System
    /// </summary>
    USGA = 3
}

