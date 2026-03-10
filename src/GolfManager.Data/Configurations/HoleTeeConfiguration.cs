using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for HoleTee
/// </summary>
public class HoleTeeConfiguration : IEntityTypeConfiguration<HoleTee>
{
    public void Configure(EntityTypeBuilder<HoleTee> builder)
    {
        builder.ToTable("HoleTees");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.TeeId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.HoleNumber)
            .IsRequired();

        // Unique constraint: Each tee can only have one entry per hole number
        builder.HasIndex(x => new { x.TeeId, x.HoleNumber })
            .IsUnique();

        builder.Property(x => x.Par)
            .IsRequired();

        builder.Property(x => x.Yardage)
            .IsRequired();

        builder.Property(x => x.Handicap)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}

