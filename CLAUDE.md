# CLAUDE.md — Template Corporativo (C# / Oracle / Clean Architecture)

> Este repositório é uma **fábrica de templates / scaffolding corporativo**, não um sistema de negócio.
> Ele contém os padrões, agentes, skills, hooks, documentos e templates usados para
> gerar e evoluir projetos reais sob demanda.

## O que este repositório é

Este é um **meta-repositório**. Ele **não** contém código de negócio. Em vez disso, ele fornece:

- **Padrões** (`docs/standards/`) — as regras vinculantes que todo projeto gerado deve seguir.
- **Agentes** (`.claude/agents/`) — papéis especializados que o Claude assume para planejar, construir e revisar.
- **Skills** (`.claude/skills/`) — procedimentos executáveis invocados com `/skill-name`.
- **Hooks** (`.claude/hooks/`) — gates de qualidade automatizados conectados via `.claude/settings.json`.
- **Templates** (`templates/`) — os documentos e formatos de código a partir dos quais novos artefatos são gerados.
- **Scripts de validação** (`scripts/`) — verificações em PowerShell para arquitetura, Oracle, testes e PRs.
- **Docs de arquitetura e ADRs** (`docs/`) — o registro de design duradouro.

## Como você (Claude) deve usá-lo

Quando o usuário pedir um dos itens a seguir, **invoque a skill correspondente** — não improvise:

| Intenção do usuário | Skill |
|---|---|
| "crie um projeto com nome X" | `/create-project` |
| "importe a história / issue #N" | `/import-story` |
| "crie uma história técnica de setup/arquitetura" | `/create-tech-story` |
| "crie as tasks da história N no azure" | `/sync-tasks` |
| "crie uma feature Y" | `/create-feature` |
| "abra arquitetura da feature Z" | `/approve-architecture` |
| "implemente o use case W" | `/create-usecase` |
| "crie um script Oracle …" | `/create-oracle-script` |
| "crie um job …" | `/create-job` |
| "crie um provider de fila …" | `/create-queue-provider` |
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
| Banco de dados | Oracle (`Oracle.ManagedDataAccess.Core`, provider EF Core Oracle) | [ADR-0004](docs/adr/0004-oracle-database.md) |
| Logging | Serilog, logs estruturados | [ADR-0005](docs/adr/0005-observability-stack.md) |
| Telemetria | OpenTelemetry (traces, métricas, logs) → OTLP Collector / Seq / Grafana Loki | [ADR-0005](docs/adr/0005-observability-stack.md) |
| Resiliência | Polly (retry, circuit breaker, timeout) | [ADR-0006](docs/adr/0006-resilience-polly.md) |
| Jobs em background | Hangfire | [ADR-0007](docs/adr/0007-jobs-hangfire.md) |
| Mensageria / filas | Providers plugáveis: Kafka, SQS, RabbitMQ, MQTT | [ADR-0008](docs/adr/0008-pluggable-queue-providers.md) |
| Testes | Unitários + Integração + Arquitetura | [ADR-0009](docs/adr/0009-testing-strategy.md) |
| Tracker de histórias | Plugável: GitHub Issues / Azure DevOps / GitLab (via config) | [ADR-0010](docs/adr/0010-pluggable-issue-trackers.md) |
| Tasks no tracker | Write-back das atividades planejadas como itens-filho da história | [ADR-0011](docs/adr/0011-task-writeback-tracker.md) |
| Tipos de história | Negócio e Técnica (arquitetura/infra/setup) | [ADR-0012](docs/adr/0012-story-types-business-technical.md) |

## Layout padrão da solução (projetos gerados)

Todo projeto criado via `/create-project` produz:

```
<ProjectName>/
├── src/
│   ├── <ProjectName>.Domain/            # Entities, value objects, domain events, sem dependências
│   ├── <ProjectName>.Application/        # Use cases, contratos do dispatcher, ports (interfaces)
│   ├── <ProjectName>.Infrastructure/     # Adapters de Oracle, mensageria, Hangfire, Polly, OTel
│   └── <ProjectName>.Api/                # Host ASP.NET Core, composition root de DI, endpoints
├── tests/
│   ├── <ProjectName>.UnitTests/
│   ├── <ProjectName>.IntegrationTests/
│   └── <ProjectName>.ArchitectureTests/
├── db/
│   └── oracle/                           # scripts de migração e seed versionados
├── docs/                                 # arquitetura, features e ADRs por projeto
└── <ProjectName>.sln
```

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
2. **Nunca** introduza uma dependência paga (MediatR ≥ v12 comercial, etc.). Use o dispatcher do repositório.
3. **Toda** nova decisão arquitetural → um novo ADR usando `templates/adr-template.md`.
4. **Toda** feature começa como um documento (`templates/feature-template.md`) antes do código.
5. **Todo** caso de uso tem testes unitários; código que toca integração tem testes de integração.
6. Logs são **estruturados** (message templates + propriedades), nunca concatenados como string.
7. Segredos nunca vão para o código-fonte. Use configuration providers / variáveis de ambiente.
8. Respeite os hooks — se um hook bloquear uma ação, corrija a causa, não o contorne.

## Checklist de qualidade (todo PR)

Veja [`docs/standards/quality-checklist.md`](docs/standards/quality-checklist.md) e
[`templates/pr-template.md`](templates/pr-template.md). O hook `pre-pr-check` e o
`scripts/validate-pr.ps1` impõem o gate.
