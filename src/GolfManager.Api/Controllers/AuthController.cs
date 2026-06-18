using System.Security.Claims;
using GolfManager.Api.Authorization;
using GolfManager.Data;
using GolfManager.Services.Auth;
using GolfManager.Shared.DTOs.Auth;
using GolfManager.Shared.DTOs.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfManager.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private const string LocalCookieScheme = "GolfManager.LocalCookie";
    private const string ExternalCookieScheme = "GolfManager.External";
    private const string MobileDeepLinkScheme = "dkgolf://oauth-callback";
    private readonly IAuthService _authService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IPasswordHasher passwordHasher,
        GolfManagerDbContext context,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _passwordHasher = passwordHasher;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result == null)
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("User with this email already exists"));

        _logger.LogInformation("User registered successfully: {Email}", request.Email);
        await SignInLocalUserAsync(result);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(RemoveClientVisibleTokens(result), "User registered successfully"));
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result == null)
            return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse("Invalid email or password"));

        _logger.LogInformation("User logged in successfully: {Email}", request.Email);
        await SignInLocalUserAsync(result);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(RemoveClientVisibleTokens(result), "Login successful"));
    }

    /// <summary>
    /// Return the current local cookie-authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse("Authentication required"));

        // Guest session — return synthesized response from cookie claims
        if (User.FindFirst(AuthorizationConstants.Claims.IsGuest)?.Value == "true")
        {
            var guestLeagueKey = User.FindFirst(AuthorizationConstants.Claims.LeagueKey)?.Value;
            var guestLeagueId = User.FindFirst(AuthorizationConstants.Claims.LeagueId)?.Value;
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(new AuthResponse
            {
                UserId = "guest",
                IsGuest = true,
                GuestLeagueKey = guestLeagueKey,
                GuestLeagueId = guestLeagueId
            }));
        }

        var result = await _authService.GetAuthResponseForUserAsync(userId);
        if (result == null)
            return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse("Authentication required"));

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(RemoveClientVisibleTokens(result)));
    }

    /// <summary>
    /// Login as an anonymous guest scoped to a specific league.
    /// Issues a short-lived rolling cookie — no database user is created.
    /// </summary>
    [HttpPost("league-guest-login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LeagueGuestLoginResponse>>> LeagueGuestLogin(
        [FromBody] LeagueGuestLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LeagueKey))
            return BadRequest(ApiResponse<LeagueGuestLoginResponse>.ErrorResponse("League key is required"));

        var league = await _context.Leagues
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Key.ToLower() == request.LeagueKey.ToLowerInvariant() && l.IsActive && !l.IsDeleted);

        if (league == null)
            return NotFound(ApiResponse<LeagueGuestLoginResponse>.ErrorResponse("League not found"));

        if (league.RequireAnonymousPassword)
        {
            if (string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(league.AnonymousPasswordHash))
                return Unauthorized(ApiResponse<LeagueGuestLoginResponse>.ErrorResponse("Password required for this league"));

            if (!_passwordHasher.VerifyPassword(request.Password, league.AnonymousPasswordHash))
                return Unauthorized(ApiResponse<LeagueGuestLoginResponse>.ErrorResponse("Incorrect password"));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "guest"),
            new(AuthorizationConstants.Claims.IsGuest, "true"),
            new(AuthorizationConstants.Claims.LeagueId, league.Id),
            new(AuthorizationConstants.Claims.LeagueKey, league.Key)
        };

        var identity = new ClaimsIdentity(claims, LocalCookieScheme);
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(4),
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(LocalCookieScheme, principal, properties);

        _logger.LogInformation("Guest login for league {LeagueKey}", league.Key);

        return Ok(ApiResponse<LeagueGuestLoginResponse>.SuccessResponse(new LeagueGuestLoginResponse
        {
            LeagueKey = league.Key,
            LeagueId = league.Id,
            LeagueName = league.Name,
            LogoUrl = league.LogoUrl,
            IsGuest = true
        }, "Guest access granted"));
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("🔄 Refresh token request received");

        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            _logger.LogWarning("❌ Refresh token is null or empty");
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Refresh token is required"));
        }

        _logger.LogInformation("Refresh token (first 10 chars): {TokenPrefix}...",
            request.RefreshToken.Substring(0, Math.Min(10, request.RefreshToken.Length)));

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (result == null)
        {
            _logger.LogWarning("❌ Token refresh failed - Invalid or expired refresh token");
            return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse("Invalid or expired refresh token"));
        }

        _logger.LogInformation("✅ Token refreshed successfully for user: {UserId}", result.UserId);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Token refreshed successfully"));
    }

    /// <summary>
    /// Logout - revoke refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] RefreshTokenRequest? request)
    {
        if (!string.IsNullOrEmpty(request?.RefreshToken))
            await _authService.RevokeRefreshTokenAsync(request.RefreshToken);

        await HttpContext.SignOutAsync(LocalCookieScheme);

        _logger.LogInformation("User logged out successfully");
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Logged out successfully"));
    }

    /// <summary>
    /// Revoke all refresh tokens for the current user
    /// </summary>
    [HttpPost("revoke-all")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeAllTokens()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<bool>.ErrorResponse("Authentication required"));

        var result = await _authService.RevokeAllUserTokensAsync(userId);

        if (!result)
            return BadRequest(ApiResponse<bool>.ErrorResponse("No active tokens found"));

        _logger.LogInformation("All tokens revoked for user: {UserId}", userId);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "All tokens revoked successfully"));
    }

    /// <summary>
    /// Initiates Google OAuth for the web app.
    /// The browser navigates here directly (forceLoad); after the Google flow the server
    /// sets the auth cookie and redirects back into the Blazor app.
    /// </summary>
    [HttpGet("web/oauth/login")]
    [AllowAnonymous]
    public IActionResult WebOAuthLogin([FromQuery] string? returnUrl = "/dashboard")
    {
        var callbackUrl = Url.Action(nameof(WebOAuthCallback), "Auth", new { returnUrl }, Request.Scheme);
        var properties = new AuthenticationProperties { RedirectUri = callbackUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Callback after Google authenticates the web user.
    /// Signs the user into the local cookie scheme and redirects into the Blazor app.
    /// </summary>
    [HttpGet("web/oauth/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> WebOAuthCallback([FromQuery] string? returnUrl = "/dashboard")
    {
        var result = await HttpContext.AuthenticateAsync(ExternalCookieScheme);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Web OAuth callback failed: {Error}", result.Failure?.Message);
            return Redirect("/login?error=oauth_failed");
        }

        var claims = result.Principal?.Claims?.ToList() ?? [];
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
        var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? string.Empty;
        var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Web OAuth callback: Google did not return an email address");
            return Redirect("/login?error=no_email");
        }

        var authResult = await _authService.LoginWithOAuthAsync(email, firstName, lastName);
        if (authResult == null)
        {
            _logger.LogError("Web OAuth callback: LoginWithOAuthAsync returned null for {Email}", email);
            return Redirect("/login?error=auth_failed");
        }

        _logger.LogInformation("Web OAuth login successful for {Email}", email);
        await SignInLocalUserAsync(authResult);

        var safeReturnUrl = (returnUrl?.StartsWith('/') == true) ? returnUrl : "/dashboard";
        return Redirect(safeReturnUrl);
    }

    /// <summary>
    /// Initiates Google OAuth for the mobile app.
    /// The mobile app opens this URL via WebAuthenticator — it challenges Google and eventually
    /// redirects back to the dkgolf:// deep link with tokens as query parameters.
    /// Google Cloud Console redirect URI to register: https://&lt;host&gt;/signin-google
    /// </summary>
    [HttpGet("mobile/oauth/login")]
    [AllowAnonymous]
    public IActionResult MobileOAuthLogin([FromQuery] string provider = "Google")
    {
        var callbackUrl = Url.Action(nameof(MobileOAuthCallback), "Auth", null, Request.Scheme);
        var properties = new AuthenticationProperties { RedirectUri = callbackUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Callback hit by the Google SDK after the user authenticates.
    /// Exchanges the Google identity for our JWT tokens and redirects to the app deep link.
    /// </summary>
    [HttpGet("mobile/oauth/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> MobileOAuthCallback()
    {
        var result = await HttpContext.AuthenticateAsync(ExternalCookieScheme);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Mobile OAuth callback failed: {Error}", result.Failure?.Message);
            return Redirect($"{MobileDeepLinkScheme}?error=oauth_failed");
        }

        var claims = result.Principal?.Claims?.ToList() ?? [];
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
        var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? string.Empty;
        var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Mobile OAuth callback: Google did not return an email address");
            return Redirect($"{MobileDeepLinkScheme}?error=no_email");
        }

        var authResult = await _authService.LoginWithOAuthAsync(email, firstName, lastName);
        if (authResult == null)
        {
            _logger.LogError("Mobile OAuth callback: LoginWithOAuthAsync returned null for {Email}", email);
            return Redirect($"{MobileDeepLinkScheme}?error=auth_failed");
        }

        _logger.LogInformation("Mobile OAuth login successful for {Email}", email);

        var expiresAt = new DateTimeOffset(authResult.ExpiresAt, TimeSpan.Zero).ToUnixTimeSeconds();
        var deepLink = $"{MobileDeepLinkScheme}" +
            $"?accessToken={Uri.EscapeDataString(authResult.AccessToken)}" +
            $"&refreshToken={Uri.EscapeDataString(authResult.RefreshToken)}" +
            $"&expiresAt={expiresAt}";

        return Redirect(deepLink);
    }

    private async Task SignInLocalUserAsync(AuthResponse authResponse)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, authResponse.UserId),
            new(ClaimTypes.Email, authResponse.Email),
            new(ClaimTypes.GivenName, authResponse.FirstName),
            new(ClaimTypes.Surname, authResponse.LastName),
            new(AuthorizationConstants.Claims.IsGlobalAdmin, authResponse.IsGlobalAdmin.ToString().ToLowerInvariant())
        };

        foreach (var mapping in authResponse.LeagueMappings)
            claims.Add(new Claim(AuthorizationConstants.Claims.LeagueId, mapping.LeagueId));

        var identity = new ClaimsIdentity(claims, LocalCookieScheme);
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14),
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(LocalCookieScheme, principal, properties);
    }

    private static AuthResponse RemoveClientVisibleTokens(AuthResponse authResponse)
    {
        authResponse.AccessToken = string.Empty;
        authResponse.RefreshToken = string.Empty;
        return authResponse;
    }
}
