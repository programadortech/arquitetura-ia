# ADR-0003: Use Case Dispatcher customizado (sem MediatR pago)

- **Status:** Aceito
- **Data:** 2026-06-22

## Contexto
Queremos o padrão mediator/dispatcher (request → handler desacoplado, behaviors de pipeline transversais),
mas as versões major atuais do MediatR têm **licença comercial**. Exigimos custo zero de licenciamento e
controle total do pipeline.

## Decisão
Implementar uma **abstração de dispatcher no próprio repositório**: `IUseCase<TRequest,TResponse>`,
`IUseCaseRequest<TResponse>`, `IUseCaseDispatcher` e `IUseCaseBehavior<,>` para preocupações transversais
(logging, validação, tracing, transações). Registrado via varredura de assembly `AddApplication()`. Sem
dependência de `MediatR`. Contrato completo em
[`docs/standards/usecase-dispatcher.md`](../standards/usecase-dispatcher.md).

## Consequências
- (+) Sem custo de licença ou acoplamento externo; pipeline totalmente sob nosso controle.
- (+) Os testes de arquitetura podem banir `MediatR` por completo.
- (−) Nós mantemos a pequena infraestrutura de dispatcher + behaviors (bem compreendida, baixo risco).

## Alternativas consideradas
- MediatR (pago): rejeitado por licenciamento.
- Injeção direta de handlers (sem dispatcher): perde o pipeline transversal uniforme.
- Outros mediators OSS: dependência extra para código que podemos manter trivialmente.
