namespace GolfManager.Core.Enums;

/// <summary>
/// How to handle missing players in scoring
/// </summary>
public enum MissingPlayerType
{
    /// <summary>
    /// No points awarded
    /// </summary>
    None = 0,

    /// <summary>
    /// Play against par
    /// </summary>
    PlayAgainstPar = 1,

    /// <summary>
    /// Blind draw from available players
    /// </summary>
    BlindDraw = 2,

    /// <summary>
    /// Use field average score
    /// </summary>
    FieldAverage = 3
}

