using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfManager.Data.Migrations;

/// <inheritdoc />
public partial class AddLeagueMemberRoles : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Role",
            table: "UserLeagues",
            type: "TEXT",
            maxLength: 20,
            nullable: false,
            defaultValue: "Member");

        migrationBuilder.Sql("""
            UPDATE UserLeagues
            SET Role = CASE
                WHEN IsLeagueAdmin = 1 THEN 'Admin'
                ELSE 'Member'
            END
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Role",
            table: "UserLeagues");
    }
}
