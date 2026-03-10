using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for SeasonEvent
/// </summary>
public class SeasonEventConfiguration : IEntityTypeConfiguration<SeasonEvent>
{
    public void Configure(EntityTypeBuilder<SeasonEvent> builder)
    {
        builder.ToTable("SeasonEvents");

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

        builder.Property(x => x.CourseId)
            .HasMaxLength(50);

        builder.Property(x => x.TeeId)
            .HasMaxLength(50);

        builder.Property(x => x.EventDate)
            .IsRequired();

        builder.Property(x => x.HolesPlayed)
            .IsRequired();

        builder.Property(x => x.EventType)
            .IsRequired();

        builder.Property(x => x.ScoringFormat)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.IsLocked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Navigation: SeasonEvent -> Course (many-to-one, optional)
        builder.HasOne(x => x.Course)
            .WithMany()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.SetNull);

        // Navigation: SeasonEvent -> Tee (many-to-one, optional)
        builder.HasOne(x => x.Tee)
            .WithMany()
            .HasForeignKey(x => x.TeeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

