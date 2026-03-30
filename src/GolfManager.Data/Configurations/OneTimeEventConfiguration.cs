using GolfManager.Core.Entities;
using GolfManager.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for OneTimeEvent
/// </summary>
public class OneTimeEventConfiguration : IEntityTypeConfiguration<OneTimeEvent>
{
    public void Configure(EntityTypeBuilder<OneTimeEvent> builder)
    {
        builder.ToTable("OneTimeEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Key)
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.EventDate)
            .IsRequired();

        // Organizer Information
        builder.Property(x => x.OrganizerId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.OrganizationName)
            .HasMaxLength(200);

        builder.Property(x => x.OrganizerEmail)
            .HasMaxLength(256);

        builder.Property(x => x.OrganizerPhone)
            .HasMaxLength(20);

        // Venue Information
        builder.Property(x => x.CourseId)
            .HasMaxLength(50);

        builder.Property(x => x.TeeId)
            .HasMaxLength(50);

        builder.Property(x => x.HolesPlayed)
            .IsRequired();

        // Tournament Settings
        builder.Property(x => x.Format)
            .IsRequired();

        builder.Property(x => x.TeamSize)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.UseHandicaps)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.MaxTeams);

        builder.Property(x => x.TotalRounds)
            .IsRequired()
            .HasDefaultValue(1);

        // Access Control
        builder.Property(x => x.AccessType)
            .IsRequired()
            .HasDefaultValue(EventAccessType.Public);

        builder.Property(x => x.RegistrationCode)
            .HasMaxLength(50);

        builder.Property(x => x.RegistrationDeadline);

        // Status
        builder.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(EventStatus.Draft);

        builder.Property(x => x.IsLocked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Payment Information
        builder.Property(x => x.Tier)
            .HasMaxLength(50);

        builder.Property(x => x.PaymentStatus)
            .HasMaxLength(50);

        builder.Property(x => x.StripePaymentIntentId)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(x => x.OrganizerId);
        builder.HasIndex(x => x.EventDate);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.AccessType);

        // Navigation: OneTimeEvent -> User (Organizer)
        builder.HasOne(x => x.Organizer)
            .WithMany()
            .HasForeignKey(x => x.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Navigation: OneTimeEvent -> Course (optional)
        builder.HasOne(x => x.Course)
            .WithMany()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.SetNull);

        // Navigation: OneTimeEvent -> Tee (optional)
        builder.HasOne(x => x.Tee)
            .WithMany()
            .HasForeignKey(x => x.TeeId)
            .OnDelete(DeleteBehavior.SetNull);

        // Navigation: OneTimeEvent -> Teams (one-to-many)
        builder.HasMany(x => x.Teams)
            .WithOne(x => x.Event)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

