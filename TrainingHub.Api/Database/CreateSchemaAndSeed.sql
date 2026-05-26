IF DB_ID(N'TrainingHubCleanDb') IS NULL
    CREATE DATABASE [TrainingHubCleanDb];
GO
USE [TrainingHubCleanDb];
GO

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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE TABLE [CertificationTracks] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_CertificationTracks] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE TABLE [Classrooms] (
        [Id] int NOT NULL IDENTITY,
        [RoomCode] int NOT NULL,
        [Capacity] int NOT NULL,
        [Location] nvarchar(200) NOT NULL,
        [HasProjector] bit NOT NULL,
        [HasLabComputer] bit NOT NULL,
        [IsAvailable] bit NOT NULL,
        CONSTRAINT [PK_Classrooms] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE TABLE [Instructors] (
        [Id] int NOT NULL IDENTITY,
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE TABLE [Trainees] (
        [Id] int NOT NULL IDENTITY,
        [FullName] nvarchar(150) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [PhoneNumber] nvarchar(30) NOT NULL,
        [RegisteredAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Trainees] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE TABLE [Courses] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [DurationHours] float NOT NULL,
        [DefaultCapacity] int NOT NULL,
        [Fee] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CategoryId] int NOT NULL,
        [PrerequisiteCourseId] int NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Courses_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE TABLE [Certificates] (
        [Id] int NOT NULL IDENTITY,
        [TraineeId] int NOT NULL,
        [CertificationTrackId] int NOT NULL,
        [CertificateReferenceNumber] nvarchar(100) NOT NULL,
        [IssuedAt] datetime2 NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Certificates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Certificates_CertificationTracks_CertificationTrackId] FOREIGN KEY ([CertificationTrackId]) REFERENCES [CertificationTracks] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Certificates_Trainees_TraineeId] FOREIGN KEY ([TraineeId]) REFERENCES [Trainees] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsRead] bit NOT NULL,
        [TraineeId] int NULL,
        [InstructorId] int NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Instructors_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [Instructors] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Notifications_Trainees_TraineeId] FOREIGN KEY ([TraineeId]) REFERENCES [Trainees] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE TABLE [CertificationTrackCourses] (
        [Id] int NOT NULL IDENTITY,
        [CertificationTrackId] int NOT NULL,
        [CourseId] int NOT NULL,
        CONSTRAINT [PK_CertificationTrackCourses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CertificationTrackCourses_CertificationTracks_CertificationTrackId] FOREIGN KEY ([CertificationTrackId]) REFERENCES [CertificationTracks] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CertificationTrackCourses_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE TABLE [CourseSessions] (
        [Id] int NOT NULL IDENTITY,
        [CourseId] int NOT NULL,
        [InstructorId] int NOT NULL,
        [ClassroomId] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Capacity] int NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CourseSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CourseSessions_Classrooms_ClassroomId] FOREIGN KEY ([ClassroomId]) REFERENCES [Classrooms] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CourseSessions_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CourseSessions_Instructors_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [Instructors] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE TABLE [Enrollments] (
        [Id] int NOT NULL IDENTITY,
        [TraineeId] int NOT NULL,
        [CourseSessionId] int NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [EnrolledAt] datetime2 NOT NULL,
        [AttendanceStatus] nvarchar(50) NOT NULL,
        [ResultStatus] nvarchar(50) NOT NULL,
        [ResultRecordedAt] datetime2 NULL,
        CONSTRAINT [PK_Enrollments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Enrollments_CourseSessions_CourseSessionId] FOREIGN KEY ([CourseSessionId]) REFERENCES [CourseSessions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Enrollments_Trainees_TraineeId] FOREIGN KEY ([TraineeId]) REFERENCES [Trainees] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] int NOT NULL IDENTITY,
        [TraineeId] int NOT NULL,
        [EnrollmentId] int NOT NULL,
        [AmountPaid] decimal(18,2) NOT NULL,
        [PaidAt] datetime2 NOT NULL,
        [PaymentMethod] nvarchar(50) NOT NULL,
        [ReferenceNumber] nvarchar(100) NOT NULL,
        [Notes] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_Enrollments_EnrollmentId] FOREIGN KEY ([EnrollmentId]) REFERENCES [Enrollments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Payments_Trainees_TraineeId] FOREIGN KEY ([TraineeId]) REFERENCES [Trainees] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_Certificates_CertificationTrackId] ON [Certificates] ([CertificationTrackId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_Certificates_TraineeId] ON [Certificates] ([TraineeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_CertificationTrackCourses_CertificationTrackId] ON [CertificationTrackCourses] ([CertificationTrackId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_CertificationTrackCourses_CourseId] ON [CertificationTrackCourses] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_Courses_CategoryId] ON [Courses] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_CourseSessions_ClassroomId] ON [CourseSessions] ([ClassroomId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_CourseSessions_CourseId] ON [CourseSessions] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_CourseSessions_InstructorId] ON [CourseSessions] ([InstructorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_Enrollments_CourseSessionId] ON [Enrollments] ([CourseSessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_Enrollments_TraineeId] ON [Enrollments] ([TraineeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_Notifications_InstructorId] ON [Notifications] ([InstructorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_Notifications_TraineeId] ON [Notifications] ([TraineeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_Payments_EnrollmentId] ON [Payments] ([EnrollmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    CREATE INDEX [IX_Payments_TraineeId] ON [Payments] ([TraineeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519124916_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260519124916_Initial', N'9.0.16');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520145220_Updating'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260520145220_Updating', N'9.0.16');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520150537_w123'
)
BEGIN
    CREATE INDEX [IX_Courses_PrerequisiteCourseId] ON [Courses] ([PrerequisiteCourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520150537_w123'
)
BEGIN
    ALTER TABLE [Courses] ADD CONSTRAINT [FK_Courses_Courses_PrerequisiteCourseId] FOREIGN KEY ([PrerequisiteCourseId]) REFERENCES [Courses] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520150537_w123'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260520150537_w123', N'9.0.16');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520150650_123'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260520150650_123', N'9.0.16');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523130000_SyncCertificateSeedData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260523130000_SyncCertificateSeedData', N'9.0.16');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525103325_UseSqlScriptSeedData'
)
BEGIN
    ALTER TABLE [Notifications] ADD [Link] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525103325_UseSqlScriptSeedData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260525103325_UseSqlScriptSeedData', N'9.0.16');
END;

IF OBJECT_ID(N'[dbo].[AspNetUsers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUsers](
        [Id] [int] IDENTITY(1,1) NOT NULL,
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
END

IF OBJECT_ID(N'[dbo].[AspNetRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoles](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END

IF OBJECT_ID(N'[dbo].[AspNetRoleClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [RoleId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[dbo].[AspNetUserClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[dbo].[AspNetUserLogins]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins](
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[dbo].[AspNetUserRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles](
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[dbo].[AspNetUserTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens](
        [UserId] int NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetRoles_NormalizedName' AND object_id = OBJECT_ID(N'[dbo].[AspNetRoles]'))
BEGIN
    CREATE UNIQUE INDEX [IX_AspNetRoles_NormalizedName] ON [dbo].[AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUsers_NormalizedEmail' AND object_id = OBJECT_ID(N'[dbo].[AspNetUsers]'))
BEGIN
    CREATE INDEX [IX_AspNetUsers_NormalizedEmail] ON [dbo].[AspNetUsers] ([NormalizedEmail]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUsers_NormalizedUserName' AND object_id = OBJECT_ID(N'[dbo].[AspNetUsers]'))
BEGIN
    CREATE UNIQUE INDEX [IX_AspNetUsers_NormalizedUserName] ON [dbo].[AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetRoleClaims_RoleId' AND object_id = OBJECT_ID(N'[dbo].[AspNetRoleClaims]'))
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims] ([RoleId]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserClaims_UserId' AND object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]'))
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims] ([UserId]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserLogins_UserId' AND object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]'))
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins] ([UserId]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserRoles_RoleId' AND object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]'))
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles] ([RoleId]);
END

COMMIT;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @LockResult int;
EXEC @LockResult = sys.sp_getapplock
    @Resource = N'TrainingHub.SeedData',
    @LockMode = N'Exclusive',
    @LockOwner = N'Transaction',
    @LockTimeout = 10000;

IF @LockResult < 0
    THROW 50000, 'Unable to obtain the TrainingHub seed-data lock.', 1;

SET IDENTITY_INSERT [Categories] ON;
IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [Id] = 1)
    INSERT INTO [Categories] ([Id], [Name], [Description])
    VALUES (1, N'Programming', N'Courses about software development and programming languages.');
IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [Id] = 2)
    INSERT INTO [Categories] ([Id], [Name], [Description])
    VALUES (2, N'Data Science', N'Courses on data analysis, machine learning and statistics.');
SET IDENTITY_INSERT [Categories] OFF;

SET IDENTITY_INSERT [Instructors] ON;
IF NOT EXISTS (SELECT 1 FROM [Instructors] WHERE [Id] = 1)
    INSERT INTO [Instructors] ([Id], [Name], [Email], [PhoneNumber], [Bio], [ExpertiseArea], [IsAvailable], [IsActive], [CreatedAt])
    VALUES (1, N'Alice Johnson', N'alice.johnson@example.com', N'555-0101', N'Senior .NET developer and trainer.', N'Software Development', 1, 1, '2026-01-01T00:00:00');
IF NOT EXISTS (SELECT 1 FROM [Instructors] WHERE [Id] = 2)
    INSERT INTO [Instructors] ([Id], [Name], [Email], [PhoneNumber], [Bio], [ExpertiseArea], [IsAvailable], [IsActive], [CreatedAt])
    VALUES (2, N'Bob Smith', N'bob.smith@example.com', N'555-0202', N'Data scientist and Python instructor.', N'Data Science', 1, 1, '2026-01-01T00:00:00');
SET IDENTITY_INSERT [Instructors] OFF;

SET IDENTITY_INSERT [Classrooms] ON;
IF NOT EXISTS (SELECT 1 FROM [Classrooms] WHERE [Id] = 1)
    INSERT INTO [Classrooms] ([Id], [RoomCode], [Capacity], [Location], [HasProjector], [HasLabComputer], [IsAvailable])
    VALUES (1, 101, 30, N'Building A - Room 101', 1, 1, 1);
IF NOT EXISTS (SELECT 1 FROM [Classrooms] WHERE [Id] = 2)
    INSERT INTO [Classrooms] ([Id], [RoomCode], [Capacity], [Location], [HasProjector], [HasLabComputer], [IsAvailable])
    VALUES (2, 202, 20, N'Building B - Lab 202', 0, 1, 1);
SET IDENTITY_INSERT [Classrooms] OFF;

SET IDENTITY_INSERT [CertificationTracks] ON;
IF NOT EXISTS (SELECT 1 FROM [CertificationTracks] WHERE [Id] = 1)
    INSERT INTO [CertificationTracks] ([Id], [Name], [Description], [IsActive])
    VALUES (1, N'Full Stack Developer', N'Track covering front-end and back-end development.', 1);
IF NOT EXISTS (SELECT 1 FROM [CertificationTracks] WHERE [Id] = 2)
    INSERT INTO [CertificationTracks] ([Id], [Name], [Description], [IsActive])
    VALUES (2, N'Data Analyst Track', N'Track focused on data analysis and visualization.', 1);
SET IDENTITY_INSERT [CertificationTracks] OFF;

SET IDENTITY_INSERT [Trainees] ON;
IF NOT EXISTS (SELECT 1 FROM [Trainees] WHERE [Id] = 1)
    INSERT INTO [Trainees] ([Id], [FullName], [Email], [PhoneNumber], [RegisteredAt], [IsActive])
    VALUES (1, N'John Doe', N'john.doe@example.com', N'555-1001', '2026-01-10T00:00:00', 1);
IF NOT EXISTS (SELECT 1 FROM [Trainees] WHERE [Id] = 2)
    INSERT INTO [Trainees] ([Id], [FullName], [Email], [PhoneNumber], [RegisteredAt], [IsActive])
    VALUES (2, N'Jane Smith', N'jane.smith@example.com', N'555-1002', '2026-02-05T00:00:00', 1);
SET IDENTITY_INSERT [Trainees] OFF;

SET IDENTITY_INSERT [Courses] ON;
IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = 1)
    INSERT INTO [Courses] ([Id], [Title], [Description], [DurationHours], [DefaultCapacity], [Fee], [IsActive], [CategoryId], [PrerequisiteCourseId])
    VALUES (1, N'C# Fundamentals', N'Introductory course to C# and .NET fundamentals.', 24.0, 20, 499.99, 1, 1, NULL);
IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = 2)
    INSERT INTO [Courses] ([Id], [Title], [Description], [DurationHours], [DefaultCapacity], [Fee], [IsActive], [CategoryId], [PrerequisiteCourseId])
    VALUES (2, N'Python for Data Analysis', N'Using Python to analyze and visualize data.', 30.0, 25, 599.50, 1, 2, NULL);
IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = 3)
    INSERT INTO [Courses] ([Id], [Title], [Description], [DurationHours], [DefaultCapacity], [Fee], [IsActive], [CategoryId], [PrerequisiteCourseId])
    VALUES (3, N'ASP.NET Core MVC', N'Building web applications using ASP.NET Core MVC.', 45.0, 20, 799.99, 1, 1, 1);
SET IDENTITY_INSERT [Courses] OFF;

SET IDENTITY_INSERT [CertificationTrackCourses] ON;
IF NOT EXISTS (SELECT 1 FROM [CertificationTrackCourses] WHERE [Id] = 1)
    INSERT INTO [CertificationTrackCourses] ([Id], [CertificationTrackId], [CourseId]) VALUES (1, 1, 1);
IF NOT EXISTS (SELECT 1 FROM [CertificationTrackCourses] WHERE [Id] = 2)
    INSERT INTO [CertificationTrackCourses] ([Id], [CertificationTrackId], [CourseId]) VALUES (2, 2, 2);
IF NOT EXISTS (SELECT 1 FROM [CertificationTrackCourses] WHERE [Id] = 3)
    INSERT INTO [CertificationTrackCourses] ([Id], [CertificationTrackId], [CourseId]) VALUES (3, 1, 3);
SET IDENTITY_INSERT [CertificationTrackCourses] OFF;

SET IDENTITY_INSERT [CourseSessions] ON;
IF NOT EXISTS (SELECT 1 FROM [CourseSessions] WHERE [Id] = 1)
    INSERT INTO [CourseSessions] ([Id], [CourseId], [InstructorId], [ClassroomId], [StartDate], [EndDate], [Capacity], [Status], [CreatedAt])
    VALUES (1, 1, 1, 1, '2026-06-01T00:00:00', '2026-06-10T00:00:00', 20, N'Scheduled', '2026-05-01T00:00:00');
IF NOT EXISTS (SELECT 1 FROM [CourseSessions] WHERE [Id] = 2)
    INSERT INTO [CourseSessions] ([Id], [CourseId], [InstructorId], [ClassroomId], [StartDate], [EndDate], [Capacity], [Status], [CreatedAt])
    VALUES (2, 2, 2, 2, '2026-07-01T00:00:00', '2026-07-15T00:00:00', 25, N'Scheduled', '2026-05-10T00:00:00');
IF NOT EXISTS (SELECT 1 FROM [CourseSessions] WHERE [Id] = 3)
    INSERT INTO [CourseSessions] ([Id], [CourseId], [InstructorId], [ClassroomId], [StartDate], [EndDate], [Capacity], [Status], [CreatedAt])
    VALUES (3, 3, 1, 1, '2026-08-01T00:00:00', '2026-08-10T00:00:00', 20, N'Scheduled', '2026-07-01T00:00:00');
SET IDENTITY_INSERT [CourseSessions] OFF;

SET IDENTITY_INSERT [Enrollments] ON;
IF NOT EXISTS (SELECT 1 FROM [Enrollments] WHERE [Id] = 1)
    INSERT INTO [Enrollments] ([Id], [TraineeId], [CourseSessionId], [Status], [EnrolledAt], [AttendanceStatus], [ResultStatus], [ResultRecordedAt])
    VALUES (1, 1, 1, N'Completed', '2026-05-20T00:00:00', N'Present', N'Pass', '2026-06-11T00:00:00');
IF NOT EXISTS (SELECT 1 FROM [Enrollments] WHERE [Id] = 2)
    INSERT INTO [Enrollments] ([Id], [TraineeId], [CourseSessionId], [Status], [EnrolledAt], [AttendanceStatus], [ResultStatus], [ResultRecordedAt])
    VALUES (2, 2, 2, N'Enrolled', '2026-05-22T00:00:00', N'Pending', N'Pending', NULL);
IF NOT EXISTS (SELECT 1 FROM [Enrollments] WHERE [Id] = 3)
    INSERT INTO [Enrollments] ([Id], [TraineeId], [CourseSessionId], [Status], [EnrolledAt], [AttendanceStatus], [ResultStatus], [ResultRecordedAt])
    VALUES (3, 1, 3, N'Completed', '2026-07-05T00:00:00', N'Present', N'Pass', '2026-08-11T00:00:00');
SET IDENTITY_INSERT [Enrollments] OFF;

SET IDENTITY_INSERT [Payments] ON;
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [Id] = 1)
    INSERT INTO [Payments] ([Id], [TraineeId], [EnrollmentId], [AmountPaid], [PaidAt], [PaymentMethod], [ReferenceNumber], [Notes])
    VALUES (1, 1, 1, 499.99, '2026-05-21T00:00:00', N'CreditCard', N'PAY-1001', N'');
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [Id] = 2)
    INSERT INTO [Payments] ([Id], [TraineeId], [EnrollmentId], [AmountPaid], [PaidAt], [PaymentMethod], [ReferenceNumber], [Notes])
    VALUES (2, 2, 2, 300.00, '2026-05-23T00:00:00', N'CreditCard', N'PAY-1002', N'');
SET IDENTITY_INSERT [Payments] OFF;

SET IDENTITY_INSERT [Certificates] ON;
IF NOT EXISTS (SELECT 1 FROM [Certificates] WHERE [Id] = 1)
    INSERT INTO [Certificates] ([Id], [TraineeId], [CertificationTrackId], [CertificateReferenceNumber], [IssuedAt], [Status])
    VALUES (1, 1, 1, N'CERT-0001', '2026-08-15T00:00:00', N'Issued');
SET IDENTITY_INSERT [Certificates] OFF;

SET IDENTITY_INSERT [Notifications] ON;
IF NOT EXISTS (SELECT 1 FROM [Notifications] WHERE [Id] = 1)
    INSERT INTO [Notifications] ([Id], [Title], [Message], [Type], [CreatedAt], [IsRead], [TraineeId], [InstructorId])
    VALUES (1, N'Welcome', N'Welcome to TrainingHub, John!', N'System', '2026-01-11T00:00:00', 0, 1, NULL);
IF NOT EXISTS (SELECT 1 FROM [Notifications] WHERE [Id] = 2)
    INSERT INTO [Notifications] ([Id], [Title], [Message], [Type], [CreatedAt], [IsRead], [TraineeId], [InstructorId])
    VALUES (2, N'Session Assigned', N'You have been assigned to C# Fundamentals.', N'Assignment', '2026-05-02T00:00:00', 0, NULL, 1);
SET IDENTITY_INSERT [Notifications] OFF;

SET IDENTITY_INSERT [AspNetRoles] ON;
IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Id] = 1)
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (1, N'TrainingCoordinator', N'TRAININGCOORDINATOR', CAST(NEWID() AS nvarchar(36)));
IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Id] = 2)
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (2, N'Instructor', N'INSTRUCTOR', CAST(NEWID() AS nvarchar(36)));
IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Id] = 3)
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (3, N'Trainee', N'TRAINEE', CAST(NEWID() AS nvarchar(36)));
SET IDENTITY_INSERT [AspNetRoles] OFF;

SET IDENTITY_INSERT [AspNetUsers] ON;
IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = 1)
    INSERT INTO [AspNetUsers] ([Id], [FullName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
    VALUES (1, N'Training Coordinator', N'coordinator@traininghub.local', N'COORDINATOR@TRAININGHUB.LOCAL', N'coordinator@traininghub.local', N'COORDINATOR@TRAININGHUB.LOCAL', 1, N'AQAAAAIAAYagAAAAEAMl6uQQGsHOIOD6umelNsOyJbEIOMjna/2iC+ecVpL7/EatyaCXferLtAVnh6tWJQ==', CAST(NEWID() AS nvarchar(36)), CAST(NEWID() AS nvarchar(36)), NULL, 0, 0, NULL, 1, 0);
IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = 2)
    INSERT INTO [AspNetUsers] ([Id], [FullName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
    VALUES (2, N'Alice Johnson', N'alice.johnson@example.com', N'ALICE.JOHNSON@EXAMPLE.COM', N'alice.johnson@example.com', N'ALICE.JOHNSON@EXAMPLE.COM', 1, N'AQAAAAIAAYagAAAAED77pilpldWBbeERhLE//jrYG1L1ZcdACK+lJD9+zUr5e5SovDa71Z/zuHoIgaDVrQ==', CAST(NEWID() AS nvarchar(36)), CAST(NEWID() AS nvarchar(36)), NULL, 0, 0, NULL, 1, 0);
IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = 3)
    INSERT INTO [AspNetUsers] ([Id], [FullName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
    VALUES (3, N'John Doe', N'john.doe@example.com', N'JOHN.DOE@EXAMPLE.COM', N'john.doe@example.com', N'JOHN.DOE@EXAMPLE.COM', 1, N'AQAAAAIAAYagAAAAELNdiXzb+Q2VrcQ9UH5W6tCedl8GjT9UptCsGntEhuUXgXl/4DpbaAkHMuJoYH6gLg==', CAST(NEWID() AS nvarchar(36)), CAST(NEWID() AS nvarchar(36)), NULL, 0, 0, NULL, 1, 0);
SET IDENTITY_INSERT [AspNetUsers] OFF;

IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = 1 AND [RoleId] = 1)
    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (1, 1);
IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = 2 AND [RoleId] = 2)
    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (2, 2);
IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = 3 AND [RoleId] = 3)
    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (3, 3);

SET IDENTITY_INSERT [Instructors] ON;
IF NOT EXISTS (SELECT 1 FROM [Instructors] WHERE [Id] = 3)
    INSERT INTO [Instructors] ([Id], [Name], [Email], [PhoneNumber], [Bio], [ExpertiseArea], [IsAvailable], [IsActive], [CreatedAt])
    VALUES (3, N'Charlie Reed', N'charlie.reed@example.com', N'555-0303', N'Cloud and DevOps trainer.', N'Cloud Infrastructure', 1, 1, '2026-01-01T00:00:00');
SET IDENTITY_INSERT [Instructors] OFF;

SET IDENTITY_INSERT [Classrooms] ON;
IF NOT EXISTS (SELECT 1 FROM [Classrooms] WHERE [Id] = 3)
    INSERT INTO [Classrooms] ([Id], [RoomCode], [Capacity], [Location], [HasProjector], [HasLabComputer], [IsAvailable])
    VALUES (3, 303, 35, N'Building C - Room 303', 1, 1, 1);
IF NOT EXISTS (SELECT 1 FROM [Classrooms] WHERE [Id] = 4)
    INSERT INTO [Classrooms] ([Id], [RoomCode], [Capacity], [Location], [HasProjector], [HasLabComputer], [IsAvailable])
    VALUES (4, 404, 18, N'Building D - Lab 404', 1, 0, 1);
SET IDENTITY_INSERT [Classrooms] OFF;

SET IDENTITY_INSERT [Trainees] ON;
IF NOT EXISTS (SELECT 1 FROM [Trainees] WHERE [Id] = 3)
    INSERT INTO [Trainees] ([Id], [FullName], [Email], [PhoneNumber], [RegisteredAt], [IsActive])
    VALUES (3, N'Maria Garcia', N'maria.garcia@example.com', N'555-1003', '2026-02-20T00:00:00', 1);
IF NOT EXISTS (SELECT 1 FROM [Trainees] WHERE [Id] = 4)
    INSERT INTO [Trainees] ([Id], [FullName], [Email], [PhoneNumber], [RegisteredAt], [IsActive])
    VALUES (4, N'Alex Lee', N'alex.lee@example.com', N'555-1004', '2026-03-01T00:00:00', 1);
SET IDENTITY_INSERT [Trainees] OFF;

SET IDENTITY_INSERT [Courses] ON;
IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = 4)
    INSERT INTO [Courses] ([Id], [Title], [Description], [DurationHours], [DefaultCapacity], [Fee], [IsActive], [CategoryId], [PrerequisiteCourseId])
    VALUES (4, N'SQL Server Essentials', N'Core SQL Server administration and query fundamentals.', 20.0, 18, 449.99, 1, 1, 1);
IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = 5)
    INSERT INTO [Courses] ([Id], [Title], [Description], [DurationHours], [DefaultCapacity], [Fee], [IsActive], [CategoryId], [PrerequisiteCourseId])
    VALUES (5, N'Power BI Reporting', N'Create dashboards and reports using Power BI.', 16.0, 22, 349.50, 1, 2, NULL);
SET IDENTITY_INSERT [Courses] OFF;

SET IDENTITY_INSERT [CertificationTrackCourses] ON;
IF NOT EXISTS (SELECT 1 FROM [CertificationTrackCourses] WHERE [Id] = 4)
    INSERT INTO [CertificationTrackCourses] ([Id], [CertificationTrackId], [CourseId]) VALUES (4, 1, 4);
IF NOT EXISTS (SELECT 1 FROM [CertificationTrackCourses] WHERE [Id] = 5)
    INSERT INTO [CertificationTrackCourses] ([Id], [CertificationTrackId], [CourseId]) VALUES (5, 2, 5);
SET IDENTITY_INSERT [CertificationTrackCourses] OFF;

SET IDENTITY_INSERT [CourseSessions] ON;
IF NOT EXISTS (SELECT 1 FROM [CourseSessions] WHERE [Id] = 4)
    INSERT INTO [CourseSessions] ([Id], [CourseId], [InstructorId], [ClassroomId], [StartDate], [EndDate], [Capacity], [Status], [CreatedAt])
    VALUES (4, 4, 3, 3, '2026-09-01T00:00:00', '2026-09-05T00:00:00', 18, N'Scheduled', '2026-08-01T00:00:00');
IF NOT EXISTS (SELECT 1 FROM [CourseSessions] WHERE [Id] = 5)
    INSERT INTO [CourseSessions] ([Id], [CourseId], [InstructorId], [ClassroomId], [StartDate], [EndDate], [Capacity], [Status], [CreatedAt])
    VALUES (5, 5, 2, 4, '2026-10-01T00:00:00', '2026-10-10T00:00:00', 22, N'Scheduled', '2026-09-01T00:00:00');
SET IDENTITY_INSERT [CourseSessions] OFF;

SET IDENTITY_INSERT [Enrollments] ON;
IF NOT EXISTS (SELECT 1 FROM [Enrollments] WHERE [Id] = 4)
    INSERT INTO [Enrollments] ([Id], [TraineeId], [CourseSessionId], [Status], [EnrolledAt], [AttendanceStatus], [ResultStatus], [ResultRecordedAt])
    VALUES (4, 3, 4, N'Enrolled', '2026-08-10T00:00:00', N'Pending', N'Pending', NULL);
IF NOT EXISTS (SELECT 1 FROM [Enrollments] WHERE [Id] = 5)
    INSERT INTO [Enrollments] ([Id], [TraineeId], [CourseSessionId], [Status], [EnrolledAt], [AttendanceStatus], [ResultStatus], [ResultRecordedAt])
    VALUES (5, 4, 5, N'Completed', '2026-09-12T00:00:00', N'Present', N'Pass', '2026-10-11T00:00:00');
IF NOT EXISTS (SELECT 1 FROM [Enrollments] WHERE [Id] = 6)
    INSERT INTO [Enrollments] ([Id], [TraineeId], [CourseSessionId], [Status], [EnrolledAt], [AttendanceStatus], [ResultStatus], [ResultRecordedAt])
    VALUES (6, 1, 5, N'Enrolled', '2026-09-15T00:00:00', N'Pending', N'Pending', NULL);
SET IDENTITY_INSERT [Enrollments] OFF;

SET IDENTITY_INSERT [Payments] ON;
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [Id] = 3)
    INSERT INTO [Payments] ([Id], [TraineeId], [EnrollmentId], [AmountPaid], [PaidAt], [PaymentMethod], [ReferenceNumber], [Notes])
    VALUES (3, 3, 4, 699.99, '2026-08-12T00:00:00', N'CreditCard', N'PAY-1003', N'');
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [Id] = 4)
    INSERT INTO [Payments] ([Id], [TraineeId], [EnrollmentId], [AmountPaid], [PaidAt], [PaymentMethod], [ReferenceNumber], [Notes])
    VALUES (4, 4, 5, 349.50, '2026-09-13T00:00:00', N'CreditCard', N'PAY-1004', N'');
SET IDENTITY_INSERT [Payments] OFF;

SET IDENTITY_INSERT [Certificates] ON;
IF NOT EXISTS (SELECT 1 FROM [Certificates] WHERE [Id] = 2)
    INSERT INTO [Certificates] ([Id], [TraineeId], [CertificationTrackId], [CertificateReferenceNumber], [IssuedAt], [Status])
    VALUES (2, 3, 1, N'CERT-0002', '2026-09-15T00:00:00', N'Issued');
IF NOT EXISTS (SELECT 1 FROM [Certificates] WHERE [Id] = 3)
    INSERT INTO [Certificates] ([Id], [TraineeId], [CertificationTrackId], [CertificateReferenceNumber], [IssuedAt], [Status])
    VALUES (3, 4, 2, N'CERT-0003', '2026-10-20T00:00:00', N'Issued');
SET IDENTITY_INSERT [Certificates] OFF;

SET IDENTITY_INSERT [Notifications] ON;
IF NOT EXISTS (SELECT 1 FROM [Notifications] WHERE [Id] = 3)
    INSERT INTO [Notifications] ([Id], [Title], [Message], [Type], [CreatedAt], [IsRead], [TraineeId], [InstructorId])
    VALUES (3, N'Upcoming Session', N'You have an upcoming SQL Server Essentials session.', N'Reminder', '2026-08-15T00:00:00', 0, 3, NULL);
IF NOT EXISTS (SELECT 1 FROM [Notifications] WHERE [Id] = 4)
    INSERT INTO [Notifications] ([Id], [Title], [Message], [Type], [CreatedAt], [IsRead], [TraineeId], [InstructorId])
    VALUES (4, N'Certificate Ready', N'Your Power BI reporting certificate is now ready.', N'System', '2026-10-21T00:00:00', 0, 4, NULL);
SET IDENTITY_INSERT [Notifications] OFF;

COMMIT TRANSACTION;
