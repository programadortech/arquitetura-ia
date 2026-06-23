# Padrão: Observabilidade (Serilog + OpenTelemetry)

Uma história correlacionada por requisição entre **logs, traces e métricas**, exportada via OTLP para um
collector que distribui para Seq / Grafana Loki / outros backends.

## Logging — Serilog, estruturado
- Message templates estruturados com propriedades nomeadas — **nunca** interpolação/concatenação de strings:
  - ✅ `Log.Information("Order {OrderId} confirmed for {CustomerId}", orderId, customerId);`
  - ❌ `Log.Information($"Order {orderId} confirmed");`
- Enriqueça todo log com: `service.name`, `service.version`, `deployment.environment`, `TraceId`, `SpanId`,
  `CorrelationId`, máquina/instância.
- Níveis: `Error` para falhas que exigem atenção, `Warning` para algo tratado mas notável, `Information` para
  marcos de negócio, `Debug` para diagnósticos de desenvolvimento. Sem spam de `Information` em loops quentes.
- **Nunca** logue segredos/PII sem máscara. Mascare no nível do sink/enricher.
- Sinks: Console (JSON em produção) + sink OpenTelemetry → Collector. Seq diretamente em dev é aceitável.

## Tracing — OpenTelemetry
- Um `ActivitySource` por serviço. Crie spans ao redor de: ingresso na API, cada caso de uso, chamadas de DB,
  publish/consume de fila, HTTP de saída, jobs.
- Nomes de span: `<Verb> <Noun>` (ex.: `ConfirmOrder`, `OracleQuery OrderById`). Defina o status em caso de erro.
- **Propague o contexto** através de limites de mensageria (injete/extraia o trace context no envelope da mensagem).
- Use libs de instrumentação: ASP.NET Core, HttpClient, EF Core/ADO, além de spans customizados.

## Métricas — `Meter` do OpenTelemetry
- Técnicas: duração da requisição, duração do caso de uso, duração da chamada de DB, contagem de retry, estado do circuit, lag de fila,
  sucesso/falha de job, taxa de erro.
- De negócio: contadores de domínio (ex.: pedidos confirmados) onde for útil.
- Mantenha a cardinalidade dos labels limitada — sem ids de usuário / valores ilimitados como labels.

## Topologia de exportação
```
App (Serilog + OTel SDK)  ──OTLP──▶  OpenTelemetry Collector  ──▶  Seq
                                                              ├──▶  Grafana Loki (logs)
                                                              ├──▶  Tempo/Jaeger (traces)
                                                              └──▶  Prometheus (metrics)
```
Todos os sinais compartilham `service.name` / `service.version` / `deployment.environment` para correlação.
Endpoint configurado via `OTEL_EXPORTER_OTLP_ENDPOINT` (configuração, não código).

## Contrato de telemetria por feature
O doc de arquitetura de cada feature lista seus spans, métricas e eventos de log estruturados (com nomes de propriedades).
O agente `observability-engineer` é o responsável por isso. Veja [ADR-0005](../adr/0005-observability-stack.md).
