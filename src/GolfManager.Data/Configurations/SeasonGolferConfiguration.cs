using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for SeasonGolfer
/// </summary>
public class SeasonGolferConfiguration : IEntityTypeConfiguration<SeasonGolfer>
{
    public void Configure(EntityTypeBuilder<SeasonGolfer> builder)
    {
        builder.ToTable("SeasonGolfers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SeasonId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LeagueId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LeagueGolferId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.GolferId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.TeamId)
            .HasMaxLength(50);

        // Unique constraint: LeagueGolfer can only participate once per season
        builder.HasIndex(x => new { x.SeasonId, x.LeagueGolferId })
            .IsUnique();

        builder.Property(x => x.JoinedAt)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Navigation: SeasonGolfer -> Golfer (many-to-one)
        builder.HasOne(x => x.Golfer)
            .WithMany()
            .HasForeignKey(x => x.GolferId)
            .OnDelete(DeleteBehavior.Restrict);

        // Navigation: SeasonGolfer -> Team (many-to-one, optional)
        builder.HasOne(x => x.Team)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

