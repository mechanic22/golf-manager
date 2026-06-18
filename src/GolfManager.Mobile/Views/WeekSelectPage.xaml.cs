using GolfManager.Mobile.ViewModels;

namespace GolfManager.Mobile.Views;

public partial class WeekSelectPage : ContentPage
{
    public WeekSelectPage(WeekSelectViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
