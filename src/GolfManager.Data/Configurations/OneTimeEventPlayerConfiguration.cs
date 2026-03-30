using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for OneTimeEventPlayer
/// </summary>
public class OneTimeEventPlayerConfiguration : IEntityTypeConfiguration<OneTimeEventPlayer>
{
    public void Configure(EntityTypeBuilder<OneTimeEventPlayer> builder)
    {
        builder.ToTable("OneTimeEventPlayers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.TeamId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.EventId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.UserId)
            .HasMaxLength(50);

        builder.Property(x => x.PlayerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Email)
            .HasMaxLength(256);

        builder.Property(x => x.Handicap)
            .HasPrecision(5, 2);

        builder.Property(x => x.PlayerNumber)
            .IsRequired();

        builder.Property(x => x.IsCaptain)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(x => x.TeamId);
        builder.HasIndex(x => x.EventId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.TeamId, x.PlayerNumber })
            .IsUnique();

        // Navigation: OneTimeEventPlayer -> OneTimeEventTeam
        builder.HasOne(x => x.Team)
            .WithMany(x => x.Players)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: OneTimeEventPlayer -> OneTimeEvent
        builder.HasOne(x => x.Event)
            .WithMany()
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: OneTimeEventPlayer -> User (optional)
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

