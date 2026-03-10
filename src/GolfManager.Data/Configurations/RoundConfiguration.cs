using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for Round
/// </summary>
public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.ToTable("Rounds");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.GolferId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LeagueGolferId)
            .HasMaxLength(50);

        builder.Property(x => x.LeagueId)
            .HasMaxLength(50);

        builder.Property(x => x.CourseId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.TeeId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.RoundDate)
            .IsRequired();

        builder.Property(x => x.HolesPlayed)
            .IsRequired();

        builder.Property(x => x.IsComplete)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Navigation: Round -> LeagueGolfer (many-to-one, optional)
        builder.HasOne(x => x.LeagueGolfer)
            .WithMany(x => x.Rounds)
            .HasForeignKey(x => x.LeagueGolferId)
            .OnDelete(DeleteBehavior.Restrict);

        // Navigation: Round -> Scorecard (one-to-one, optional)
        builder.HasOne(x => x.Scorecard)
            .WithOne(x => x.Round)
            .HasForeignKey<Scorecard>(x => x.RoundId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: Round -> RoundHoles (one-to-many)
        builder.HasMany(x => x.Holes)
            .WithOne(x => x.Round)
            .HasForeignKey(x => x.RoundId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

