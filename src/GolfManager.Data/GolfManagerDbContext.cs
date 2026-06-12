using System.Linq.Expressions;
using GolfManager.Core.Common;
using GolfManager.Core.Entities;
using GolfManager.Core.Services;
using GolfManager.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace GolfManager.Data;

/// <summary>
/// Main database context for GolfManager
/// </summary>
public class GolfManagerDbContext : DbContext
{
    private readonly ITenantService? _tenantService;
    private readonly ICurrentUserService? _currentUserService;

    public GolfManagerDbContext(
        DbContextOptions<GolfManagerDbContext> options,
        ITenantService? tenantService = null,
        ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    // User & Authentication
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    // Golfer (Global)
    public DbSet<Golfer> Golfers { get; set; } = null!;
    public DbSet<GolferClub> GolferClubs { get; set; } = null!;
    public DbSet<HandicapHistory> HandicapHistories { get; set; } = null!;

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
    public DbSet<SeasonEventMatch> SeasonEventMatches { get; set; } = null!;

    // One-Time Events
    public DbSet<OneTimeEvent> OneTimeEvents { get; set; } = null!;
    public DbSet<OneTimeEventTeam> OneTimeEventTeams { get; set; } = null!;
    public DbSet<OneTimeEventPlayer> OneTimeEventPlayers { get; set; } = null!;

    // Course Data (Global)
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Tee> Tees { get; set; } = null!;
    public DbSet<Hole> Holes { get; set; } = null!;
    public DbSet<HoleTee> HoleTees { get; set; } = null!;

    // Event Scores (persisted for locked events)
    public DbSet<SeasonEventPlayerScore> SeasonEventPlayerScores { get; set; } = null!;
    public DbSet<SeasonEventMatchScore> SeasonEventMatchScores { get; set; } = null!;

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
    /// Configure global query filters for soft-delete and multi-tenancy.
    /// All BaseEntity types get !IsDeleted; ITenantEntity types additionally filter by LeagueId.
    /// EF Core allows only one HasQueryFilter per entity, so both conditions are combined here.
    /// Callers that need to see deleted or cross-tenant data must use .IgnoreQueryFilters().
    /// </summary>
    private void ConfigureGlobalFilters(ModelBuilder modelBuilder)
    {
        var currentLeagueId = _tenantService?.GetCurrentLeagueId();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType.IsAbstract) continue;

            var isBase = typeof(BaseEntity).IsAssignableFrom(clrType);
            var isTenant = typeof(ITenantEntity).IsAssignableFrom(clrType);

            if (!isBase && !isTenant) continue;

            var parameter = Expression.Parameter(clrType, "e");
            Expression? filter = null;

            if (isBase)
            {
                var isDeletedProp = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                filter = Expression.Not(isDeletedProp);
            }

            if (isTenant)
            {
                var leagueIdProp = Expression.Property(parameter, nameof(ITenantEntity.LeagueId));
                var leagueIdConst = Expression.Constant(currentLeagueId);
                var tenantFilter = Expression.Equal(leagueIdProp, leagueIdConst);
                filter = filter is null ? tenantFilter : Expression.AndAlso(filter, tenantFilter);
            }

            if (filter is not null)
                modelBuilder.Entity(clrType).HasQueryFilter(Expression.Lambda(filter, parameter));
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
        var userId = _currentUserService?.UserId ?? "system";

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = userId;
            }
        }
    }
}
