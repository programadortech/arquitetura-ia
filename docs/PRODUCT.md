# PRODUCT.md — Contexto do Produto

> Contexto vivo que a IA **lê primeiro** ao evoluir o produto. Mantenha curto e atualizado.

## Visão
- **Produto:** Plataforma2A.Auth — serviço de autenticação e identidade.
- **Domínio/segmento:** plataforma corporativa (auth/identidade).

## Estado atual
- **Solução:** criada em `src/` (monorepo, ADR-0019). Builda verde com `-warnaserror`; testes verdes.
- **Banco:** SQL Server · **Acesso a dados:** EF Core + Unit of Work (`IUnitOfWork`/`EfUnitOfWork`).
- **Fila:** RabbitMQ (abstração) · **Jobs:** none · **API docs:** Scalar + Swagger · **Gateway:** none.
- **Erros:** Result/Notification + envelope `ApiResponse` + middleware global (`GlobalExceptionHandler`).
- **Config:** por ambiente (Development/Staging/Production) + launchSettings.
- **Endpoints:** `/` (info), `/health`, `/scalar`, `/swagger`, `/openapi/v1.json` (docs fora de produção).
- **Auth (AZ-12094):** ASP.NET Core Identity + JWT Bearer + refresh token com rotação; rate limiting nativo.
  Endpoints `/api/auth/{login,refresh-token,change-password,forgot-password,reset-password}`.

## Decisões-chave do produto
- Tratamento de erros: Result/Notification + envelope (ADR-0014).
- Acesso a dados: EF Core + UoW (ADR-0020).
- Sem AutoMapper — mappers estáticos (ADR-0021).
- Integrações: catálogo `docs/integrations/` (ADR-0016).
- Autenticação: Identity + JWT + refresh token (ADR-0024).

## Convenções específicas do produto
- _(a definir conforme o domínio de auth evolui)_

## Backlog / features
- Índice: [`features/README.md`](features/README.md).
- **AZ-12094 — Autenticação e Gerenciamento de Senha** — implementada (branch `feature/12094-...`, PR para `dev`). Docs: [feature](features/AZ-12094-autenticacao-e-gerenciamento-de-senha.md) · [arquitetura](architecture/AZ-12094-autenticacao-e-gerenciamento-de-senha.md).

## Fluxo de PR e revisão
- Branches sempre da `main`: `feature/{id}-{slug}` → PR para `dev`; `hotfix/{id}-{slug}` → PR para `staging` (ADR-0023).
- Revisão automatizada por GitHub Action (Claude) a cada PR para `dev`/`staging`, postando APPROVE/REQUEST CHANGES
  como bot (ADR-0025 / [`standards/pr-review-automation.md`](standards/pr-review-automation.md)). Requer secret `ANTHROPIC_API_KEY`.
- Abertura/gerência de PR daqui usa o **GitHub CLI (`gh`)** autenticado (`gh auth login`).

## Integrações ativas
- _(nenhuma ainda — e-mail entra com o fluxo de reset de senha; decidir provedor pelo catálogo)_

---
> Atualize este arquivo ao final de cada feature/decisão relevante (faz parte do "Done").
