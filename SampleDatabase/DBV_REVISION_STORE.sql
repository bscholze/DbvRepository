﻿CREATE TABLE [dbo].[DBV_REVISION_STORE]
(
	[REVISION] INT NOT NULL PRIMARY KEY IDENTITY, 
    [NAME] NVARCHAR(255) NOT NULL, 
    [NAME_LC] NVARCHAR(255) NOT NULL,
    [COMMENTS] NVARCHAR(MAX) NOT NULL, 
    [AUTHOR] NVARCHAR(50) NOT NULL, 
    [TIMESTAMP] DATETIME NOT NULL, 
    [DELETED] NCHAR(1) NOT NULL DEFAULT 'N',
	[CONTENT] VARBINARY(MAX) NULL, 
    CONSTRAINT [CK_DBV_REVISION_STORE_DELETED] CHECK (DELETED IN ('N','Y')) 
)

GO

CREATE NONCLUSTERED INDEX [IX_DBV_REVISION_STORE_NAME] ON [dbo].[DBV_REVISION_STORE] ([NAME_LC])

GO
