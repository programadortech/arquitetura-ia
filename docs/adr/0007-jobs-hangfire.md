# ADR-0007: Jobs em background com Hangfire

- **Status:** Aceito
- **Data:** 2026-06-22

## Contexto
Precisamos de processamento em background e agendado confiável (fire-and-forget, atrasado, recorrente) com
persistência, retries e visibilidade, integrado ao nosso armazenamento Oracle.

## Decisão
Usar **Hangfire** com armazenamento **Oracle**. Os jobs são orquestradores enxutos que despacham um use case via
`IUseCaseDispatcher` — sem lógica de negócio nos jobs. Jobs recorrentes usam ids estáveis registrados de forma idempotente.
O dashboard é protegido. Regras em [`docs/standards/jobs.md`](../standards/jobs.md).

## Consequências
- (+) Jobs duráveis com retries, agendamento e dashboard embutidos; consistente com o Oracle.
- (+) A lógica de negócio permanece na camada de aplicação e é testável de forma independente.
- (−) Schema de armazenamento do Hangfire no Oracle para gerenciar; o dashboard precisa ter controle de acesso.

## Alternativas consideradas
- Quartz.NET: capaz, mas sem a história embutida de dashboard/enfileiramento que queremos.
- Apenas `BackgroundService` hospedado: sem garantias de persistência/retry/agendamento.
