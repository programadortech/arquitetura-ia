# Architecture Decision Records

Numerados sequencialmente, imutáveis depois de Aceitos. Substitua em vez de editar. Template:
[`templates/adr-template.md`](../../templates/adr-template.md).

| ADR | Título | Status |
|---|---|---|
| [0001](0001-record-architecture-decisions.md) | Registrar decisões de arquitetura | Aceito |
| [0002](0002-clean-architecture.md) | Clean Architecture | Aceito |
| [0003](0003-usecase-dispatcher-no-mediatr.md) | Use Case Dispatcher customizado (sem MediatR pago) | Aceito |
| [0004](0004-oracle-database.md) | Oracle como banco de dados relacional | Substituída por ADR-0013 |
| [0005](0005-observability-stack.md) | Observabilidade — Serilog + OpenTelemetry | Aceito |
| [0006](0006-resilience-polly.md) | Resiliência com Polly | Aceito |
| [0007](0007-jobs-hangfire.md) | Jobs em background com Hangfire | Aceito |
| [0008](0008-pluggable-queue-providers.md) | Provedores de fila plugáveis | Aceito |
| [0009](0009-testing-strategy.md) | Estratégia de testes | Aceito |
| [0010](0010-pluggable-issue-trackers.md) | Trackers de histórias plugáveis (GitHub / Azure DevOps / GitLab) | Aceito |
| [0011](0011-task-writeback-tracker.md) | Write-back de tasks para o tracker | Aceito |
| [0012](0012-story-types-business-technical.md) | Tipos de história — negócio e técnica | Aceito |
| [0013](0013-pluggable-database-providers.md) | Bancos de dados relacionais plugáveis (Oracle / SQL Server / PostgreSQL / MySQL) | Aceito |
| [0014](0014-error-handling-result-notification.md) | Tratamento de erros — Result/Notification + Envelope + middleware | Aceito |
| [0015](0015-pluggable-api-documentation.md) | Documentação de API plugável (OpenAPI + Scalar/Swagger) | Aceito |
| [0016](0016-pluggable-integrations-catalog.md) | Integrações plugáveis + catálogo (docs/integrations) | Aceito |
| [0017](0017-optional-api-gateway-yarp.md) | API Gateway opcional (YARP) | Aceito |
| [0018](0018-optional-hangfire-jobs.md) | Jobs (Hangfire) opcional no scaffold | Aceito |
| [0019](0019-product-monorepo-src-layout.md) | Produto no monorepo (solução em src/) | Aceito |
| [0020](0020-data-access-efcore-or-dapper-uow.md) | Acesso a dados plugável — EF Core ou Dapper, com Unit of Work | Aceito |

> Próximo número livre: **0021**. Adicione novos ADRs com `/approve-architecture` ou manualmente a partir do template.
