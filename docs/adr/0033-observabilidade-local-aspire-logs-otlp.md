# ADR-0033: Dashboard de observabilidade local (Aspire) + logs via OTLP

- **Status:** Aceita
- **Data:** 2026-06-25
- **Decisores:** Acaciano (tech lead), Claude

## Contexto
A stack de observabilidade (ADR-0005) exporta traces/métricas via **OTLP** para `localhost:4317`, mas o repositório
não trazia um **backend com UI** para visualizar em dev — abrir `http://localhost:4317` (endpoint de ingestão gRPC,
não uma página) não mostra nada. Além disso, os **logs** iam só para o **console** (Serilog), não para o OTLP, então
não apareciam junto de traces/métricas.

## Decisão
1. **Dashboard local = .NET Aspire Dashboard** (um container) via `docker-compose.observability.yml`: recebe OTLP e
   mostra **traces + métricas + logs** numa UI (`http://localhost:18888`). Ingestão OTLP gRPC mapeada para o host
   `4317` (→ `18889` no container), que é o destino padrão dos apps.
2. **Logs também via OTLP:** o Serilog passa a exportar para OTLP (sink `Serilog.Sinks.OpenTelemetry`) **além** do
   console, para os logs caírem no dashboard correlacionados com os traces.
3. **Endpoint OTLP por configuração:** `AddObservability` lê `OpenTelemetry:Otlp:Endpoint` (por ambiente) e aplica
   aos exportadores de traces/métricas/logs; sem endpoint válido, usa o default e não quebra.

## Consequências
- (+) Em dev, basta `docker compose -f docker-compose.observability.yml up -d` + rodar a API para ver tudo num lugar.
- (+) Logs/traces/métricas correlacionados; endpoint flexível por ambiente.
- (−) Dashboard Aspire é para **dev** (sobe sem autenticação); produção usa o backend real (Collector/Seq/Grafana — ADR-0005).
- (−) Mais um pacote (sink OTLP do Serilog) e a dependência de Docker para o dashboard local.

## Alternativas consideradas
- **OTel Collector + Jaeger + Grafana/Loki:** mais realista/completo, porém vários containers e configuração — exagero para o loop de dev.
- **Seq:** ótimo para logs, mas o Aspire cobre traces+métricas+logs num container só.

## Referências
- `docker-compose.observability.yml` · `Api/Extensions/ServiceCollectionExtensions.cs` (AddObservability) · ADR-0005 (stack de observabilidade) · `docs/setup-local.md`.
