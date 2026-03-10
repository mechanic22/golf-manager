using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for League
/// </summary>
public class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.ToTable("Leagues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Key)
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.CustomDomain)
            .HasMaxLength(200);

        builder.HasIndex(x => x.CustomDomain)
            .IsUnique()
            .HasFilter("[CustomDomain] IS NOT NULL");

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Navigation: League -> UserLeagues (one-to-many)
        builder.HasMany(x => x.UserLeagues)
            .WithOne(x => x.League)
            .HasForeignKey(x => x.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: League -> LeagueGolfers (one-to-many)
        builder.HasMany(x => x.LeagueGolfers)
            .WithOne(x => x.League)
            .HasForeignKey(x => x.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: League -> Seasons (one-to-many)
        builder.HasMany(x => x.Seasons)
            .WithOne(x => x.League)
            .HasForeignKey(x => x.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

