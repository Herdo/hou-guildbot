﻿CREATE TABLE [config].[Game]
(
	[GameID] SMALLINT NOT NULL IDENTITY (1, 1),
	[LongName] VARCHAR(512) NOT NULL,
	[ShortName] VARCHAR(16) NOT NULL,
	[ModifiedByUserID] INT NOT NULL,
	[ModifiedAtTimestamp] DATETIME2 NOT NULL,
	[PrimaryGameDiscordRoleID] DECIMAL(20,0) NULL,
	[IncludeInGuildMembersStatistic] BIT NOT NULL,
	[IncludeInGamesMenu] BIT NOT NULL,
	[GameInterestEmojiName] VARCHAR(256) NULL,
	[GameInterestRoleId] DECIMAL(20,0) NULL,
	CONSTRAINT PK_Game_GameID PRIMARY KEY (GameID),
	CONSTRAINT UQ_Game_LongName UNIQUE (LongName),
	CONSTRAINT UQ_Game_ShortName UNIQUE (ShortName),
	CONSTRAINT FK_Game_User_ModifiedByUserID FOREIGN KEY (ModifiedByUserID) REFERENCES [hou].[User] (UserID)	
)