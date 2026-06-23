using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for HandicapHistory
/// </summary>
public class HandicapHistoryConfiguration : IEntityTypeConfiguration<HandicapHistory>
{
    public void Configure(EntityTypeBuilder<HandicapHistory> builder)
    {
        builder.ToTable("HandicapHistories");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(h => h.GolferId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(h => h.LeagueId)
            .HasMaxLength(50);

        builder.Property(h => h.SeasonId)
            .HasMaxLength(50);

        builder.Property(h => h.HandicapIndex)
            .IsRequired();

        builder.Property(h => h.EffectiveDate)
            .IsRequired();

        builder.Property(h => h.CalculationMethod)
            .HasMaxLength(50);

        builder.Property(h => h.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(h => h.Golfer)
            .WithMany()
            .HasForeignKey(h => h.GolferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.League)
            .WithMany()
            .HasForeignKey(h => h.LeagueId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(h => h.Season)
            .WithMany()
            .HasForeignKey(h => h.SeasonId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for efficient querying
        builder.HasIndex(h => new { h.GolferId, h.EffectiveDate })
            .HasDatabaseName("IX_HandicapHistory_Golfer_Date")
            .IsDescending(false, true); // Golfer ASC, EffectiveDate DESC

        builder.HasIndex(h => new { h.GolferId, h.LeagueId, h.EffectiveDate })
            .HasDatabaseName("IX_HandicapHistory_League")
            .HasFilter("\"LeagueId\" IS NOT NULL")
            .IsDescending(false, false, true); // EffectiveDate DESC

        builder.HasIndex(h => new { h.GolferId, h.SeasonId, h.EffectiveDate })
            .HasDatabaseName("IX_HandicapHistory_Season")
            .HasFilter("\"SeasonId\" IS NOT NULL")
            .IsDescending(false, false, true); // EffectiveDate DESC
    }
}
