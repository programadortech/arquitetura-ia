# ADR-P0001 (Plataforma2ASmart.Auth): ASP.NET Core Identity + JWT + Refresh Token

- **Status:** Aceita
- **Data:** 2026-06-25
- **Decisores:** Acaciano (tech lead), Claude
- **Escopo:** produto **Plataforma2ASmart.Auth** (decisão específica do produto; ADRs transversais ficam em `docs/adr/` na raiz).
- **Feature:** [AZ-12094 — Autenticação e Gerenciamento de Senha](../features/AZ-12094-autenticacao-e-gerenciamento-de-senha.md)

## Contexto
O esqueleto do produto (criado por `/create-project`) não traz autenticação. A AZ-12094 exige login, refresh
token, troca e recuperação de senha. Precisamos de um provedor de identidade e um esquema de tokens.

## Decisão
Adotar **ASP.NET Core Identity** (usuários, senhas com hash, roles, tokens de reset) + **JWT Bearer** para o
Access Token e **Refresh Token** próprio, persistido em **hash** com rotação:
- `ApplicationUser : IdentityUser<Guid>`; `AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` + `DbSet<RefreshToken>`.
- **Access Token 15 min**, **Refresh 7 dias** com rotação (revoga o anterior). Login por **e-mail**.
- Ports na Application (sem vazar tipos do Identity): `IIdentityService`, `IJwtTokenGenerator`, `IRefreshTokenStore`, `IEmailSender`.
- Erros de negócio via **`Result`** (BuildingBlocks); Api responde no **envelope** + status codes; contratos em `Api/Contracts/Auth/`.
- **Rate Limiting nativo** por IP nos endpoints públicos (login/forgot/reset) → 429 (AC #11).
- Criação/rotação de refresh token **transacional** via `IUnitOfWork` (BuildingBlocks).

## Consequências
- (+) Reaproveita o Identity (seguro e testado) e o JWT padrão de mercado; ports mantêm a Application limpa.
- (+) Refresh em hash + rotação reduz risco de replay; inativos/expirados nunca reutilizados.
- (−) Adiciona dependências de Identity/JWT ao produto e tabelas do Identity ao banco (migração).
- (−) `Jwt:Key` é segredo (via env/secret store; dev via user-secrets — ADR-0022).

## Alternativas consideradas
- **Sessão/cookie:** mais simples, mas não atende clientes/API stateless nem o fluxo de refresh do work item.
- **IdentityServer/OpenIddict:** poderoso, mas overkill para o escopo atual (sem OAuth/OIDC de terceiros).

## Referências
- [Arquitetura AZ-12094](../architecture/AZ-12094-autenticacao-e-gerenciamento-de-senha.md)
- Transversais: [ADR-0014 erros](../../../docs/adr/0014-error-handling-result-notification.md) · [ADR-0028 camada de API](../../../docs/adr/0028-padroes-camada-api.md) · [ADR-0030 monorepo](../../../docs/adr/0030-monorepo-multiproduto.md)
