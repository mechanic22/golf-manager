using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for OneTimeEventTeam
/// </summary>
public class OneTimeEventTeamConfiguration : IEntityTypeConfiguration<OneTimeEventTeam>
{
    public void Configure(EntityTypeBuilder<OneTimeEventTeam> builder)
    {
        builder.ToTable("OneTimeEventTeams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.EventId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.TeamName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TeamNumber)
            .IsRequired();

        // Captain Information
        builder.Property(x => x.CaptainUserId)
            .HasMaxLength(50);

        builder.Property(x => x.CaptainName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CaptainEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.CaptainPhone)
            .HasMaxLength(20);

        // Registration
        builder.Property(x => x.RegisteredAt)
            .IsRequired();

        builder.Property(x => x.IsCheckedIn)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CheckedInAt);

        // Scoring
        builder.Property(x => x.TotalScore);

        builder.Property(x => x.NetScore);

        builder.Property(x => x.Position);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(x => x.EventId);
        builder.HasIndex(x => x.CaptainUserId);
        builder.HasIndex(x => new { x.EventId, x.TeamNumber })
            .IsUnique();

        // Navigation: OneTimeEventTeam -> OneTimeEvent
        builder.HasOne(x => x.Event)
            .WithMany(x => x.Teams)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: OneTimeEventTeam -> User (Captain, optional)
        builder.HasOne(x => x.Captain)
            .WithMany()
            .HasForeignKey(x => x.CaptainUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Navigation: OneTimeEventTeam -> Players (one-to-many)
        builder.HasMany(x => x.Players)
            .WithOne(x => x.Team)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: OneTimeEventTeam -> Rounds (one-to-many)
        builder.HasMany(x => x.Rounds)
            .WithOne(x => x.OneTimeEventTeam)
            .HasForeignKey(x => x.OneTimeEventTeamId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

