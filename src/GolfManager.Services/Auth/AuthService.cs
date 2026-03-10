using GolfManager.Core.Entities;
using GolfManager.Data;
using GolfManager.Shared.DTOs.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GolfManager.Services.Auth;

public class AuthService : IAuthService
{
    private readonly GolfManagerDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly int _refreshTokenExpirationDays;

    public AuthService(
        GolfManagerDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "30");
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            return null; // User already exists
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsGlobalAdmin = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.Email,
            IsActive = true
        };

        _context.Users.Add(user);

        // Generate tokens
        var refreshToken = CreateRefreshToken(user.Id);
        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        // Get user's league IDs (empty for new users)
        var leagueIds = new List<string>();

        var accessToken = _jwtTokenService.GenerateAccessToken(user, leagueIds);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsGlobalAdmin = user.IsGlobalAdmin,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt
        };
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Find user by email
        var user = await _context.Users
            .Include(u => u.UserLeagues)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !user.IsActive)
        {
            return null; // User not found or inactive
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null; // Invalid password
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;

        // Generate new refresh token
        var refreshToken = CreateRefreshToken(user.Id);
        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        // Get user's league IDs
        var leagueIds = user.UserLeagues.Select(ul => ul.LeagueId).ToList();

        var accessToken = _jwtTokenService.GenerateAccessToken(user, leagueIds);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsGlobalAdmin = user.IsGlobalAdmin,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt
        };
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserLeagues)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (token == null || !token.IsActive)
        {
            return null; // Invalid or expired token
        }

        // Revoke old token and create new one (token rotation)
        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;

        var newRefreshToken = CreateRefreshToken(token.UserId);
        token.ReplacedByToken = newRefreshToken.Token;

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Get user's league IDs
        var leagueIds = token.User.UserLeagues.Select(ul => ul.LeagueId).ToList();

        var accessToken = _jwtTokenService.GenerateAccessToken(token.User, leagueIds);

        return new AuthResponse
        {
            UserId = token.User.Id,
            Email = token.User.Email,
            FirstName = token.User.FirstName,
            LastName = token.User.LastName,
            IsGlobalAdmin = token.User.IsGlobalAdmin,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = newRefreshToken.ExpiresAt
        };
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (token == null || token.IsRevoked)
        {
            return false;
        }

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RevokeAllUserTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        if (!tokens.Any())
        {
            return false;
        }

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private RefreshToken CreateRefreshToken(string userId)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Token = _jwtTokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
    }
}

