/*
 Reversão da migração: AZ-12114 — Cadastro e Edição de Usuário (SQL Server)
 Remove as colunas Name e IsActive (e seus defaults) de AspNetUsers.
 Idempotente.
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'[DF_AspNetUsers_Name]', N'D') IS NOT NULL
    ALTER TABLE [AspNetUsers] DROP CONSTRAINT [DF_AspNetUsers_Name];
IF COL_LENGTH(N'[AspNetUsers]', N'Name') IS NOT NULL
    ALTER TABLE [AspNetUsers] DROP COLUMN [Name];

IF OBJECT_ID(N'[DF_AspNetUsers_IsActive]', N'D') IS NOT NULL
    ALTER TABLE [AspNetUsers] DROP CONSTRAINT [DF_AspNetUsers_IsActive];
IF COL_LENGTH(N'[AspNetUsers]', N'IsActive') IS NOT NULL
    ALTER TABLE [AspNetUsers] DROP COLUMN [IsActive];

COMMIT TRANSACTION;
