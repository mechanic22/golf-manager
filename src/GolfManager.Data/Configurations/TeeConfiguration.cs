using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for Tee
/// </summary>
public class TeeConfiguration : IEntityTypeConfiguration<Tee>
{
    public void Configure(EntityTypeBuilder<Tee> builder)
    {
        builder.ToTable("Tees");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CourseId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.HtmlColorCode)
            .IsRequired()
            .HasMaxLength(7)
            .HasDefaultValue("#FFFFFF");

        builder.Property(x => x.RatingOut)
            .IsRequired();

        builder.Property(x => x.SlopeOut)
            .IsRequired();

        builder.Property(x => x.RatingIn)
            .IsRequired();

        builder.Property(x => x.SlopeIn)
            .IsRequired();

        builder.Property(x => x.YardsOut)
            .IsRequired();

        builder.Property(x => x.YardsIn)
            .IsRequired();

        builder.Property(x => x.ParOut)
            .IsRequired();

        builder.Property(x => x.ParIn)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Ignore computed properties
        builder.Ignore(x => x.TotalRating);
        builder.Ignore(x => x.AverageSlope);
        builder.Ignore(x => x.TotalYards);
        builder.Ignore(x => x.TotalPar);

        // Navigation: Tee -> HoleTees (one-to-many)
        builder.HasMany(x => x.HoleTees)
            .WithOne(x => x.Tee)
            .HasForeignKey(x => x.TeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: Tee -> Rounds (one-to-many)
        builder.HasMany(x => x.Rounds)
            .WithOne(x => x.Tee)
            .HasForeignKey(x => x.TeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

