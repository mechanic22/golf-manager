using GolfManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfManager.Data.Configurations;

/// <summary>
/// Entity configuration for SeasonTeam
/// </summary>
public class SeasonTeamConfiguration : IEntityTypeConfiguration<SeasonTeam>
{
    public void Configure(EntityTypeBuilder<SeasonTeam> builder)
    {
        builder.ToTable("SeasonTeams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SeasonId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LeagueId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Wins)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Losses)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Ties)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}

