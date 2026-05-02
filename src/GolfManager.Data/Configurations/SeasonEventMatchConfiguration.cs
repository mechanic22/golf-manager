using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for SeasonEventMatch
/// </summary>
public class SeasonEventMatchConfiguration : IEntityTypeConfiguration<SeasonEventMatch>
{
    public void Configure(EntityTypeBuilder<SeasonEventMatch> builder)
    {
        builder.ToTable("SeasonEventMatches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SeasonEventId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LeagueId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ScorecardId)
            .HasMaxLength(50);

        builder.Property(x => x.HomeTeamId)
            .HasMaxLength(50);

        builder.Property(x => x.AwayTeamId)
            .HasMaxLength(50);

        builder.Property(x => x.IsComplete)
            .IsRequired()
            .HasDefaultValue(false);

        // Navigation: SeasonEventMatch -> SeasonEvent (many-to-one)
        builder.HasOne(x => x.SeasonEvent)
            .WithMany()
            .HasForeignKey(x => x.SeasonEventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: SeasonEventMatch -> Scorecard (many-to-one, optional)
        builder.HasOne(x => x.Scorecard)
            .WithMany()
            .HasForeignKey(x => x.ScorecardId)
            .OnDelete(DeleteBehavior.SetNull);

        // Navigation: SeasonEventMatch -> HomeTeam (many-to-one, optional)
        builder.HasOne(x => x.HomeTeam)
            .WithMany()
            .HasForeignKey(x => x.HomeTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        // Navigation: SeasonEventMatch -> AwayTeam (many-to-one, optional)
        builder.HasOne(x => x.AwayTeam)
            .WithMany()
            .HasForeignKey(x => x.AwayTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.SeasonEventId);
        builder.HasIndex(x => x.LeagueId);
    }
}
