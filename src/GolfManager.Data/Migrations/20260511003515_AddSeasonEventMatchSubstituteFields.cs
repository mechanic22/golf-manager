using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonEventMatchSubstituteFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AwaySubSeasonGolferId",
                table: "SeasonEventMatches",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeSubSeasonGolferId",
                table: "SeasonEventMatches",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwaySubSeasonGolferId",
                table: "SeasonEventMatches");

            migrationBuilder.DropColumn(
                name: "HomeSubSeasonGolferId",
                table: "SeasonEventMatches");
        }
    }
}
