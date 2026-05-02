using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonEventMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeasonEventMatches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SeasonEventId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ScorecardId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HomeTeamId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    AwayTeamId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HomePoints = table.Column<double>(type: "REAL", nullable: true),
                    AwayPoints = table.Column<double>(type: "REAL", nullable: true),
                    StartingHole = table.Column<int>(type: "INTEGER", nullable: true),
                    StartingFlight = table.Column<int>(type: "INTEGER", nullable: true),
                    IsComplete = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonEventMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonEventMatches_Scorecards_ScorecardId",
                        column: x => x.ScorecardId,
                        principalTable: "Scorecards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SeasonEventMatches_SeasonEvents_SeasonEventId",
                        column: x => x.SeasonEventId,
                        principalTable: "SeasonEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonEventMatches_SeasonTeams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalTable: "SeasonTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SeasonEventMatches_SeasonTeams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "SeasonTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEventMatches_AwayTeamId",
                table: "SeasonEventMatches",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEventMatches_HomeTeamId",
                table: "SeasonEventMatches",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEventMatches_LeagueId",
                table: "SeasonEventMatches",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEventMatches_ScorecardId",
                table: "SeasonEventMatches",
                column: "ScorecardId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEventMatches_SeasonEventId",
                table: "SeasonEventMatches",
                column: "SeasonEventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeasonEventMatches");
        }
    }
}
