using System.Security.Claims;
using GolfManager.Api.Authorization;
using GolfManager.Data;
using GolfManager.Services.Auth;
using GolfManager.Shared.DTOs.Auth;
using GolfManager.Shared.DTOs.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfManager.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private const string LocalCookieScheme = "GolfManager.LocalCookie";
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
