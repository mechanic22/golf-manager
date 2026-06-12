using GolfManager.Web.Layout;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Season;

public partial class SeasonSettings : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;

    [CascadingParameter] private SeasonLayoutContext? Context { get; set; }

    protected override void OnParametersSet()
    {
        if (Context != null && !Context.IsAdmin)
            Navigation.NavigateTo($"/league/{LeagueKey}/season/{SeasonKey}");
    }
}
