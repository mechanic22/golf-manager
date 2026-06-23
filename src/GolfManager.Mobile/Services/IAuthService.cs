namespace GolfManager.Mobile.Services;

public interface IAuthService
{
    string? AccessToken { get; }
    bool IsAuthenticated { get; }
    Task<bool> LoginAsync(string email, string password);
    Task<bool> TryRestoreSessionAsync();
    Task<bool> LoginWithOAuthAsync(string provider);
    Task<bool> CanUseBiometricAsync();
    Task<bool> BiometricLoginAsync();
    Task LogoutAsync();
}
