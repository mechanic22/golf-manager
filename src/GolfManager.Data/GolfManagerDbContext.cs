using GolfManager.Core.Common;
using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GolfManager.Data;

/// <summary>
/// Main database context for GolfManager
/// </summary>
public class GolfManagerDbContext : DbContext
{
    public GolfManagerDbContext(DbContextOptions<GolfManagerDbContext> options)
        : base(options)
    {
    }

    // User & Authentication
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    // Golfer (Global)
    public DbSet<Golfer> Golfers { get; set; } = null!;
    public DbSet<GolferClub> GolferClubs { get; set; } = null!;

    // Multi-Tenancy
    public DbSet<League> Leagues { get; set; } = null!;
    public DbSet<UserLeague> UserLeagues { get; set; } = null!;
    public DbSet<LeagueGolfer> LeagueGolfers { get; set; } = null!;

    // Season Management
    public DbSet<Season> Seasons { get; set; } = null!;
    public DbSet<SeasonGolfer> SeasonGolfers { get; set; } = null!;
    public DbSet<SeasonTeam> SeasonTeams { get; set; } = null!;
    public DbSet<SeasonEvent> SeasonEvents { get; set; } = null!;

    // Course Data (Global)
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Tee> Tees { get; set; } = null!;
    public DbSet<Hole> Holes { get; set; } = null!;
    public DbSet<HoleTee> HoleTees { get; set; } = null!;

    // Scoring
    public DbSet<Round> Rounds { get; set; } = null!;
    public DbSet<RoundHole> RoundHoles { get; set; } = null!;
    public DbSet<Scorecard> Scorecards { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GolfManagerDbContext).Assembly);

        // Configure global query filters for multi-tenancy
        ConfigureGlobalFilters(modelBuilder);
    }

    /// <summary>
    /// Configure global query filters for tenant isolation
    /// </summary>
    private void ConfigureGlobalFilters(ModelBuilder modelBuilder)
    {
        // Note: In a real implementation, you would inject the current tenant ID
        // and apply filters dynamically. For now, we're just setting up the structure.
        
        // Example of how tenant filtering would work:
        // modelBuilder.Entity<Season>().HasQueryFilter(e => e.LeagueId == _currentTenantId);
        
        // For now, we'll configure this in the entity configurations
    }

    /// <summary>
    /// Override SaveChanges to automatically set audit fields
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically set audit fields
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Automatically update CreatedAt, UpdatedAt, CreatedBy, UpdatedBy fields
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                // TODO: Set CreatedBy from current user context
                // entry.Entity.CreatedBy = _currentUserService.UserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                // TODO: Set UpdatedBy from current user context
                // entry.Entity.UpdatedBy = _currentUserService.UserId;
            }
        }
    }
}

