using System.Net;
using System.Security.Cryptography;
using DnsClient;
using DnsClient.Protocol;
using GolfManager.Core.Configuration;
using GolfManager.Core.Entities;
using GolfManager.Core.Enums;
using GolfManager.Core.Services;
using GolfManager.Data;
using GolfManager.Services.Auth;
using GolfManager.Shared.DTOs.League;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GolfManager.Services.League;

/// <summary>
/// Service for managing leagues
/// </summary>
public class LeagueService : ILeagueService
{
    private readonly GolfManagerDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IShortIdService _shortIdService;
    private readonly ILogger<LeagueService> _logger;
    private readonly CustomDomainVerificationOptions _verificationOptions;
    private readonly LookupClient _lookupClient;

    public LeagueService(
        GolfManagerDbContext context,
        IPasswordHasher passwordHasher,
        IShortIdService shortIdService,
        IOptions<CustomDomainVerificationOptions> verificationOptions,
        ILogger<LeagueService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _shortIdService = shortIdService;
        _logger = logger;
        _verificationOptions = verificationOptions?.Value ?? new CustomDomainVerificationOptions();
        _lookupClient = new LookupClient();
    }

    public async Task<List<LeagueResponse>> GetUserLeaguesAsync(string userId)
    {
        // Use IgnoreQueryFilters() because we're explicitly filtering by userId
        var userLeagues = await _context.UserLeagues
            .IgnoreQueryFilters()
            .Include(ul => ul.League)
            .Where(ul => ul.UserId == userId && ul.IsActive && ul.League.IsActive)
            .Select(ul => ul.League)
            .ToListAsync();

        var responses = new List<LeagueResponse>();
        foreach (var league in userLeagues)
        {
            responses.Add(await MapToResponseAsync(league, userId));
        }

        return responses;
    }

    public async Task<LeagueResponse?> GetLeagueByIdAsync(string leagueId, string? userId = null)
    {
        var league = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Id == leagueId && l.IsActive);

        if (league == null)
        {
            return null;
        }

        return await MapToResponseAsync(league, userId);
    }

    public async Task<LeagueResponse?> GetLeagueByKeyAsync(string leagueKey, string? userId = null, string? anonymousAccessPassword = null)
    {
        if (string.IsNullOrWhiteSpace(leagueKey))
        {
            return null;
        }

        var normalizedKey = leagueKey.Trim().ToLowerInvariant();

        var league = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Key.ToLower() == normalizedKey && l.IsActive);

        if (league == null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(userId)
            && league.RequireAnonymousPassword
            && !IsAnonymousAccessAllowed(league, anonymousAccessPassword))
        {
            return null;
        }

        return await MapToResponseAsync(league, userId);
    }

    public async Task<bool> VerifyAnonymousAccessAsync(string leagueKey, string password)
    {
        if (string.IsNullOrWhiteSpace(leagueKey))
        {
            return false;
        }

        var normalizedKey = leagueKey.Trim().ToLowerInvariant();

        var league = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Key.ToLower() == normalizedKey && l.IsActive);

        if (league == null)
        {
            return false;
        }

        return IsAnonymousAccessAllowed(league, password);
    }

    public async Task<LeagueResponse> CreateLeagueAsync(CreateLeagueRequest request, string userId)
    {
        var normalizedKey = NormalizeLeagueKey(request.Key);
        if (string.IsNullOrWhiteSpace(normalizedKey))
        {
            throw new InvalidOperationException("League key is required.");
        }

        var normalizedName = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new InvalidOperationException("League name is required.");
        }

        // Check if league key already exists
        var existingLeague = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Key.ToLower() == normalizedKey);

        if (existingLeague != null)
        {
            throw new InvalidOperationException($"League with key '{normalizedKey}' already exists");
        }

        var normalizedDomain = NormalizeCustomDomain(request.CustomDomain);
        await EnsureCustomDomainAvailableAsync(normalizedDomain);
        var useCustomDomain = request.UseCustomDomain && !string.IsNullOrWhiteSpace(normalizedDomain);

        // Create the league
        var league = new Core.Entities.League
        {
            Id = _shortIdService.GenerateId(),
            Key = normalizedKey,
            Name = normalizedName,
            Description = request.Description?.Trim(),
            LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim(),
            WelcomeHeadline = string.IsNullOrWhiteSpace(request.WelcomeHeadline) ? null : request.WelcomeHeadline.Trim(),
            WelcomeSubhead = string.IsNullOrWhiteSpace(request.WelcomeSubhead) ? null : request.WelcomeSubhead.Trim(),
            EmptyStateMessage = string.IsNullOrWhiteSpace(request.EmptyStateMessage) ? null : request.EmptyStateMessage.Trim(),
            CommissionerName = string.IsNullOrWhiteSpace(request.CommissionerName) ? null : request.CommissionerName.Trim(),
            AnnouncementTitle = string.IsNullOrWhiteSpace(request.AnnouncementTitle) ? null : request.AnnouncementTitle.Trim(),
            AnnouncementBody = string.IsNullOrWhiteSpace(request.AnnouncementBody) ? null : request.AnnouncementBody.Trim(),
            CustomDomain = !string.IsNullOrWhiteSpace(normalizedDomain) ? normalizedDomain : null,
            UseCustomDomain = useCustomDomain,
            CustomDomainVerificationToken = !string.IsNullOrWhiteSpace(normalizedDomain)
                ? GenerateDomainVerificationToken()
                : null,
            CustomDomainVerifiedAt = null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsActive = true
        };

        _context.Leagues.Add(league);

        // Add the creator as a league admin
        var userLeague = new UserLeague
        {
            Id = _shortIdService.GenerateId(),
            UserId = userId,
            LeagueId = league.Id,
            IsLeagueAdmin = true,
            Role = LeagueMemberRole.Owner,
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsActive = true
        };

        _context.UserLeagues.Add(userLeague);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created league {LeagueKey} ({LeagueId}) by user {UserId}", league.Key, league.Id, userId);

        return await MapToResponseAsync(league, userId);
    }

    public async Task<LeagueResponse> UpdateLeagueAsync(string leagueId, UpdateLeagueRequest request, string userId)
    {
        var league = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Id == leagueId && l.IsActive);

        if (league == null)
        {
            throw new KeyNotFoundException($"League with ID {leagueId} not found");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Name))
        {
            league.Name = request.Name.Trim();
        }

        if (request.Description != null)
        {
            league.Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();
        }

        if (request.LogoUrl != null)
        {
            league.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl)
                ? null
                : request.LogoUrl.Trim();
        }

        if (request.WelcomeHeadline != null)
        {
            league.WelcomeHeadline = string.IsNullOrWhiteSpace(request.WelcomeHeadline)
                ? null
                : request.WelcomeHeadline.Trim();
        }

        if (request.WelcomeSubhead != null)
        {
            league.WelcomeSubhead = string.IsNullOrWhiteSpace(request.WelcomeSubhead)
                ? null
                : request.WelcomeSubhead.Trim();
        }

        if (request.EmptyStateMessage != null)
        {
            league.EmptyStateMessage = string.IsNullOrWhiteSpace(request.EmptyStateMessage)
                ? null
                : request.EmptyStateMessage.Trim();
        }

        if (request.CommissionerName != null)
        {
            league.CommissionerName = string.IsNullOrWhiteSpace(request.CommissionerName)
                ? null
                : request.CommissionerName.Trim();
        }

        if (request.AnnouncementTitle != null)
        {
            league.AnnouncementTitle = string.IsNullOrWhiteSpace(request.AnnouncementTitle)
                ? null
                : request.AnnouncementTitle.Trim();
        }

        if (request.AnnouncementBody != null)
        {
            league.AnnouncementBody = string.IsNullOrWhiteSpace(request.AnnouncementBody)
                ? null
                : request.AnnouncementBody.Trim();
        }

        if (request.CustomDomain != null)
        {
            var normalizedDomain = NormalizeCustomDomain(request.CustomDomain);
            if (!string.Equals(normalizedDomain, league.CustomDomain, StringComparison.OrdinalIgnoreCase))
            {
                await EnsureCustomDomainAvailableAsync(normalizedDomain, league.Id);
                league.CustomDomain = !string.IsNullOrWhiteSpace(normalizedDomain) ? normalizedDomain : null;
                league.CustomDomainVerificationToken = !string.IsNullOrWhiteSpace(normalizedDomain)
                    ? GenerateDomainVerificationToken()
                    : null;
                league.CustomDomainVerifiedAt = null;
            }
        }

        if (request.UseCustomDomain.HasValue)
        {
            league.UseCustomDomain = request.UseCustomDomain.Value && !string.IsNullOrWhiteSpace(league.CustomDomain);
        }

        if (request.ClearAnonymousAccessPassword == true)
        {
            league.AnonymousPasswordHash = null;
            league.AnonymousPasswordUpdatedAt = DateTime.UtcNow;
        }

        if (request.AnonymousAccessPassword != null)
        {
            var trimmedPassword = request.AnonymousAccessPassword.Trim();
            if (trimmedPassword.Length == 0)
            {
                league.AnonymousPasswordHash = null;
            }
            else
            {
                league.AnonymousPasswordHash = _passwordHasher.HashPassword(trimmedPassword);
            }

            league.AnonymousPasswordUpdatedAt = DateTime.UtcNow;
        }

        if (request.RequireAnonymousPassword.HasValue)
        {
            if (request.RequireAnonymousPassword.Value && string.IsNullOrEmpty(league.AnonymousPasswordHash))
            {
                throw new InvalidOperationException("Set an anonymous access password before enabling anonymous password protection.");
            }

            league.RequireAnonymousPassword = request.RequireAnonymousPassword.Value;
        }

        league.UpdatedAt = DateTime.UtcNow;
        league.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated league {LeagueId} by user {UserId}", leagueId, userId);

        return await MapToResponseAsync(league, userId);
    }

    public async Task<bool> DeleteLeagueAsync(string leagueId, string userId)
    {
        var league = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Id == leagueId && l.IsActive);

        if (league == null)
        {
            return false;
        }

        league.IsActive = false;
        league.UpdatedAt = DateTime.UtcNow;
        league.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted league {LeagueId} by user {UserId}", leagueId, userId);

        return true;
    }

    public async Task<LeagueResponse> VerifyCustomDomainAsync(string leagueId, string userId)
    {
        var league = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Id == leagueId && l.IsActive);

        if (league == null)
        {
            throw new KeyNotFoundException($"League with ID {leagueId} not found");
        }

        if (string.IsNullOrWhiteSpace(league.CustomDomain) || !league.UseCustomDomain)
        {
            throw new InvalidOperationException("Custom domain is not configured for this league.");
        }

        if (string.IsNullOrWhiteSpace(league.CustomDomainVerificationToken))
        {
            throw new InvalidOperationException("No custom domain verification token is available for this league.");
        }

        var domain = league.CustomDomain.Trim();
        var validationErrors = new List<string>();

        if (_verificationOptions.AllowedIps.Any())
        {
            var hostAddresses = await Dns.GetHostAddressesAsync(domain);
            if (hostAddresses == null || !hostAddresses.Any())
            {
                validationErrors.Add("Custom domain did not resolve to any IP addresses.");
            }
            else
            {
                var allowedAddresses = _verificationOptions.AllowedIps
                    .Select(ip => IPAddress.TryParse(ip, out var parsed) ? parsed : null)
                    .Where(ip => ip != null)
                    .Select(ip => ip!)
                    .ToList();

                if (!allowedAddresses.Any())
                {
                    validationErrors.Add("Custom domain verification is configured, but there are no allowed IP addresses.");
                }
                else if (!hostAddresses.Any(addr => allowedAddresses.Contains(addr)))
                {
                    validationErrors.Add("Custom domain DNS does not resolve to an allowed server IP.");
                }
            }
        }

        if (_verificationOptions.RequireTxtRecord)
        {
            var recordName = _verificationOptions.TxtRecordPrefix.TrimEnd('.') + "." + domain;
            var queryResult = await _lookupClient.QueryAsync(recordName, QueryType.TXT);
            var txtValues = queryResult.Answers
                .TxtRecords()
                .SelectMany(record => record.Text)
                .Select(text => text.Trim())
                .ToList();

            if (!txtValues.Contains(league.CustomDomainVerificationToken))
            {
                validationErrors.Add($"TXT record verification failed. Add a TXT record for {recordName} containing the verification token.");
            }
        }

        if (validationErrors.Any())
        {
            throw new InvalidOperationException(string.Join(" ", validationErrors));
        }

        league.CustomDomainVerifiedAt = DateTime.UtcNow;
        league.UpdatedAt = DateTime.UtcNow;
        league.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Verified custom domain {CustomDomain} for league {LeagueId} by user {UserId}", league.CustomDomain, leagueId, userId);

        return await MapToResponseAsync(league, userId);
    }

    public async Task<List<LeagueMemberResponse>> GetLeagueMembersAsync(string leagueId)
    {
        // Get all members (both active and inactive), not deleted
        var members = await _context.UserLeagues
            .IgnoreQueryFilters()
            .Include(ul => ul.User)
            .Where(ul => ul.LeagueId == leagueId && !ul.IsDeleted)
            .OrderBy(ul => ul.User.LastName)
            .ThenBy(ul => ul.User.FirstName)
            .ToListAsync();

        var responses = new List<LeagueMemberResponse>();

        foreach (var member in members)
        {
            // Try to find associated player/golfer
            var golfer = await _context.LeagueGolfers
                .IgnoreQueryFilters()
                .Include(lg => lg.Golfer)
                .FirstOrDefaultAsync(lg => lg.LeagueId == leagueId && lg.Golfer.UserId == member.UserId && lg.IsActive);

            responses.Add(new LeagueMemberResponse
            {
                UserId = member.UserId,
                Email = member.User.Email,
                FirstName = member.User.FirstName,
                LastName = member.User.LastName,
                IsLeagueAdmin = member.IsLeagueAdmin,
                Role = NormalizeRole(member),
                IsActive = member.IsActive,
                JoinedAt = member.JoinedAt,
                PlayerId = golfer?.GolferId,
                PlayerDisplayName = golfer?.Golfer?.DisplayName
            });
        }

        return responses;
    }

    public async Task<LeagueMemberResponse> AddLeagueMemberAsync(string leagueId, AddLeagueMemberRequest request, string currentUserId)
    {
        // Verify league exists
        var league = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Id == leagueId && l.IsActive);

        if (league == null)
        {
            throw new KeyNotFoundException($"League with ID {leagueId} not found");
        }

        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        // If user doesn't exist, create them
        if (user == null)
        {
            // Validate that FirstName and LastName are provided for new users
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            {
                throw new InvalidOperationException($"User with email {request.Email} not found. FirstName and LastName are required to create a new user.");
            }

            // Generate a random password for the new user
            var randomPassword = GenerateRandomPassword();

            user = new User
            {
                Id = _shortIdService.GenerateId(),
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(randomPassword),
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsGlobalAdmin = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId,
                IsActive = true
            };

            _context.Users.Add(user);

            _logger.LogInformation("Created new user {Email} ({UserId}) while adding to league {LeagueId}", user.Email, user.Id, leagueId);

            // TODO: Send invitation email with temporary password or password reset link
        }

        // Check if user is already a member
        var existingMembership = await _context.UserLeagues
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(ul => ul.UserId == user.Id && ul.LeagueId == leagueId);

        if (existingMembership != null)
        {
            if (existingMembership.IsActive)
            {
                throw new InvalidOperationException($"User {request.Email} is already a member of this league");
            }
            else
            {
                // Reactivate the membership
                existingMembership.IsActive = true;
                existingMembership.Role = NormalizeRequestedRole(request.Role, request.IsLeagueAdmin);
                existingMembership.IsLeagueAdmin = IsAdminRole(existingMembership.Role);
                existingMembership.UpdatedAt = DateTime.UtcNow;
                existingMembership.UpdatedBy = currentUserId;
            }
        }
        else
        {
            var role = NormalizeRequestedRole(request.Role, request.IsLeagueAdmin);

            // Create new membership
            var userLeague = new UserLeague
            {
                Id = _shortIdService.GenerateId(),
                UserId = user.Id,
                LeagueId = leagueId,
                IsLeagueAdmin = IsAdminRole(role),
                Role = role,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId,
                IsActive = true
            };

            _context.UserLeagues.Add(userLeague);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Added user {UserId} to league {LeagueId} by {CurrentUserId}", user.Id, leagueId, currentUserId);

        return new LeagueMemberResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsLeagueAdmin = IsAdminRole(NormalizeRequestedRole(request.Role, request.IsLeagueAdmin)),
            Role = NormalizeRequestedRole(request.Role, request.IsLeagueAdmin),
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generate a random password for new users
    /// </summary>
    private static string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
        var random = RandomNumberGenerator.Create();
        var bytes = new byte[16];
        random.GetBytes(bytes);

        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }

    public async Task<bool> RemoveLeagueMemberAsync(string leagueId, string userId, string currentUserId)
    {
        var membership = await _context.UserLeagues
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.LeagueId == leagueId && ul.IsActive);

        if (membership == null)
        {
            return false;
        }

        // Prevent removing the last admin
        var adminCount = await _context.UserLeagues
            .IgnoreQueryFilters()
            .CountAsync(ul => ul.LeagueId == leagueId && ul.IsLeagueAdmin && ul.IsActive);

        if (IsAdminRole(NormalizeRole(membership)) && adminCount <= 1)
        {
            throw new InvalidOperationException("Cannot remove the last admin from the league");
        }

        // Soft delete
        membership.IsActive = false;
        membership.UpdatedAt = DateTime.UtcNow;
        membership.UpdatedBy = currentUserId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed user {UserId} from league {LeagueId} by {CurrentUserId}", userId, leagueId, currentUserId);

        return true;
    }

    public async Task<LeagueMemberResponse> UpdateLeagueMemberAsync(string leagueId, string userId, UpdateLeagueMemberRequest request, string currentUserId)
    {
        var membership = await _context.UserLeagues
            .IgnoreQueryFilters()
            .Include(ul => ul.User)
            .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.LeagueId == leagueId && ul.IsActive);

        if (membership == null)
        {
            throw new KeyNotFoundException($"User {userId} is not a member of league {leagueId}");
        }

        // If demoting from admin, check that there's at least one other admin
        var currentRole = NormalizeRole(membership);
        var nextRole = request.Role ?? (request.IsLeagueAdmin.HasValue
            ? (request.IsLeagueAdmin.Value ? LeagueMemberRole.Admin : LeagueMemberRole.Member)
            : currentRole);

        if (IsAdminRole(currentRole) && !IsAdminRole(nextRole))
        {
            var adminCount = await _context.UserLeagues
                .IgnoreQueryFilters()
                .CountAsync(ul => ul.LeagueId == leagueId && ul.IsLeagueAdmin && ul.IsActive);

            if (adminCount <= 1)
            {
                throw new InvalidOperationException("Cannot demote the last admin of the league");
            }
        }

        if (request.Role.HasValue || request.IsLeagueAdmin.HasValue)
        {
            membership.Role = nextRole;
            membership.IsLeagueAdmin = IsAdminRole(nextRole);
        }

        if (request.IsActive.HasValue)
        {
            membership.IsActive = request.IsActive.Value;
        }

        membership.UpdatedAt = DateTime.UtcNow;
        membership.UpdatedBy = currentUserId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated user {UserId} role in league {LeagueId} by {CurrentUserId}", userId, leagueId, currentUserId);

        return new LeagueMemberResponse
        {
            UserId = membership.UserId,
            Email = membership.User.Email,
            FirstName = membership.User.FirstName,
            LastName = membership.User.LastName,
            IsLeagueAdmin = membership.IsLeagueAdmin,
            Role = NormalizeRole(membership),
            IsActive = membership.IsActive,
            JoinedAt = membership.JoinedAt
        };
    }

    private async Task<LeagueResponse> MapToResponseAsync(Core.Entities.League league, string? userId = null)
    {
        // Use IgnoreQueryFilters() for counts
        var memberCount = await _context.UserLeagues
            .IgnoreQueryFilters()
            .CountAsync(ul => ul.LeagueId == league.Id && ul.IsActive);

        var playerCount = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .CountAsync(lg => lg.LeagueId == league.Id && lg.IsActive);

        var seasonCount = await _context.Seasons
            .IgnoreQueryFilters()
            .CountAsync(s => s.LeagueId == league.Id && s.IsActive);

        bool isCurrentUserAdmin = false;
        if (!string.IsNullOrEmpty(userId))
        {
            isCurrentUserAdmin = await _context.UserLeagues
                .IgnoreQueryFilters()
                .AnyAsync(ul =>
                    ul.UserId == userId
                    && ul.LeagueId == league.Id
                    && ul.IsActive
                    && (ul.Role == LeagueMemberRole.Owner || ul.Role == LeagueMemberRole.Admin));
        }

        return new LeagueResponse
        {
            Id = league.Id,
            Key = league.Key,
            Name = league.Name,
            Description = league.Description,
            LogoUrl = league.LogoUrl,
            WelcomeHeadline = league.WelcomeHeadline,
            WelcomeSubhead = league.WelcomeSubhead,
            EmptyStateMessage = league.EmptyStateMessage,
            CommissionerName = league.CommissionerName,
            AnnouncementTitle = league.AnnouncementTitle,
            AnnouncementBody = league.AnnouncementBody,
            ActiveSeasonId = league.ActiveSeasonId,
            MemberCount = memberCount,
            PlayerCount = playerCount,
            SeasonCount = seasonCount,
            IsCurrentUserAdmin = isCurrentUserAdmin,
            CustomDomain = league.CustomDomain,
            UseCustomDomain = league.UseCustomDomain,
            CustomDomainVerificationToken = isCurrentUserAdmin ? league.CustomDomainVerificationToken : null,
            CustomDomainVerifiedAt = league.CustomDomainVerifiedAt,
            RequireAnonymousPassword = league.RequireAnonymousPassword,
            HasAnonymousPassword = !string.IsNullOrEmpty(league.AnonymousPasswordHash),
            AnonymousPasswordUpdatedAt = league.AnonymousPasswordUpdatedAt,
            CreatedAt = league.CreatedAt,
            UpdatedAt = league.UpdatedAt
        };
    }

    private bool IsAnonymousAccessAllowed(Core.Entities.League league, string? password)
    {
        if (!league.RequireAnonymousPassword)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(league.AnonymousPasswordHash))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        return _passwordHasher.VerifyPassword(password.Trim(), league.AnonymousPasswordHash);
    }

    private static string GenerateDomainVerificationToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    private static string NormalizeLeagueKey(string? key)
    {
        return key?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    private static string? NormalizeCustomDomain(string? domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return null;
        }

        return domain.Trim().TrimEnd('.').ToLowerInvariant();
    }

    private async Task EnsureCustomDomainAvailableAsync(string? customDomain, string? leagueIdToExclude = null)
    {
        if (string.IsNullOrWhiteSpace(customDomain))
        {
            return;
        }

        var customDomainInUse = await _context.Leagues
            .AnyAsync(l =>
                l.IsActive
                && l.CustomDomain != null
                && l.CustomDomain.ToLower() == customDomain
                && (leagueIdToExclude == null || l.Id != leagueIdToExclude));

        if (customDomainInUse)
        {
            throw new InvalidOperationException($"Custom domain '{customDomain}' is already in use by another league.");
        }
    }

    private static LeagueMemberRole NormalizeRequestedRole(LeagueMemberRole role, bool isLeagueAdmin)
    {
        if (isLeagueAdmin && role is LeagueMemberRole.Member or LeagueMemberRole.Viewer)
        {
            return LeagueMemberRole.Admin;
        }

        return role;
    }

    private static LeagueMemberRole NormalizeRole(UserLeague membership)
    {
        return membership.Role == LeagueMemberRole.Member && membership.IsLeagueAdmin
            ? LeagueMemberRole.Admin
            : membership.Role;
    }

    private static bool IsAdminRole(LeagueMemberRole role)
    {
        return role is LeagueMemberRole.Owner or LeagueMemberRole.Admin;
    }
}
