IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620062414_InitialCreate'
)
BEGIN
    CREATE TABLE [Resumes] (
        [Id] int NOT NULL IDENTITY,
        [FileName] nvarchar(max) NOT NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [ExtractedText] nvarchar(max) NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Resumes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620062414_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250620062414_InitialCreate', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620070221_AddParsedFields'
)
BEGIN
    ALTER TABLE [Resumes] ADD [Email] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620070221_AddParsedFields'
)
BEGIN
    ALTER TABLE [Resumes] ADD [Experience] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620070221_AddParsedFields'
)
BEGIN
    ALTER TABLE [Resumes] ADD [Name] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620070221_AddParsedFields'
)
BEGIN
    ALTER TABLE [Resumes] ADD [Phone] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620070221_AddParsedFields'
)
BEGIN
    ALTER TABLE [Resumes] ADD [Skills] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620070221_AddParsedFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250620070221_AddParsedFields', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620092933_ResumeFix'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250620092933_ResumeFix', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703050238_AddFileHashToResume'
)
BEGIN
    ALTER TABLE [Resumes] ADD [FileHash] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703050238_AddFileHashToResume'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250703050238_AddFileHashToResume', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250707064054_AddYearsOfExperienceToResume'
)
BEGIN
    ALTER TABLE [Resumes] ADD [YearsOfExperience] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250707064054_AddYearsOfExperienceToResume'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250707064054_AddYearsOfExperienceToResume', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250709054701_AddRemarkToResume'
)
BEGIN
    ALTER TABLE [Resumes] ADD [Remark] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250709054701_AddRemarkToResume'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250709054701_AddRemarkToResume', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721070357_AddIdentityTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250721070357_AddIdentityTables', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722104052_MakeUserIdNullable'
)
BEGIN
    ALTER TABLE [Resumes] ADD [UserId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722104052_MakeUserIdNullable'
)
BEGIN
    CREATE INDEX [IX_Resumes_UserId] ON [Resumes] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722104052_MakeUserIdNullable'
)
BEGIN
    ALTER TABLE [Resumes] ADD CONSTRAINT [FK_Resumes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722104052_MakeUserIdNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250722104052_MakeUserIdNullable', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250724045913_MakeResumeFieldsNullable'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Resumes]') AND [c].[name] = N'FilePath');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Resumes] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Resumes] ALTER COLUMN [FilePath] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250724045913_MakeResumeFieldsNullable'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Resumes]') AND [c].[name] = N'FileName');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Resumes] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Resumes] ALTER COLUMN [FileName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250724045913_MakeResumeFieldsNullable'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Resumes]') AND [c].[name] = N'ExtractedText');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Resumes] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Resumes] ALTER COLUMN [ExtractedText] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250724045913_MakeResumeFieldsNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250724045913_MakeResumeFieldsNullable', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250728052910_AddIsApprovedToUsers'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [IsApproved] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250728052910_AddIsApprovedToUsers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250728052910_AddIsApprovedToUsers', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250728072542_AddIsDeniedToUsers'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [IsDenied] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250728072542_AddIsDeniedToUsers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250728072542_AddIsDeniedToUsers', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250729054742_AddUserActivityTable'
)
BEGIN
    CREATE TABLE [UserActivities] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ActivityType] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        CONSTRAINT [PK_UserActivities] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserActivities_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250729054742_AddUserActivityTable'
)
BEGIN
    CREATE INDEX [IX_UserActivities_UserId] ON [UserActivities] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250729054742_AddUserActivityTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250729054742_AddUserActivityTable', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250729115925_AddActivityLogTable'
)
BEGIN
    DROP TABLE [UserActivities];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250729115925_AddActivityLogTable'
)
BEGIN
    CREATE TABLE [ActivityLogs] (
        [Id] int NOT NULL IDENTITY,
        [ActionType] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        CONSTRAINT [PK_ActivityLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ActivityLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250729115925_AddActivityLogTable'
)
BEGIN
    CREATE INDEX [IX_ActivityLogs_UserId] ON [ActivityLogs] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250729115925_AddActivityLogTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250729115925_AddActivityLogTable', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917053242_AddClientOnboarding'
)
BEGIN
    CREATE TABLE [Clients] (
        [Id] int NOT NULL IDENTITY,
        [CompanyName] nvarchar(max) NOT NULL,
        [WebsiteUrl] nvarchar(max) NOT NULL,
        [CompanyType] nvarchar(max) NOT NULL,
        [CompanySize] nvarchar(max) NOT NULL,
        [HeadquarterLocation] nvarchar(max) NOT NULL,
        [OtherOfficeLocations] nvarchar(max) NOT NULL,
        [ContactName] nvarchar(max) NOT NULL,
        [Designation] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [PreferredCommunication] nvarchar(max) NOT NULL,
        [EngagementTypes] nvarchar(max) NOT NULL,
        [AcceptTerms] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917053242_AddClientOnboarding'
)
BEGIN
    CREATE TABLE [ClientDocuments] (
        [Id] int NOT NULL IDENTITY,
        [NDAPath] nvarchar(max) NOT NULL,
        [MSAPath] nvarchar(max) NOT NULL,
        [CorporatePresentationPath] nvarchar(max) NOT NULL,
        [CorporatePresentationText] nvarchar(max) NOT NULL,
        [ClientId] int NOT NULL,
        CONSTRAINT [PK_ClientDocuments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClientDocuments_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917053242_AddClientOnboarding'
)
BEGIN
    CREATE TABLE [Spokespersons] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Designation] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [PreferredCommunication] nvarchar(max) NOT NULL,
        [ClientId] int NOT NULL,
        CONSTRAINT [PK_Spokespersons] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Spokespersons_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917053242_AddClientOnboarding'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ClientDocuments_ClientId] ON [ClientDocuments] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917053242_AddClientOnboarding'
)
BEGIN
    CREATE INDEX [IX_Spokespersons_ClientId] ON [Spokespersons] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917053242_AddClientOnboarding'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250917053242_AddClientOnboarding', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917061904_FixCreatedAtDefault'
)
BEGIN
    DROP INDEX [IX_ClientDocuments_ClientId] ON [ClientDocuments];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917061904_FixCreatedAtDefault'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'AcceptTerms');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Clients] DROP COLUMN [AcceptTerms];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917061904_FixCreatedAtDefault'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'ContactName');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Clients] DROP COLUMN [ContactName];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917061904_FixCreatedAtDefault'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'Designation');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Clients] DROP COLUMN [Designation];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917061904_FixCreatedAtDefault'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'Email');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Clients] DROP COLUMN [Email];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917061904_FixCreatedAtDefault'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'EngagementTypes');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Clients] DROP COLUMN [EngagementTypes];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917061904_FixCreatedAtDefault'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'Phone');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Clients] DROP COLUMN [Phone];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917061904_FixCreatedAtDefault'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'PreferredCommunication');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [Clients] DROP COLUMN [PreferredCommunication];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917061904_FixCreatedAtDefault'
)
BEGIN
    CREATE INDEX [IX_ClientDocuments_ClientId] ON [ClientDocuments] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917061904_FixCreatedAtDefault'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250917061904_FixCreatedAtDefault', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917062311_FixClientEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250917062311_FixClientEntity', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917062910_RestoreClientColumns'
)
BEGIN
    ALTER TABLE [Clients] ADD [AcceptTerms] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917062910_RestoreClientColumns'
)
BEGIN
    ALTER TABLE [Clients] ADD [ContactName] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917062910_RestoreClientColumns'
)
BEGIN
    ALTER TABLE [Clients] ADD [Designation] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917062910_RestoreClientColumns'
)
BEGIN
    ALTER TABLE [Clients] ADD [Email] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917062910_RestoreClientColumns'
)
BEGIN
    ALTER TABLE [Clients] ADD [EngagementTypes] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917062910_RestoreClientColumns'
)
BEGIN
    ALTER TABLE [Clients] ADD [Phone] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917062910_RestoreClientColumns'
)
BEGIN
    ALTER TABLE [Clients] ADD [PreferredCommunication] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250917062910_RestoreClientColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250917062910_RestoreClientColumns', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    ALTER TABLE [ClientRequirements] ADD [Certifications] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    ALTER TABLE [ClientRequirements] ADD [Deadline] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    ALTER TABLE [ClientRequirements] ADD [Education] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    ALTER TABLE [ClientRequirements] ADD [ExpectedJoiningDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    ALTER TABLE [ClientRequirements] ADD [ExperienceMax] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    ALTER TABLE [ClientRequirements] ADD [ExperienceMin] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    ALTER TABLE [ClientRequirements] ADD [NoticePeriod] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    ALTER TABLE [ClientRequirements] ADD [RequirementPriority] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    ALTER TABLE [ClientRequirements] ADD [SalaryRange] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    ALTER TABLE [ClientRequirements] ADD [ScreeningQuestions] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    ALTER TABLE [ClientRequirements] ADD [SkillsPrimary] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    ALTER TABLE [ClientRequirements] ADD [SkillsRequired] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    ALTER TABLE [ClientRequirements] ADD [SkillsSecondary] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClientDocuments]') AND [c].[name] = N'NDAPath');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [ClientDocuments] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [ClientDocuments] ALTER COLUMN [NDAPath] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClientDocuments]') AND [c].[name] = N'MSAPath');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [ClientDocuments] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [ClientDocuments] ALTER COLUMN [MSAPath] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClientDocuments]') AND [c].[name] = N'CorporatePresentationText');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [ClientDocuments] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [ClientDocuments] ALTER COLUMN [CorporatePresentationText] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClientDocuments]') AND [c].[name] = N'CorporatePresentationPath');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [ClientDocuments] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [ClientDocuments] ALTER COLUMN [CorporatePresentationPath] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923052311_MakeClientDocumentsOptional'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250923052311_MakeClientDocumentsOptional', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923084820_RecreateClientRequirements'
)
BEGIN
    CREATE TABLE [ClientRequirements] (
        [Id] int NOT NULL IDENTITY,
        [ClientId] int NOT NULL,
        [JobTitle] nvarchar(max) NOT NULL,
        [Positions] int NOT NULL,
        [JobLocation] nvarchar(max) NULL,
        [EmploymentType] nvarchar(max) NULL,
        [WorkShift] nvarchar(max) NULL,
        [SkillsPrimary] nvarchar(max) NULL,
        [SkillsSecondary] nvarchar(max) NULL,
        [SkillsRequired] nvarchar(max) NULL,
        [Responsibilities] nvarchar(max) NULL,
        [ExperienceMin] int NULL,
        [ExperienceMax] int NULL,
        [Education] nvarchar(max) NULL,
        [Certifications] nvarchar(max) NULL,
        [SalaryRange] nvarchar(max) NULL,
        [BillingType] nvarchar(max) NULL,
        [NoticePeriod] nvarchar(max) NULL,
        [RequirementPriority] nvarchar(max) NULL,
        [Deadline] datetime2 NULL,
        [ExpectedJoiningDate] datetime2 NULL,
        [ScreeningQuestions] nvarchar(max) NULL,
        [SpecialInstructions] nvarchar(max) NULL,
        [AttachmentsPath] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_ClientRequirements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClientRequirements_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923084820_RecreateClientRequirements'
)
BEGIN
    CREATE INDEX [IX_ClientRequirements_ClientId] ON [ClientRequirements] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923084820_RecreateClientRequirements'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250923084820_RecreateClientRequirements', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009071144_AddRequirementAssignments'
)
BEGIN
    CREATE TABLE [RequirementAssignments] (
        [Id] int NOT NULL IDENTITY,
        [RequirementId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [AssignedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_RequirementAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RequirementAssignments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RequirementAssignments_ClientRequirements_RequirementId] FOREIGN KEY ([RequirementId]) REFERENCES [ClientRequirements] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009071144_AddRequirementAssignments'
)
BEGIN
    CREATE INDEX [IX_RequirementAssignments_RequirementId] ON [RequirementAssignments] ([RequirementId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009071144_AddRequirementAssignments'
)
BEGIN
    CREATE INDEX [IX_RequirementAssignments_UserId] ON [RequirementAssignments] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009071144_AddRequirementAssignments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251009071144_AddRequirementAssignments', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013090421_AddResumeRequirementLinkTable'
)
BEGIN
    CREATE TABLE [ResumeRequirementLinks] (
        [Id] int NOT NULL IDENTITY,
        [ResumeId] int NOT NULL,
        [RequirementId] int NOT NULL,
        [LinkedByUserId] nvarchar(450) NOT NULL,
        [LinkedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ResumeRequirementLinks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ResumeRequirementLinks_AspNetUsers_LinkedByUserId] FOREIGN KEY ([LinkedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ResumeRequirementLinks_ClientRequirements_RequirementId] FOREIGN KEY ([RequirementId]) REFERENCES [ClientRequirements] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ResumeRequirementLinks_Resumes_ResumeId] FOREIGN KEY ([ResumeId]) REFERENCES [Resumes] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013090421_AddResumeRequirementLinkTable'
)
BEGIN
    CREATE INDEX [IX_ResumeRequirementLinks_LinkedByUserId] ON [ResumeRequirementLinks] ([LinkedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013090421_AddResumeRequirementLinkTable'
)
BEGIN
    CREATE INDEX [IX_ResumeRequirementLinks_RequirementId] ON [ResumeRequirementLinks] ([RequirementId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013090421_AddResumeRequirementLinkTable'
)
BEGIN
    CREATE INDEX [IX_ResumeRequirementLinks_ResumeId] ON [ResumeRequirementLinks] ([ResumeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013090421_AddResumeRequirementLinkTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251013090421_AddResumeRequirementLinkTable', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015064829_TeamLeadReview'
)
BEGIN
    ALTER TABLE [ResumeRequirementLinks] ADD [LastComment] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015064829_TeamLeadReview'
)
BEGIN
    ALTER TABLE [ResumeRequirementLinks] ADD [RowVersion] rowversion NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015064829_TeamLeadReview'
)
BEGIN
    ALTER TABLE [ResumeRequirementLinks] ADD [Status] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015064829_TeamLeadReview'
)
BEGIN
    ALTER TABLE [ResumeRequirementLinks] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015064829_TeamLeadReview'
)
BEGIN
    CREATE TABLE [CandidateStatusHistories] (
        [Id] int NOT NULL IDENTITY,
        [ResumeId] int NOT NULL,
        [RequirementId] int NOT NULL,
        [Status] int NOT NULL,
        [Comment] nvarchar(2000) NULL,
        [ChangedByUserId] nvarchar(max) NOT NULL,
        [ChangedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CandidateStatusHistories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015064829_TeamLeadReview'
)
BEGIN
    CREATE INDEX [IX_CandidateStatusHistories_RequirementId_ResumeId_ChangedAt] ON [CandidateStatusHistories] ([RequirementId], [ResumeId], [ChangedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015064829_TeamLeadReview'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251015064829_TeamLeadReview', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017052745_Notifications_Init'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Type] nvarchar(40) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Body] nvarchar(1000) NULL,
        [Url] nvarchar(500) NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017052745_Notifications_Init'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId_IsRead_CreatedAt] ON [Notifications] ([UserId], [IsRead], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017052745_Notifications_Init'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251017052745_Notifications_Init', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023121047_PanelScreening_Init'
)
BEGIN
    CREATE TABLE [PanelAssignments] (
        [Id] int NOT NULL IDENTITY,
        [RequirementId] int NOT NULL,
        [ResumeId] int NOT NULL,
        [PanelUserId] nvarchar(450) NOT NULL,
        [AssignedByUserId] nvarchar(max) NULL,
        [AssignedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PanelAssignments] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023121047_PanelScreening_Init'
)
BEGIN
    CREATE TABLE [PanelFeedbacks] (
        [Id] int NOT NULL IDENTITY,
        [RequirementId] int NOT NULL,
        [ResumeId] int NOT NULL,
        [PanelUserId] nvarchar(450) NOT NULL,
        [Decision] int NOT NULL,
        [Remark] nvarchar(1000) NULL,
        [DecidedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PanelFeedbacks] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023121047_PanelScreening_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PanelAssignments_PanelUserId_RequirementId_ResumeId] ON [PanelAssignments] ([PanelUserId], [RequirementId], [ResumeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023121047_PanelScreening_Init'
)
BEGIN
    CREATE INDEX [IX_PanelFeedbacks_PanelUserId_RequirementId_ResumeId_DecidedAt] ON [PanelFeedbacks] ([PanelUserId], [RequirementId], [ResumeId], [DecidedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023121047_PanelScreening_Init'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251023121047_PanelScreening_Init', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024063649_InterviewScheduling_Init'
)
BEGIN
    CREATE TABLE [InterviewSchedules] (
        [Id] int NOT NULL IDENTITY,
        [RequirementId] int NOT NULL,
        [ResumeId] int NOT NULL,
        [Round] int NOT NULL,
        [ScheduledStartUtc] datetime2 NOT NULL,
        [ScheduledEndUtc] datetime2 NOT NULL,
        [Mode] nvarchar(40) NOT NULL,
        [LocationOrLink] nvarchar(500) NULL,
        [PanelUserIdsCsv] nvarchar(2000) NULL,
        [Notes] nvarchar(1000) NULL,
        [Status] int NOT NULL,
        [CreatedByUserId] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_InterviewSchedules] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024063649_InterviewScheduling_Init'
)
BEGIN
    CREATE INDEX [IX_InterviewSchedules_RequirementId_ResumeId_Round] ON [InterviewSchedules] ([RequirementId], [ResumeId], [Round]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024063649_InterviewScheduling_Init'
)
BEGIN
    CREATE INDEX [IX_InterviewSchedules_ScheduledStartUtc] ON [InterviewSchedules] ([ScheduledStartUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024063649_InterviewScheduling_Init'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251024063649_InterviewScheduling_Init', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027053906_InterviewOutcome_Fields'
)
BEGIN
    ALTER TABLE [InterviewSchedules] ADD [ActualEndUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027053906_InterviewOutcome_Fields'
)
BEGIN
    ALTER TABLE [InterviewSchedules] ADD [ActualStartUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027053906_InterviewOutcome_Fields'
)
BEGIN
    ALTER TABLE [InterviewSchedules] ADD [Outcome] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027053906_InterviewOutcome_Fields'
)
BEGIN
    ALTER TABLE [InterviewSchedules] ADD [OutcomeNote] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027053906_InterviewOutcome_Fields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251027053906_InterviewOutcome_Fields', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027072113_InterviewFeedback_Init'
)
BEGIN
    CREATE TABLE [InterviewFeedbacks] (
        [Id] int NOT NULL IDENTITY,
        [InterviewScheduleId] int NOT NULL,
        [RequirementId] int NOT NULL,
        [ResumeId] int NOT NULL,
        [Round] int NOT NULL,
        [PanelUserId] nvarchar(450) NOT NULL,
        [Decision] int NOT NULL,
        [Comments] nvarchar(2000) NULL,
        [TechScore] int NULL,
        [CommScore] int NULL,
        [CultureScore] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_InterviewFeedbacks] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027072113_InterviewFeedback_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InterviewFeedbacks_InterviewScheduleId_PanelUserId] ON [InterviewFeedbacks] ([InterviewScheduleId], [PanelUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027072113_InterviewFeedback_Init'
)
BEGIN
    CREATE INDEX [IX_InterviewFeedbacks_RequirementId_ResumeId_Round] ON [InterviewFeedbacks] ([RequirementId], [ResumeId], [Round]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027072113_InterviewFeedback_Init'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251027072113_InterviewFeedback_Init', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251029115235_MatchScore_Init'
)
BEGIN
    DROP INDEX [IX_ResumeRequirementLinks_RequirementId] ON [ResumeRequirementLinks];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251029115235_MatchScore_Init'
)
BEGIN
    ALTER TABLE [ResumeRequirementLinks] ADD [LastScoredAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251029115235_MatchScore_Init'
)
BEGIN
    ALTER TABLE [ResumeRequirementLinks] ADD [MatchBreakdownJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251029115235_MatchScore_Init'
)
BEGIN
    ALTER TABLE [ResumeRequirementLinks] ADD [MatchScore] smallint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251029115235_MatchScore_Init'
)
BEGIN
    CREATE INDEX [IX_ResumeRequirementLinks_RequirementId_MatchScore] ON [ResumeRequirementLinks] ([RequirementId], [MatchScore]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251029115235_MatchScore_Init'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251029115235_MatchScore_Init', N'8.0.17');
END;
GO

COMMIT;
GO

