# Monorepo multi-produto — C# / Clean Architecture + Fábrica

Um **monorepo multi-produto**: cada produto vive em `apps/<Produto>/` (isolado, Clean Architecture), os blocos
transversais em `building-blocks/BuildingBlocks.*`, e a **fábrica** (padrões, agentes, skills, scripts, templates)
é compartilhada na raiz para gerar/evoluir os produtos com qualidade consistente. Ver
[ADR-0030](docs/adr/0030-monorepo-multiproduto.md).

> Leia o [`CLAUDE.md`](CLAUDE.md) primeiro — é o documento de governança de como este repositório é usado.
>
> 📖 **Guia visual do processo (HTML):** abra [`docs/guia/index.html`](docs/guia/index.html) no navegador —
> documentação moderna e completa de todo o fluxo (histórias → arquitetura → código → PR), agentes, skills,
> tipos de história, integração com tracker e guia do PO.
>
> 💻 **Rodar a API localmente:** ver [`docs/setup-local.md`](docs/setup-local.md) — pré-requisitos, `user-secrets`
> (`Jwt:Key`), banco e como subir.

## O que tem aqui dentro
```
.
├── .claude/          # agents + skills + hooks (fábrica)
├── docs/             # standards · adr · integrations · guia (transversal)
├── templates/        # formatos de artefatos
├── scripts/          # gates de validação (PowerShell)
├── building-blocks/  # código compartilhado: BuildingBlocks.Application · BuildingBlocks.Api
├── apps/             # OS PRODUTOS — apps/<Produto>/ (src, tests, db, docs, .slnx)
└── CLAUDE.md
```

## Como usar
Peça ao Claude em linguagem natural — cada frase mapeia para uma skill:

| Diga… | Executa |
|---|---|
| "crie um projeto com nome X" | `/create-project` |
| "crie uma feature Y" | `/create-feature` |
| "abra arquitetura da feature Z" | `/approve-architecture` |
| "implemente o use case W" | `/create-usecase` |
| "crie um script de banco / Oracle …" | `/create-db-script` |
| "crie um job …" | `/create-job` |
| "crie um provider de fila …" | `/create-queue-provider` |
| "crie os testes …" | `/create-tests` |
| "revise o PR …" | `/review-pr` |

## Stack obrigatória
.NET 10 · Clean Architecture · dispatcher de use case próprio (**sem MediatR pago**) · Oracle ·
Serilog + OpenTelemetry (OTLP → Seq / Grafana Loki / Collector) · Polly · Hangfire ·
filas plugáveis (Kafka / SQS / RabbitMQ / MQTT) · testes unitários + integração + arquitetura.

Veja os [ADRs](docs/adr/) para a justificativa e os [standards](docs/standards/) para as regras vinculantes.

## Gates de validação
```powershell
pwsh scripts/validate-clean-architecture.ps1
pwsh scripts/validate-architecture.ps1
pwsh scripts/validate-db-scripts.ps1
pwsh scripts/validate-tests.ps1
pwsh scripts/validate-pr.ps1     # gate agregado de PR
```

## Convenções
- Nova decisão arquitetural → um novo ADR (`templates/adr-template.md`).
- Nova feature → começa como documento antes de qualquer código.
- Todo caso de uso tem testes unitários; todo PR roda o checklist de qualidade.

## Para Product Owners
Como escrever histórias no tracker (Azure DevOps) para que sejam importáveis e implementáveis pela IA:
**[docs/standards/escrita-de-historias.md](docs/standards/escrita-de-historias.md)** — formato de
Description + Acceptance Criteria (Given/When/Then), Definition of Ready/Done e um exemplo preenchido.
