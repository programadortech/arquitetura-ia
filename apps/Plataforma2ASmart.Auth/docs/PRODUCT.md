# PRODUCT.md — Plataforma2ASmart.Auth

> Contexto vivo deste produto. A IA **lê primeiro** ao evoluí-lo. Manter curto e atualizado.

## Visão
- **Produto:** Plataforma2ASmart.Auth — serviço de autenticação/identidade (a definir conforme as histórias).
- **Local:** `apps/Plataforma2ASmart.Auth/` (monorepo multi-produto — ADR-0030).

## Estado atual
- **Autenticação (AZ-12094) implementada:** ASP.NET Core Identity + JWT + refresh token rotacionado; login/refresh/change/forgot/reset; rate limit por IP. Ports isolam Identity/JWT/refresh/e-mail.
- **Administração de usuários (AZ-12114) implementada:** `CreateUser`/`UpdateUser` (cadastro com senha opcional → temporária + e-mail de boas-vindas preparado; edição de perfil/roles/status); autorização por **policy `Users.Manage`**; gate de **usuário inativo** no login.
- **Banco:** SQL Server · **Acesso a dados:** EF Core + Unit of Work (`EfUnitOfWork` → `IUnitOfWork` do BuildingBlocks). Migrações em `db/sqlserver/migrations` (`0001` Identity+refresh, `0002` perfil `Name`/`IsActive`).
- **Camada de API:** Controllers (ADR-0028), `Program.cs` enxuto via `Extensions/`, envelope/exception handler do **BuildingBlocks**, OpenAPI + Scalar + Swagger.
- **Blocos compartilhados:** `building-blocks/BuildingBlocks.*` (dispatcher, Result/Notification, envelope, IUnitOfWork, behaviors).
- **Config por ambiente:** Development/Staging/Production + launchSettings (porta 5090 dev). Build `-warnaserror` e testes verdes.

## Decisões-chave
- **Produto** (`docs/adr/`): [P0001 Identity+JWT](adr/0001-identity-jwt-autenticacao.md) · [P0002 Administração de usuários + autorização por policy](adr/0002-administracao-usuarios-autorizacao.md).
- **Transversais** (`docs/adr/` na raiz): Clean Architecture (0002), dispatcher próprio (0003), Result/envelope (0014), EF/Dapper+UoW (0020), config por ambiente (0022), camada de API (0028), código limpo (0029), monorepo multi-produto (0030).

## Backlog / features
- Índice: [`features/README.md`](features/README.md).
- **AZ-12094 — Autenticação e Gerenciamento de Senha** — implementada. Doc: [`features/AZ-12094-...md`](features/AZ-12094-autenticacao-e-gerenciamento-de-senha.md).
- **AZ-12114 — Cadastro e Edição de Usuário** — implementada (em PR). Doc: [`features/AZ-12114-...md`](features/AZ-12114-cadastro-e-edicao-de-usuario.md).

## Setup local
- `dotnet run --project apps/Plataforma2ASmart.Auth/src/Plataforma2ASmart.Auth.Api` (API em http://localhost:5090; docs em `/scalar`).
- Banco: ajuste a connection string `Default` (Development já aponta para `localhost`).

---
> Atualize este arquivo ao final de cada feature/decisão relevante (faz parte do "Done").
