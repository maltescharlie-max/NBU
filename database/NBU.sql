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
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE TABLE [Publications] (
        [Id] int NOT NULL IDENTITY,
        [Type] nvarchar(30) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [PublishedAt] datetime2 NOT NULL,
        [EventDate] datetime2 NULL,
        CONSTRAINT [PK_Publications] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE TABLE [Scholarships] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Requirements] nvarchar(max) NOT NULL,
        [Amount] decimal(12,2) NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_Scholarships] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE TABLE [Students] (
        [Id] int NOT NULL IDENTITY,
        [StudentCode] nvarchar(20) NOT NULL,
        [FirstName] nvarchar(80) NOT NULL,
        [LastName] nvarchar(80) NOT NULL,
        [NationalId] nvarchar(30) NOT NULL,
        [Email] nvarchar(160) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [Career] nvarchar(120) NOT NULL,
        [University] nvarchar(120) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [PublicationId] int NULL,
        [Message] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsRead] bit NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Publications_PublicationId] FOREIGN KEY ([PublicationId]) REFERENCES [Publications] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Notifications_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE TABLE [ScholarshipApplications] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [ScholarshipId] int NOT NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [Observations] nvarchar(max) NULL,
        CONSTRAINT [PK_ScholarshipApplications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ScholarshipApplications_Scholarships_ScholarshipId] FOREIGN KEY ([ScholarshipId]) REFERENCES [Scholarships] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ScholarshipApplications_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] int NOT NULL IDENTITY,
        [ScholarshipApplicationId] int NOT NULL,
        [Amount] decimal(12,2) NOT NULL,
        [DepositDate] date NOT NULL,
        [Period] nvarchar(50) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_ScholarshipApplications_ScholarshipApplicationId] FOREIGN KEY ([ScholarshipApplicationId]) REFERENCES [ScholarshipApplications] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_PublicationId] ON [Notifications] ([PublicationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_StudentId] ON [Notifications] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_ScholarshipApplicationId] ON [Payments] ([ScholarshipApplicationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ScholarshipApplications_ScholarshipId] ON [ScholarshipApplications] ([ScholarshipId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ScholarshipApplications_StudentId] ON [ScholarshipApplications] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Students_Email] ON [Students] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Students_NationalId] ON [Students] ([NationalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Students_StudentCode] ON [Students] ([StudentCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235120_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028235120_InitialCreate', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235519_UniqueStudentScholarshipApplication'
)
BEGIN
    DROP INDEX [IX_ScholarshipApplications_StudentId] ON [ScholarshipApplications];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235519_UniqueStudentScholarshipApplication'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Scholarships]') AND [c].[name] = N'Requirements');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Scholarships] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Scholarships] ALTER COLUMN [Requirements] nvarchar(2000) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235519_UniqueStudentScholarshipApplication'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Scholarships]') AND [c].[name] = N'Name');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Scholarships] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Scholarships] ALTER COLUMN [Name] nvarchar(120) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235519_UniqueStudentScholarshipApplication'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Scholarships]') AND [c].[name] = N'Description');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Scholarships] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Scholarships] ALTER COLUMN [Description] nvarchar(2000) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235519_UniqueStudentScholarshipApplication'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ScholarshipApplications]') AND [c].[name] = N'Observations');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [ScholarshipApplications] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [ScholarshipApplications] ALTER COLUMN [Observations] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235519_UniqueStudentScholarshipApplication'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ScholarshipApplications_StudentId_ScholarshipId] ON [ScholarshipApplications] ([StudentId], [ScholarshipId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028235519_UniqueStudentScholarshipApplication'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028235519_UniqueStudentScholarshipApplication', N'8.0.33');
END;
GO

COMMIT;
GO

