namespace GolfManager.Core.Enums;

/// <summary>
/// Maximum score allowed for handicap calculation
/// </summary>
public enum MaxScoreForHandicap
{
    /// <summary>
    /// No maximum (use actual score)
    /// </summary>
    None = 0,

    /// <summary>
    /// Two over par
    /// </summary>
    PlusTwo = 1,

    /// <summary>
    /// Three over par
    /// </summary>
    PlusThree = 2,

    /// <summary>
    /// Four over par
    /// </summary>
    PlusFour = 3,

    /// <summary>
    /// Five over par
    /// </summary>
    PlusFive = 4,

    /// <summary>
    /// Six over par
    /// </summary>
    PlusSix = 5,

    /// <summary>
    /// Double par
    /// </summary>
    DoublePar = 6
}

