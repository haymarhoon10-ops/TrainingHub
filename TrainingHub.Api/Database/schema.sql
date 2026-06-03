SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ============================================================
   ASP.NET Core Identity schema
   ============================================================ */

IF OBJECT_ID(N'[dbo].[AspNetUsers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUsers] (
        [Id] int IDENTITY(1,1) NOT NULL,
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

IF OBJECT_ID(N'[dbo].[AspNetRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoles] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[AspNetRoleClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [RoleId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
            FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[dbo].[AspNetUserClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[dbo].[AspNetUserLogins]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[dbo].[AspNetUserRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles] (
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
            FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[dbo].[AspNetUserTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens] (
        [UserId] int NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

/* ============================================================
   TrainingHub domain schema
   ============================================================ */

IF OBJECT_ID(N'[dbo].[Categories]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Categories] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[CertificationTracks]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CertificationTracks] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_CertificationTracks] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Classrooms]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Classrooms] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [RoomCode] int NOT NULL,
        [Capacity] int NOT NULL,
        [Location] nvarchar(200) NOT NULL,
        [HasProjector] bit NOT NULL,
        [HasLabComputer] bit NOT NULL,
        [IsAvailable] bit NOT NULL,
        CONSTRAINT [PK_Classrooms] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Instructors]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Instructors] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [PhoneNumber] nvarchar(30) NOT NULL,
        [Bio] nvarchar(1000) NOT NULL,
        [ExpertiseArea] nvarchar(200) NOT NULL,
        [IsAvailable] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Instructors] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Trainees]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Trainees] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [FullName] nvarchar(150) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [PhoneNumber] nvarchar(30) NOT NULL,
        [RegisteredAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Trainees] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Courses]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Courses] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [DurationHours] float NOT NULL,
        [DefaultCapacity] int NOT NULL,
        [Fee] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CategoryId] int NOT NULL,
        [PrerequisiteCourseId] int NULL,
        [RequiresProjector] bit NOT NULL CONSTRAINT [DF_Courses_RequiresProjector] DEFAULT (CONVERT(bit, 0)),
        [RequiresLabComputer] bit NOT NULL CONSTRAINT [DF_Courses_RequiresLabComputer] DEFAULT (CONVERT(bit, 0)),
        CONSTRAINT [PK_Courses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Courses_Categories_CategoryId]
            FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Courses_Courses_PrerequisiteCourseId]
            FOREIGN KEY ([PrerequisiteCourseId]) REFERENCES [dbo].[Courses] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Certificates]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Certificates] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [TraineeId] int NOT NULL,
        [CertificationTrackId] int NOT NULL,
        [CertificateReferenceNumber] nvarchar(100) NOT NULL,
        [IssuedAt] datetime2 NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Certificates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Certificates_CertificationTracks_CertificationTrackId]
            FOREIGN KEY ([CertificationTrackId]) REFERENCES [dbo].[CertificationTracks] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Certificates_Trainees_TraineeId]
            FOREIGN KEY ([TraineeId]) REFERENCES [dbo].[Trainees] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Notifications]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Notifications] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsRead] bit NOT NULL,
        [TraineeId] int NULL,
        [InstructorId] int NULL,
        [Link] nvarchar(500) NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Instructors_InstructorId]
            FOREIGN KEY ([InstructorId]) REFERENCES [dbo].[Instructors] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Notifications_Trainees_TraineeId]
            FOREIGN KEY ([TraineeId]) REFERENCES [dbo].[Trainees] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[dbo].[CertificationTrackCourses]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CertificationTrackCourses] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [CertificationTrackId] int NOT NULL,
        [CourseId] int NOT NULL,
        CONSTRAINT [PK_CertificationTrackCourses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CertificationTrackCourses_CertificationTracks_CertificationTrackId]
            FOREIGN KEY ([CertificationTrackId]) REFERENCES [dbo].[CertificationTracks] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CertificationTrackCourses_Courses_CourseId]
            FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[dbo].[CourseSessions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CourseSessions] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [CourseId] int NOT NULL,
        [InstructorId] int NOT NULL,
        [ClassroomId] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Capacity] int NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CourseSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CourseSessions_Classrooms_ClassroomId]
            FOREIGN KEY ([ClassroomId]) REFERENCES [dbo].[Classrooms] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CourseSessions_Courses_CourseId]
            FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CourseSessions_Instructors_InstructorId]
            FOREIGN KEY ([InstructorId]) REFERENCES [dbo].[Instructors] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Enrollments]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Enrollments] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [TraineeId] int NOT NULL,
        [CourseSessionId] int NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [EnrolledAt] datetime2 NOT NULL,
        [AttendanceStatus] nvarchar(50) NOT NULL,
        [ResultStatus] nvarchar(50) NOT NULL,
        [ResultRecordedAt] datetime2 NULL,
        CONSTRAINT [PK_Enrollments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Enrollments_CourseSessions_CourseSessionId]
            FOREIGN KEY ([CourseSessionId]) REFERENCES [dbo].[CourseSessions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Enrollments_Trainees_TraineeId]
            FOREIGN KEY ([TraineeId]) REFERENCES [dbo].[Trainees] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Payments]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Payments] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [TraineeId] int NOT NULL,
        [EnrollmentId] int NOT NULL,
        [AmountPaid] decimal(18,2) NOT NULL,
        [PaidAt] datetime2 NOT NULL,
        [PaymentMethod] nvarchar(50) NOT NULL,
        [ReferenceNumber] nvarchar(100) NOT NULL,
        [Notes] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_Enrollments_EnrollmentId]
            FOREIGN KEY ([EnrollmentId]) REFERENCES [dbo].[Enrollments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Payments_Trainees_TraineeId]
            FOREIGN KEY ([TraineeId]) REFERENCES [dbo].[Trainees] ([Id]) ON DELETE NO ACTION
    );
END;
GO

/* ============================================================
   Indexes for lookup speed and foreign-key relationships
   ============================================================ */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_AspNetRoles_NormalizedName' AND [object_id] = OBJECT_ID(N'[dbo].[AspNetRoles]'))
    CREATE UNIQUE INDEX [IX_AspNetRoles_NormalizedName] ON [dbo].[AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_AspNetUsers_NormalizedEmail' AND [object_id] = OBJECT_ID(N'[dbo].[AspNetUsers]'))
    CREATE INDEX [IX_AspNetUsers_NormalizedEmail] ON [dbo].[AspNetUsers] ([NormalizedEmail]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_AspNetUsers_NormalizedUserName' AND [object_id] = OBJECT_ID(N'[dbo].[AspNetUsers]'))
    CREATE UNIQUE INDEX [IX_AspNetUsers_NormalizedUserName] ON [dbo].[AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_AspNetRoleClaims_RoleId' AND [object_id] = OBJECT_ID(N'[dbo].[AspNetRoleClaims]'))
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims] ([RoleId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_AspNetUserClaims_UserId' AND [object_id] = OBJECT_ID(N'[dbo].[AspNetUserClaims]'))
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims] ([UserId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_AspNetUserLogins_UserId' AND [object_id] = OBJECT_ID(N'[dbo].[AspNetUserLogins]'))
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins] ([UserId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_AspNetUserRoles_RoleId' AND [object_id] = OBJECT_ID(N'[dbo].[AspNetUserRoles]'))
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles] ([RoleId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Certificates_CertificationTrackId' AND [object_id] = OBJECT_ID(N'[dbo].[Certificates]'))
    CREATE INDEX [IX_Certificates_CertificationTrackId] ON [dbo].[Certificates] ([CertificationTrackId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Certificates_TraineeId' AND [object_id] = OBJECT_ID(N'[dbo].[Certificates]'))
    CREATE INDEX [IX_Certificates_TraineeId] ON [dbo].[Certificates] ([TraineeId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_CertificationTrackCourses_CertificationTrackId' AND [object_id] = OBJECT_ID(N'[dbo].[CertificationTrackCourses]'))
    CREATE INDEX [IX_CertificationTrackCourses_CertificationTrackId] ON [dbo].[CertificationTrackCourses] ([CertificationTrackId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_CertificationTrackCourses_CourseId' AND [object_id] = OBJECT_ID(N'[dbo].[CertificationTrackCourses]'))
    CREATE INDEX [IX_CertificationTrackCourses_CourseId] ON [dbo].[CertificationTrackCourses] ([CourseId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Courses_CategoryId' AND [object_id] = OBJECT_ID(N'[dbo].[Courses]'))
    CREATE INDEX [IX_Courses_CategoryId] ON [dbo].[Courses] ([CategoryId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Courses_PrerequisiteCourseId' AND [object_id] = OBJECT_ID(N'[dbo].[Courses]'))
    CREATE INDEX [IX_Courses_PrerequisiteCourseId] ON [dbo].[Courses] ([PrerequisiteCourseId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_CourseSessions_ClassroomId' AND [object_id] = OBJECT_ID(N'[dbo].[CourseSessions]'))
    CREATE INDEX [IX_CourseSessions_ClassroomId] ON [dbo].[CourseSessions] ([ClassroomId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_CourseSessions_CourseId' AND [object_id] = OBJECT_ID(N'[dbo].[CourseSessions]'))
    CREATE INDEX [IX_CourseSessions_CourseId] ON [dbo].[CourseSessions] ([CourseId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_CourseSessions_InstructorId' AND [object_id] = OBJECT_ID(N'[dbo].[CourseSessions]'))
    CREATE INDEX [IX_CourseSessions_InstructorId] ON [dbo].[CourseSessions] ([InstructorId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Enrollments_CourseSessionId' AND [object_id] = OBJECT_ID(N'[dbo].[Enrollments]'))
    CREATE INDEX [IX_Enrollments_CourseSessionId] ON [dbo].[Enrollments] ([CourseSessionId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Enrollments_TraineeId' AND [object_id] = OBJECT_ID(N'[dbo].[Enrollments]'))
    CREATE INDEX [IX_Enrollments_TraineeId] ON [dbo].[Enrollments] ([TraineeId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Notifications_InstructorId' AND [object_id] = OBJECT_ID(N'[dbo].[Notifications]'))
    CREATE INDEX [IX_Notifications_InstructorId] ON [dbo].[Notifications] ([InstructorId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Notifications_TraineeId' AND [object_id] = OBJECT_ID(N'[dbo].[Notifications]'))
    CREATE INDEX [IX_Notifications_TraineeId] ON [dbo].[Notifications] ([TraineeId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Payments_EnrollmentId' AND [object_id] = OBJECT_ID(N'[dbo].[Payments]'))
    CREATE INDEX [IX_Payments_EnrollmentId] ON [dbo].[Payments] ([EnrollmentId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Payments_TraineeId' AND [object_id] = OBJECT_ID(N'[dbo].[Payments]'))
    CREATE INDEX [IX_Payments_TraineeId] ON [dbo].[Payments] ([TraineeId]);
GO

/* ============================================================
   EF Core migration metadata
   ============================================================ */

IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260519124916_Initial')
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260519124916_Initial', N'9.0.5');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260520145220_Updating')
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260520145220_Updating', N'9.0.12');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260520150537_w123')
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260520150537_w123', N'9.0.12');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260520150650_123')
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260520150650_123', N'9.0.12');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260523130000_SyncCertificateSeedData')
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260523130000_SyncCertificateSeedData', N'9.0.5');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260525103325_UseSqlScriptSeedData')
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260525103325_UseSqlScriptSeedData', N'9.0.5');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260602143000_AddCourseEquipmentRequirements')
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260602143000_AddCourseEquipmentRequirements', N'9.0.5');
GO
