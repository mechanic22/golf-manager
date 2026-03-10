using GolfManager.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GolfManager.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot _databaseRoot = new InMemoryDatabaseRoot();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to "Testing" so Program.cs skips SQL Server registration
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<GolfManagerDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using in-memory database for testing
            // Use shared database root so all scopes see the same data
            services.AddDbContext<GolfManagerDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase", _databaseRoot);
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<GolfManagerDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();
        });
    }
}

