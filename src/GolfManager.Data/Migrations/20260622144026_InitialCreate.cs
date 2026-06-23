using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    NumberOfHoles = table.Column<int>(type: "integer", nullable: false, defaultValue: 18),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WelcomeHeadline = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WelcomeSubhead = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EmptyStateMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CommissionerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AnnouncementTitle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    AnnouncementBody = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActiveSeasonId = table.Column<string>(type: "text", nullable: true),
                    CustomDomain = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UseCustomDomain = table.Column<bool>(type: "boolean", nullable: false),
                    CustomDomainVerificationToken = table.Column<string>(type: "text", nullable: true),
                    CustomDomainVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPubliclyDiscoverable = table.Column<bool>(type: "boolean", nullable: false),
                    RequireAnonymousPassword = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AnonymousPasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AnonymousPasswordUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsGlobalAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Holes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CourseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HoleNumber = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TeeLatitude = table.Column<double>(type: "double precision", nullable: true),
                    TeeLongitude = table.Column<double>(type: "double precision", nullable: true),
                    GreenLatitude = table.Column<double>(type: "double precision", nullable: true),
                    GreenLongitude = table.Column<double>(type: "double precision", nullable: true),
                    GreenRadius = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Holes_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tees",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CourseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HtmlColorCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#FFFFFF"),
                    RatingOut = table.Column<double>(type: "double precision", nullable: false),
                    SlopeOut = table.Column<int>(type: "integer", nullable: false),
                    RatingIn = table.Column<double>(type: "double precision", nullable: false),
                    SlopeIn = table.Column<int>(type: "integer", nullable: false),
                    YardsOut = table.Column<int>(type: "integer", nullable: false),
                    YardsIn = table.Column<int>(type: "integer", nullable: false),
                    ParOut = table.Column<int>(type: "integer", nullable: false),
                    ParIn = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tees_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasons_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Golfers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    HomeCity = table.Column<string>(type: "text", nullable: true),
                    HomeState = table.Column<string>(type: "text", nullable: true),
                    GlobalHandicap = table.Column<double>(type: "double precision", nullable: true),
                    GlobalHandicapUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Golfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Golfers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RevokedByIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReplacedByToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoleTees",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TeeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HoleNumber = table.Column<int>(type: "integer", nullable: false),
                    Par = table.Column<int>(type: "integer", nullable: false),
                    Yardage = table.Column<int>(type: "integer", nullable: false),
                    Handicap = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoleTees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoleTees_Tees_TeeId",
                        column: x => x.TeeId,
                        principalTable: "Tees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OneTimeEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrganizerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OrganizationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OrganizerEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    OrganizerPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CourseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TeeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HolesPlayed = table.Column<int>(type: "integer", nullable: false),
                    Format = table.Column<int>(type: "integer", nullable: false),
                    TeamSize = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    UseHandicaps = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MaxTeams = table.Column<int>(type: "integer", nullable: true),
                    TotalRounds = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    AccessType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RegistrationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RegistrationDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Tier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaymentStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StripePaymentIntentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneTimeEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OneTimeEvents_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OneTimeEvents_Tees_TeeId",
                        column: x => x.TeeId,
                        principalTable: "Tees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OneTimeEvents_Users_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SeasonEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SeasonId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CourseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TeeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HolesPlayed = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    ScoringFormat = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GameOfDayTitle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    GameOfDayWinnerSeasonGolferId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GameOfDayWinnerDisplayName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TeamSize = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    UseHandicaps = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonEvents_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SeasonEvents_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonEvents_Tees_TeeId",
                        column: x => x.TeeId,
                        principalTable: "Tees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SeasonSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SeasonId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HandicapType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MaxHandicap = table.Column<int>(type: "integer", nullable: true),
                    MaxScoreForHandicap = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IndividualScoringType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TeamScoringType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MissingPlayerType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MissingTeamType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DefaultCourseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultStartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "SeasonTeams",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SeasonId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SeasonPoints = table.Column<double>(type: "double precision", nullable: true),
                    Wins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Losses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Ties = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonTeams_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GolferClubs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GolferId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClubType = table.Column<int>(type: "integer", nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AverageDistance = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsInBag = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GolferClubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GolferClubs_Golfers_GolferId",
                        column: x => x.GolferId,
                        principalTable: "Golfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HandicapHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GolferId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SeasonId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HandicapIndex = table.Column<double>(type: "double precision", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CalculationMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RoundsUsed = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "LeagueGolfers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GolferId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LeagueHandicap = table.Column<double>(type: "double precision", nullable: true),
                    LeagueHandicapUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalRounds = table.Column<int>(type: "integer", nullable: false),
                    AverageScore = table.Column<double>(type: "double precision", nullable: true),
                    BestScore = table.Column<int>(type: "integer", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueGolfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeagueGolfers_Golfers_GolferId",
                        column: x => x.GolferId,
                        principalTable: "Golfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeagueGolfers_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OneTimeEventTeams",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EventId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TeamName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TeamNumber = table.Column<int>(type: "integer", nullable: false),
                    CaptainUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CaptainName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CaptainEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CaptainPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCheckedIn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CheckedInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalScore = table.Column<int>(type: "integer", nullable: true),
                    NetScore = table.Column<int>(type: "integer", nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneTimeEventTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OneTimeEventTeams_OneTimeEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "OneTimeEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OneTimeEventTeams_Users_CaptainUserId",
                        column: x => x.CaptainUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SeasonGolfers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SeasonId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueGolferId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GolferId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TeamId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SeasonHandicap = table.Column<double>(type: "double precision", nullable: true),
                    TotalEvents = table.Column<int>(type: "integer", nullable: false),
                    AverageScore = table.Column<double>(type: "double precision", nullable: true),
                    TotalPoints = table.Column<double>(type: "double precision", nullable: true),
                    IsPaidForSeason = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonGolfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonGolfers_Golfers_GolferId",
                        column: x => x.GolferId,
                        principalTable: "Golfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeasonGolfers_LeagueGolfers_LeagueGolferId",
                        column: x => x.LeagueGolferId,
                        principalTable: "LeagueGolfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonGolfers_SeasonTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "SeasonTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SeasonGolfers_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLeagues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueGolferId = table.Column<string>(type: "character varying(50)", nullable: true),
                    IsLeagueAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Member"),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLeagues_LeagueGolfers_LeagueGolferId",
                        column: x => x.LeagueGolferId,
                        principalTable: "LeagueGolfers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserLeagues_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLeagues_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OneTimeEventPlayers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TeamId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EventId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PlayerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Handicap = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    PlayerNumber = table.Column<int>(type: "integer", nullable: false),
                    IsCaptain = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneTimeEventPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OneTimeEventPlayers_OneTimeEventTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "OneTimeEventTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OneTimeEventPlayers_OneTimeEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "OneTimeEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OneTimeEventPlayers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Rounds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GolferId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueGolferId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CourseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TeeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RoundDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HolesPlayed = table.Column<int>(type: "integer", nullable: false),
                    TotalScore = table.Column<int>(type: "integer", nullable: true),
                    NetScore = table.Column<int>(type: "integer", nullable: true),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OneTimeEventId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OneTimeEventTeamId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsTeamRound = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Format = table.Column<int>(type: "integer", nullable: true),
                    RoundNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RoundLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rounds_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rounds_Golfers_GolferId",
                        column: x => x.GolferId,
                        principalTable: "Golfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rounds_LeagueGolfers_LeagueGolferId",
                        column: x => x.LeagueGolferId,
                        principalTable: "LeagueGolfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rounds_OneTimeEventTeams_OneTimeEventTeamId",
                        column: x => x.OneTimeEventTeamId,
                        principalTable: "OneTimeEventTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Rounds_OneTimeEvents_OneTimeEventId",
                        column: x => x.OneTimeEventId,
                        principalTable: "OneTimeEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Rounds_Tees_TeeId",
                        column: x => x.TeeId,
                        principalTable: "Tees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SeasonEventPlayerScores",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SeasonEventId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    SeasonGolferId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    RawScore = table.Column<int>(type: "integer", nullable: true),
                    Handicap = table.Column<double>(type: "double precision", nullable: true),
                    NetScore = table.Column<double>(type: "double precision", nullable: true),
                    EventPoints = table.Column<double>(type: "double precision", nullable: true),
                    IsMissing = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MissScore = table.Column<double>(type: "double precision", nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TeamId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    TeamName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonEventPlayerScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonEventPlayerScores_SeasonEvents_SeasonEventId",
                        column: x => x.SeasonEventId,
                        principalTable: "SeasonEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonEventPlayerScores_SeasonGolfers_SeasonGolferId",
                        column: x => x.SeasonGolferId,
                        principalTable: "SeasonGolfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoundHoles",
                columns: table => new
                {
                    RoundId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HoleNumber = table.Column<int>(type: "integer", nullable: false),
                    GrossScore = table.Column<int>(type: "integer", nullable: true),
                    NetScore = table.Column<int>(type: "integer", nullable: true),
                    Putts = table.Column<int>(type: "integer", nullable: true),
                    FairwayHit = table.Column<bool>(type: "boolean", nullable: true),
                    GreenInRegulation = table.Column<bool>(type: "boolean", nullable: true),
                    Penalties = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundHoles", x => new { x.RoundId, x.HoleNumber });
                    table.ForeignKey(
                        name: "FK_RoundHoles_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scorecards",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RoundId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Weather = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Temperature = table.Column<int>(type: "integer", nullable: true),
                    Wind = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CourseConditions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlayingPartners = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scorecards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scorecards_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeasonEventMatches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SeasonEventId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScorecardId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HomeTeamId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AwayTeamId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HomeSubSeasonGolferId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AwaySubSeasonGolferId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HomePoints = table.Column<double>(type: "double precision", nullable: true),
                    AwayPoints = table.Column<double>(type: "double precision", nullable: true),
                    StartingHole = table.Column<int>(type: "integer", nullable: true),
                    StartingFlight = table.Column<int>(type: "integer", nullable: true),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "SeasonEventMatchScores",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SeasonEventId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    SeasonEventMatchId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    HomeTeamId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    HomeTeamName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HomePoints = table.Column<double>(type: "double precision", nullable: true),
                    AwayTeamId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    AwayTeamName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AwayPoints = table.Column<double>(type: "double precision", nullable: true),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    StartingHole = table.Column<int>(type: "integer", nullable: true),
                    StartingFlight = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonEventMatchScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonEventMatchScores_SeasonEventMatches_SeasonEventMatchId",
                        column: x => x.SeasonEventMatchId,
                        principalTable: "SeasonEventMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeasonEventMatchScores_SeasonEvents_SeasonEventId",
                        column: x => x.SeasonEventId,
                        principalTable: "SeasonEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Key",
                table: "Courses",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GolferClubs_GolferId",
                table: "GolferClubs",
                column: "GolferId");

            migrationBuilder.CreateIndex(
                name: "IX_Golfers_UserId",
                table: "Golfers",
                column: "UserId",
                unique: true);

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
                filter: "\"LeagueId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HandicapHistory_Season",
                table: "HandicapHistories",
                columns: new[] { "GolferId", "SeasonId", "EffectiveDate" },
                descending: new[] { false, false, true },
                filter: "\"SeasonId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Holes_CourseId_HoleNumber",
                table: "Holes",
                columns: new[] { "CourseId", "HoleNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoleTees_TeeId_HoleNumber",
                table: "HoleTees",
                columns: new[] { "TeeId", "HoleNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeagueGolfers_GolferId_LeagueId",
                table: "LeagueGolfers",
                columns: new[] { "GolferId", "LeagueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeagueGolfers_LeagueId",
                table: "LeagueGolfers",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_CustomDomain",
                table: "Leagues",
                column: "CustomDomain",
                unique: true,
                filter: "\"CustomDomain\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_Key",
                table: "Leagues",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEventPlayers_EventId",
                table: "OneTimeEventPlayers",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEventPlayers_TeamId",
                table: "OneTimeEventPlayers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEventPlayers_TeamId_PlayerNumber",
                table: "OneTimeEventPlayers",
                columns: new[] { "TeamId", "PlayerNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEventPlayers_UserId",
                table: "OneTimeEventPlayers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEvents_AccessType",
                table: "OneTimeEvents",
                column: "AccessType");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEvents_CourseId",
                table: "OneTimeEvents",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEvents_EventDate",
                table: "OneTimeEvents",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEvents_Key",
                table: "OneTimeEvents",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEvents_OrganizerId",
                table: "OneTimeEvents",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEvents_Status",
                table: "OneTimeEvents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEvents_TeeId",
                table: "OneTimeEvents",
                column: "TeeId");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEventTeams_CaptainUserId",
                table: "OneTimeEventTeams",
                column: "CaptainUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEventTeams_EventId",
                table: "OneTimeEventTeams",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeEventTeams_EventId_TeamNumber",
                table: "OneTimeEventTeams",
                columns: new[] { "EventId", "TeamNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_CourseId",
                table: "Rounds",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_GolferId",
                table: "Rounds",
                column: "GolferId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_LeagueGolferId",
                table: "Rounds",
                column: "LeagueGolferId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_OneTimeEventId",
                table: "Rounds",
                column: "OneTimeEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_OneTimeEventTeamId",
                table: "Rounds",
                column: "OneTimeEventTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_TeeId",
                table: "Rounds",
                column: "TeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Scorecards_RoundId",
                table: "Scorecards",
                column: "RoundId",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEventMatchScores_SeasonEventId_LeagueId",
                table: "SeasonEventMatchScores",
                columns: new[] { "SeasonEventId", "LeagueId" });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEventMatchScores_SeasonEventMatchId_LeagueId",
                table: "SeasonEventMatchScores",
                columns: new[] { "SeasonEventMatchId", "LeagueId" });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEventPlayerScores_SeasonEventId_LeagueId",
                table: "SeasonEventPlayerScores",
                columns: new[] { "SeasonEventId", "LeagueId" });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEventPlayerScores_SeasonGolferId_LeagueId",
                table: "SeasonEventPlayerScores",
                columns: new[] { "SeasonGolferId", "LeagueId" });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEventPlayerScores_TeamId_LeagueId",
                table: "SeasonEventPlayerScores",
                columns: new[] { "TeamId", "LeagueId" });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEvents_CourseId",
                table: "SeasonEvents",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEvents_SeasonId",
                table: "SeasonEvents",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEvents_TeeId",
                table: "SeasonEvents",
                column: "TeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonGolfers_GolferId",
                table: "SeasonGolfers",
                column: "GolferId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonGolfers_LeagueGolferId",
                table: "SeasonGolfers",
                column: "LeagueGolferId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonGolfers_SeasonId_LeagueGolferId",
                table: "SeasonGolfers",
                columns: new[] { "SeasonId", "LeagueGolferId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeasonGolfers_TeamId",
                table: "SeasonGolfers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_LeagueId_Key",
                table: "Seasons",
                columns: new[] { "LeagueId", "Key" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_SeasonTeams_SeasonId",
                table: "SeasonTeams",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Tees_CourseId",
                table: "Tees",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagues_LeagueGolferId",
                table: "UserLeagues",
                column: "LeagueGolferId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagues_LeagueId",
                table: "UserLeagues",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagues_UserId_LeagueId",
                table: "UserLeagues",
                columns: new[] { "UserId", "LeagueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GolferClubs");

            migrationBuilder.DropTable(
                name: "HandicapHistories");

            migrationBuilder.DropTable(
                name: "Holes");

            migrationBuilder.DropTable(
                name: "HoleTees");

            migrationBuilder.DropTable(
                name: "OneTimeEventPlayers");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RoundHoles");

            migrationBuilder.DropTable(
                name: "SeasonEventMatchScores");

            migrationBuilder.DropTable(
                name: "SeasonEventPlayerScores");

            migrationBuilder.DropTable(
                name: "SeasonSettings");

            migrationBuilder.DropTable(
                name: "UserLeagues");

            migrationBuilder.DropTable(
                name: "SeasonEventMatches");

            migrationBuilder.DropTable(
                name: "SeasonGolfers");

            migrationBuilder.DropTable(
                name: "Scorecards");

            migrationBuilder.DropTable(
                name: "SeasonEvents");

            migrationBuilder.DropTable(
                name: "SeasonTeams");

            migrationBuilder.DropTable(
                name: "Rounds");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "LeagueGolfers");

            migrationBuilder.DropTable(
                name: "OneTimeEventTeams");

            migrationBuilder.DropTable(
                name: "Golfers");

            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.DropTable(
                name: "OneTimeEvents");

            migrationBuilder.DropTable(
                name: "Tees");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Courses");
        }
    }
}
