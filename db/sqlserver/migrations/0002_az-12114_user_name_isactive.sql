/*
 Migração: AZ-12114 — Cadastro e Edição de Usuário
 Provider: SQL Server
 Adiciona as colunas Name e IsActive em AspNetUsers (ADR-0026).
 Idempotente: pode ser reexecutada com segurança. Requer a migração 0001 (Identity) aplicada.
 Reversão: db/sqlserver/migrations/0002_az-12114_user_name_isactive.down.sql
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
