# Observabilidade

Contratos de telemetria e referências operacionais. Regras:
[`../standards/observability.md`](../standards/observability.md).

## Contrato de telemetria por feature (template)
Para cada feature, documente:
- **Spans**: nome, atributos, onde são criados.
- **Métricas**: nome, tipo (counter/histogram/gauge), unidade, labels.
- **Eventos de log estruturado**: template da mensagem + nomes de propriedades + nível.
- **Correlação**: como o contexto de trace flui (API → caso de uso → DB → fila → job).

## Topologia de exportação
App (Serilog + OTel SDK) → OTLP → OpenTelemetry Collector → Seq / Grafana Loki / Tempo / Prometheus.
Endpoint via `OTEL_EXPORTER_OTLP_ENDPOINT`. Todos os sinais compartilham `service.name` / `service.version` /
`deployment.environment`.

## Catálogo
| Feature | Spans | Métricas | Eventos de log |
|---|---|---|---|
| _nenhum ainda_ | — | — | — |
