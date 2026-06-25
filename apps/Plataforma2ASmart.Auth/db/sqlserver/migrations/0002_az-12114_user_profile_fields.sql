/*
 Migração: AZ-12114 — Cadastro e Edição de Usuário (Plataforma2ASmart.Auth)
 Provider: SQL Server. Adiciona os campos de perfil [Name] e [IsActive] em [AspNetUsers].
 [IsActive] default 1 (usuários existentes permanecem ativos). Idempotente.
 Reversão: 0002_az-12114_user_profile_fields.down.sql
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH(N'[AspNetUsers]', N'Name') IS NULL
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Name] NVARCHAR(256) NOT NULL CONSTRAINT [DF_AspNetUsers_Name] DEFAULT (N'');
END;

IF COL_LENGTH(N'[AspNetUsers]', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [AspNetUsers] ADD [IsActive] BIT NOT NULL CONSTRAINT [DF_AspNetUsers_IsActive] DEFAULT (1);
END;

COMMIT TRANSACTION;
