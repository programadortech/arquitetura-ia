# ADR-P0003 (Plataforma2ASmart.Auth): Sessão — refresh token em cookie httpOnly + CORS com credenciais

- **Status:** Aceita
- **Data:** 2026-06-25
- **Decisores:** Acaciano (tech lead), Claude
- **Escopo:** produto **Plataforma2ASmart.Auth** (decisão específica do produto).
- **Relacionados:** [ADR-P0001 Identity+JWT](0001-identity-jwt-autenticacao.md) · ADR-0009 do repo de front (`arquitetura-ia-frontend`).

## Contexto
O SPA de front-end (repo separado) decidiu **access token em memória + refresh em cookie httpOnly** (ADR-0009 lá).
A AZ-12094 entregou o refresh token **no corpo** da resposta e a API era **sem cookies** e **sem CORS**. Para o
front consumir a API de outra origem com a sessão segura, o back-end precisa alinhar.

## Decisão
1. **Refresh token em cookie httpOnly:** login e refresh **gravam** o refresh num cookie `refresh_token`
   (`HttpOnly`, `Secure`, `SameSite`, `Path=/api/auth`, expiração = `Jwt:RefreshTokenDays`). O corpo passa a
   devolver **apenas o access token** (`AuthAccessResponse`), que o front mantém em memória.
2. **`/api/auth/refresh-token` lê o refresh do cookie** (não do corpo). Sem cookie → 401; refresh inválido → 401 e
   o cookie é **limpo**. A rotação atômica do refresh (AZ-12094) é preservada.
2b. **`/api/auth/logout`** encerra a sessão: **revoga o refresh token no servidor** (pelo cookie) e **limpa o cookie**,
   retornando **204**. Idempotente (sem cookie/invalidado → 204 mesmo assim). Sem logout, o refresh seguiria válido até expirar.
3. **CORS com credenciais:** policy `spa` com origens por configuração (`Cors:AllowedOrigins`) + `AllowCredentials`.
4. **Configuração por ambiente:** Development → `SameSite=Lax`, `Secure=false`, origem `http://localhost:4200`
   (same-site localhost, sem HTTPS). Staging/Production → `SameSite=None`, `Secure=true`, origem do SPA (tokenizada).
5. **Sem auto-cadastro público:** cadastro permanece em `POST /api/users` (AZ-12114, policy `Users.Manage`, bootstrap
   do 1º usuário); **não** há `/auth/register`.

## Consequências
- (+) Refresh fora do alcance do JavaScript (mitiga XSS); access efêmero em memória; alinhado ao front.
- (+) CORS explícito por origem/ambiente.
- (−) Front e API em produção precisam de **HTTPS** e `SameSite=None`; exige configurar as origens permitidas.
- (−) Mudança no contrato: `/refresh-token` sem corpo; respostas de login/refresh sem `refreshToken` (contrato OpenAPI regenerado).

## Alternativas consideradas
- **Refresh no corpo (status quo):** mais simples, mas expõe o refresh ao JS/XSS e diverge do front.
- **Token em `localStorage`:** idem risco de XSS.

## Referências
- `Api/Controllers/AuthController.cs` · `Api/Authentication/RefreshCookieOptions.cs` · `Extensions/ServiceCollectionExtensions.cs` (CORS) · ADR-0032 (contrato OpenAPI).
