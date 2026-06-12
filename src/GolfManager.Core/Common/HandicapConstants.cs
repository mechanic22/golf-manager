namespace GolfManager.Core.Common;

public static class HandicapConstants
{
    // WHS (World Handicap System)
    public const double WhsMultiplier = 0.96;
    public const double WhsMaxIndex = 54.0;

    // Bob's League informal method
    public const double BobsMultiplier = 0.80;
    public const double BobsMaxIndex = 36.0;

    // Standard slope rating used when tee data is unavailable
    public const int StandardSlopeRating = 113;

    // Default par when tee data is unavailable
    public const int DefaultPar = 72;
}
