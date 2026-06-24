/*
 Reversão da migração: AZ-12094 — Autenticação e Gerenciamento de Senha (SQL Server)
 Remove REFRESH_TOKEN e as tabelas do ASP.NET Core Identity.
 Idempotente: pode ser reexecutada com segurança.
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DROP TABLE IF EXISTS [REFRESH_TOKEN];
DROP TABLE IF EXISTS [AspNetUserTokens];
DROP TABLE IF EXISTS [AspNetUserRoles];
DROP TABLE IF EXISTS [AspNetUserLogins];
DROP TABLE IF EXISTS [AspNetUserClaims];
DROP TABLE IF EXISTS [AspNetRoleClaims];
DROP TABLE IF EXISTS [AspNetUsers];
DROP TABLE IF EXISTS [AspNetRoles];

COMMIT TRANSACTION;
