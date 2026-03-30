using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiRoundSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoundLabel",
                table: "Rounds",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoundNumber",
                table: "Rounds",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TotalRounds",
                table: "OneTimeEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoundLabel",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "RoundNumber",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "TotalRounds",
                table: "OneTimeEvents");
        }
    }
}
