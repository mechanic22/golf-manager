using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeagueAnonymousAccessPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnonymousPasswordHash",
                table: "Leagues",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AnonymousPasswordUpdatedAt",
                table: "Leagues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireAnonymousPassword",
                table: "Leagues",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnonymousPasswordHash",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "AnonymousPasswordUpdatedAt",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "RequireAnonymousPassword",
                table: "Leagues");
        }
    }
}
