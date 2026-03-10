using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for GolferClub
/// </summary>
public class GolferClubConfiguration : IEntityTypeConfiguration<GolferClub>
{
    public void Configure(EntityTypeBuilder<GolferClub> builder)
    {
        builder.ToTable("GolferClubs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.GolferId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ClubType)
            .IsRequired();

        builder.Property(x => x.Brand)
            .HasMaxLength(100);

        builder.Property(x => x.Model)
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.IsInBag)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Navigation: GolferClub -> Golfer (many-to-one)
        builder.HasOne(x => x.Golfer)
            .WithMany(x => x.Clubs)
            .HasForeignKey(x => x.GolferId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

