using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for Golfer
/// </summary>
public class GolferConfiguration : IEntityTypeConfiguration<Golfer>
{
    public void Configure(EntityTypeBuilder<Golfer> builder)
    {
        builder.ToTable("Golfers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Nickname)
            .HasMaxLength(100);

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Navigation: Golfer -> LeagueGolfers (one-to-many)
        builder.HasMany(x => x.LeagueGolfers)
            .WithOne(x => x.Golfer)
            .HasForeignKey(x => x.GolferId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: Golfer -> GolferClubs (one-to-many)
        builder.HasMany(x => x.Clubs)
            .WithOne(x => x.Golfer)
            .HasForeignKey(x => x.GolferId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: Golfer -> Rounds (one-to-many)
        builder.HasMany(x => x.Rounds)
            .WithOne(x => x.Golfer)
            .HasForeignKey(x => x.GolferId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

