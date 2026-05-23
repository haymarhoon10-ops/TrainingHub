using Microsoft.EntityFrameworkCore;

namespace TrainingHub.Data
{
    public static class IdentitySchemaInitializer
    {
        public static async Task EnsureCreatedAsync(TrainingHubDbContext dbContext)
        {
            var usersTableExists = await dbContext.Database.ExecuteSqlRawAsync(@"
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
END");
        }
    }
}