using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for LeagueGolfer
/// </summary>
public class LeagueGolferConfiguration : IEntityTypeConfiguration<LeagueGolfer>
{
    public void Configure(EntityTypeBuilder<LeagueGolfer> builder)
    {
        builder.ToTable("LeagueGolfers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.GolferId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LeagueId)
            .IsRequired()
            .HasMaxLength(50);

        // Unique constraint: Golfer can only have one profile per league
        builder.HasIndex(x => new { x.GolferId, x.LeagueId })
            .IsUnique();

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Nickname)
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.JoinedAt)
            .IsRequired();

        // Navigation: LeagueGolfer -> SeasonGolfers (one-to-many)
        builder.HasMany(x => x.SeasonGolfers)
            .WithOne(x => x.LeagueGolfer)
            .HasForeignKey(x => x.LeagueGolferId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: LeagueGolfer -> Rounds (one-to-many)
        builder.HasMany(x => x.Rounds)
            .WithOne(x => x.LeagueGolfer)
            .HasForeignKey(x => x.LeagueGolferId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

