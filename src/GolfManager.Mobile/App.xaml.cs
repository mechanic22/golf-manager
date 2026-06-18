using GolfManager.Mobile.Services;

namespace GolfManager.Mobile;

public partial class App : Application
{
    private readonly IAuthService _auth;

    public App(IAuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell();
        _ = RestoreSessionAsync(shell);
        return new Window(shell);
    }

    private async Task RestoreSessionAsync(AppShell shell)
    {
        try
        {
            var isAuthenticated = await _auth.TryRestoreSessionAsync();
            if (isAuthenticated)
                await shell.GoToAsync("league-select");
        }
        catch
        {
            // Session restore failed — user stays on login screen
        }
    }
}
