using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

public class SeasonEventMatchScoreConfiguration : IEntityTypeConfiguration<SeasonEventMatchScore>
{
    public void Configure(EntityTypeBuilder<SeasonEventMatchScore> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.Property(x => x.SeasonEventId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(x => x.SeasonEventMatchId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(x => x.LeagueId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(x => x.HomeTeamId)
            .HasMaxLength(36);

        builder.Property(x => x.HomeTeamName)
            .HasMaxLength(256);

        builder.Property(x => x.HomePoints);

        builder.Property(x => x.AwayTeamId)
            .HasMaxLength(36);

        builder.Property(x => x.AwayTeamName)
            .HasMaxLength(256);

        builder.Property(x => x.AwayPoints);

        builder.Property(x => x.IsComplete)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.StartingHole);
        builder.Property(x => x.StartingFlight);

        // Foreign keys
        builder.HasOne(x => x.SeasonEvent)
            .WithMany(se => se.MatchScores)
            .HasForeignKey(x => x.SeasonEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SeasonEventMatch)
            .WithMany()
            .HasForeignKey(x => x.SeasonEventMatchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.SeasonEventId, x.LeagueId });
        builder.HasIndex(x => new { x.SeasonEventMatchId, x.LeagueId });
    }
}
