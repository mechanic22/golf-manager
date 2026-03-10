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
    /// Scramble (team format)
    /// </summary>
    Scramble = 4,

    /// <summary>
    /// Best ball
    /// </summary>
    BestBall = 5
}

