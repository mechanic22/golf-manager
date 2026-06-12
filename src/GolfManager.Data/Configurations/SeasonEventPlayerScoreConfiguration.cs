using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

public class SeasonEventPlayerScoreConfiguration : IEntityTypeConfiguration<SeasonEventPlayerScore>
{
    public void Configure(EntityTypeBuilder<SeasonEventPlayerScore> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.Property(x => x.SeasonEventId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(x => x.SeasonGolferId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(x => x.LeagueId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.TeamId)
            .HasMaxLength(36);

        builder.Property(x => x.TeamName)
            .HasMaxLength(256);

        builder.Property(x => x.RawScore);
        builder.Property(x => x.Handicap);
        builder.Property(x => x.NetScore);
        builder.Property(x => x.EventPoints);
        builder.Property(x => x.IsMissing).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.MissScore);

        // Foreign keys
        builder.HasOne(x => x.SeasonEvent)
            .WithMany(se => se.PlayerScores)
            .HasForeignKey(x => x.SeasonEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SeasonGolfer)
            .WithMany()
            .HasForeignKey(x => x.SeasonGolferId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.SeasonEventId, x.LeagueId });
        builder.HasIndex(x => new { x.SeasonGolferId, x.LeagueId });
        builder.HasIndex(x => new { x.TeamId, x.LeagueId });
    }
}
