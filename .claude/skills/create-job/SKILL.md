---
name: create-job
description: Cria um job em background com Hangfire (recorrente ou fire-and-forget) que despacha um caso de uso, com idempotência, resiliência e observabilidade. Use para trabalho em background agendado/assíncrono.
---

# Skill: create-job

Adiciona um job Hangfire que delega à camada de application via o dispatcher.

## Inputs
- **JobName**, tipo de gatilho (cron recorrente | enqueue/fire-and-forget | delayed), e o caso de uso que ele executa.

## Steps
1. Leia `docs/standards/jobs.md` e `docs/standards/observability.md`.
2. Implemente uma classe de job enxuta em Infrastructure cujo método:
   - Resolve `IUseCaseDispatcher` e despacha o caso de uso relevante (sem lógica de negócio no job).
   - Seja **idempotente** (seguro para rodar duas vezes) — proteja com chaves/dedup conforme necessário.
   - Envolva o trabalho em uma política Polly e um span OTel; registre início/sucesso/falha com propriedades estruturadas.
   - Respeite cancelamento/shutdown.
3. Registre-o:
   - Recorrente: `RecurringJob.AddOrUpdate(...)` com um job id estável e cron, registrado de forma idempotente no startup.
   - Fire-and-forget/delayed: enfileire a partir do caso de uso/adaptador relevante via `IBackgroundJobClient`.
4. Garanta que o dashboard do Hangfire esteja protegido e que o storage (Oracle) esteja configurado.
5. Adicione testes: teste unitário da orquestração do job (dispatcher mockado); teste de integração se ele tocar infra.

## Suggested agents
`backend-developer` → `observability-engineer` → `devops-engineer` (agendamento/infra) → `tech-lead-reviewer`.

## Done when
O job está registrado de forma idempotente, observável, resiliente, testado e não contém lógica de negócio.
