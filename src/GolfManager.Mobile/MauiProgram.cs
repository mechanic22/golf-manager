using GolfManager.Mobile.Configuration;
using GolfManager.Mobile.Services;
using GolfManager.Mobile.ViewModels;
using GolfManager.Mobile.Views;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GolfManager.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configuration
        var settings = LoadAppSettings();
        builder.Services.AddSingleton(settings);

        // HttpClient — shared singleton with base address from config
        builder.Services.AddSingleton(sp =>
        {
            var cfg = sp.GetRequiredService<AppSettings>();
#if DEBUG
            // Trust self-signed dev certs on Simulator/Emulator
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            return new HttpClient(handler) { BaseAddress = new Uri(cfg.ApiBaseUrl) };
#else
            return new HttpClient { BaseAddress = new Uri(cfg.ApiBaseUrl) };
#endif
        });

        // Singleton services (stateful / long-lived)
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IGpsService, GpsService>();
        builder.Services.AddSingleton<DistanceService>();

        // Transient services (stateless API wrappers)
        builder.Services.AddTransient<IApiService, ApiService>();
        builder.Services.AddTransient<ILeagueService, LeagueService>();
        builder.Services.AddTransient<IEventService, EventService>();
        builder.Services.AddTransient<ICourseService, CourseService>();
        builder.Services.AddTransient<IRoundService, RoundService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LeagueSelectViewModel>();
        builder.Services.AddTransient<WeekSelectViewModel>();
        builder.Services.AddTransient<HoleViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LeagueSelectPage>();
        builder.Services.AddTransient<WeekSelectPage>();
        builder.Services.AddTransient<HolePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Prevent Mono/iOS from crashing on unobserved task exceptions
        TaskScheduler.UnobservedTaskException += (_, e) => e.SetObserved();

        return builder.Build();
    }

    private static AppSettings LoadAppSettings()
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<AppSettings>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }
}
