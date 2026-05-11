using GolfManager.Web.Components.Icons;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages;

public partial class Icons : ComponentBase
{
    private record IconInfo(string Name, string Path);

    private List<IconInfo> GetGolfIcons() => new()
    {
        new("GolfFlag", GolfIcons.GolfFlag),
        new("Trophy", GolfIcons.Trophy),
        new("Scorecard", GolfIcons.Scorecard),
        new("Leaderboard", GolfIcons.Leaderboard),
        new("GolfCourse", GolfIcons.GolfCourse),
        new("Target", GolfIcons.Target)
    };

    private List<IconInfo> GetCommonIcons() => new()
    {
        new("Home", GolfIcons.Home),
        new("Dashboard", GolfIcons.Dashboard),
        new("People", GolfIcons.People),
        new("Person", GolfIcons.Person),
        new("Calendar", GolfIcons.Calendar),
        new("Event", GolfIcons.Event),
        new("Settings", GolfIcons.Settings),
        new("Add", GolfIcons.Add),
        new("Edit", GolfIcons.Edit),
        new("Delete", GolfIcons.Delete),
        new("Close", GolfIcons.Close),
        new("Check", GolfIcons.Check),
        new("Search", GolfIcons.Search),
        new("Menu", GolfIcons.Menu),
        new("MoreVert", GolfIcons.MoreVert),
        new("ArrowBack", GolfIcons.ArrowBack),
        new("ArrowForward", GolfIcons.ArrowForward),
        new("Logout", GolfIcons.Logout),
        new("Login", GolfIcons.Login),
        new("Star", GolfIcons.Star),
        new("Info", GolfIcons.Info),
        new("Warning", GolfIcons.Warning),
        new("TrendingUp", GolfIcons.TrendingUp),
        new("TrendingDown", GolfIcons.TrendingDown)
    };
}
