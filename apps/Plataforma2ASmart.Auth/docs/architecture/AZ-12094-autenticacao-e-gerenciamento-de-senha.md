# Arquitetura: Autenticação e Gerenciamento de Senha

- **Feature:** [`../features/AZ-12094-autenticacao-e-gerenciamento-de-senha.md`](../features/AZ-12094-autenticacao-e-gerenciamento-de-senha.md)
- **Status:** Aprovada · **Data:** 2026-06-25 · **Branch:** `feature/12094-auth-plataforma2asmart`
- **ADRs:** [P0001 — Identity + JWT](../adr/0001-identity-jwt-autenticacao.md) (produto); transversais 0014/0028/0020/0029/0030.

## 1. Resumo
Autenticação com **ASP.NET Core Identity** + **JWT** e **refresh token** rotacionado/persistido (hash) no SQL Server.
Casos de uso na Application retornam **`Result<T>`** (BuildingBlocks); a Api responde no **envelope** com status codes.
Ports isolam Identity/JWT/refresh/e-mail; **rate limiting** nativo por IP protege os endpoints públicos.

## 2. Casos de uso (todos retornam Result)
| Use case | Request → Response | Efeitos | AC |
|---|---|---|---|
| `Login` | `LoginRequest(email,password)` → `Result<AuthTokensResponse>` | valida credenciais, emite JWT + refresh | #1–#3 |
| `RefreshToken` | `RefreshTokenRequest(accessToken,refreshToken)` → `Result<AuthTokensResponse>` | rotaciona refresh (transacional), novo JWT | #4, #5 |
| `ChangePassword` | `ChangePasswordRequest(userId,current,new,confirm)` → `Result<Unit>` | troca senha (logado) | #6, #7 |
| `ForgotPassword` | `ForgotPasswordRequest(email)` → `Result<Unit>` | gera token reset, envia e-mail (sem revelar existência) | #8 |
| `ResetPassword` | `ResetPasswordRequest(email,token,new,confirm)` → `Result<Unit>` | redefine senha, revoga refresh tokens | #9, #10 |

## 3. Camadas (consumindo BuildingBlocks)
- **Domain:** `RefreshToken` (entidade pura: `Id`, `UserId`, `TokenHash`, `ExpiresAt`, `RevokedAt`, `CreatedAt`, `IsActive`).
- **Application:** 5 handlers (`IUseCase<,>` → `Result`), `AuthTokensResponse`, ports `IIdentityService`,
  `IJwtTokenGenerator`, `IRefreshTokenStore`, `IEmailSender`. Usa `IUnitOfWork` (BuildingBlocks) na rotação.
- **Infrastructure:** `ApplicationUser : IdentityUser<Guid>`, `AppDbContext : IdentityDbContext` (+ `RefreshToken`),
  `IdentityService` (UserManager), `JwtTokenGenerator` (HMAC-SHA256), `RefreshTokenStore` (EF, hash SHA-256), `SmtpEmailSender`.
- **Api:** `AuthController` (fino) + contratos em `Contracts/Auth/`; JWT Bearer + Rate Limiter via `Extensions/`.

## 4. Modelo de dados (SQL Server)
- Tabelas do **Identity** (`AspNetUsers`, `AspNetRoles`, …) via `IdentityDbContext`.
- **`REFRESH_TOKEN`**: `Id (PK)`, `UserId`, `TokenHash (unique)`, `ExpiresAt`, `RevokedAt?`, `CreatedAt`;
  índices único em `TokenHash` e em `UserId`. Migração em `db/sqlserver/migrations`.

## 5. Erros (Result → envelope)
| Situação | Error.Type → HTTP |
|---|---|
| Credenciais/refresh/senha atual/token reset inválidos | `Unauthorized` → 401 |
| Confirmação de senha não confere | `Validation` → 400 |
| Brute force (rate limit) | `429` (middleware) |
| Sucesso | 200 |

## 6. Segurança / Resiliência / Observabilidade
- Senha no Identity (hash); refresh **hash SHA-256**; JWT assinado (`Jwt:Key` via secret store; dev via user-secrets).
- Reset **não revela** existência de e-mail. Rate limit por IP em `login`/`forgot`/`reset`.
- Rotação de refresh **transacional** (`IUnitOfWork`). Acesso a banco sob pipeline Polly `database`. Logs estruturados.

## 7. Tokens
- Access **15 min**, Refresh **7 dias** (rotacionado; anterior revogado). Login por **e-mail**.
- E-mail via `IEmailSender` + SMTP (dev) — provedor real pelo catálogo `docs/integrations/email`.

## 8. Plano de tasks
Sincronizado no Azure DevOps como filhas da história **#12094** (`/sync-tasks`): #12131 Domain RefreshToken ·
#12132 ports + AuthTokensResponse · #12133 Login · #12134 RefreshToken (rotação) · #12135 ChangePassword ·
#12136 ForgotPassword · #12137 ResetPassword · #12138 ApplicationUser+AppDbContext · #12139 IdentityService ·
#12140 JwtTokenGenerator · #12141 RefreshTokenStore · #12142 SmtpEmailSender+EfUnitOfWork ·
#12143 AuthController+contratos+JWT+rate limit · #12144 migração SQL Server · #12145 testes · #12146 observabilidade/Polly.

---
> Próximo: implementação por handler (consumindo BuildingBlocks); migração; testes. PR para `dev`.
