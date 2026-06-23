# ADR-0006: Resiliência com Polly

- **Status:** Aceito
- **Data:** 2026-06-22

## Contexto
Dependências externas (Oracle, filas, HTTP) falham de forma transitória. Precisamos de resiliência consistente
e limitada, que falhe rápido e evite tempestades de retry.

## Decisão
Usar **Polly** (pipelines de resiliência v8 / `Microsoft.Extensions.Http.Resilience`) para timeout, retry
(backoff exponencial + jitter, limitado, somente transitório), circuit breaker e, quando relevante, fallback /
rate limiting. Pipelines nomeados registrados centralmente e resolvidos por nome; parâmetros em configuração.
Regras em [`docs/standards/resilience.md`](../standards/resilience.md).

## Consequências
- (+) Resiliência uniforme, configurável e observável; sem retries ilimitados.
- (+) Falhas isoladas via circuit breakers; resistência a DoS via timeouts.
- (−) Requer idempotência antes de retentar escritas/publicações (garantido por revisão).

## Alternativas consideradas
- Loops de retry feitos à mão: inconsistentes, propensos a erro, sem circuit breaking.
- Sem resiliência: inaceitável para confiabilidade em produção.
