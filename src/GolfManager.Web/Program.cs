using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GolfManager.Web;
using GolfManager.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to point to the API
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "https://localhost:7001";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILeagueService, LeagueService>();

var host = builder.Build();

// Initialize auth service
var authService = host.Services.GetRequiredService<IAuthService>();
if (authService is AuthService authServiceImpl)
{
    await authServiceImpl.InitializeAsync();
}

await host.RunAsync();
