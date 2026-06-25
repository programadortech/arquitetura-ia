# PRODUCT.md — Plataforma2ASmart.Auth

> Contexto vivo deste produto. A IA **lê primeiro** ao evoluí-lo. Manter curto e atualizado.

## Visão
- **Produto:** Plataforma2ASmart.Auth — serviço de autenticação/identidade (a definir conforme as histórias).
- **Local:** `apps/Plataforma2ASmart.Auth/` (monorepo multi-produto — ADR-0030).

## Estado atual
- **Esqueleto criado** (`/create-project`): Clean Architecture (Domain/Application/Infrastructure/Api), build verde com `-warnaserror`; testes verdes. **Sem lógica de negócio ainda.**
- **Banco:** SQL Server · **Acesso a dados:** EF Core + Unit of Work (`EfUnitOfWork` → `IUnitOfWork` do BuildingBlocks).
- **Camada de API:** Controllers (ADR-0028), `Program.cs` enxuto via `Extensions/`, envelope/exception handler do **BuildingBlocks**, OpenAPI + Scalar + Swagger.
- **Blocos compartilhados:** `building-blocks/BuildingBlocks.*` (dispatcher, Result/Notification, envelope, IUnitOfWork, behaviors).
- **Config por ambiente:** Development/Staging/Production + launchSettings (porta 5090 dev).

## Decisões-chave (transversais — ver `docs/adr/` na raiz)
- Clean Architecture (0002), dispatcher próprio (0003), Result/envelope (0014), EF/Dapper+UoW (0020), config por ambiente (0022), camada de API (0028), código limpo (0029), monorepo multi-produto (0030).

## Backlog / features
- Índice: [`features/README.md`](features/README.md).
- **AZ-12094 — Autenticação e Gerenciamento de Senha** — importada (a refinar). Doc: [`features/AZ-12094-autenticacao-e-gerenciamento-de-senha.md`](features/AZ-12094-autenticacao-e-gerenciamento-de-senha.md).

## Setup local
- `dotnet run --project apps/Plataforma2ASmart.Auth/src/Plataforma2ASmart.Auth.Api` (API em http://localhost:5090; docs em `/scalar`).
- Banco: ajuste a connection string `Default` (Development já aponta para `localhost`).

---
> Atualize este arquivo ao final de cada feature/decisão relevante (faz parte do "Done").
