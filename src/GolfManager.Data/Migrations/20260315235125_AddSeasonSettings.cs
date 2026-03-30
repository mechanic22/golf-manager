using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeasonSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SeasonId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    HandicapType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MaxHandicap = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxScoreForHandicap = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IndividualScoringType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TeamScoringType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MissingPlayerType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MissingTeamType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DefaultCourseId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DefaultStartTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonSettings_Courses_DefaultCourseId",
                        column: x => x.DefaultCourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SeasonSettings_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonSettings_DefaultCourseId",
                table: "SeasonSettings",
                column: "DefaultCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonSettings_LeagueId",
                table: "SeasonSettings",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonSettings_SeasonId",
                table: "SeasonSettings",
                column: "SeasonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeasonSettings");
        }
    }
}
