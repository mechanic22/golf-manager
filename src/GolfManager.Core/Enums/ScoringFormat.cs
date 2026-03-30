namespace GolfManager.Core.Enums;

/// <summary>
/// Scoring format for events
/// </summary>
public enum ScoringFormat
{
    /// <summary>
    /// Stroke play (total strokes)
    /// </summary>
    StrokePlay = 0,

    /// <summary>
    /// Match play (hole-by-hole competition)
    /// </summary>
    MatchPlay = 1,

    /// <summary>
    /// Stableford (points-based)
    /// </summary>
    Stableford = 2,

    /// <summary>
    /// Two-point system
    /// </summary>
    TwoPoint = 3,

    /// <summary>
    /// Scramble (team format - best shot per hole)
    /// </summary>
    Scramble = 4,

    /// <summary>
    /// Best ball (best individual score per hole)
    /// </summary>
    BestBall = 5,

    /// <summary>
    /// Chapman/Pinehurst format (alternate shot after drives)
    /// </summary>
    Chapman = 6,

    /// <summary>
    /// Shamble (scramble off tee, then individual play)
    /// </summary>
    Shamble = 7,

    /// <summary>
    /// Skins game (lowest score wins the hole)
    /// </summary>
    Skins = 8,

    /// <summary>
    /// Nassau (separate competitions for front 9, back 9, and total)
    /// </summary>
    Nassau = 9,

    /// <summary>
    /// Vegas scoring (team format with point system)
    /// </summary>
    Vegas = 10
}

