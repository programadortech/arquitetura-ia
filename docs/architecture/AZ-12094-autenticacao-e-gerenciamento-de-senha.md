# Arquitetura: Autenticação e Gerenciamento de Senha

- **Feature:** [`../features/AZ-12094-autenticacao-e-gerenciamento-de-senha.md`](../features/AZ-12094-autenticacao-e-gerenciamento-de-senha.md)
- **Status:** Aprovada · **Data:** 2026-06-24 · **Branch:** `feature/12094-autenticacao-e-gerenciamento-de-senha`
- **ADRs:** [0024 — ASP.NET Core Identity + JWT](../adr/0024-identity-jwt-autenticacao.md) (+ 0014 erros, 0020 UoW, 0021 mappers)

## 1. Resumo
Autenticação com **ASP.NET Core Identity** + **JWT** e **refresh token** rotacionado/persistido (hash) no
SQL Server. Casos de uso na Application retornam **`Result<T>`** (sem throw); a Api responde no **envelope
`ApiResponse`**. Ports isolam Identity/JWT/refresh/e-mail; **rate limiting** nativo protege os endpoints públicos.

## 2. Casos de uso (todos retornam Result)
| Use case | Request → Response | Efeitos | AC |
|---|---|---|---|
| `Login` | `LoginRequest(email,password)` → `Result<AuthTokensResponse>` | valida credenciais, emite JWT + refresh | #1–#3, #11 |
| `RefreshToken` | `RefreshTokenRequest(accessToken,refreshToken)` → `Result<AuthTokensResponse>` | rotaciona refresh, novo JWT | #4, #5 |
| `ChangePassword` | `ChangePasswordRequest(userId,current,new,confirm)` → `Result<Unit>` | troca senha (logado) | #6, #7 |
| `ForgotPassword` | `ForgotPasswordRequest(email)` → `Result<Unit>` | gera token reset, envia e-mail (sem revelar existência) | #8 |
| `ResetPassword` | `ResetPasswordRequest(email,token,new,confirm)` → `Result<Unit>` | redefine senha, invalida refresh tokens | #9, #10 |

## 3. Camadas
- **Domain:** `RefreshToken` (entidade pura: `Id`, `UserId`, `TokenHash`, `ExpiresAt`, `RevokedAt`, `CreatedAt`, `IsActive`).
- **Application:** os 5 handlers (`IUseCase<,>` → `Result`), `AuthTokensResponse`, mappers estáticos, e ports:
  `IIdentityService`, `IJwtTokenGenerator`, `IRefreshTokenStore`, `IEmailSender` (+ `IUnitOfWork` já existente).
- **Infrastructure:** `ApplicationUser : IdentityUser<Guid>`, `AppDbContext : IdentityDbContext` (+ `RefreshToken`),
  `IdentityService` (UserManager), `JwtTokenGenerator`, `RefreshTokenStore` (EF, hash SHA-256), `SmtpEmailSender`.
- **Api:** endpoints `/api/auth/*` que despacham via `IUseCaseDispatcher` e mapeiam `Result → ApiResponse`;
  JWT Bearer; Rate Limiter.

## 4. Modelo de dados (SQL Server)
- Tabelas do **Identity** (`AspNetUsers`, `AspNetRoles`, …) via `IdentityDbContext`.
- **`REFRESH_TOKEN`**: `ID (PK)`, `USER_ID`, `TOKEN_HASH (unique)`, `EXPIRES_AT`, `REVOKED_AT?`, `CREATED_AT`;
  índices `UQ_REFRESH_TOKEN_HASH`, `IX_REFRESH_TOKEN_USER_ID`. Migração em `db/sqlserver/migrations`.

## 5. Erros (Result → envelope)
| Situação | Error.Type → HTTP |
|---|---|
| Credenciais inválidas / refresh inválido / senha atual inválida / token reset inválido | `Unauthorized` → 401 |
| Confirmação de senha não confere | `Validation` → 400 |
| Brute force (rate limit) | `429` (middleware de rate limit) |
| Sucesso | 200 |

## 6. Segurança / Resiliência / Observabilidade
- Senha no Identity (hash); refresh **hash SHA-256**; JWT assinado (`Jwt:Key` via secret store).
- Reset **não revela** existência de e-mail. Rate limit (.NET) em `login`/`forgot`/`reset`.
- Acesso a banco sob pipeline Polly `database`. Logs estruturados + métricas dos fluxos.

## 7. Tokens
- Access **15 min**, Refresh **7 dias** (rotacionado a cada renovação; anterior revogado). Login por **e-mail**.
- E-mail via `IEmailSender` + SMTP (dev) — provedor real pelo catálogo `docs/integrations/email`.

## 8. Plano de tasks (write-back via /sync-tasks)
Domain RefreshToken · ports · 5 use cases · Identity+AppDbContext · Jwt+RefreshTokenStore · SmtpEmailSender ·
endpoints+JWT+rate limit · migração SQL Server · testes · observabilidade.

---
> Próximo: `/create-usecase` por handler; `/create-db-script` para a migração; `/create-tests`. PR para `dev`.
