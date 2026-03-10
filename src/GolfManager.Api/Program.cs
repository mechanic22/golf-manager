using System.Text;
using GolfManager.Api.Authorization;
using GolfManager.Api.Authorization.Handlers;
using GolfManager.Api.Authorization.Requirements;
using GolfManager.Api.Middleware;
using GolfManager.Data;
using GolfManager.Data.Services;
using GolfManager.Services.Auth;
using GolfManager.Services.Event;
using GolfManager.Services.League;
using GolfManager.Services.Player;
using GolfManager.Services.Round;
using GolfManager.Services.Season;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add DbContext - conditionally register based on environment
// This allows tests to override with InMemory provider
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<GolfManagerDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Add tenant service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();

// Add authentication services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add HttpContextAccessor (required for authorization handlers to access route data)
builder.Services.AddHttpContextAccessor();

// Add league authorization service
builder.Services.AddScoped<ILeagueAuthorizationService, LeagueAuthorizationService>();

// Add league service
builder.Services.AddScoped<ILeagueService, LeagueService>();

// Add player service
builder.Services.AddScoped<IPlayerService, PlayerService>();

// Add season service
builder.Services.AddScoped<ISeasonService, SeasonService>();

// Add event service
builder.Services.AddScoped<IEventService, EventService>();

// Add round service
builder.Services.AddScoped<IRoundService, RoundService>();

// Add authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, LeagueMemberHandler>();
builder.Services.AddScoped<IAuthorizationHandler, LeagueAdminHandler>();

// Configure JWT authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GolfManager";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GolfManager";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add global exception handling middleware (must be first)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

// Add CORS
app.UseCors("AllowAll");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();

// Make the implicit Program class public for integration tests
public partial class Program { }
