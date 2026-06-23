# ADR-0005: Observabilidade — Serilog + OpenTelemetry

- **Status:** Aceito
- **Data:** 2026-06-22

## Contexto
Sistemas em produção precisam ser diagnosticáveis: logs, traces e métricas correlacionados, exportáveis para qualquer
backend que o ambiente execute (Seq, Grafana Loki ou um OTLP Collector).

## Decisão
Usar **Serilog** para logging estruturado e **OpenTelemetry** para traces e métricas. Exportar tudo
via **OTLP** para um **OpenTelemetry Collector**, que distribui para Seq / Grafana Loki / Tempo / Prometheus.
Todos os sinais compartilham `service.name`, `service.version`, `deployment.environment` e um trace id propagado.
Regras em [`docs/standards/observability.md`](../standards/observability.md).

## Consequências
- (+) Agnóstico a backend, neutro em relação a fornecedor via OTLP; uma narrativa correlacionada por requisição.
- (+) Logs estruturados permitem consulta/alertas; o contexto de trace flui através das mensagens.
- (−) Requer infraestrutura de collector e disciplina de design de telemetria por funcionalidade.

## Alternativas consideradas
- Logs em texto puro: não consultáveis, sem correlação.
- SDKs específicos de fornecedor diretamente: lock-in; o OTLP nos mantém portáveis.
