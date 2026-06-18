using GolfManager.Mobile.Views;

namespace GolfManager.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("league-select", typeof(LeagueSelectPage));
        Routing.RegisterRoute("week-select", typeof(WeekSelectPage));
        Routing.RegisterRoute("hole", typeof(HolePage));
    }
}
