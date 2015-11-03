﻿CREATE TABLE [dbo].[DBV_REPOSITORY]
(
	[ID] INT NOT NULL PRIMARY KEY IDENTITY, 
    [NAME] NVARCHAR(255) NOT NULL, 
    [TAGS] NVARCHAR(255) NULL, 
    [DESCRIPTION] NVARCHAR(MAX) NULL, 
    DATA VARBINARY(MAX) NULL, 
    USERNAME NCHAR(50) NOT NULL, 
    CREATEDTMS DATETIME NOT NULL
)

GO

CREATE CLUSTERED INDEX [IX_DBV_REPOSITORY_NAME] ON [dbo].[DBV_REPOSITORY] ([NAME], [ID])