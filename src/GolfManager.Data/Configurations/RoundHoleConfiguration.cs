using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for RoundHole
/// </summary>
public class RoundHoleConfiguration : IEntityTypeConfiguration<RoundHole>
{
    public void Configure(EntityTypeBuilder<RoundHole> builder)
    {
        builder.ToTable("RoundHoles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.RoundId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.HoleNumber)
            .IsRequired();

        // Unique constraint: Each round can only have one entry per hole number
        builder.HasIndex(x => new { x.RoundId, x.HoleNumber })
            .IsUnique();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Navigation: RoundHole -> Round (many-to-one)
        builder.HasOne(x => x.Round)
            .WithMany(x => x.Holes)
            .HasForeignKey(x => x.RoundId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

