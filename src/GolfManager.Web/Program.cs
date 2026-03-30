using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GolfManager.Web;
using GolfManager.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

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

// Register seed data service as singleton (same data for all users)
builder.Services.AddSingleton<SeedDataService>();

// Configure HttpClient with lazy authentication handler
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "https://localhost:7012";
builder.Services.AddScoped(sp =>
{
    // Create handler with lazy factory to avoid early service resolution
    var handler = new AuthenticatedHttpClientHandler(() => sp.GetRequiredService<IAuthService>())
    {
        InnerHandler = new HttpClientHandler()
    };

    var httpClient = new HttpClient(handler)
    {
        BaseAddress = new Uri(apiBaseAddress)
    };

    return httpClient;
});

await builder.Build().RunAsync();
