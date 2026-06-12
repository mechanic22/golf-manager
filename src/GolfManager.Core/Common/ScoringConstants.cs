namespace GolfManager.Core.Common;

public static class ScoringConstants
{
    // Match play hole-by-hole points
    public const double HoleWinPoints = 2.0;
    public const double HoleHalvePoints = 1.0;
    public const double HoleForfeitWinPoints = 1.5;
    public const double HoleForfeitLosePoints = 0.5;

    // Match bonus points awarded on top of hole points
    public const double MatchBonusWin = 4.0;
    public const double MatchBonusTie = 2.0;

    // Fallback totals used when hole-by-hole data is unavailable (9-hole match maximum)
    public const double FallbackMatchWin = 22.0;
    public const double FallbackMatchTie = 11.0;
}
