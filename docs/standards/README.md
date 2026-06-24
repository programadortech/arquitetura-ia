# Padrões (vinculantes)

Estas são as regras que todo projeto gerado deve seguir. Conflitos devem ser resolvidos seguindo
o padrão ou registrando um novo ADR — nunca por divergência silenciosa.

- [architecture.md](architecture.md) — camadas da Clean Architecture e regra de dependência.
- [usecase-dispatcher.md](usecase-dispatcher.md) — abstração de dispatcher própria (sem MediatR pago).
- [database.md](database.md) — banco relacional **plugável** (Oracle / SQL Server / PostgreSQL / MySQL).
- [oracle.md](oracle.md) — notas específicas do provider Oracle.
- [observability.md](observability.md) — Serilog + OpenTelemetry, logs estruturados, exportação OTLP.
- [resilience.md](resilience.md) — políticas Polly (timeout/retry/circuit breaker).
- [error-handling.md](error-handling.md) — Result/Notification + envelope `ApiResponse` + middleware global.
- [mapping.md](mapping.md) — mapeamento explícito via mappers estáticos (**sem AutoMapper**).
- [api-documentation.md](api-documentation.md) — OpenAPI plugável (Scalar / Swagger / ReDoc).
- [integrations.md](integrations.md) — integrações plugáveis + catálogo (docs/integrations).
- [api-gateway.md](api-gateway.md) — API Gateway opcional (YARP).
- [configuration.md](configuration.md) — configuração por ambiente (Development/Staging/Production).
- [branching.md](branching.md) — branches e fluxo de PR (feature→dev, hotfix→staging; sempre da main).
- [queue-providers.md](queue-providers.md) — Kafka/SQS/RabbitMQ/MQTT plugáveis.
- [jobs.md](jobs.md) — jobs em background/agendados com Hangfire (opcional).
- [testing.md](testing.md) — testes unitários + integração + arquitetura.
- [issue-trackers.md](issue-trackers.md) — trackers de histórias plugáveis (GitHub / Azure DevOps / GitLab).
- [escrita-de-historias.md](escrita-de-historias.md) — padrão de escrita de histórias para POs (Azure DevOps).
- [quality-checklist.md](quality-checklist.md) — o gate por PR.
