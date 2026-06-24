# PRODUCT.md — Contexto do Produto

> Contexto vivo que a IA **lê primeiro** ao evoluir o produto. Mantenha curto e atualizado.

## Visão
- **Produto:** Plataforma2A.Auth — serviço de autenticação e identidade.
- **Domínio/segmento:** plataforma corporativa (auth/identidade).

## Estado atual
- **Solução:** criada em `src/` (monorepo, ADR-0019). Builda verde com `-warnaserror`; testes verdes.
- **Banco:** SQL Server · **Acesso a dados:** EF Core + Unit of Work (`IUnitOfWork`/`EfUnitOfWork`).
- **Fila:** RabbitMQ (abstração) · **Jobs:** none · **API docs:** Scalar + Swagger · **Gateway:** none.
- **Camada de API:** **Controllers** (ADR-0028) — borda fina, `Program.cs` enxuto via `Extensions/`, status codes semânticos (201 no create); gate `validate-api-conventions.ps1` no CI.
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
- **AZ-12094 — Autenticação e Gerenciamento de Senha** — implementada (mergeada em `dev`, PR #1). Docs: [feature](features/AZ-12094-autenticacao-e-gerenciamento-de-senha.md) · [arquitetura](architecture/AZ-12094-autenticacao-e-gerenciamento-de-senha.md).
- **AZ-12114 — Cadastro e Edição de Usuário** — importada (pronta p/ arquitetura). Admin CRUD de usuários (Identity), roles, status, senha opcional + interface de e-mail de boas-vindas (envio é história futura). Doc: [feature](features/AZ-12114-cadastro-e-edicao-de-usuario.md).

## Fluxo de PR e revisão (custo zero)
- Branches sempre da `main`: `feature/{id}-{slug}` → PR para `dev`; `hotfix/{id}-{slug}` → PR para `staging` (ADR-0023).
- **Gate grátis em CI** (`ci.yml`: build/test/arquitetura) é check obrigatório (ruleset `protect-dev-staging`). Revisão de IA é **local sob demanda** via `/review-pr` (sem custo). Ver ADR-0025 / [`standards/pr-review-automation.md`](standards/pr-review-automation.md).
- **Merge é manual (decisão do usuário):** o agente abre o PR, acompanha o CI e, quando verde, **avisa**; o usuário faz o merge na plataforma.
- Abertura/gerência de PR daqui usa o **GitHub CLI (`gh`)** autenticado. Segredos dos scripts de tracker via `.env` (gitignored) ou variável de ambiente.

## Integrações ativas
- _(nenhuma ainda — e-mail entra com o fluxo de reset de senha; decidir provedor pelo catálogo)_

---
> Atualize este arquivo ao final de cada feature/decisão relevante (faz parte do "Done").
