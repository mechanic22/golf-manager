using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace GolfManager.Web.Services;

/// <summary>
/// HTTP message handler that includes the local auth cookie on API requests.
/// </summary>
public class AuthenticatedHttpClientHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return await base.SendAsync(request, cancellationToken);
    }
}
