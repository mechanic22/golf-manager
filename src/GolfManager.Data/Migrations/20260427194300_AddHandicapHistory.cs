using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHandicapHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserLeagues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "UserLeagues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserLeagues",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Tees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Tees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tees",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SeasonTeams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SeasonTeams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SeasonTeams",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SeasonSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SeasonSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SeasonSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Seasons",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Seasons",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Seasons",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SeasonGolfers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SeasonGolfers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SeasonGolfers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SeasonEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SeasonEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SeasonEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SeasonEventMatches",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SeasonEventMatches",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SeasonEventMatches",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Scorecards",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Scorecards",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Scorecards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Rounds",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Rounds",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Rounds",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RoundHoles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "RoundHoles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RoundHoles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RefreshTokens",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "RefreshTokens",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RefreshTokens",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OneTimeEventTeams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OneTimeEventTeams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OneTimeEventTeams",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OneTimeEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OneTimeEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OneTimeEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OneTimeEventPlayers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OneTimeEventPlayers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OneTimeEventPlayers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Leagues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Leagues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Leagues",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "LeagueGolfers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "LeagueGolfers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "LeagueGolfers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "HoleTees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "HoleTees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "HoleTees",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Holes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Holes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Holes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Golfers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Golfers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Golfers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "GolferClubs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "GolferClubs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "GolferClubs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Courses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Courses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Courses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "HandicapHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    GolferId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    SeasonId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HandicapIndex = table.Column<double>(type: "REAL", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CalculationMethod = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    RoundsUsed = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HandicapHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HandicapHistories_Golfers_GolferId",
                        column: x => x.GolferId,
                        principalTable: "Golfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HandicapHistories_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HandicapHistories_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HandicapHistories_LeagueId",
                table: "HandicapHistories",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_HandicapHistories_SeasonId",
                table: "HandicapHistories",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_HandicapHistory_Golfer_Date",
                table: "HandicapHistories",
                columns: new[] { "GolferId", "EffectiveDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_HandicapHistory_League",
                table: "HandicapHistories",
                columns: new[] { "GolferId", "LeagueId", "EffectiveDate" },
                descending: new[] { false, false, true },
                filter: "LeagueId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HandicapHistory_Season",
                table: "HandicapHistories",
                columns: new[] { "GolferId", "SeasonId", "EffectiveDate" },
                descending: new[] { false, false, true },
                filter: "SeasonId IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HandicapHistories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Tees");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Tees");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tees");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SeasonTeams");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SeasonTeams");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SeasonTeams");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SeasonSettings");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SeasonSettings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SeasonSettings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SeasonGolfers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SeasonGolfers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SeasonGolfers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SeasonEvents");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SeasonEvents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SeasonEvents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SeasonEventMatches");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SeasonEventMatches");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SeasonEventMatches");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Scorecards");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Scorecards");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Scorecards");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RoundHoles");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "RoundHoles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RoundHoles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OneTimeEventTeams");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OneTimeEventTeams");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OneTimeEventTeams");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OneTimeEvents");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OneTimeEvents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OneTimeEvents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OneTimeEventPlayers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OneTimeEventPlayers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OneTimeEventPlayers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "LeagueGolfers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "LeagueGolfers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "LeagueGolfers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "HoleTees");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "HoleTees");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "HoleTees");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Holes");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Holes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Holes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Golfers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Golfers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Golfers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "GolferClubs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "GolferClubs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "GolferClubs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Courses");
        }
    }
}
