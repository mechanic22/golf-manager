namespace GolfManager.Shared.DTOs.Handicap;

public enum HandicapCalculationMethod
{
    /// <summary>
    /// USGA World Handicap System (WHS) — uses best 8 of last 20 score differentials × 0.96
    /// </summary>
    WorldHandicapSystem,

    /// <summary>
    /// Bob's League Handicap — 80% of the difference between average score and course par
    /// </summary>
    BobsLeague,

    /// <summary>
    /// Scratch — no handicap (handicap index = 0)
    /// </summary>
    Scratch,

    /// <summary>
    /// Manually entered
    /// </summary>
    Manual
}
