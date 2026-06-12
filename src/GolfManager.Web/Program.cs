using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GolfManager.Web;
using GolfManager.Web.Infrastructure;
using GolfManager.Web.Features.Auth;
using GolfManager.Web.Features.Dashboard;
using GolfManager.Web.Features.League;
using GolfManager.Web.Features.Season;
using GolfManager.Web.Features.Events;
using GolfManager.Web.Features.Profile;
using GolfManager.Web.Features.Admin;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register AppState (singleton for global state)
builder.Services.AddSingleton<AppState>();

// Register LeagueContextHandler
builder.Services.AddTransient<LeagueContextHandler>();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<ILeagueService, LeagueService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<ISeasonSettingsService, SeasonSettingsService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IRoundService, RoundService>();
builder.Services.AddScoped<IOneTimeEventService, OneTimeEventService>();
builder.Services.AddScoped<IHandicapService, HandicapService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// In hosted mode the API and Web share the same origin, so use the host's base address.
// ApiBaseAddress in appsettings can still override this for standalone/dev scenarios.
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(sp =>
{
    // Create league context handler (adds X-League-Context header)
    var leagueContextHandler = sp.GetRequiredService<LeagueContextHandler>();
    leagueContextHandler.InnerHandler = new HttpClientHandler();

    // Create auth handler (includes local auth cookie)
    var authHandler = new AuthenticatedHttpClientHandler
    {
        InnerHandler = leagueContextHandler // Chain handlers
    };

    var httpClient = new HttpClient(authHandler)
    {
        BaseAddress = new Uri(apiBaseAddress)
    };

    return httpClient;
});

await builder.Build().RunAsync();
