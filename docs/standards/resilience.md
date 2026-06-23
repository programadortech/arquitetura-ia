# Padrão: Resiliência (Polly)

Toda chamada que cruza um limite de processo/rede (Oracle, filas, HTTP, cache) deve ser envolvida em uma
política de resiliência. Usamos **Polly** (`Microsoft.Extensions.Http.Resilience` / pipelines do Polly v8).

## Blocos de construção da política padrão
- **Timeout** — toda chamada externa tem um limite superior (resistência a DoS + fail-fast).
- **Retry (nova tentativa)** — somente falhas transientes, com **exponential backoff + jitter**, tentativas limitadas (ex.: 3).
  Nunca faça retry cego de operações não idempotentes.
- **Circuit breaker** — abre após um limiar de falhas para aliviar a carga sobre uma dependência com problemas.
- **Fallback** — onde existe uma resposta degradada sensata.
- **Bulkhead / rate limiter** — para isolar e limitar a concorrência em recursos compartilhados.

## Pipelines padrão (nomeados, registrados centralmente)
| Pipeline | Usado para | Composição |
|---|---|---|
| `database` | chamadas de DB (qualquer provider) | timeout + retry(transient) + circuit breaker |
| `http-outbound` | HTTP externo | timeout + retry + circuit breaker + (opcional) fallback |
| `queue-publish` | produção de mensagens | timeout + retry + circuit breaker |
| `queue-consume` | consumo | retry e então dead-letter (sem retry infinito) |

Registre via um `ResiliencePipelineRegistry` em Infrastructure; resolva por nome. Configurável via
`appsettings` (tentativas, atrasos, limiares) — sem números mágicos no código.

## Regras
- Sem retries ilimitados em lugar nenhum (previne tempestades de retry / DoS acidental).
- Retries apenas para falhas **transientes**; classifique exceções explicitamente.
- Idempotência obrigatória antes de fazer retry de escritas/publicações.
- Toda transição de retry/circuit emite uma métrica + log estruturado (veja `observability.md`).
- Mensagens venenosas vão para uma DLQ/fila de parking após tentativas limitadas (veja `queue-providers.md`).

Veja [ADR-0006](../adr/0006-resilience-polly.md).
