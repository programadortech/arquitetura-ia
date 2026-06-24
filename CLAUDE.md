# CLAUDE.md — Monorepo do Produto (C# / Clean Architecture) + Fábrica

> Este repositório é o **monorepo do produto**: o código vive na raiz em `src/` + `tests/`, e a
> **fábrica** (padrões, agentes, skills, hooks, templates) convive ao lado para **gerar e evoluir** o
> produto com contexto persistente. Ver [ADR-0019](docs/adr/0019-product-monorepo-src-layout.md).

## O que este repositório é

Um **monorepo**: **produto** (`src/`, `tests/`) + **fábrica** que o constrói/evolui:

- **Produto** (`src/`, `tests/`, `<Produto>.sln`) — a solução .NET em Clean Architecture (criada por `/create-project`).
- **Contexto do produto** (`docs/PRODUCT.md`) — visão, estado, decisões-chave e convenções. **Leia primeiro** ao evoluir o produto.
- **Padrões** (`docs/standards/`) — regras vinculantes que o produto segue.
- **Agentes** (`.claude/agents/`) — papéis especializados para planejar, construir e revisar.
- **Skills** (`.claude/skills/`) — procedimentos executáveis invocados com `/skill-name`.
- **Hooks** (`.claude/hooks/`) — gates de qualidade via `.claude/settings.json`.
- **Templates** (`templates/`) — formatos a partir dos quais novos artefatos são gerados.
- **Catálogo de integrações** (`docs/integrations/`) — provedores plugáveis (e-mail/SMS/storage/pagamentos) que os agentes consultam para decidir.
- **Scripts de validação** (`scripts/`) — verificações em PowerShell (arquitetura, banco, testes, PRs).
- **ADRs** (`docs/adr/`) — o registro de design duradouro.

> Para iniciar um **produto diferente**, clone este repo como *starter* e limpe `src/`/`tests/` e os docs do produto.

## Como você (Claude) deve usá-lo

Quando o usuário pedir um dos itens a seguir, **invoque a skill correspondente** — não improvise:

| Intenção do usuário | Skill |
|---|---|
| "crie um projeto com nome X" | `/create-project` |
| "importe a história / issue #N" | `/import-story` |
| "faça um brainstorm / refine a história N" | `/brainstorm-story` |
| "crie uma história técnica de setup/arquitetura" | `/create-tech-story` |
| "crie as tasks da história N no azure" | `/sync-tasks` |
| "crie uma feature Y" | `/create-feature` |
| "abra arquitetura da feature Z" | `/approve-architecture` |
| "implemente o use case W" | `/create-usecase` |
| "crie um script de banco / Oracle …" | `/create-db-script` |
| "crie um job …" | `/create-job` |
| "crie um provider de fila …" | `/create-queue-provider` |
| "adicione uma integração (e-mail/SMS/…)" | `/create-integration` |
| "crie os testes …" | `/create-tests` |
| "revise o PR …" | `/review-pr` |

Antes de qualquer mudança não trivial, consulte os **padrões** relevantes em `docs/standards/` e os
**ADRs** em `docs/adr/`. Se um pedido conflitar com um padrão, exponha o conflito e proponha
seguir o padrão ou registrar um novo ADR — nunca divirja silenciosamente.

## Stack tecnológica obrigatória

| Preocupação | Decisão | ADR |
|---|---|---|
| Linguagem / runtime | C# / .NET 10 (LTS) | [ADR-0001](docs/adr/0001-record-architecture-decisions.md) |
| Arquitetura | Clean Architecture (Domain → Application → Infrastructure → Api) | [ADR-0002](docs/adr/0002-clean-architecture.md) |
| Casos de uso | Abstração de dispatcher própria (`IUseCase` + `IUseCaseDispatcher`) — **sem MediatR pago** | [ADR-0003](docs/adr/0003-usecase-dispatcher-no-mediatr.md) |
| Banco de dados | Plugável: Oracle / SQL Server / PostgreSQL / MySQL (via config; padrão Oracle) | [ADR-0013](docs/adr/0013-pluggable-database-providers.md) |
| Logging | Serilog, logs estruturados | [ADR-0005](docs/adr/0005-observability-stack.md) |
| Telemetria | OpenTelemetry (traces, métricas, logs) → OTLP Collector / Seq / Grafana Loki | [ADR-0005](docs/adr/0005-observability-stack.md) |
| Resiliência | Polly (retry, circuit breaker, timeout) | [ADR-0006](docs/adr/0006-resilience-polly.md) |
| Tratamento de erros | Result/Notification + envelope `ApiResponse` + middleware global (sem `throw` para negócio) | [ADR-0014](docs/adr/0014-error-handling-result-notification.md) |
| Documentação de API | OpenAPI nativo + UI plugável (Scalar + Swagger, default) | [ADR-0015](docs/adr/0015-pluggable-api-documentation.md) |
| Integrações | Plugáveis + catálogo `docs/integrations/` (e-mail/SMS/storage/pagamentos) | [ADR-0016](docs/adr/0016-pluggable-integrations-catalog.md) |
| API Gateway | Opcional (YARP) — `gateway: yarp \| none` | [ADR-0017](docs/adr/0017-optional-api-gateway-yarp.md) |
| Jobs em background | **Opcional** (Hangfire) — `jobs: hangfire \| none` (default none) | [ADR-0018](docs/adr/0018-optional-hangfire-jobs.md) · [ADR-0007](docs/adr/0007-jobs-hangfire.md) |
| Mensageria / filas | Providers plugáveis: Kafka, SQS, RabbitMQ, MQTT | [ADR-0008](docs/adr/0008-pluggable-queue-providers.md) |
| Banco de dados | Plugável: Oracle / SQL Server / PostgreSQL / MySQL | [ADR-0013](docs/adr/0013-pluggable-database-providers.md) |
| Acesso a dados | EF Core **ou** Dapper (selecionável), ambos com **Unit of Work** | [ADR-0020](docs/adr/0020-data-access-efcore-or-dapper-uow.md) |
| Testes | Unitários + Integração + Arquitetura | [ADR-0009](docs/adr/0009-testing-strategy.md) |
| Tracker de histórias | Plugável: GitHub Issues / Azure DevOps / GitLab (via config) | [ADR-0010](docs/adr/0010-pluggable-issue-trackers.md) |
| Tasks no tracker | Write-back das atividades planejadas como itens-filho da história | [ADR-0011](docs/adr/0011-task-writeback-tracker.md) |
| Tipos de história | Negócio e Técnica (arquitetura/infra/setup) | [ADR-0012](docs/adr/0012-story-types-business-technical.md) |
| Layout | Monorepo: produto em `src/` + fábrica embutida; contexto em `docs/PRODUCT.md` | [ADR-0019](docs/adr/0019-product-monorepo-src-layout.md) |

## Layout padrão da solução (projetos gerados)

`/create-project` cria a solução **na raiz do monorepo** (não em subpasta):

```
/ (raiz do monorepo)
├── src/
│   ├── <Produto>.Domain/            # Entities, value objects, domain events, sem dependências
│   ├── <Produto>.Application/        # Use cases, dispatcher, ports, Result/Notification
│   ├── <Produto>.Infrastructure/     # Adapters de banco, mensageria, integrações, Polly, OTel
│   ├── <Produto>.Api/                # Host ASP.NET Core, DI, endpoints, envelope, middleware, OpenAPI
│   └── <Produto>.Gateway/            # (opcional, YARP) ponto único de entrada para o front
├── tests/  (UnitTests · IntegrationTests · ArchitectureTests)
├── db/<provider>/                    # migração e seed versionados (provider plugável)
├── docs/                             # PRODUCT.md, features, architecture, adr, standards, integrations
├── .claude/ · templates/ · scripts/  # a fábrica (tooling)
└── <Produto>.sln
```

Opções do `/create-project`: `db` (oracle|sqlserver|postgresql|mysql) · `dataaccess` (efcore|dapper, default efcore) ·
`queue` (kafka|sqs|rabbitmq|mqtt) · `jobs` (hangfire|none, default none) · `apidocs` (scalar,swagger|…) ·
`gateway` (yarp|none, default none). Ambos os ORMs têm **Unit of Work** (ver
[ADR-0020](docs/adr/0020-data-access-efcore-or-dapper-uow.md)).

## A regra de dependência (inegociável)

```
Api ──▶ Application ──▶ Domain
 │            ▲
 └──▶ Infrastructure ─┘   (Infrastructure implementa as ports da Application; Domain não depende de nada)
```

- `Domain` não referencia **nada**.
- `Application` referencia **apenas** `Domain`. Ela define **ports** (interfaces); nunca referencia Infrastructure.
- `Infrastructure` referencia `Application` + `Domain` e implementa as ports (Oracle, filas, jobs, telemetria).
- `Api` referencia tudo **apenas** para compor a injeção de dependências. Sem lógica de negócio na `Api`.
- Os testes de arquitetura (`scripts/validate-clean-architecture.ps1` + NetArchTest) impõem isso.

## Regras de trabalho para o Claude

1. **Sempre execute o script de validação relevante** após gerar código (veja `scripts/`).
2. **Nunca** introduza uma dependência paga/proibida: MediatR (≥ v12 comercial) → use o dispatcher do repositório;
   **AutoMapper** → use mappers estáticos explícitos (`ToResponse`/`ToEntity`, ver `docs/standards/mapping.md`).
3. **Toda** nova decisão arquitetural → um novo ADR usando `templates/adr-template.md`.
4. **Toda** feature começa como um documento (`templates/feature-template.md`) antes do código.
5. **Todo** caso de uso tem testes unitários; código que toca integração tem testes de integração.
6. Logs são **estruturados** (message templates + propriedades), nunca concatenados como string.
7. Segredos nunca vão para o código-fonte. Use configuration providers / variáveis de ambiente.
8. Respeite os hooks — se um hook bloquear uma ação, corrija a causa, não o contorne.
9. **Mantenha a documentação sempre atualizada** (faz parte do "Done"): ao mudar fluxo, skills, padrões,
   stack ou opções, atualize `docs/guia/index.html`, o `CLAUDE.md`, os índices afetados e o `docs/PRODUCT.md`.
   Toda nova decisão tem ADR. Documentação desatualizada é defeito.

## Checklist de qualidade (todo PR)

Veja [`docs/standards/quality-checklist.md`](docs/standards/quality-checklist.md) e
[`templates/pr-template.md`](templates/pr-template.md). O hook `pre-pr-check` e o
`scripts/validate-pr.ps1` impõem o gate.
