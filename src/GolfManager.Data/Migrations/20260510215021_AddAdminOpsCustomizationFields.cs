using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminOpsCustomizationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaidForSeason",
                table: "SeasonGolfers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "SeasonGolfers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GameOfDayTitle",
                table: "SeasonEvents",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GameOfDayWinnerDisplayName",
                table: "SeasonEvents",
                type: "TEXT",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GameOfDayWinnerSeasonGolferId",
                table: "SeasonEvents",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnnouncementBody",
                table: "Leagues",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnnouncementTitle",
                table: "Leagues",
                type: "TEXT",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaidForSeason",
                table: "SeasonGolfers");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "SeasonGolfers");

            migrationBuilder.DropColumn(
                name: "GameOfDayTitle",
                table: "SeasonEvents");

            migrationBuilder.DropColumn(
                name: "GameOfDayWinnerDisplayName",
                table: "SeasonEvents");

            migrationBuilder.DropColumn(
                name: "GameOfDayWinnerSeasonGolferId",
                table: "SeasonEvents");

            migrationBuilder.DropColumn(
                name: "AnnouncementBody",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "AnnouncementTitle",
                table: "Leagues");
        }
    }
}
