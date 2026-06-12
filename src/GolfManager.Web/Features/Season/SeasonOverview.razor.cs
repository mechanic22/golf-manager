using GolfManager.Web.Layout;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Season;

public partial class SeasonOverview : ComponentBase
{
    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;

    [CascadingParameter] private SeasonLayoutContext? Context { get; set; }
}
