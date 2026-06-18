using GolfManager.Mobile.ViewModels;

namespace GolfManager.Mobile.Views;

public partial class LeagueSelectPage : ContentPage
{
    private readonly LeagueSelectViewModel _vm;

    public LeagueSelectPage(LeagueSelectViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;

        // No back arrow — users should not return to the login screen
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IsVisible = false,
            IsEnabled = false
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    protected override bool OnBackButtonPressed() => true; // consume hardware back on Android
}
