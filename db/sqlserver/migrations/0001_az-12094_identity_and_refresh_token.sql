/*
 Migração: AZ-12094 — Autenticação e Gerenciamento de Senha
 Provider: SQL Server
 Cria o schema do ASP.NET Core Identity (chaves GUID) + a tabela REFRESH_TOKEN.
 Idempotente: pode ser reexecutada com segurança.
 Reversão: ver db/sqlserver/migrations/0001_az-12094_identity_and_refresh_token.down.sql
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- ============================ ASP.NET Core Identity ============================

IF OBJECT_ID(N'[AspNetRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id]               UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_AspNetRoles] PRIMARY KEY,
        [Name]             NVARCHAR(256)    NULL,
        [NormalizedName]   NVARCHAR(256)    NULL,
        [ConcurrencyStamp] NVARCHAR(MAX)    NULL
    );
    CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
END;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id]                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_AspNetUsers] PRIMARY KEY,
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
        [AccessFailedCount]    INT              NOT NULL
    );
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
    CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
END;

IF OBJECT_ID(N'[AspNetRoleClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id]         INT              NOT NULL IDENTITY CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY,
        [RoleId]     UNIQUEIDENTIFIER NOT NULL,
        [ClaimType]  NVARCHAR(MAX)    NULL,
        [ClaimValue] NVARCHAR(MAX)    NULL,
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF OBJECT_ID(N'[AspNetUserClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id]         INT              NOT NULL IDENTITY CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY,
        [UserId]     UNIQUEIDENTIFIER NOT NULL,
        [ClaimType]  NVARCHAR(MAX)    NULL,
        [ClaimValue] NVARCHAR(MAX)    NULL,
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF OBJECT_ID(N'[AspNetUserLogins]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider]       NVARCHAR(450)    NOT NULL,
        [ProviderKey]         NVARCHAR(450)    NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX)    NULL,
        [UserId]              UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF OBJECT_ID(N'[AspNetUserRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [RoleId] UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF OBJECT_ID(N'[AspNetUserTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId]        UNIQUEIDENTIFIER NOT NULL,
        [LoginProvider] NVARCHAR(450)    NOT NULL,
        [Name]          NVARCHAR(450)    NOT NULL,
        [Value]         NVARCHAR(MAX)    NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

-- ============================ REFRESH_TOKEN ============================

IF OBJECT_ID(N'[REFRESH_TOKEN]', N'U') IS NULL
BEGIN
    CREATE TABLE [REFRESH_TOKEN] (
        [Id]        UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_REFRESH_TOKEN] PRIMARY KEY,
        [UserId]    UNIQUEIDENTIFIER NOT NULL,
        [TokenHash] NVARCHAR(128)    NOT NULL,
        [ExpiresAt] DATETIMEOFFSET   NOT NULL,
        [CreatedAt] DATETIMEOFFSET   NOT NULL,
        [RevokedAt] DATETIMEOFFSET   NULL,
        CONSTRAINT [FK_REFRESH_TOKEN_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [UX_REFRESH_TOKEN_TokenHash] ON [REFRESH_TOKEN] ([TokenHash]);
    CREATE INDEX [IX_REFRESH_TOKEN_UserId] ON [REFRESH_TOKEN] ([UserId]);
END;

COMMIT TRANSACTION;
