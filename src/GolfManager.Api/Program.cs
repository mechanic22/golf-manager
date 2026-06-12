using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using GolfManager.Api.Authorization;
using GolfManager.Api.Authorization.Handlers;
using GolfManager.Api.Authorization.Requirements;
using GolfManager.Api.BackgroundServices;
using GolfManager.Api.Middleware;
using GolfManager.Data;
using GolfManager.Data.Seed;
using GolfManager.Data.Services;
using GolfManager.Core.Configuration;
using GolfManager.Core.Services;
using GolfManager.Services.Auth;
using GolfManager.Services.Common;
using GolfManager.Services.Course;
using GolfManager.Services.Event;
using GolfManager.Services.Handicap;
using GolfManager.Services.League;
using GolfManager.Services.OneTimeEvent;
using GolfManager.Services.Player;
using GolfManager.Services.Round;
using GolfManager.Services.Season;
using GolfManager.Services.Simulation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// CORS for external API clients (Swagger UI, Postman, etc.)
// The hosted Blazor WASM app is same-origin and doesn't need CORS.
var allowedOrigins = builder.Configuration["AllowedOrigins"]
    ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        else
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

// Add DbContext - conditionally register based on environment
// This allows tests to override with InMemory provider
if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";

    builder.Services.AddDbContext<GolfManagerDbContext>(options =>
    {
        if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        }
        else // Default to SQLite
        {
            options.UseSqlite(connectionString);
        }

        // Enable detailed errors in development
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });
}

// Add tenant service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();

// Add short ID generator service
builder.Services.AddSingleton<IShortIdService, ShortIdService>();

// Add authentication services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add league authorization service
builder.Services.AddScoped<ILeagueAuthorizationService, LeagueAuthorizationService>();

// Add league service
builder.Services.AddScoped<ILeagueService, LeagueService>();

// Add custom domain verification config
builder.Services.Configure<CustomDomainVerificationOptions>(builder.Configuration.GetSection("CustomDomainVerification"));

// Add player service
builder.Services.AddScoped<IPlayerService, PlayerService>();

// Add season service
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<ISeasonSettingsService, SeasonSettingsService>();
builder.Services.AddScoped<ISeasonSimulationService, SeasonSimulationService>();

// Add event service
builder.Services.AddScoped<IEventScoringService, EventScoringService>();
builder.Services.AddScoped<IEventService, EventService>();

// Add season scoring background queues and workers
builder.Services.AddSingleton<IHandicapRecalculationQueue, HandicapRecalculationQueue>();
builder.Services.AddSingleton<ISeasonPointsRecalculationQueue, SeasonPointsRecalculationQueue>();
builder.Services.AddHostedService<HandicapRecalculationWorker>();
builder.Services.AddHostedService<SeasonPointsRecalculationWorker>();

// Add round service
builder.Services.AddScoped<IRoundService, RoundService>();

// Add course service
builder.Services.AddScoped<ICourseService, CourseService>();

// Add handicap service
builder.Services.AddScoped<IHandicapService, HandicapService>();

// Add one-time event services
builder.Services.AddScoped<IOneTimeEventService, OneTimeEventService>();
builder.Services.AddScoped<ITeamRegistrationService, TeamRegistrationService>();

// Add authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, LeagueMemberHandler>();
builder.Services.AddScoped<IAuthorizationHandler, LeagueAdminHandler>();

// Configure authentication. Browser requests use the local auth cookie; API/tooling
// requests can continue to send bearer JWTs.
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GolfManager";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GolfManager";
const string SmartAuthScheme = "SmartAuth";
const string LocalCookieScheme = "GolfManager.LocalCookie";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = SmartAuthScheme;
    options.DefaultChallengeScheme = SmartAuthScheme;
    options.DefaultSignInScheme = LocalCookieScheme;
})
.AddPolicyScheme(SmartAuthScheme, "Cookie or Bearer", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        return authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? JwtBearerDefaults.AuthenticationScheme
            : LocalCookieScheme;
    };
})
.AddCookie(LocalCookieScheme, options =>
{
    options.Cookie.Name = "GolfManagerAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);

    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    };
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            if(logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(context.Exception, "JWT Authentication failed: {Message}", context.Exception.Message);
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            if(logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("JWT Token validated successfully for user: {UserId}",
                    context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            }
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            if(logger.IsEnabled(LogLevel.Information))
            {
                var hasAuth = context.Request.Headers.ContainsKey("Authorization");
                logger.LogInformation("JWT OnMessageReceived - Has Authorization header: {HasAuth}", hasAuth);
                if (hasAuth)
                {
                    var authHeader = context.Request.Headers.Authorization.ToString();
                    logger.LogInformation("Authorization header: {AuthHeader}",
                        authHeader.Length > 30 ? string.Concat(authHeader.AsSpan(0, 30), "...") : authHeader);
                }
            }
            return Task.CompletedTask;
        }
    };
});

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    // Global admin policy
    options.AddPolicy(AuthorizationConstants.Policies.GlobalAdmin, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AuthorizationConstants.Claims.IsGlobalAdmin, "true");
    });

    // League member policy - user must be a member of the league
    options.AddPolicy(AuthorizationConstants.Policies.LeagueMember, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new LeagueMemberRequirement());
    });

    // League admin policy - user must be an admin of the league
    options.AddPolicy(AuthorizationConstants.Policies.LeagueAdmin, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new LeagueAdminRequirement());
    });

    // Guest league viewer — anonymous guest with a scoped league session cookie
    options.AddPolicy(AuthorizationConstants.Policies.GuestLeagueViewer, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AuthorizationConstants.Claims.IsGuest, "true");
    });

    // League or global admin policy
    options.AddPolicy(AuthorizationConstants.Policies.LeagueOrGlobalAdmin, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            // Check if user is global admin
            var isGlobalAdmin = context.User.FindFirst(AuthorizationConstants.Claims.IsGlobalAdmin)?.Value == "true";
            if (isGlobalAdmin)
            {
                return true;
            }

            // Otherwise, check league admin requirement
            // This will be handled by the LeagueAdminHandler
            return false;
        });
        policy.AddRequirements(new LeagueAdminRequirement());
    });
});

var app = builder.Build();

// Seed the database in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<GolfManagerDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbSeeder>>();
    var shortIdService = scope.ServiceProvider.GetRequiredService<IShortIdService>();

    // Apply schema changes in development; fall back for legacy EnsureCreated databases.
    try
    {
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Migration failed during development startup. Falling back to EnsureCreated and compatibility checks.");
        await context.Database.EnsureCreatedAsync();
    }

    // Seed demo data
    var seeder = new DbSeeder(context, logger, shortIdService);
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add global exception handling middleware (must be first)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Serve Blazor WebAssembly static files (framework + app files)
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// Add CORS (for external API clients)
app.UseCors("WebClient");

// Add authentication middleware
app.UseAuthentication();

// Add league context validation middleware (after auth, validates user membership)
app.UseLeagueContext();

// Add authorization middleware
app.UseAuthorization();

// Map health checks, controllers, and SPA fallback
app.MapHealthChecks("/health");
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

// Make the implicit Program class public for integration tests
public partial class Program { }
