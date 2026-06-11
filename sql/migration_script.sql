/* =====================================================================
   InvenScan — SQL Server schema creation script
   ---------------------------------------------------------------------
   Equivalent of EF Core migration 20260610164301_InitialCreate.

   You normally do NOT need this file: the application calls
   Database.Migrate() on startup and creates the schema automatically.
   Use this script only for manual / DBA-driven provisioning.

   Run order: 1) this file  2) seed_data.sql
   Target: Microsoft SQL Server 2017+ / Azure SQL.
   ===================================================================== */

IF DB_ID('InvenScanDb') IS NULL
    CREATE DATABASE [InvenScanDb];
GO

USE [InvenScanDb];
GO

/* ---------- Master tables ---------- */

IF OBJECT_ID('dbo.tb_Item', 'U') IS NULL
CREATE TABLE [dbo].[tb_Item] (
    [Id]          INT IDENTITY(1,1)   NOT NULL,
    [ItemCode]    NVARCHAR(50)        NOT NULL,
    [ItemName]    NVARCHAR(200)       NOT NULL,
    [Description] NVARCHAR(MAX)       NOT NULL,
    [Unit]        NVARCHAR(20)        NOT NULL,
    [MinStock]    INT                 NOT NULL,
    [CreatedBy]   NVARCHAR(MAX)       NOT NULL,
    [CreatedAt]   DATETIME2           NOT NULL,
    [UpdatedAt]   DATETIME2           NULL,
    [IsDelete]    BIT                 NOT NULL,
    CONSTRAINT [PK_tb_Item] PRIMARY KEY ([Id])
);
GO

IF OBJECT_ID('dbo.tb_Location', 'U') IS NULL
CREATE TABLE [dbo].[tb_Location] (
    [Id]           INT IDENTITY(1,1)  NOT NULL,
    [LocationCode] NVARCHAR(50)       NOT NULL,
    [LocationName] NVARCHAR(200)      NOT NULL,
    [Description]  NVARCHAR(MAX)      NOT NULL,
    [CreatedBy]    NVARCHAR(MAX)      NOT NULL,
    [CreatedAt]    DATETIME2          NOT NULL,
    [IsDelete]     BIT                NOT NULL,
    CONSTRAINT [PK_tb_Location] PRIMARY KEY ([Id])
);
GO

IF OBJECT_ID('dbo.tb_User', 'U') IS NULL
CREATE TABLE [dbo].[tb_User] (
    [Id]           INT IDENTITY(1,1)  NOT NULL,
    [UserId]       NVARCHAR(50)       NOT NULL,
    [FullName]     NVARCHAR(100)      NOT NULL,
    [PasswordHash] NVARCHAR(MAX)      NOT NULL,
    [Role]         NVARCHAR(20)       NOT NULL,
    [IsActive]     BIT                NOT NULL,
    [CreatedAt]    DATETIME2          NOT NULL,
    CONSTRAINT [PK_tb_User] PRIMARY KEY ([Id])
);
GO

IF OBJECT_ID('dbo.tb_Tag', 'U') IS NULL
CREATE TABLE [dbo].[tb_Tag] (
    [Id]         INT IDENTITY(1,1)    NOT NULL,
    [TagId]      NVARCHAR(100)        NOT NULL,
    [EpcTag]     NVARCHAR(100)        NOT NULL,
    [ItemId]     INT                  NOT NULL,
    [LocationId] INT                  NOT NULL,
    [Status]     NVARCHAR(20)         NOT NULL,
    [CreatedAt]  DATETIME2            NOT NULL,
    [UpdatedAt]  DATETIME2            NULL,
    CONSTRAINT [PK_tb_Tag] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_tb_Tag_tb_Item_ItemId]
        FOREIGN KEY ([ItemId]) REFERENCES [dbo].[tb_Item]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tb_Tag_tb_Location_LocationId]
        FOREIGN KEY ([LocationId]) REFERENCES [dbo].[tb_Location]([Id]) ON DELETE CASCADE
);
GO

/* ---------- Transaction headers ---------- */

IF OBJECT_ID('dbo.tb_StockIn', 'U') IS NULL
CREATE TABLE [dbo].[tb_StockIn] (
    [Id]         INT IDENTITY(1,1)    NOT NULL,
    [DocNumber]  NVARCHAR(50)         NOT NULL,
    [LocationId] INT                  NOT NULL,
    [Notes]      NVARCHAR(MAX)        NOT NULL,
    [CreatedBy]  NVARCHAR(MAX)        NOT NULL,
    [CreatedAt]  DATETIME2            NOT NULL,
    [Status]     NVARCHAR(20)         NOT NULL,
    CONSTRAINT [PK_tb_StockIn] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_tb_StockIn_tb_Location_LocationId]
        FOREIGN KEY ([LocationId]) REFERENCES [dbo].[tb_Location]([Id]) ON DELETE CASCADE
);
GO

IF OBJECT_ID('dbo.tb_StockPrep', 'U') IS NULL
CREATE TABLE [dbo].[tb_StockPrep] (
    [Id]        INT IDENTITY(1,1)     NOT NULL,
    [DocNumber] NVARCHAR(50)          NOT NULL,
    [Notes]     NVARCHAR(MAX)         NOT NULL,
    [Status]    NVARCHAR(20)          NOT NULL,
    [CreatedBy] NVARCHAR(MAX)         NOT NULL,
    [CreatedAt] DATETIME2             NOT NULL,
    CONSTRAINT [PK_tb_StockPrep] PRIMARY KEY ([Id])
);
GO

IF OBJECT_ID('dbo.tb_StockTaking', 'U') IS NULL
CREATE TABLE [dbo].[tb_StockTaking] (
    [Id]          INT IDENTITY(1,1)   NOT NULL,
    [SessionCode] NVARCHAR(50)        NOT NULL,
    [Remark]      NVARCHAR(MAX)       NOT NULL,
    [Status]      NVARCHAR(20)        NOT NULL,
    [CreatedBy]   NVARCHAR(MAX)       NOT NULL,
    [CreatedAt]   DATETIME2           NOT NULL,
    [ClosedAt]    DATETIME2           NULL,
    CONSTRAINT [PK_tb_StockTaking] PRIMARY KEY ([Id])
);
GO

/* ---------- Transaction details ---------- */

IF OBJECT_ID('dbo.tb_StockInDetail', 'U') IS NULL
CREATE TABLE [dbo].[tb_StockInDetail] (
    [Id]          INT IDENTITY(1,1)   NOT NULL,
    [StockInId]   INT                 NOT NULL,
    [TagId]       INT                 NULL,
    [ItemId]      INT                 NOT NULL,
    [ScannedCode] NVARCHAR(MAX)       NOT NULL,
    [ScanType]    NVARCHAR(20)        NOT NULL,
    [CreatedAt]   DATETIME2           NOT NULL,
    CONSTRAINT [PK_tb_StockInDetail] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_tb_StockInDetail_tb_Item_ItemId]
        FOREIGN KEY ([ItemId]) REFERENCES [dbo].[tb_Item]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tb_StockInDetail_tb_StockIn_StockInId]
        FOREIGN KEY ([StockInId]) REFERENCES [dbo].[tb_StockIn]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tb_StockInDetail_tb_Tag_TagId]
        FOREIGN KEY ([TagId]) REFERENCES [dbo].[tb_Tag]([Id])
);
GO

IF OBJECT_ID('dbo.tb_StockPrepDetail', 'U') IS NULL
CREATE TABLE [dbo].[tb_StockPrepDetail] (
    [Id]           INT IDENTITY(1,1)  NOT NULL,
    [StockPrepId]  INT                NOT NULL,
    [ItemId]       INT                NOT NULL,
    [LocationId]   INT                NOT NULL,
    [RequestedQty] INT                NOT NULL,
    [PickedQty]    INT                NOT NULL,
    [Status]       NVARCHAR(20)       NOT NULL,
    [ScannedCode]  NVARCHAR(MAX)      NOT NULL,
    [CreatedBy]    NVARCHAR(MAX)      NOT NULL,
    [UpdatedAt]    DATETIME2          NULL,
    CONSTRAINT [PK_tb_StockPrepDetail] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_tb_StockPrepDetail_tb_Item_ItemId]
        FOREIGN KEY ([ItemId]) REFERENCES [dbo].[tb_Item]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tb_StockPrepDetail_tb_Location_LocationId]
        FOREIGN KEY ([LocationId]) REFERENCES [dbo].[tb_Location]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tb_StockPrepDetail_tb_StockPrep_StockPrepId]
        FOREIGN KEY ([StockPrepId]) REFERENCES [dbo].[tb_StockPrep]([Id]) ON DELETE CASCADE
);
GO

IF OBJECT_ID('dbo.tb_StockTakingDetail', 'U') IS NULL
CREATE TABLE [dbo].[tb_StockTakingDetail] (
    [Id]        INT IDENTITY(1,1)     NOT NULL,
    [SttId]     INT                   NOT NULL,
    [TagId]     INT                   NOT NULL,
    [ItemId]    INT                   NOT NULL,
    [Action]    NVARCHAR(20)          NOT NULL,
    [ScannedAt] DATETIME2             NULL,
    [CreatedBy] NVARCHAR(MAX)         NOT NULL,
    CONSTRAINT [PK_tb_StockTakingDetail] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_tb_StockTakingDetail_tb_Item_ItemId]
        FOREIGN KEY ([ItemId]) REFERENCES [dbo].[tb_Item]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tb_StockTakingDetail_tb_StockTaking_SttId]
        FOREIGN KEY ([SttId]) REFERENCES [dbo].[tb_StockTaking]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tb_StockTakingDetail_tb_Tag_TagId]
        FOREIGN KEY ([TagId]) REFERENCES [dbo].[tb_Tag]([Id]) ON DELETE CASCADE
);
GO

/* ---------- Indexes ---------- */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_Item_ItemCode')
    CREATE UNIQUE INDEX [IX_tb_Item_ItemCode] ON [dbo].[tb_Item]([ItemCode]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_Location_LocationCode')
    CREATE UNIQUE INDEX [IX_tb_Location_LocationCode] ON [dbo].[tb_Location]([LocationCode]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_User_UserId')
    CREATE UNIQUE INDEX [IX_tb_User_UserId] ON [dbo].[tb_User]([UserId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_Tag_TagId')
    CREATE UNIQUE INDEX [IX_tb_Tag_TagId] ON [dbo].[tb_Tag]([TagId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_Tag_EpcTag')
    CREATE UNIQUE INDEX [IX_tb_Tag_EpcTag] ON [dbo].[tb_Tag]([EpcTag]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_Tag_ItemId')
    CREATE INDEX [IX_tb_Tag_ItemId] ON [dbo].[tb_Tag]([ItemId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_Tag_LocationId')
    CREATE INDEX [IX_tb_Tag_LocationId] ON [dbo].[tb_Tag]([LocationId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_StockIn_DocNumber')
    CREATE UNIQUE INDEX [IX_tb_StockIn_DocNumber] ON [dbo].[tb_StockIn]([DocNumber]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_StockIn_LocationId')
    CREATE INDEX [IX_tb_StockIn_LocationId] ON [dbo].[tb_StockIn]([LocationId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_StockInDetail_ItemId')
    CREATE INDEX [IX_tb_StockInDetail_ItemId] ON [dbo].[tb_StockInDetail]([ItemId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_StockInDetail_StockInId')
    CREATE INDEX [IX_tb_StockInDetail_StockInId] ON [dbo].[tb_StockInDetail]([StockInId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_StockInDetail_TagId')
    CREATE INDEX [IX_tb_StockInDetail_TagId] ON [dbo].[tb_StockInDetail]([TagId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_StockPrep_DocNumber')
    CREATE UNIQUE INDEX [IX_tb_StockPrep_DocNumber] ON [dbo].[tb_StockPrep]([DocNumber]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_StockPrepDetail_ItemId')
    CREATE INDEX [IX_tb_StockPrepDetail_ItemId] ON [dbo].[tb_StockPrepDetail]([ItemId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_StockPrepDetail_LocationId')
    CREATE INDEX [IX_tb_StockPrepDetail_LocationId] ON [dbo].[tb_StockPrepDetail]([LocationId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_StockPrepDetail_StockPrepId')
    CREATE INDEX [IX_tb_StockPrepDetail_StockPrepId] ON [dbo].[tb_StockPrepDetail]([StockPrepId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_StockTaking_SessionCode')
    CREATE UNIQUE INDEX [IX_tb_StockTaking_SessionCode] ON [dbo].[tb_StockTaking]([SessionCode]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_StockTakingDetail_ItemId')
    CREATE INDEX [IX_tb_StockTakingDetail_ItemId] ON [dbo].[tb_StockTakingDetail]([ItemId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_StockTakingDetail_SttId')
    CREATE INDEX [IX_tb_StockTakingDetail_SttId] ON [dbo].[tb_StockTakingDetail]([SttId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tb_StockTakingDetail_TagId')
    CREATE INDEX [IX_tb_StockTakingDetail_TagId] ON [dbo].[tb_StockTakingDetail]([TagId]);
GO

/* ---------- EF Core migrations history ----------
   Registers the InitialCreate migration so the application's
   Database.Migrate() call recognises the schema as already applied. */

IF OBJECT_ID('dbo.__EFMigrationsHistory', 'U') IS NULL
CREATE TABLE [dbo].[__EFMigrationsHistory] (
    [MigrationId]    NVARCHAR(150) NOT NULL,
    [ProductVersion] NVARCHAR(32)  NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
);
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260610164301_InitialCreate')
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260610164301_InitialCreate', N'8.0.0');
GO
