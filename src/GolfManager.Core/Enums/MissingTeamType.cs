namespace GolfManager.Core.Enums;

/// <summary>
/// How to handle missing teams in scoring
/// </summary>
public enum MissingTeamType
{
    /// <summary>
    /// No points awarded
    /// </summary>
    NoPoints = 0,

    /// <summary>
    /// Partial points based on players present
    /// </summary>
    PartialPoints = 1,

    /// <summary>
    /// Use field average
    /// </summary>
    FieldAverage = 2
}

