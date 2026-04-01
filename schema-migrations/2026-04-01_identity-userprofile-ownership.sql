-- ============================================================
-- Migration: Identity, UserProfileEntries, Ownership FK
-- Date: 2026-04-01
-- ============================================================

-- -------------------------------------------------------
-- ASP.NET Identity tables
-- -------------------------------------------------------

CREATE TABLE [dbo].[AspNetRoles] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [Name]             NVARCHAR(256)    NULL,
    [NormalizedName]   NVARCHAR(256)    NULL,
    [ConcurrencyStamp] NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
CREATE UNIQUE INDEX [RoleNameIndex] ON [dbo].[AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   UNIQUEIDENTIFIER NOT NULL,
    [UserName]             NVARCHAR(256)    NULL,
    [NormalizedUserName]   NVARCHAR(256)    NULL,
    [Email]                NVARCHAR(256)    NULL,
    [NormalizedEmail]      NVARCHAR(256)    NULL,
    [EmailConfirmed]       BIT              NOT NULL,
    [PasswordHash]         NVARCHAR(MAX)    NULL,
    [SecurityStamp]        NVARCHAR(MAX)    NULL,
    [ConcurrencyStamp]     NVARCHAR(MAX)    NULL,
    [PhoneNumber]          NVARCHAR(MAX)    NULL,
    [PhoneNumberConfirmed] BIT              NOT NULL,
    [TwoFactorEnabled]     BIT              NOT NULL,
    [LockoutEnd]           DATETIMEOFFSET   NULL,
    [LockoutEnabled]       BIT              NOT NULL,
    [AccessFailedCount]    INT              NOT NULL,
    [DateOfBirth]          DATE             NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
CREATE UNIQUE INDEX [UserNameIndex]  ON [dbo].[AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
CREATE        INDEX [EmailIndex]     ON [dbo].[AspNetUsers] ([NormalizedEmail]);

CREATE TABLE [dbo].[AspNetUserRoles] (
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles]  ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id]         INT              IDENTITY(1,1) NOT NULL,
    [UserId]     UNIQUEIDENTIFIER NOT NULL,
    [ClaimType]  NVARCHAR(MAX)    NULL,
    [ClaimValue] NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[AspNetRoleClaims] (
    [Id]         INT              IDENTITY(1,1) NOT NULL,
    [RoleId]     UNIQUEIDENTIFIER NOT NULL,
    [ClaimType]  NVARCHAR(MAX)    NULL,
    [ClaimValue] NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider]       NVARCHAR(128)    NOT NULL,
    [ProviderKey]         NVARCHAR(128)    NOT NULL,
    [ProviderDisplayName] NVARCHAR(MAX)    NULL,
    [UserId]              UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[AspNetUserTokens] (
    [UserId]        UNIQUEIDENTIFIER NOT NULL,
    [LoginProvider] NVARCHAR(128)    NOT NULL,
    [Name]          NVARCHAR(128)    NOT NULL,
    [Value]         NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

-- -------------------------------------------------------
-- Default roles seed
-- -------------------------------------------------------
INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
VALUES
    (NEWID(), 'User',  'USER',  NEWID()),
    (NEWID(), 'Admin', 'ADMIN', NEWID());

-- -------------------------------------------------------
-- UserProfileEntries
-- -------------------------------------------------------
CREATE TABLE [dbo].[UserProfileEntries] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [UserId]     UNIQUEIDENTIFIER NOT NULL,
    [RecordedAt] DATETIME2        NOT NULL,
    [WeightKg]   NUMERIC(5,2)     NULL,
    [HeightCm]   NUMERIC(5,2)     NULL,
    [Sex]        NVARCHAR(1)      NULL,
    [Job]        NVARCHAR(20)     NULL,
    CONSTRAINT [PK_UserProfileEntries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserProfileEntries_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

-- -------------------------------------------------------
-- Ownership FK su Foods, Dishes, Brands
-- -------------------------------------------------------
ALTER TABLE [dbo].[Foods]
    ADD [OwnerId] UNIQUEIDENTIFIER NULL
        CONSTRAINT [FK_Foods_Owner] FOREIGN KEY REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE SET NULL;

ALTER TABLE [dbo].[Dishes]
    ADD [OwnerId] UNIQUEIDENTIFIER NULL
        CONSTRAINT [FK_Dishes_Owner] FOREIGN KEY REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE SET NULL;

ALTER TABLE [dbo].[Brands]
    ADD [OwnerId] UNIQUEIDENTIFIER NULL
        CONSTRAINT [FK_Brands_Owner] FOREIGN KEY REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE SET NULL;
