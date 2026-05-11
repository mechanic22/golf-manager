using GolfManager.Core.Entities;
using GolfManager.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for UserLeague
/// </summary>
public class UserLeagueConfiguration : IEntityTypeConfiguration<UserLeague>
{
    public void Configure(EntityTypeBuilder<UserLeague> builder)
    {
        builder.ToTable("UserLeagues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LeagueId)
            .IsRequired()
            .HasMaxLength(50);

        // Unique constraint: User can only be in a league once
        builder.HasIndex(x => new { x.UserId, x.LeagueId })
            .IsUnique();

        builder.Property(x => x.IsLeagueAdmin)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20)
            .HasSentinel(LeagueMemberRole.Member)
            .HasDefaultValue(LeagueMemberRole.Member);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.JoinedAt)
            .IsRequired();
    }
}
