# Padrão: Jobs em Background (Hangfire)

Trabalho em background e agendado usa **Hangfire** com armazenamento em **Oracle**. Os jobs são finos: resolvem o
dispatcher e executam um caso de uso. **Sem lógica de negócio nos jobs.**

## Tipos de job
- **Fire-and-forget** — enfileire a partir de um caso de uso/adapter via `IBackgroundJobClient.Enqueue(...)`.
- **Delayed** — `Schedule(...)` para trabalho diferido.
- **Recurring** — `RecurringJob.AddOrUpdate("<stable-id>", ..., cron)` registrado de forma idempotente no startup.
- **Continuations** — encadeie trabalho subsequente após o sucesso.

## Regras
- Um método de job apenas: resolve `IUseCaseDispatcher`, monta a request, despacha, mapeia o resultado. Nada além disso.
- **Idempotente**: seguro para rodar duas vezes (Hangfire faz retry em caso de falha). Proteja com chaves/dedup para efeitos colaterais.
- Envolva o trabalho no pipeline Polly relevante; emita um span OTel + logs estruturados (id do job, tentativa, resultado).
- Respeite cancelamento/desligamento gracioso; limite o tempo de execução.
- Jobs recorrentes usam **ids estáveis** e são (re)registrados de forma idempotente — sem duplicatas entre reinícios.
- Configure retry/tentativas explicitamente; encaminhe jobs esgotados para um estado de falha para alerta (sem perda silenciosa).

## Infraestrutura
- Armazenamento: Oracle (Hangfire.Oracle / compatível). Dashboard **protegido** (auth obrigatória), não público.
- Server registrado no host da Api (ou um host worker dedicado) via `AddHangfire` + `AddHangfireServer`.
- Filas/prioridades definidas explicitamente; jobs longos em uma fila separada das sensíveis a latência.

## Testes
- Teste unitariamente a orquestração do job com o dispatcher mockado.
- Teste de integração para enqueue/execução se tocar infraestrutura real.

Veja [ADR-0007](../adr/0007-jobs-hangfire.md). Crie jobs via `/create-job`.
