using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity Framework configuration for SeasonSettings
/// </summary>
public class SeasonSettingsConfiguration : IEntityTypeConfiguration<SeasonSettings>
{
    public void Configure(EntityTypeBuilder<SeasonSettings> builder)
    {
        builder.ToTable("SeasonSettings");

        // Primary Key
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        // Foreign Keys
        builder.Property(x => x.SeasonId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LeagueId)
            .IsRequired()
            .HasMaxLength(50);

        // Handicap Settings
        builder.Property(x => x.HandicapType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.MaxHandicap);

        builder.Property(x => x.MaxScoreForHandicap)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Scoring Settings
        builder.Property(x => x.IndividualScoringType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.TeamScoringType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.MissingPlayerType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.MissingTeamType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Defaults
        builder.Property(x => x.DefaultCourseId)
            .HasMaxLength(50);

        builder.Property(x => x.DefaultStartTime);

        // Indexes
        builder.HasIndex(x => x.SeasonId)
            .IsUnique(); // One settings per season

        builder.HasIndex(x => x.LeagueId);

        // Navigation: SeasonSettings -> Season (one-to-one)
        builder.HasOne(x => x.Season)
            .WithOne()
            .HasForeignKey<SeasonSettings>(x => x.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: SeasonSettings -> Course (optional)
        builder.HasOne(x => x.DefaultCourse)
            .WithMany()
            .HasForeignKey(x => x.DefaultCourseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

