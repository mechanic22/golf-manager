BEGIN TRANSACTION;
ALTER TABLE "SeasonEvents" ADD "Status" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "SeasonEvents" ADD "TeamSize" INTEGER NOT NULL DEFAULT 1;

ALTER TABLE "SeasonEvents" ADD "UseHandicaps" INTEGER NOT NULL DEFAULT 1;

ALTER TABLE "Rounds" ADD "Format" INTEGER NULL;

ALTER TABLE "Rounds" ADD "IsTeamRound" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Rounds" ADD "OneTimeEventId" TEXT NULL;

ALTER TABLE "Rounds" ADD "OneTimeEventTeamId" TEXT NULL;

CREATE TABLE "OneTimeEvents" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_OneTimeEvents" PRIMARY KEY,
    "Key" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "EventDate" TEXT NOT NULL,
    "OrganizerId" TEXT NOT NULL,
    "OrganizationName" TEXT NULL,
    "OrganizerEmail" TEXT NULL,
    "OrganizerPhone" TEXT NULL,
    "CourseId" TEXT NULL,
    "TeeId" TEXT NULL,
    "HolesPlayed" INTEGER NOT NULL,
    "Format" INTEGER NOT NULL,
    "TeamSize" INTEGER NOT NULL DEFAULT 1,
    "UseHandicaps" INTEGER NOT NULL DEFAULT 1,
    "MaxTeams" INTEGER NULL,
    "AccessType" INTEGER NOT NULL DEFAULT 0,
    "RegistrationCode" TEXT NULL,
    "RegistrationDeadline" TEXT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "IsLocked" INTEGER NOT NULL DEFAULT 0,
    "Tier" TEXT NULL,
    "PaymentStatus" TEXT NULL,
    "StripePaymentIntentId" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_OneTimeEvents_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_OneTimeEvents_Tees_TeeId" FOREIGN KEY ("TeeId") REFERENCES "Tees" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_OneTimeEvents_Users_OrganizerId" FOREIGN KEY ("OrganizerId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "OneTimeEventTeams" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_OneTimeEventTeams" PRIMARY KEY,
    "EventId" TEXT NOT NULL,
    "TeamName" TEXT NOT NULL,
    "TeamNumber" INTEGER NOT NULL,
    "CaptainUserId" TEXT NULL,
    "CaptainName" TEXT NOT NULL,
    "CaptainEmail" TEXT NOT NULL,
    "CaptainPhone" TEXT NULL,
    "RegisteredAt" TEXT NOT NULL,
    "IsCheckedIn" INTEGER NOT NULL DEFAULT 0,
    "CheckedInAt" TEXT NULL,
    "TotalScore" INTEGER NULL,
    "NetScore" INTEGER NULL,
    "Position" INTEGER NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_OneTimeEventTeams_OneTimeEvents_EventId" FOREIGN KEY ("EventId") REFERENCES "OneTimeEvents" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_OneTimeEventTeams_Users_CaptainUserId" FOREIGN KEY ("CaptainUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

CREATE TABLE "OneTimeEventPlayers" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_OneTimeEventPlayers" PRIMARY KEY,
    "TeamId" TEXT NOT NULL,
    "EventId" TEXT NOT NULL,
    "UserId" TEXT NULL,
    "PlayerName" TEXT NOT NULL,
    "Email" TEXT NULL,
    "Handicap" TEXT NULL,
    "PlayerNumber" INTEGER NOT NULL,
    "IsCaptain" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_OneTimeEventPlayers_OneTimeEventTeams_TeamId" FOREIGN KEY ("TeamId") REFERENCES "OneTimeEventTeams" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_OneTimeEventPlayers_OneTimeEvents_EventId" FOREIGN KEY ("EventId") REFERENCES "OneTimeEvents" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_OneTimeEventPlayers_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_Rounds_OneTimeEventId" ON "Rounds" ("OneTimeEventId");

CREATE INDEX "IX_Rounds_OneTimeEventTeamId" ON "Rounds" ("OneTimeEventTeamId");

CREATE INDEX "IX_OneTimeEventPlayers_EventId" ON "OneTimeEventPlayers" ("EventId");

CREATE INDEX "IX_OneTimeEventPlayers_TeamId" ON "OneTimeEventPlayers" ("TeamId");

CREATE UNIQUE INDEX "IX_OneTimeEventPlayers_TeamId_PlayerNumber" ON "OneTimeEventPlayers" ("TeamId", "PlayerNumber");

CREATE INDEX "IX_OneTimeEventPlayers_UserId" ON "OneTimeEventPlayers" ("UserId");

CREATE INDEX "IX_OneTimeEvents_AccessType" ON "OneTimeEvents" ("AccessType");

CREATE INDEX "IX_OneTimeEvents_CourseId" ON "OneTimeEvents" ("CourseId");

CREATE INDEX "IX_OneTimeEvents_EventDate" ON "OneTimeEvents" ("EventDate");

CREATE UNIQUE INDEX "IX_OneTimeEvents_Key" ON "OneTimeEvents" ("Key");

CREATE INDEX "IX_OneTimeEvents_OrganizerId" ON "OneTimeEvents" ("OrganizerId");

CREATE INDEX "IX_OneTimeEvents_Status" ON "OneTimeEvents" ("Status");

CREATE INDEX "IX_OneTimeEvents_TeeId" ON "OneTimeEvents" ("TeeId");

CREATE INDEX "IX_OneTimeEventTeams_CaptainUserId" ON "OneTimeEventTeams" ("CaptainUserId");

CREATE INDEX "IX_OneTimeEventTeams_EventId" ON "OneTimeEventTeams" ("EventId");

CREATE UNIQUE INDEX "IX_OneTimeEventTeams_EventId_TeamNumber" ON "OneTimeEventTeams" ("EventId", "TeamNumber");

CREATE TABLE "ef_temp_Users" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "FirstName" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsGlobalAdmin" INTEGER NOT NULL DEFAULT 0,
    "LastLoginAt" TEXT NULL,
    "LastName" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL
);

INSERT INTO "ef_temp_Users" ("Id", "CreatedAt", "CreatedBy", "Email", "FirstName", "IsActive", "IsGlobalAdmin", "LastLoginAt", "LastName", "PasswordHash", "UpdatedAt", "UpdatedBy")
SELECT "Id", "CreatedAt", "CreatedBy", "Email", "FirstName", "IsActive", "IsGlobalAdmin", "LastLoginAt", "LastName", "PasswordHash", "UpdatedAt", "UpdatedBy"
FROM "Users";

CREATE TABLE "ef_temp_UserLeagues" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_UserLeagues" PRIMARY KEY,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsLeagueAdmin" INTEGER NOT NULL DEFAULT 0,
    "JoinedAt" TEXT NOT NULL,
    "LeagueGolferId" TEXT NULL,
    "LeagueId" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "FK_UserLeagues_LeagueGolfers_LeagueGolferId" FOREIGN KEY ("LeagueGolferId") REFERENCES "LeagueGolfers" ("Id"),
    CONSTRAINT "FK_UserLeagues_Leagues_LeagueId" FOREIGN KEY ("LeagueId") REFERENCES "Leagues" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserLeagues_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_UserLeagues" ("Id", "CreatedAt", "CreatedBy", "IsActive", "IsLeagueAdmin", "JoinedAt", "LeagueGolferId", "LeagueId", "UpdatedAt", "UpdatedBy", "UserId")
SELECT "Id", "CreatedAt", "CreatedBy", "IsActive", "IsLeagueAdmin", "JoinedAt", "LeagueGolferId", "LeagueId", "UpdatedAt", "UpdatedBy", "UserId"
FROM "UserLeagues";

CREATE TABLE "ef_temp_Tees" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Tees" PRIMARY KEY,
    "CourseId" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "HtmlColorCode" TEXT NOT NULL DEFAULT '#FFFFFF',
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "Name" TEXT NOT NULL,
    "ParIn" INTEGER NOT NULL,
    "ParOut" INTEGER NOT NULL,
    "RatingIn" REAL NOT NULL,
    "RatingOut" REAL NOT NULL,
    "SlopeIn" INTEGER NOT NULL,
    "SlopeOut" INTEGER NOT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "YardsIn" INTEGER NOT NULL,
    "YardsOut" INTEGER NOT NULL,
    CONSTRAINT "FK_Tees_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Tees" ("Id", "CourseId", "CreatedAt", "CreatedBy", "HtmlColorCode", "IsActive", "Name", "ParIn", "ParOut", "RatingIn", "RatingOut", "SlopeIn", "SlopeOut", "UpdatedAt", "UpdatedBy", "YardsIn", "YardsOut")
SELECT "Id", "CourseId", "CreatedAt", "CreatedBy", "HtmlColorCode", "IsActive", "Name", "ParIn", "ParOut", "RatingIn", "RatingOut", "SlopeIn", "SlopeOut", "UpdatedAt", "UpdatedBy", "YardsIn", "YardsOut"
FROM "Tees";

CREATE TABLE "ef_temp_SeasonTeams" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_SeasonTeams" PRIMARY KEY,
    "AvatarUrl" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "LeagueId" TEXT NOT NULL,
    "Losses" INTEGER NOT NULL DEFAULT 0,
    "Name" TEXT NOT NULL,
    "SeasonId" TEXT NOT NULL,
    "SeasonPoints" REAL NULL,
    "Ties" INTEGER NOT NULL DEFAULT 0,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "Wins" INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT "FK_SeasonTeams_Seasons_SeasonId" FOREIGN KEY ("SeasonId") REFERENCES "Seasons" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_SeasonTeams" ("Id", "AvatarUrl", "CreatedAt", "CreatedBy", "IsActive", "LeagueId", "Losses", "Name", "SeasonId", "SeasonPoints", "Ties", "UpdatedAt", "UpdatedBy", "Wins")
SELECT "Id", "AvatarUrl", "CreatedAt", "CreatedBy", "IsActive", "LeagueId", "Losses", "Name", "SeasonId", "SeasonPoints", "Ties", "UpdatedAt", "UpdatedBy", "Wins"
FROM "SeasonTeams";

CREATE TABLE "ef_temp_Seasons" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Seasons" PRIMARY KEY,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "EndDate" TEXT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsLocked" INTEGER NOT NULL DEFAULT 0,
    "Key" TEXT NOT NULL,
    "LeagueId" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "StartDate" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    CONSTRAINT "FK_Seasons_Leagues_LeagueId" FOREIGN KEY ("LeagueId") REFERENCES "Leagues" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Seasons" ("Id", "CreatedAt", "CreatedBy", "EndDate", "IsActive", "IsLocked", "Key", "LeagueId", "Name", "StartDate", "UpdatedAt", "UpdatedBy")
SELECT "Id", "CreatedAt", "CreatedBy", "EndDate", "IsActive", "IsLocked", "Key", "LeagueId", "Name", "StartDate", "UpdatedAt", "UpdatedBy"
FROM "Seasons";

CREATE TABLE "ef_temp_SeasonGolfers" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_SeasonGolfers" PRIMARY KEY,
    "AverageScore" REAL NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "GolferId" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "JoinedAt" TEXT NOT NULL,
    "LeagueGolferId" TEXT NOT NULL,
    "LeagueId" TEXT NOT NULL,
    "SeasonHandicap" REAL NULL,
    "SeasonId" TEXT NOT NULL,
    "TeamId" TEXT NULL,
    "TotalEvents" INTEGER NOT NULL,
    "TotalPoints" REAL NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    CONSTRAINT "FK_SeasonGolfers_Golfers_GolferId" FOREIGN KEY ("GolferId") REFERENCES "Golfers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SeasonGolfers_LeagueGolfers_LeagueGolferId" FOREIGN KEY ("LeagueGolferId") REFERENCES "LeagueGolfers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SeasonGolfers_SeasonTeams_TeamId" FOREIGN KEY ("TeamId") REFERENCES "SeasonTeams" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_SeasonGolfers_Seasons_SeasonId" FOREIGN KEY ("SeasonId") REFERENCES "Seasons" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_SeasonGolfers" ("Id", "AverageScore", "CreatedAt", "CreatedBy", "GolferId", "IsActive", "JoinedAt", "LeagueGolferId", "LeagueId", "SeasonHandicap", "SeasonId", "TeamId", "TotalEvents", "TotalPoints", "UpdatedAt", "UpdatedBy")
SELECT "Id", "AverageScore", "CreatedAt", "CreatedBy", "GolferId", "IsActive", "JoinedAt", "LeagueGolferId", "LeagueId", "SeasonHandicap", "SeasonId", "TeamId", "TotalEvents", "TotalPoints", "UpdatedAt", "UpdatedBy"
FROM "SeasonGolfers";

CREATE TABLE "ef_temp_SeasonEvents" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_SeasonEvents" PRIMARY KEY,
    "CourseId" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "Description" TEXT NULL,
    "EventDate" TEXT NOT NULL,
    "EventType" INTEGER NOT NULL,
    "HolesPlayed" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsLocked" INTEGER NOT NULL DEFAULT 0,
    "LeagueId" TEXT NOT NULL,
    "Name" TEXT NULL,
    "ScoringFormat" INTEGER NOT NULL,
    "SeasonId" TEXT NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "TeamSize" INTEGER NOT NULL DEFAULT 1,
    "TeeId" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "UseHandicaps" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_SeasonEvents_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_SeasonEvents_Seasons_SeasonId" FOREIGN KEY ("SeasonId") REFERENCES "Seasons" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SeasonEvents_Tees_TeeId" FOREIGN KEY ("TeeId") REFERENCES "Tees" ("Id") ON DELETE SET NULL
);

INSERT INTO "ef_temp_SeasonEvents" ("Id", "CourseId", "CreatedAt", "CreatedBy", "Description", "EventDate", "EventType", "HolesPlayed", "IsActive", "IsLocked", "LeagueId", "Name", "ScoringFormat", "SeasonId", "Status", "TeamSize", "TeeId", "UpdatedAt", "UpdatedBy", "UseHandicaps")
SELECT "Id", "CourseId", "CreatedAt", "CreatedBy", "Description", "EventDate", "EventType", "HolesPlayed", "IsActive", "IsLocked", "LeagueId", "Name", "ScoringFormat", "SeasonId", "Status", "TeamSize", "TeeId", "UpdatedAt", "UpdatedBy", "UseHandicaps"
FROM "SeasonEvents";

CREATE TABLE "ef_temp_Scorecards" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Scorecards" PRIMARY KEY,
    "CourseConditions" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ImageUrl" TEXT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "Notes" TEXT NULL,
    "PlayingPartners" TEXT NULL,
    "RoundId" TEXT NOT NULL,
    "Temperature" INTEGER NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "Weather" TEXT NULL,
    "Wind" TEXT NULL,
    CONSTRAINT "FK_Scorecards_Rounds_RoundId" FOREIGN KEY ("RoundId") REFERENCES "Rounds" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Scorecards" ("Id", "CourseConditions", "CreatedAt", "CreatedBy", "ImageUrl", "IsActive", "Notes", "PlayingPartners", "RoundId", "Temperature", "UpdatedAt", "UpdatedBy", "Weather", "Wind")
SELECT "Id", "CourseConditions", "CreatedAt", "CreatedBy", "ImageUrl", "IsActive", "Notes", "PlayingPartners", "RoundId", "Temperature", "UpdatedAt", "UpdatedBy", "Weather", "Wind"
FROM "Scorecards";

CREATE TABLE "ef_temp_Rounds" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Rounds" PRIMARY KEY,
    "CourseId" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "Format" INTEGER NULL,
    "GolferId" TEXT NOT NULL,
    "HandicapUsed" REAL NULL,
    "HolesPlayed" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsComplete" INTEGER NOT NULL DEFAULT 0,
    "IsTeamRound" INTEGER NOT NULL DEFAULT 0,
    "LeagueGolferId" TEXT NULL,
    "LeagueId" TEXT NULL,
    "NetScore" INTEGER NULL,
    "Notes" TEXT NULL,
    "OneTimeEventId" TEXT NULL,
    "OneTimeEventTeamId" TEXT NULL,
    "RoundDate" TEXT NOT NULL,
    "TeeId" TEXT NOT NULL,
    "TotalScore" INTEGER NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    CONSTRAINT "FK_Rounds_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Rounds_Golfers_GolferId" FOREIGN KEY ("GolferId") REFERENCES "Golfers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Rounds_LeagueGolfers_LeagueGolferId" FOREIGN KEY ("LeagueGolferId") REFERENCES "LeagueGolfers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Rounds_OneTimeEventTeams_OneTimeEventTeamId" FOREIGN KEY ("OneTimeEventTeamId") REFERENCES "OneTimeEventTeams" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Rounds_OneTimeEvents_OneTimeEventId" FOREIGN KEY ("OneTimeEventId") REFERENCES "OneTimeEvents" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Rounds_Tees_TeeId" FOREIGN KEY ("TeeId") REFERENCES "Tees" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_Rounds" ("Id", "CourseId", "CreatedAt", "CreatedBy", "Format", "GolferId", "HandicapUsed", "HolesPlayed", "IsActive", "IsComplete", "IsTeamRound", "LeagueGolferId", "LeagueId", "NetScore", "Notes", "OneTimeEventId", "OneTimeEventTeamId", "RoundDate", "TeeId", "TotalScore", "UpdatedAt", "UpdatedBy")
SELECT "Id", "CourseId", "CreatedAt", "CreatedBy", "Format", "GolferId", "HandicapUsed", "HolesPlayed", "IsActive", "IsComplete", "IsTeamRound", "LeagueGolferId", "LeagueId", "NetScore", "Notes", "OneTimeEventId", "OneTimeEventTeamId", "RoundDate", "TeeId", "TotalScore", "UpdatedAt", "UpdatedBy"
FROM "Rounds";

CREATE TABLE "ef_temp_RoundHoles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_RoundHoles" PRIMARY KEY,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "FairwayHit" INTEGER NULL,
    "GreenInRegulation" INTEGER NULL,
    "GrossScore" INTEGER NULL,
    "HoleNumber" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "NetScore" INTEGER NULL,
    "Notes" TEXT NULL,
    "Penalties" INTEGER NULL,
    "Putts" INTEGER NULL,
    "RoundId" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    CONSTRAINT "FK_RoundHoles_Rounds_RoundId" FOREIGN KEY ("RoundId") REFERENCES "Rounds" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_RoundHoles" ("Id", "CreatedAt", "CreatedBy", "FairwayHit", "GreenInRegulation", "GrossScore", "HoleNumber", "IsActive", "NetScore", "Notes", "Penalties", "Putts", "RoundId", "UpdatedAt", "UpdatedBy")
SELECT "Id", "CreatedAt", "CreatedBy", "FairwayHit", "GreenInRegulation", "GrossScore", "HoleNumber", "IsActive", "NetScore", "Notes", "Penalties", "Putts", "RoundId", "UpdatedAt", "UpdatedBy"
FROM "RoundHoles";

CREATE TABLE "ef_temp_RefreshTokens" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_RefreshTokens" PRIMARY KEY,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedByIp" TEXT NULL,
    "ExpiresAt" TEXT NOT NULL,
    "IsRevoked" INTEGER NOT NULL DEFAULT 0,
    "ReplacedByToken" TEXT NULL,
    "RevokedAt" TEXT NULL,
    "RevokedByIp" TEXT NULL,
    "Token" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_RefreshTokens" ("Id", "CreatedAt", "CreatedBy", "CreatedByIp", "ExpiresAt", "IsRevoked", "ReplacedByToken", "RevokedAt", "RevokedByIp", "Token", "UpdatedAt", "UpdatedBy", "UserId")
SELECT "Id", "CreatedAt", "CreatedBy", "CreatedByIp", "ExpiresAt", "IsRevoked", "ReplacedByToken", "RevokedAt", "RevokedByIp", "Token", "UpdatedAt", "UpdatedBy", "UserId"
FROM "RefreshTokens";

CREATE TABLE "ef_temp_Leagues" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Leagues" PRIMARY KEY,
    "ActiveSeasonId" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CustomDomain" TEXT NULL,
    "CustomDomainVerificationToken" TEXT NULL,
    "CustomDomainVerifiedAt" TEXT NULL,
    "Description" TEXT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "Key" TEXT NOT NULL,
    "LogoUrl" TEXT NULL,
    "Name" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "UseCustomDomain" INTEGER NOT NULL
);

INSERT INTO "ef_temp_Leagues" ("Id", "ActiveSeasonId", "CreatedAt", "CreatedBy", "CustomDomain", "CustomDomainVerificationToken", "CustomDomainVerifiedAt", "Description", "IsActive", "Key", "LogoUrl", "Name", "UpdatedAt", "UpdatedBy", "UseCustomDomain")
SELECT "Id", "ActiveSeasonId", "CreatedAt", "CreatedBy", "CustomDomain", "CustomDomainVerificationToken", "CustomDomainVerifiedAt", "Description", "IsActive", "Key", "LogoUrl", "Name", "UpdatedAt", "UpdatedBy", "UseCustomDomain"
FROM "Leagues";

CREATE TABLE "ef_temp_LeagueGolfers" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_LeagueGolfers" PRIMARY KEY,
    "AverageScore" REAL NULL,
    "BestScore" INTEGER NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "DisplayName" TEXT NOT NULL,
    "GolferId" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "JoinedAt" TEXT NOT NULL,
    "LeagueHandicap" REAL NULL,
    "LeagueHandicapUpdatedAt" TEXT NULL,
    "LeagueId" TEXT NOT NULL,
    "Nickname" TEXT NULL,
    "TotalRounds" INTEGER NOT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    CONSTRAINT "FK_LeagueGolfers_Golfers_GolferId" FOREIGN KEY ("GolferId") REFERENCES "Golfers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_LeagueGolfers_Leagues_LeagueId" FOREIGN KEY ("LeagueId") REFERENCES "Leagues" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_LeagueGolfers" ("Id", "AverageScore", "BestScore", "CreatedAt", "CreatedBy", "DisplayName", "GolferId", "IsActive", "JoinedAt", "LeagueHandicap", "LeagueHandicapUpdatedAt", "LeagueId", "Nickname", "TotalRounds", "UpdatedAt", "UpdatedBy")
SELECT "Id", "AverageScore", "BestScore", "CreatedAt", "CreatedBy", "DisplayName", "GolferId", "IsActive", "JoinedAt", "LeagueHandicap", "LeagueHandicapUpdatedAt", "LeagueId", "Nickname", "TotalRounds", "UpdatedAt", "UpdatedBy"
FROM "LeagueGolfers";

CREATE TABLE "ef_temp_HoleTees" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HoleTees" PRIMARY KEY,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "Handicap" INTEGER NOT NULL,
    "HoleNumber" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "Par" INTEGER NOT NULL,
    "TeeId" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "Yardage" INTEGER NOT NULL,
    CONSTRAINT "FK_HoleTees_Holes_HoleNumber" FOREIGN KEY ("HoleNumber") REFERENCES "Holes" ("HoleNumber") ON DELETE CASCADE,
    CONSTRAINT "FK_HoleTees_Tees_TeeId" FOREIGN KEY ("TeeId") REFERENCES "Tees" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_HoleTees" ("Id", "CreatedAt", "CreatedBy", "Handicap", "HoleNumber", "IsActive", "Par", "TeeId", "UpdatedAt", "UpdatedBy", "Yardage")
SELECT "Id", "CreatedAt", "CreatedBy", "Handicap", "HoleNumber", "IsActive", "Par", "TeeId", "UpdatedAt", "UpdatedBy", "Yardage"
FROM "HoleTees";

CREATE TABLE "ef_temp_Holes" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Holes" PRIMARY KEY,
    "CourseId" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "Description" TEXT NULL,
    "GreenBackLatitude" REAL NULL,
    "GreenBackLongitude" REAL NULL,
    "GreenFrontLatitude" REAL NULL,
    "GreenFrontLongitude" REAL NULL,
    "GreenLatitude" REAL NULL,
    "GreenLongitude" REAL NULL,
    "HoleNumber" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "Name" TEXT NULL,
    "TeeLatitude" REAL NULL,
    "TeeLongitude" REAL NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    CONSTRAINT "AK_Holes_HoleNumber" UNIQUE ("HoleNumber"),
    CONSTRAINT "FK_Holes_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Holes" ("Id", "CourseId", "CreatedAt", "CreatedBy", "Description", "GreenBackLatitude", "GreenBackLongitude", "GreenFrontLatitude", "GreenFrontLongitude", "GreenLatitude", "GreenLongitude", "HoleNumber", "IsActive", "Name", "TeeLatitude", "TeeLongitude", "UpdatedAt", "UpdatedBy")
SELECT "Id", "CourseId", "CreatedAt", "CreatedBy", "Description", "GreenBackLatitude", "GreenBackLongitude", "GreenFrontLatitude", "GreenFrontLongitude", "GreenLatitude", "GreenLongitude", "HoleNumber", "IsActive", "Name", "TeeLatitude", "TeeLongitude", "UpdatedAt", "UpdatedBy"
FROM "Holes";

CREATE TABLE "ef_temp_Golfers" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Golfers" PRIMARY KEY,
    "AvatarUrl" TEXT NULL,
    "Bio" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "DisplayName" TEXT NOT NULL,
    "GlobalHandicap" REAL NULL,
    "GlobalHandicapUpdatedAt" TEXT NULL,
    "HomeCity" TEXT NULL,
    "HomeState" TEXT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "Nickname" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "FK_Golfers_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_Golfers" ("Id", "AvatarUrl", "Bio", "CreatedAt", "CreatedBy", "DisplayName", "GlobalHandicap", "GlobalHandicapUpdatedAt", "HomeCity", "HomeState", "IsActive", "Nickname", "PhoneNumber", "UpdatedAt", "UpdatedBy", "UserId")
SELECT "Id", "AvatarUrl", "Bio", "CreatedAt", "CreatedBy", "DisplayName", "GlobalHandicap", "GlobalHandicapUpdatedAt", "HomeCity", "HomeState", "IsActive", "Nickname", "PhoneNumber", "UpdatedAt", "UpdatedBy", "UserId"
FROM "Golfers";

CREATE TABLE "ef_temp_GolferClubs" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_GolferClubs" PRIMARY KEY,
    "AverageDistance" INTEGER NULL,
    "Brand" TEXT NULL,
    "ClubType" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "GolferId" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsInBag" INTEGER NOT NULL DEFAULT 1,
    "Model" TEXT NULL,
    "Notes" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    CONSTRAINT "FK_GolferClubs_Golfers_GolferId" FOREIGN KEY ("GolferId") REFERENCES "Golfers" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_GolferClubs" ("Id", "AverageDistance", "Brand", "ClubType", "CreatedAt", "CreatedBy", "GolferId", "IsActive", "IsInBag", "Model", "Notes", "UpdatedAt", "UpdatedBy")
SELECT "Id", "AverageDistance", "Brand", "ClubType", "CreatedAt", "CreatedBy", "GolferId", "IsActive", "IsInBag", "Model", "Notes", "UpdatedAt", "UpdatedBy"
FROM "GolferClubs";

CREATE TABLE "ef_temp_Courses" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Courses" PRIMARY KEY,
    "Address" TEXT NULL,
    "City" TEXT NULL,
    "Country" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "Description" TEXT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "Key" TEXT NOT NULL,
    "Latitude" REAL NULL,
    "Longitude" REAL NULL,
    "Name" TEXT NOT NULL,
    "NumberOfHoles" INTEGER NOT NULL DEFAULT 18,
    "PhoneNumber" TEXT NULL,
    "PostalCode" TEXT NULL,
    "State" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "WebsiteUrl" TEXT NULL
);

INSERT INTO "ef_temp_Courses" ("Id", "Address", "City", "Country", "CreatedAt", "CreatedBy", "Description", "IsActive", "Key", "Latitude", "Longitude", "Name", "NumberOfHoles", "PhoneNumber", "PostalCode", "State", "UpdatedAt", "UpdatedBy", "WebsiteUrl")
SELECT "Id", "Address", "City", "Country", "CreatedAt", "CreatedBy", "Description", "IsActive", "Key", "Latitude", "Longitude", "Name", "NumberOfHoles", "PhoneNumber", "PostalCode", "State", "UpdatedAt", "UpdatedBy", "WebsiteUrl"
FROM "Courses";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Users";

ALTER TABLE "ef_temp_Users" RENAME TO "Users";

DROP TABLE "UserLeagues";

ALTER TABLE "ef_temp_UserLeagues" RENAME TO "UserLeagues";

DROP TABLE "Tees";

ALTER TABLE "ef_temp_Tees" RENAME TO "Tees";

DROP TABLE "SeasonTeams";

ALTER TABLE "ef_temp_SeasonTeams" RENAME TO "SeasonTeams";

DROP TABLE "Seasons";

ALTER TABLE "ef_temp_Seasons" RENAME TO "Seasons";

DROP TABLE "SeasonGolfers";

ALTER TABLE "ef_temp_SeasonGolfers" RENAME TO "SeasonGolfers";

DROP TABLE "SeasonEvents";

ALTER TABLE "ef_temp_SeasonEvents" RENAME TO "SeasonEvents";

DROP TABLE "Scorecards";

ALTER TABLE "ef_temp_Scorecards" RENAME TO "Scorecards";

DROP TABLE "Rounds";

ALTER TABLE "ef_temp_Rounds" RENAME TO "Rounds";

DROP TABLE "RoundHoles";

ALTER TABLE "ef_temp_RoundHoles" RENAME TO "RoundHoles";

DROP TABLE "RefreshTokens";

ALTER TABLE "ef_temp_RefreshTokens" RENAME TO "RefreshTokens";

DROP TABLE "Leagues";

ALTER TABLE "ef_temp_Leagues" RENAME TO "Leagues";

DROP TABLE "LeagueGolfers";

ALTER TABLE "ef_temp_LeagueGolfers" RENAME TO "LeagueGolfers";

DROP TABLE "HoleTees";

ALTER TABLE "ef_temp_HoleTees" RENAME TO "HoleTees";

DROP TABLE "Holes";

ALTER TABLE "ef_temp_Holes" RENAME TO "Holes";

DROP TABLE "Golfers";

ALTER TABLE "ef_temp_Golfers" RENAME TO "Golfers";

DROP TABLE "GolferClubs";

ALTER TABLE "ef_temp_GolferClubs" RENAME TO "GolferClubs";

DROP TABLE "Courses";

ALTER TABLE "ef_temp_Courses" RENAME TO "Courses";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

CREATE INDEX "IX_UserLeagues_LeagueGolferId" ON "UserLeagues" ("LeagueGolferId");

CREATE INDEX "IX_UserLeagues_LeagueId" ON "UserLeagues" ("LeagueId");

CREATE UNIQUE INDEX "IX_UserLeagues_UserId_LeagueId" ON "UserLeagues" ("UserId", "LeagueId");

CREATE INDEX "IX_Tees_CourseId" ON "Tees" ("CourseId");

CREATE INDEX "IX_SeasonTeams_SeasonId" ON "SeasonTeams" ("SeasonId");

CREATE UNIQUE INDEX "IX_Seasons_LeagueId_Key" ON "Seasons" ("LeagueId", "Key");

CREATE INDEX "IX_SeasonGolfers_GolferId" ON "SeasonGolfers" ("GolferId");

CREATE INDEX "IX_SeasonGolfers_LeagueGolferId" ON "SeasonGolfers" ("LeagueGolferId");

CREATE UNIQUE INDEX "IX_SeasonGolfers_SeasonId_LeagueGolferId" ON "SeasonGolfers" ("SeasonId", "LeagueGolferId");

CREATE INDEX "IX_SeasonGolfers_TeamId" ON "SeasonGolfers" ("TeamId");

CREATE INDEX "IX_SeasonEvents_CourseId" ON "SeasonEvents" ("CourseId");

CREATE INDEX "IX_SeasonEvents_SeasonId" ON "SeasonEvents" ("SeasonId");

CREATE INDEX "IX_SeasonEvents_TeeId" ON "SeasonEvents" ("TeeId");

CREATE UNIQUE INDEX "IX_Scorecards_RoundId" ON "Scorecards" ("RoundId");

CREATE INDEX "IX_Rounds_CourseId" ON "Rounds" ("CourseId");

CREATE INDEX "IX_Rounds_GolferId" ON "Rounds" ("GolferId");

CREATE INDEX "IX_Rounds_LeagueGolferId" ON "Rounds" ("LeagueGolferId");

CREATE INDEX "IX_Rounds_OneTimeEventId" ON "Rounds" ("OneTimeEventId");

CREATE INDEX "IX_Rounds_OneTimeEventTeamId" ON "Rounds" ("OneTimeEventTeamId");

CREATE INDEX "IX_Rounds_TeeId" ON "Rounds" ("TeeId");

CREATE UNIQUE INDEX "IX_RoundHoles_RoundId_HoleNumber" ON "RoundHoles" ("RoundId", "HoleNumber");

CREATE UNIQUE INDEX "IX_RefreshTokens_Token" ON "RefreshTokens" ("Token");

CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");

CREATE UNIQUE INDEX "IX_Leagues_CustomDomain" ON "Leagues" ("CustomDomain") WHERE [CustomDomain] IS NOT NULL;

CREATE UNIQUE INDEX "IX_Leagues_Key" ON "Leagues" ("Key");

CREATE UNIQUE INDEX "IX_LeagueGolfers_GolferId_LeagueId" ON "LeagueGolfers" ("GolferId", "LeagueId");

CREATE INDEX "IX_LeagueGolfers_LeagueId" ON "LeagueGolfers" ("LeagueId");

CREATE INDEX "IX_HoleTees_HoleNumber" ON "HoleTees" ("HoleNumber");

CREATE UNIQUE INDEX "IX_HoleTees_TeeId_HoleNumber" ON "HoleTees" ("TeeId", "HoleNumber");

CREATE UNIQUE INDEX "IX_Holes_CourseId_HoleNumber" ON "Holes" ("CourseId", "HoleNumber");

CREATE UNIQUE INDEX "IX_Golfers_UserId" ON "Golfers" ("UserId");

CREATE INDEX "IX_GolferClubs_GolferId" ON "GolferClubs" ("GolferId");

CREATE UNIQUE INDEX "IX_Courses_Key" ON "Courses" ("Key");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260315215940_UnifiedEventSystem', '10.0.4');

