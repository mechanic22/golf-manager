using GolfManager.Core.Common;
using GolfManager.Core.Entities;
using GolfManager.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace GolfManager.Data;

/// <summary>
/// Main database context for GolfManager
/// </summary>
public class GolfManagerDbContext : DbContext
{
    private readonly ITenantService? _tenantService;

    public GolfManagerDbContext(
        DbContextOptions<GolfManagerDbContext> options,
        ITenantService? tenantService = null)
        : base(options)
    {
        _tenantService = tenantService;
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
    public DbSet<SeasonSettings> SeasonSettings { get; set; } = null!;
    public DbSet<SeasonGolfer> SeasonGolfers { get; set; } = null!;
    public DbSet<SeasonTeam> SeasonTeams { get; set; } = null!;
    public DbSet<SeasonEvent> SeasonEvents { get; set; } = null!;

    // One-Time Events
    public DbSet<OneTimeEvent> OneTimeEvents { get; set; } = null!;
    public DbSet<OneTimeEventTeam> OneTimeEventTeams { get; set; } = null!;
    public DbSet<OneTimeEventPlayer> OneTimeEventPlayers { get; set; } = null!;

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
    /// Automatically filters all ITenantEntity queries by the current LeagueId
    /// </summary>
    private void ConfigureGlobalFilters(ModelBuilder modelBuilder)
    {
        // Get the current league ID from the tenant service
        var currentLeagueId = _tenantService?.GetCurrentLeagueId();

        // Apply query filter to all entities that implement ITenantEntity
        // This ensures that queries automatically filter by the current league
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Create a filter expression: entity => entity.LeagueId == currentLeagueId
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ITenantEntity.LeagueId));
                var leagueIdValue = System.Linq.Expressions.Expression.Constant(currentLeagueId);
                var comparison = System.Linq.Expressions.Expression.Equal(property, leagueIdValue);
                var lambda = System.Linq.Expressions.Expression.Lambda(comparison, parameter);

                // Apply the filter
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
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

