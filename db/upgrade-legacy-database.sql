/*
    One-time upgrade of the existing (.NET Framework / EF6 / ASP.NET Identity 2) GotFired database
    so the .NET 10 app (EF Core + ASP.NET Core Identity) can use it without data loss.

    - Adds the ASP.NET Core Identity columns, indexes and tables to the Identity v2 tables.
    - Records the EF Core "Baseline" migration as applied (the schema already exists).

    Safe to run more than once. Take a full backup first.
    Additive only for the application tables: the old .NET Framework app keeps working against the
    upgraded database (the extra columns/tables are ignored by EF6), which keeps rollback possible.

    Usage:  sqlcmd -S <server> -d <database> -U <user> -i db/upgrade-legacy-database.sql -b
*/
SET XACT_ABORT ON;
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON; -- required for the filtered indexes (sqlcmd defaults it to OFF)
SET ANSI_NULLS ON;
BEGIN TRANSACTION;

------------------------------------------------------------------------------------------------
-- AspNetUsers
------------------------------------------------------------------------------------------------
IF COL_LENGTH('dbo.AspNetUsers', 'NormalizedUserName') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD NormalizedUserName nvarchar(256) NULL;
IF COL_LENGTH('dbo.AspNetUsers', 'NormalizedEmail') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD NormalizedEmail nvarchar(256) NULL;
IF COL_LENGTH('dbo.AspNetUsers', 'ConcurrencyStamp') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD ConcurrencyStamp nvarchar(max) NULL;
IF COL_LENGTH('dbo.AspNetUsers', 'LockoutEnd') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD LockoutEnd datetimeoffset NULL;
GO

-- Core Identity looks users up by the upper-invariant normalized name. UPPER() matches that for
-- ASCII user names; check non-ASCII names after running (see the query at the end).
UPDATE dbo.AspNetUsers SET NormalizedUserName = UPPER(UserName) WHERE NormalizedUserName IS NULL;
UPDATE dbo.AspNetUsers SET NormalizedEmail = UPPER(Email) WHERE NormalizedEmail IS NULL AND Email IS NOT NULL;
UPDATE dbo.AspNetUsers SET ConcurrencyStamp = CONVERT(nvarchar(36), NEWID()) WHERE ConcurrencyStamp IS NULL;
-- Identity v2 stored lockout end as UTC datetime. LockoutEndDateUtc is kept for the old app (rollback).
IF COL_LENGTH('dbo.AspNetUsers', 'LockoutEndDateUtc') IS NOT NULL
    EXEC('UPDATE dbo.AspNetUsers SET LockoutEnd = TODATETIMEOFFSET(LockoutEndDateUtc, 0)
          WHERE LockoutEnd IS NULL AND LockoutEndDateUtc IS NOT NULL');

-- v2 UserNameIndex was UNIQUE on UserName; Core's is UNIQUE on NormalizedUserName.
IF EXISTS (SELECT 1 FROM sys.indexes i
           JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
           JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
           WHERE i.object_id = OBJECT_ID('dbo.AspNetUsers') AND i.name = 'UserNameIndex' AND c.name = 'UserName')
    DROP INDEX UserNameIndex ON dbo.AspNetUsers;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'UserNameIndex')
    CREATE UNIQUE INDEX UserNameIndex ON dbo.AspNetUsers (NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'EmailIndex')
    CREATE INDEX EmailIndex ON dbo.AspNetUsers (NormalizedEmail);
GO

------------------------------------------------------------------------------------------------
-- AspNetRoles
------------------------------------------------------------------------------------------------
IF COL_LENGTH('dbo.AspNetRoles', 'NormalizedName') IS NULL
    ALTER TABLE dbo.AspNetRoles ADD NormalizedName nvarchar(256) NULL;
IF COL_LENGTH('dbo.AspNetRoles', 'ConcurrencyStamp') IS NULL
    ALTER TABLE dbo.AspNetRoles ADD ConcurrencyStamp nvarchar(max) NULL;
GO

UPDATE dbo.AspNetRoles SET NormalizedName = UPPER(Name) WHERE NormalizedName IS NULL;
UPDATE dbo.AspNetRoles SET ConcurrencyStamp = CONVERT(nvarchar(36), NEWID()) WHERE ConcurrencyStamp IS NULL;

IF EXISTS (SELECT 1 FROM sys.indexes i
           JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
           JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
           WHERE i.object_id = OBJECT_ID('dbo.AspNetRoles') AND i.name = 'RoleNameIndex' AND c.name = 'Name')
    DROP INDEX RoleNameIndex ON dbo.AspNetRoles;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.AspNetRoles') AND name = 'RoleNameIndex')
    CREATE UNIQUE INDEX RoleNameIndex ON dbo.AspNetRoles (NormalizedName) WHERE NormalizedName IS NOT NULL;
GO

------------------------------------------------------------------------------------------------
-- AspNetUserLogins: Core keys on (LoginProvider, ProviderKey) and adds ProviderDisplayName
------------------------------------------------------------------------------------------------
IF COL_LENGTH('dbo.AspNetUserLogins', 'ProviderDisplayName') IS NULL
    ALTER TABLE dbo.AspNetUserLogins ADD ProviderDisplayName nvarchar(max) NULL;

IF (SELECT COUNT(*) FROM sys.index_columns ic
    JOIN sys.key_constraints k ON k.parent_object_id = ic.object_id AND k.unique_index_id = ic.index_id
    WHERE k.name = 'PK_dbo.AspNetUserLogins') = 3
BEGIN
    ALTER TABLE dbo.AspNetUserLogins DROP CONSTRAINT [PK_dbo.AspNetUserLogins];
    ALTER TABLE dbo.AspNetUserLogins ADD CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY (LoginProvider, ProviderKey);
END
GO

------------------------------------------------------------------------------------------------
-- AspNetUserRoles: v2 had a separate IX_UserId, redundant with the (UserId, RoleId) primary key
------------------------------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.AspNetUserRoles') AND name = 'IX_UserId')
    DROP INDEX IX_UserId ON dbo.AspNetUserRoles;
GO

------------------------------------------------------------------------------------------------
-- New Core Identity tables
------------------------------------------------------------------------------------------------
IF OBJECT_ID('dbo.AspNetRoleClaims') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetRoleClaims (
        Id int IDENTITY(1,1) NOT NULL,
        RoleId nvarchar(128) NOT NULL,
        ClaimType nvarchar(max) NULL,
        ClaimValue nvarchar(max) NULL,
        CONSTRAINT [PK_dbo.AspNetRoleClaims] PRIMARY KEY (Id),
        CONSTRAINT [FK_dbo.AspNetRoleClaims_dbo.AspNetRoles_RoleId] FOREIGN KEY (RoleId)
            REFERENCES dbo.AspNetRoles (Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_RoleId ON dbo.AspNetRoleClaims (RoleId);
END

IF OBJECT_ID('dbo.AspNetUserTokens') IS NULL
    CREATE TABLE dbo.AspNetUserTokens (
        UserId nvarchar(128) NOT NULL,
        LoginProvider nvarchar(128) NOT NULL,
        Name nvarchar(128) NOT NULL,
        Value nvarchar(max) NULL,
        CONSTRAINT [PK_dbo.AspNetUserTokens] PRIMARY KEY (UserId, LoginProvider, Name),
        CONSTRAINT [FK_dbo.AspNetUserTokens_dbo.AspNetUsers_UserId] FOREIGN KEY (UserId)
            REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE
    );
GO

------------------------------------------------------------------------------------------------
-- EF Core migration history: the Baseline migration describes the schema that now exists.
------------------------------------------------------------------------------------------------
IF OBJECT_ID('dbo.__EFMigrationsHistory') IS NULL
    CREATE TABLE dbo.__EFMigrationsHistory (
        MigrationId nvarchar(150) NOT NULL,
        ProductVersion nvarchar(32) NOT NULL,
        CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
    );

IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = '20261118213655_Baseline')
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20261118213655_Baseline', '10.0.15');
GO

COMMIT TRANSACTION;
GO

-- Review: user names whose UPPER() may differ from .NET's ToUpperInvariant (non-ASCII characters).
SELECT Id, UserName, NormalizedUserName FROM dbo.AspNetUsers
WHERE UserName COLLATE Latin1_General_BIN2 LIKE '%[^ -~]%';
