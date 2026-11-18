/*
  TEST FIXTURE ONLY. Recreates the schema that EF6 migration 201808141943456_sgk and ASP.NET Identity 2
  produced (same types, constraint and index names), plus a little data. Matches the 2025-08 production backup:
  no FK/index on DismissalCases.SGKTerminationReasonID; create the database with COLLATE Turkish_CI_AS and
  COMPATIBILITY_LEVEL 100 to match production. The user names contain an "i" on purpose (Turkish casing). so db/upgrade-legacy-database.sql
  and the .NET 10 app can be verified without a production backup. Login: legacy-admin / legacy-pass
*/
CREATE TABLE [dbo].[AnswerTemplates] (
        [ID] [int] NOT NULL IDENTITY,
        [Name] [nvarchar](max),
        [Content] [nvarchar](max),
        [Editor] [int] NOT NULL,
        [Creator] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [EditedDate] [datetime] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [RowVersion] [nvarchar](16) NOT NULL,
        CONSTRAINT [PK_dbo.AnswerTemplates] PRIMARY KEY ([ID])
);
CREATE TABLE [dbo].[Applicants] (
        [ID] [int] NOT NULL IDENTITY,
        [AgeInterval] [tinyint],
        [EducationalState] [tinyint],
        [Gender] [tinyint] NOT NULL,
        [Email] [nvarchar](max),
        [PhoneNumber] [nvarchar](max),
        [FullName] [nvarchar](max),
        [PositionId] [int],
        [Editor] [int] NOT NULL,
        [Creator] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [EditedDate] [datetime] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [RowVersion] [nvarchar](16) NOT NULL,
        CONSTRAINT [PK_dbo.Applicants] PRIMARY KEY ([ID])
);
CREATE TABLE [dbo].[Categories] (
        [ID] [int] NOT NULL IDENTITY,
        [Name] [nvarchar](max),
        [Editor] [int] NOT NULL,
        [Creator] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [EditedDate] [datetime] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [RowVersion] [nvarchar](16) NOT NULL,
        CONSTRAINT [PK_dbo.Categories] PRIMARY KEY ([ID])
);
CREATE TABLE [dbo].[Cities] (
        [ID] [int] NOT NULL IDENTITY,
        [Name] [nvarchar](max),
        [Editor] [int] NOT NULL,
        [Creator] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [EditedDate] [datetime] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [RowVersion] [nvarchar](16) NOT NULL,
        CONSTRAINT [PK_dbo.Cities] PRIMARY KEY ([ID])
);
CREATE TABLE [dbo].[CompanySectors] (
        [ID] [int] NOT NULL IDENTITY,
        [Name] [nvarchar](max),
        [Editor] [int] NOT NULL,
        [Creator] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [EditedDate] [datetime] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [RowVersion] [nvarchar](16) NOT NULL,
        CONSTRAINT [PK_dbo.CompanySectors] PRIMARY KEY ([ID])
);
CREATE TABLE [dbo].[DeclaredTerminationReasons] (
        [ID] [int] NOT NULL IDENTITY,
        [Name] [nvarchar](max),
        [Editor] [int] NOT NULL,
        [Creator] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [EditedDate] [datetime] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [RowVersion] [nvarchar](16) NOT NULL,
        CONSTRAINT [PK_dbo.DeclaredTerminationReasons] PRIMARY KEY ([ID])
);
CREATE TABLE [dbo].[SupportedBies] (
        [ID] [int] NOT NULL IDENTITY,
        [Name] [nvarchar](max),
        [Editor] [int] NOT NULL,
        [Creator] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [EditedDate] [datetime] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [RowVersion] [nvarchar](16) NOT NULL,
        CONSTRAINT [PK_dbo.SupportedBies] PRIMARY KEY ([ID])
);
CREATE TABLE [dbo].[Tags] (
        [ID] [int] NOT NULL IDENTITY,
        [Name] [nvarchar](max),
        [Editor] [int] NOT NULL,
        [Creator] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [EditedDate] [datetime] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [RowVersion] [nvarchar](16) NOT NULL,
        CONSTRAINT [PK_dbo.Tags] PRIMARY KEY ([ID])
);
CREATE TABLE [dbo].[SGKTerminationReasons] (
        [ID] [int] NOT NULL IDENTITY,
        [Code] [nvarchar](max),
        [Name] [nvarchar](max),
        [Editor] [int] NOT NULL,
        [Creator] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [EditedDate] [datetime] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [RowVersion] [nvarchar](16) NOT NULL,
        CONSTRAINT [PK_dbo.SGKTerminationReasons] PRIMARY KEY ([ID])
);
CREATE TABLE [dbo].[Comments] (
        [ID] [int] NOT NULL IDENTITY,
        [DismissalCaseID] [int] NOT NULL,
        [Text] [nvarchar](max),
        [UserName] [nvarchar](max),
        [Editor] [int] NOT NULL,
        [Creator] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [EditedDate] [datetime] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [RowVersion] [nvarchar](16) NOT NULL,
        CONSTRAINT [PK_dbo.Comments] PRIMARY KEY ([ID])
);
CREATE TABLE [dbo].[DismissalCases] (
        [ID] [int] NOT NULL IDENTITY,
        [Guid] [nvarchar](max),
        [ApplicantId] [int] NOT NULL,
        [CityID] [int] NOT NULL,
        [EmploymentTerminationDate] [datetime],
        [CompanySectorID] [int],
        [DeclaredTerminationReasonID] [int],
        [OtherDeclaredTerminationReason] [nvarchar](max),
        [CategoryID] [int],
        [EmploymentDurationSince] [tinyint] NOT NULL,
        [EmployeeCount] [tinyint] NOT NULL,
        [CompanySectorName] [nvarchar](max),
        [CompanyDescription] [nvarchar](max),
        [HasSignedAnyDocument] [bit] NOT NULL,
        [SignedDocuments] [nvarchar](max),
        [DismissalState] [tinyint] NOT NULL,
        [WantShareHisExperience] [bit] NOT NULL,
        [DetailedExperience] [nvarchar](max),
        [AdditionalInfo] [nvarchar](max),
        [AppealState] [tinyint] NOT NULL,
        [Notes] [nvarchar](max),
        [SGKTerminationReasonID] [int],
        [Editor] [int] NOT NULL,
        [Creator] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [EditedDate] [datetime] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [RowVersion] [nvarchar](16) NOT NULL,
        CONSTRAINT [PK_dbo.DismissalCases] PRIMARY KEY ([ID])
);
CREATE TABLE [dbo].[DismissalCaseSupprtedBies] (
        [ID] [int] NOT NULL IDENTITY,
        [DismissalCaseId] [int] NOT NULL,
        [SupportedById] [int] NOT NULL,
        [Editor] [int] NOT NULL,
        [Creator] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [EditedDate] [datetime] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [RowVersion] [nvarchar](16) NOT NULL,
        CONSTRAINT [PK_dbo.DismissalCaseSupprtedBies] PRIMARY KEY ([ID])
);
CREATE TABLE [dbo].[DismissalCaseTags] (
        [ID] [int] NOT NULL IDENTITY,
        [DismissalCaseID] [int] NOT NULL,
        [TagID] [int] NOT NULL,
        [Editor] [int] NOT NULL,
        [Creator] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [EditedDate] [datetime] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [RowVersion] [nvarchar](16) NOT NULL,
        CONSTRAINT [PK_dbo.DismissalCaseTags] PRIMARY KEY ([ID])
);
CREATE TABLE [dbo].[AspNetRoles] (
        [Id] [nvarchar](128) NOT NULL,
        [Name] [nvarchar](256) NOT NULL,
        CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY ([Id])
);
CREATE TABLE [dbo].[AspNetUserRoles] (
        [UserId] [nvarchar](128) NOT NULL,
        [RoleId] [nvarchar](128) NOT NULL,
        CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId])
);
CREATE TABLE [dbo].[AspNetUsers] (
        [Id] [nvarchar](128) NOT NULL,
        [FriendlyName] [nvarchar](max),
        [Email] [nvarchar](256),
        [EmailConfirmed] [bit] NOT NULL,
        [PasswordHash] [nvarchar](max),
        [SecurityStamp] [nvarchar](max),
        [PhoneNumber] [nvarchar](max),
        [PhoneNumberConfirmed] [bit] NOT NULL,
        [TwoFactorEnabled] [bit] NOT NULL,
        [LockoutEndDateUtc] [datetime],
        [LockoutEnabled] [bit] NOT NULL,
        [AccessFailedCount] [int] NOT NULL,
        [UserName] [nvarchar](256) NOT NULL,
        CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY ([Id])
);
CREATE TABLE [dbo].[AspNetUserClaims] (
        [Id] [int] NOT NULL IDENTITY,
        [UserId] [nvarchar](128) NOT NULL,
        [ClaimType] [nvarchar](max),
        [ClaimValue] [nvarchar](max),
        CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY ([Id])
);
CREATE TABLE [dbo].[AspNetUserLogins] (
        [LoginProvider] [nvarchar](128) NOT NULL,
        [ProviderKey] [nvarchar](128) NOT NULL,
        [UserId] [nvarchar](128) NOT NULL,
        CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey], [UserId])
);
CREATE TABLE [dbo].[__MigrationHistory] (
        [MigrationId] [nvarchar](150) NOT NULL,
        [ContextKey] [nvarchar](300) NOT NULL,
        [Model] [varbinary](max) NOT NULL,
        [ProductVersion] [nvarchar](32) NOT NULL,
        CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY ([MigrationId], [ContextKey])
);
CREATE INDEX [IX_DismissalCaseID] ON [dbo].[Comments]([DismissalCaseID]);
CREATE INDEX [IX_ApplicantId] ON [dbo].[DismissalCases]([ApplicantId]);
CREATE INDEX [IX_CityID] ON [dbo].[DismissalCases]([CityID]);
CREATE INDEX [IX_CompanySectorID] ON [dbo].[DismissalCases]([CompanySectorID]);
CREATE INDEX [IX_DeclaredTerminationReasonID] ON [dbo].[DismissalCases]([DeclaredTerminationReasonID]);
CREATE INDEX [IX_CategoryID] ON [dbo].[DismissalCases]([CategoryID]);
CREATE INDEX [IX_DismissalCaseId] ON [dbo].[DismissalCaseSupprtedBies]([DismissalCaseId]);
CREATE INDEX [IX_SupportedById] ON [dbo].[DismissalCaseSupprtedBies]([SupportedById]);
CREATE INDEX [IX_DismissalCaseID] ON [dbo].[DismissalCaseTags]([DismissalCaseID]);
CREATE INDEX [IX_TagID] ON [dbo].[DismissalCaseTags]([TagID]);
CREATE INDEX [IX_UserId] ON [dbo].[AspNetUserRoles]([UserId]);
CREATE INDEX [IX_RoleId] ON [dbo].[AspNetUserRoles]([RoleId]);
CREATE INDEX [IX_UserId] ON [dbo].[AspNetUserClaims]([UserId]);
CREATE INDEX [IX_UserId] ON [dbo].[AspNetUserLogins]([UserId]);
CREATE UNIQUE INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]([Name]);
CREATE UNIQUE INDEX [UserNameIndex] ON [dbo].[AspNetUsers]([UserName]);
ALTER TABLE [dbo].[Comments] ADD CONSTRAINT [FK_dbo.Comments_dbo.DismissalCases_DismissalCaseID] FOREIGN KEY ([DismissalCaseID]) REFERENCES [dbo].[DismissalCases] ([ID]) ON DELETE CASCADE;
ALTER TABLE [dbo].[DismissalCases] ADD CONSTRAINT [FK_dbo.DismissalCases_dbo.Applicants_ApplicantId] FOREIGN KEY ([ApplicantId]) REFERENCES [dbo].[Applicants] ([ID]) ON DELETE CASCADE;
ALTER TABLE [dbo].[DismissalCases] ADD CONSTRAINT [FK_dbo.DismissalCases_dbo.Categories_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Categories] ([ID]);
ALTER TABLE [dbo].[DismissalCases] ADD CONSTRAINT [FK_dbo.DismissalCases_dbo.Cities_CityID] FOREIGN KEY ([CityID]) REFERENCES [dbo].[Cities] ([ID]) ON DELETE CASCADE;
ALTER TABLE [dbo].[DismissalCases] ADD CONSTRAINT [FK_dbo.DismissalCases_dbo.CompanySectors_CompanySectorID] FOREIGN KEY ([CompanySectorID]) REFERENCES [dbo].[CompanySectors] ([ID]);
ALTER TABLE [dbo].[DismissalCases] ADD CONSTRAINT [FK_dbo.DismissalCases_dbo.DeclaredTerminationReasons_DeclaredTerminationReasonID] FOREIGN KEY ([DeclaredTerminationReasonID]) REFERENCES [dbo].[DeclaredTerminationReasons] ([ID]);
ALTER TABLE [dbo].[DismissalCaseSupprtedBies] ADD CONSTRAINT [FK_dbo.DismissalCaseSupprtedBies_dbo.DismissalCases_DismissalCaseId] FOREIGN KEY ([DismissalCaseId]) REFERENCES [dbo].[DismissalCases] ([ID]) ON DELETE CASCADE;
ALTER TABLE [dbo].[DismissalCaseSupprtedBies] ADD CONSTRAINT [FK_dbo.DismissalCaseSupprtedBies_dbo.SupportedBies_SupportedById] FOREIGN KEY ([SupportedById]) REFERENCES [dbo].[SupportedBies] ([ID]) ON DELETE CASCADE;
ALTER TABLE [dbo].[DismissalCaseTags] ADD CONSTRAINT [FK_dbo.DismissalCaseTags_dbo.DismissalCases_DismissalCaseID] FOREIGN KEY ([DismissalCaseID]) REFERENCES [dbo].[DismissalCases] ([ID]) ON DELETE CASCADE;
ALTER TABLE [dbo].[DismissalCaseTags] ADD CONSTRAINT [FK_dbo.DismissalCaseTags_dbo.Tags_TagID] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([ID]) ON DELETE CASCADE;
ALTER TABLE [dbo].[AspNetUserRoles] ADD CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[AspNetUserRoles] ADD CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[AspNetUserClaims] ADD CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[AspNetUserLogins] ADD CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
INSERT [dbo].[__MigrationHistory] VALUES ('201808141943456_sgk', 'GotFired.DAL.Migrations.Configuration', 0x00, '6.1.3-40302');

-- Data
INSERT [dbo].[AspNetRoles] (Id, Name) VALUES ('r-admin','admin'), ('r-editor','editor'), ('r-user','user');
INSERT [dbo].[AspNetUsers] (Id, FriendlyName, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName)
VALUES ('u-1', 'Legacy Admin', NULL, 0, 'AAABAgMEBQYHCAkKCwwNDg/2fSLMK7VIkOgyZRyEJLw9br5Kj2ULO7QdQxTkcZ6iVA==', 'b0f2d7a5-0000-0000-0000-000000000000', 0, 0, 0, 0, 'legacy-admin'),
       ('u-2', 'Legacy User', 'user@example.com', 0, 'AAABAgMEBQYHCAkKCwwNDg/2fSLMK7VIkOgyZRyEJLw9br5Kj2ULO7QdQxTkcZ6iVA==', 'b0f2d7a5-0000-0000-0000-000000000001', 0, 0, 0, 0, 'legacy-user');
INSERT [dbo].[AspNetUserRoles] VALUES ('u-1','r-admin'), ('u-2','r-user');
SET IDENTITY_INSERT [dbo].[Cities] ON; INSERT [dbo].[Cities] (ID, Name, Editor, Creator, CreatedDate, EditedDate, IsDeleted, IsActive, RowVersion) VALUES (1, N'İstanbul', 0, 0, GETDATE(), GETDATE(), 0, 1, 'A1B2C3D411111111'); SET IDENTITY_INSERT [dbo].[Cities] OFF;
INSERT [dbo].[SupportedBies] (Name, Editor, Creator, CreatedDate, EditedDate, IsDeleted, IsActive, RowVersion) VALUES (N'Sendika', 0, 0, GETDATE(), GETDATE(), 0, 1, 'A1B2C3D411111111');
INSERT [dbo].[SGKTerminationReasons] (Code, Name, Editor, Creator, CreatedDate, EditedDate, IsDeleted, IsActive, RowVersion) VALUES ('04', N'Haklı sebep bildirilmeden fesih', 0, 0, GETDATE(), GETDATE(), 0, 1, 'A1B2C3D411111111');
INSERT [dbo].[DeclaredTerminationReasons] (Name, Editor, Creator, CreatedDate, EditedDate, IsDeleted, IsActive, RowVersion) VALUES (N'Performans Düşüklüğü', 0, 0, GETDATE(), GETDATE(), 0, 1, 'A1B2C3D411111111');
INSERT [dbo].[AnswerTemplates] (Name, Content, Editor, Creator, CreatedDate, EditedDate, IsDeleted, IsActive, RowVersion) VALUES (N'Karşılama', N'<p>Merhaba</p>', 0, 0, GETDATE(), GETDATE(), 0, 1, 'A1B2C3D411111111');
INSERT [dbo].[Tags] (Name, Editor, Creator, CreatedDate, EditedDate, IsDeleted, IsActive, RowVersion) VALUES (N'tazminat', 0, 0, GETDATE(), GETDATE(), 0, 1, 'A1B2C3D411111111');
INSERT [dbo].[Applicants] (AgeInterval, EducationalState, Gender, Email, FullName, Editor, Creator, CreatedDate, EditedDate, IsDeleted, IsActive, RowVersion) VALUES (3, 3, 2, 'applicant@example.com', N'Eski Başvuru', 0, 0, GETDATE(), GETDATE(), 0, 1, 'A1B2C3D411111111');
INSERT [dbo].[DismissalCases] (Guid, ApplicantId, CityID, EmploymentDurationSince, EmployeeCount, HasSignedAnyDocument, DismissalState, WantShareHisExperience, AppealState, DeclaredTerminationReasonID, SGKTerminationReasonID, Editor, Creator, CreatedDate, EditedDate, IsDeleted, IsActive, RowVersion)
VALUES ('11111111-2222-3333-4444-555555555555', 1, 1, 4, 2, 0, 1, 1, 1, 1, 1, 1, 1, GETDATE(), GETDATE(), 0, 1, 'A1B2C3D411111111');
INSERT [dbo].[DismissalCaseSupprtedBies] (DismissalCaseId, SupportedById, Editor, Creator, CreatedDate, EditedDate, IsDeleted, IsActive, RowVersion) VALUES (1, 1, 0, 0, GETDATE(), GETDATE(), 0, 1, 'A1B2C3D411111111');
INSERT [dbo].[Comments] (DismissalCaseID, Text, UserName, Editor, Creator, CreatedDate, EditedDate, IsDeleted, IsActive, RowVersion) VALUES (1, N'İlk mesaj', N'Eski Başvuru', 0, 0, GETDATE(), GETDATE(), 0, 1, 'A1B2C3D411111111');
