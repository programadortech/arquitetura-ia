# ADR-0008: Provedores de fila plugáveis

- **Status:** Aceito
- **Data:** 2026-06-22

## Contexto
Ambientes/times diferentes usam sistemas de mensageria diferentes (Kafka, SQS, RabbitMQ, MQTT). O código de negócio
não deve se acoplar a nenhum broker específico.

## Decisão
Definir **ports de mensageria na Application** (`IQueuePublisher`, `IQueueConsumer<T>`, `MessageEnvelope<T>`)
e implementar os provedores na Infrastructure, selecionados por configuração (`Messaging:Provider`). Os tipos
do SDK do provedor ficam confinados a `Infrastructure/Messaging/<Provider>/`. Entrega at-least-once com consumidores
idempotentes e dead-lettering de mensagens envenenadas. Regras em
[`docs/standards/queue-providers.md`](../standards/queue-providers.md).

## Consequências
- (+) Troca de brokers por configuração; código de negócio inalterado; testável via abstração.
- (+) Semântica de confiabilidade consistente (idempotência, DLQ, propagação de trace) entre provedores.
- (−) Abstração de menor denominador comum; recursos específicos de provedor acessados via options/pontos de extensão.

## Alternativas consideradas
- Acoplar diretamente ao SDK de um broker: lock-in, código de negócio não testável.
- Um framework pesado de service bus: mais abstração do que o necessário; mantemos um port enxuto e próprio.
