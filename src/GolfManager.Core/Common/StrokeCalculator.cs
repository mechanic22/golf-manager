using GolfManager.Core.Entities;
using GolfManager.Core.Enums;

namespace GolfManager.Core.Common;

public static class StrokeCalculator
{
    /// <summary>
    /// Builds a per-hole stroke allocation map from a player's handicap index and the course's
    /// handicap-difficulty ranking (HoleTee.Handicap). Positive handicaps receive strokes on
    /// hardest holes first; negative handicaps (scratch+) give them away on easiest holes first.
    /// </summary>
    public static Dictionary<int, int> AllocateHandicapStrokes(double handicap, List<HoleTee> holeTees, int? maxHandicap)
    {
        var capped = Math.Abs((int)Math.Floor(handicap));
        if (maxHandicap.HasValue)
            capped = Math.Min(capped, maxHandicap.Value);

        var sign = handicap < 0 ? -1 : 1;
        var strokeMap = holeTees.ToDictionary(h => h.HoleNumber, _ => 0);
        var ordered = sign > 0
            ? holeTees.OrderBy(h => h.Handicap).ToList()
            : holeTees.OrderByDescending(h => h.Handicap).ToList();

        var idx = 0;
        while (capped > 0 && ordered.Count > 0)
        {
            strokeMap[ordered[idx].HoleNumber] += sign;
            idx = (idx + 1) % ordered.Count;
            capped--;
        }

        return strokeMap;
    }

    /// <summary>
    /// Calculates a net round score by subtracting handicap strokes from each hole's gross score.
    /// Returns null when no holes have been scored. Uses the pre-stored NetScore when available.
    /// </summary>
    public static double? CalculateNetRoundScore(Round? round, double handicap, List<HoleTee> holeTees, List<int> activeHoleNumbers, int? maxHandicap)
    {
        if (round == null) return null;

        var scoredHoles = round.Holes
            .Where(h => activeHoleNumbers.Contains(h.HoleNumber) && h.GrossScore.HasValue)
            .ToList();

        if (scoredHoles.Count == 0)
            return round.TotalScore.HasValue ? round.TotalScore.Value - handicap : null;

        if (holeTees.Count == 0)
            return scoredHoles.Sum(h => h.GrossScore ?? 0) - handicap;

        var strokeMap = AllocateHandicapStrokes(handicap, holeTees, maxHandicap);
        return scoredHoles.Sum(h => (h.GrossScore ?? 0) - strokeMap.GetValueOrDefault(h.HoleNumber));
    }

    /// <summary>
    /// Returns the hole numbers relevant for scoring based on how many holes were played.
    /// </summary>
    public static List<int> GetHoleNumbersForScoring(HolesPlayed holesPlayed) => holesPlayed switch
    {
        HolesPlayed.Back => Enumerable.Range(10, 9).ToList(),
        HolesPlayed.Eighteen => Enumerable.Range(1, 18).ToList(),
        HolesPlayed.Front => Enumerable.Range(1, 9).ToList(),
        HolesPlayed.Nine => Enumerable.Range(1, 9).ToList(),
        _ => new List<int>()
    };
}
