# Padrão: Clean Architecture

Regras vinculantes para todo projeto gerado. Aplicadas por `scripts/validate-clean-architecture.ps1`
e pelo projeto `*.ArchitectureTests` (NetArchTest).

## Camadas e dependências

```
            ┌─────────────┐
            │     Api     │  composition root, endpoints, DI only
            └──────┬──────┘
       ┌───────────┼────────────┐
       ▼                        ▼
┌─────────────┐         ┌──────────────────┐
│ Application │◀────────│  Infrastructure  │ implements Application ports
│  use cases  │ ports   │ Oracle/queues/   │
│  + ports    │         │ jobs/telemetry   │
└──────┬──────┘         └──────────────────┘
       ▼
┌─────────────┐
│   Domain    │  no dependencies
└─────────────┘
```

- **Domain** — entidades, value objects, eventos de domínio, serviços de domínio, invariantes. Não referencia nada
  (sem EF, sem Serilog, sem SDKs de providers).
- **Application** — casos de uso (handlers `IUseCase<,>`), os contratos do dispatcher, DTOs e **portas**
  (interfaces) para tudo que é externo. Referencia **apenas** Domain. **Nunca** deve referenciar Infrastructure.
- **Infrastructure** — implementa as portas: repositórios Oracle (EF Core / managed driver), queue providers,
  jobs Hangfire, políticas Polly, adapters OpenTelemetry/Serilog. Referencia Application + Domain.
- **Api** — host ASP.NET Core. Referencia todos os projetos **apenas para compor a DI** e expor endpoints.
  Os endpoints são finos: validam, despacham via `IUseCaseDispatcher`, mapeiam a resposta. Sem lógica de negócio.

## Regras (testáveis)
1. Domain tem zero dependências de projeto/framework.
2. Application depende apenas de Domain.
3. Application **não** tem `using *.Infrastructure`.
4. Infrastructure não depende de Api.
5. Todo recurso externo é acessado através de uma porta definida em Application.
6. Casos de uso são invocados somente através de `IUseCaseDispatcher`.
7. Nenhuma referência a `MediatR`.

## Convenções de nomenclatura e estrutura
- Casos de uso: `Application/UseCases/<Area>/<Action>/` contendo `XxxRequest`, `XxxResponse`, `XxxHandler`.
- Portas: `Application/Ports/` (ex.: `IOrderRepository`, `IQueuePublisher`).
- As implementações espelham o nome da porta sem o `I` inicial, em `Infrastructure/<Area>/`.
- Um caso de uso = um handler = um limite de transação (padrão).

## Veja também
[`usecase-dispatcher.md`](usecase-dispatcher.md) · [`oracle.md`](oracle.md) ·
[`observability.md`](observability.md) · [`resilience.md`](resilience.md) ·
[`queue-providers.md`](queue-providers.md) · [`jobs.md`](jobs.md) · [`testing.md`](testing.md)
