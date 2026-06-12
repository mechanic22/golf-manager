using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

public class RoundHoleConfiguration : IEntityTypeConfiguration<RoundHole>
{
    public void Configure(EntityTypeBuilder<RoundHole> builder)
    {
        builder.ToTable("RoundHoles");

        builder.HasKey(x => new { x.RoundId, x.HoleNumber });

        builder.Property(x => x.RoundId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.HoleNumber)
            .IsRequired();

        builder.HasOne(x => x.Round)
            .WithMany(x => x.Holes)
            .HasForeignKey(x => x.RoundId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
