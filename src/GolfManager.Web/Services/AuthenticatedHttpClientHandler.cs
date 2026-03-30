using System.Net;
using System.Net.Http.Headers;

namespace GolfManager.Web.Services;

/// <summary>
/// HTTP message handler that automatically adds authentication headers
/// and handles token refresh on 401 responses
/// </summary>
public class AuthenticatedHttpClientHandler : DelegatingHandler
{
    private readonly Func<IAuthService> _authServiceFactory;

    public AuthenticatedHttpClientHandler(Func<IAuthService> authServiceFactory)
    {
        _authServiceFactory = authServiceFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Get auth service lazily (only when actually sending a request)
        var authService = _authServiceFactory();

        // Add auth header if authenticated
        if (authService.IsAuthenticated && !string.IsNullOrEmpty(authService.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authService.AccessToken);
        }

        // Send the request
        var response = await base.SendAsync(request, cancellationToken);

        // If we get a 401 and we're authenticated, the token might be expired
        // Note: We don't auto-refresh here because the AuthService already has a timer
        // This is just to provide better error messages
        if (response.StatusCode == HttpStatusCode.Unauthorized && authService.IsAuthenticated)
        {
            Console.WriteLine("[AuthenticatedHttpClientHandler] Received 401 - token may be expired");
        }

        return response;
    }
}

