using GolfManager.Shared.DTOs.League;
using GolfManager.Shared.DTOs.Season;

namespace GolfManager.Web.Features.Season.Layout;

/// <summary>
/// Cascaded from SeasonLayout to all season pages so they don't re-fetch league/season data.
/// </summary>
public class SeasonLayoutContext
{
    public required LeagueResponse League { get; init; }
    public required SeasonResponse Season { get; init; }
    public bool IsAdmin { get; init; }
}
