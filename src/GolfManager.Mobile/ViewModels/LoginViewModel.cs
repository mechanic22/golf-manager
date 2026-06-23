using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GolfManager.Mobile.Services;

namespace GolfManager.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBiometricAvailable;

    public LoginViewModel(IAuthService auth)
    {
        _auth = auth;
        _ = CheckBiometricAsync();
    }

    private async Task CheckBiometricAsync()
    {
        try { IsBiometricAvailable = await _auth.CanUseBiometricAsync(); }
        catch { IsBiometricAvailable = false; }
    }

    [RelayCommand]
    private async Task LoginWithBiometricAsync()
    {
        ErrorMessage = null;
        IsLoading = true;
        try
        {
            var success = await _auth.BiometricLoginAsync();
            if (success)
                await Shell.Current.GoToAsync("league-select");
            else
                ErrorMessage = "Biometric sign-in failed or was cancelled.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Biometric sign-in failed: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task LoginWithGoogleAsync()
    {
        ErrorMessage = null;
        IsLoading = true;
        try
        {
            var success = await _auth.LoginWithOAuthAsync("Google");
            if (success)
                await Shell.Current.GoToAsync("league-select");
            else
                ErrorMessage = "Google sign-in was cancelled or failed.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Google sign-in failed: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        IsLoading = true;
        try
        {
            var success = await _auth.LoginAsync(Email, Password);
            if (success)
                await Shell.Current.GoToAsync("league-select");
            else
                ErrorMessage = "Invalid email or password.";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Cannot reach server: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
        }
        finally { IsLoading = false; }
    }
}
