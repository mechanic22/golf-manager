using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaguePersonalityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoleTees_Holes_HoleNumber",
                table: "HoleTees");

            migrationBuilder.DropIndex(
                name: "IX_HoleTees_HoleNumber",
                table: "HoleTees");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Holes_HoleNumber",
                table: "Holes");

            migrationBuilder.AddColumn<string>(
                name: "CommissionerName",
                table: "Leagues",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmptyStateMessage",
                table: "Leagues",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WelcomeHeadline",
                table: "Leagues",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WelcomeSubhead",
                table: "Leagues",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommissionerName",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "EmptyStateMessage",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "WelcomeHeadline",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "WelcomeSubhead",
                table: "Leagues");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Holes_HoleNumber",
                table: "Holes",
                column: "HoleNumber");

            migrationBuilder.CreateIndex(
                name: "IX_HoleTees_HoleNumber",
                table: "HoleTees",
                column: "HoleNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_HoleTees_Holes_HoleNumber",
                table: "HoleTees",
                column: "HoleNumber",
                principalTable: "Holes",
                principalColumn: "HoleNumber",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
