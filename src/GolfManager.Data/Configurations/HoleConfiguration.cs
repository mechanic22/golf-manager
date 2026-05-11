using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for Hole
/// </summary>
public class HoleConfiguration : IEntityTypeConfiguration<Hole>
{
    public void Configure(EntityTypeBuilder<Hole> builder)
    {
        builder.ToTable("Holes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CourseId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.HoleNumber)
            .IsRequired();

        // Unique constraint: Hole number must be unique within a course
        builder.HasIndex(x => new { x.CourseId, x.HoleNumber })
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // HoleTees are modeled from Tee + HoleNumber; avoid mapping a direct FK to HoleNumber,
        // which would force a global alternate key on Holes.HoleNumber.
        builder.Ignore(x => x.HoleTees);
    }
}

