using System.Security.Claims;
using GolfManager.Api.Authorization;
using GolfManager.Services.Auth;
using GolfManager.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolfManager.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private const string LocalCookieScheme = "GolfManager.LocalCookie";
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result == null)
        {
            return BadRequest(new { message = "User with this email already exists" });
        }

        _logger.LogInformation("User registered successfully: {Email}", request.Email);
        await SignInLocalUserAsync(result);
        return Ok(RemoveClientVisibleTokens(result));
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        _logger.LogInformation("User logged in successfully: {Email}", request.Email);
        await SignInLocalUserAsync(result);
        return Ok(RemoveClientVisibleTokens(result));
    }

    /// <summary>
    /// Return the current local cookie-authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _authService.GetAuthResponseForUserAsync(userId);
        return result == null ? Unauthorized() : Ok(RemoveClientVisibleTokens(result));
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("🔄 Refresh token request received");

        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            _logger.LogWarning("❌ Refresh token is null or empty");
            return BadRequest(new { message = "Refresh token is required" });
        }

        _logger.LogInformation("Refresh token (first 10 chars): {TokenPrefix}...",
            request.RefreshToken.Substring(0, Math.Min(10, request.RefreshToken.Length)));

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (result == null)
        {
            _logger.LogWarning("❌ Token refresh failed - Invalid or expired refresh token");
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        _logger.LogInformation("✅ Token refreshed successfully for user: {UserId}", result.UserId);
        return Ok(result);
    }

    /// <summary>
    /// Logout - revoke refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest? request)
    {
        if (!string.IsNullOrEmpty(request?.RefreshToken))
        {
            await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
        }

        await HttpContext.SignOutAsync(LocalCookieScheme);

        _logger.LogInformation("User logged out successfully");
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Revoke all refresh tokens for the current user
    /// </summary>
    [HttpPost("revoke-all")]
    [Authorize]
    public async Task<ActionResult> RevokeAllTokens()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _authService.RevokeAllUserTokensAsync(userId);

        if (!result)
        {
            return BadRequest(new { message = "No active tokens found" });
        }

        _logger.LogInformation("All tokens revoked for user: {UserId}", userId);
        return Ok(new { message = "All tokens revoked successfully" });
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
        {
            claims.Add(new Claim(AuthorizationConstants.Claims.LeagueId, mapping.LeagueId));
        }

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
