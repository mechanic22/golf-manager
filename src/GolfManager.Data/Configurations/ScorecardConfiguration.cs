using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for Scorecard
/// </summary>
public class ScorecardConfiguration : IEntityTypeConfiguration<Scorecard>
{
    public void Configure(EntityTypeBuilder<Scorecard> builder)
    {
        builder.ToTable("Scorecards");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.RoundId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.RoundId)
            .IsUnique();

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Weather)
            .HasMaxLength(100);

        builder.Property(x => x.Wind)
            .HasMaxLength(100);

        builder.Property(x => x.CourseConditions)
            .HasMaxLength(500);

        builder.Property(x => x.PlayingPartners)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}

