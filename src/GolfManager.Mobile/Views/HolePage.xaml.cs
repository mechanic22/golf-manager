using GolfManager.Mobile.ViewModels;

namespace GolfManager.Mobile.Views;

public partial class HolePage : ContentPage
{
    private readonly HoleViewModel _vm;

    public HolePage(HoleViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _vm.StartGpsAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.StopGps();
    }
}
