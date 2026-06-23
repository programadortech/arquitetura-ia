# ADR-0018: Jobs (Hangfire) opcional no scaffold

- **Status:** Aceito
- **Data:** 2026-06-23
- **Refina:** [ADR-0007](0007-jobs-hangfire.md)

## Contexto
Nem toda API precisa de jobs em background. Incluir Hangfire por padrão adiciona dependências, storage e
um servidor que tenta conectar no start — peso desnecessário para serviços sem trabalho assíncrono.

## Decisão
Hangfire passa a ser **opcional** no `/create-project` (`jobs: hangfire | none`, **default `none`**).
Quando `hangfire`, o scaffold registra Hangfire (storage no banco do projeto), dashboard protegido e
o `AddJobs()`/`AddHangfireServer()`. Quando `none`, nenhuma dependência de jobs é adicionada.
[ADR-0007](0007-jobs-hangfire.md) segue válido como padrão de **como** fazer jobs quando habilitados.

## Consequências
- (+) Projetos nascem mais leves; jobs entram só quando necessários.
- (+) Sem servidor Hangfire tentando conectar quando não há jobs.
- (−) Habilitar depois exige rodar a configuração de jobs (documentado na skill `/create-job`).

## Alternativas consideradas
- Hangfire sempre presente: peso e acoplamento desnecessários.
- Outro agendador por padrão: fora de escopo; mantém-se Hangfire quando habilitado (ADR-0007).
