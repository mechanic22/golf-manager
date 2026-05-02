using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GolfManager.Web;
using GolfManager.Web.Services;

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
builder.Services.AddScoped<GolfManager.Web.Services.IHandicapService, GolfManager.Web.Services.HandicapService>();

// Configure HttpClient with lazy authentication handler
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "https://localhost:7012";
builder.Services.AddScoped(sp =>
{
    // Create league context handler (adds X-League-Context header)
    var leagueContextHandler = sp.GetRequiredService<LeagueContextHandler>();
    leagueContextHandler.InnerHandler = new HttpClientHandler();

    // Create auth handler (adds Authorization header)
    var authHandler = new AuthenticatedHttpClientHandler(() => sp.GetRequiredService<IAuthService>())
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
