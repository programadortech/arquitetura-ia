# ADR-0024: ASP.NET Core Identity + JWT para autenticação (Plataforma2A.Auth)

- **Status:** Aceito
- **Data:** 2026-06-24
- **Feature:** AZ-12094 — Autenticação e Gerenciamento de Senha

## Contexto
A AZ-12094 exige login, refresh token, troca e redefinição de senha, com política de senha, tokens de
redefinição e segurança (não revelar e-mail, proteção brute force). O esqueleto não inclui autenticação.

## Decisão
Adotar **ASP.NET Core Identity** (usuários, hash de senha, política, tokens de reset) com EF Core/SQL Server
(`IdentityDbContext`) e **JWT Bearer** para o Access Token. O **Refresh Token** é entidade própria,
persistida como **hash** e **rotacionada** a cada renovação. A Application acessa tudo por **ports**
(`IIdentityService`, `IJwtTokenGenerator`, `IRefreshTokenStore`, `IEmailSender`) e retorna **`Result`**;
a Api responde no **envelope `ApiResponse`** (ADR-0014). Rate limiting nativo do .NET protege os endpoints
públicos. Tokens: Access 15 min, Refresh 7 dias.

## Consequências
- (+) Solução madura/segura para senhas e tokens; Application agnóstica (Identity/JWT só na Infrastructure).
- (+) Access curto + refresh rotacionado equilibra segurança e UX.
- (−) `IdentityDbContext` exige `FrameworkReference Microsoft.AspNetCore.App` na Infrastructure.
- (−) Mais tabelas (Identity) e cuidado transacional na rotação do refresh.

## Alternativas consideradas
- Autenticação caseira: risco de segurança — rejeitada.
- Provedor externo (IdentityServer/Auth0/Entra): fora do escopo atual; possível ADR futuro.
