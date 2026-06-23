# Template Corporativo — C# / Oracle / Clean Architecture

Uma **fábrica de templates e scaffolding** (não um sistema de negócio) para gerar projetos .NET
corporativos com arquitetura, observabilidade, resiliência, mensageria e disciplina de qualidade consistentes.

> Leia o [`CLAUDE.md`](CLAUDE.md) primeiro — é o documento de governança de como este repositório é usado.

## O que tem aqui dentro
```
.
├── .claude/
│   ├── agents/      # 9 papéis especializados de revisão/construção
│   ├── skills/      # /create-project, /create-feature, /create-usecase, … (9 skills)
│   ├── hooks/       # gates de qualidade conectados via settings.json
│   └── settings.json
├── docs/
│   ├── standards/   # regras vinculantes (architecture, dispatcher, oracle, observability, …)
│   ├── adr/         # architecture decision records
│   ├── architecture/ features/ usecases/ database/ observability/ testing/
├── templates/       # project, feature, architecture, usecase, pr, test-plan, adr
├── scripts/         # gates de validação em PowerShell
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
| "crie um script Oracle …" | `/create-oracle-script` |
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
pwsh scripts/validate-oracle-scripts.ps1
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
