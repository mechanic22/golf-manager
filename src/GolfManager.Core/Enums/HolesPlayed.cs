namespace GolfManager.Core.Enums;

/// <summary>
/// Number of holes played in an event or round
/// </summary>
public enum HolesPlayed
{
    /// <summary>
    /// Not specified
    /// </summary>
    None = 0,

    /// <summary>
    /// 9 holes
    /// </summary>
    Nine = 9,

    /// <summary>
    /// 18 holes
    /// </summary>
    Eighteen = 18,

    /// <summary>
    /// Front 9 holes only
    /// </summary>
    Front = 91,

    /// <summary>
    /// Back 9 holes only
    /// </summary>
    Back = 92
}

