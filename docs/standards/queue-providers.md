# Padrão: Queue Providers Plugáveis

A mensageria é abstraída atrás de **portas em Application**; os providers concretos vivem em Infrastructure e são
selecionados por configuração. Providers suportados: **Kafka, SQS, RabbitMQ, MQTT**. Trocar de provider é uma
mudança de configuração, não de código.

## Portas (Application)
```csharp
public interface IQueuePublisher
{
    Task PublishAsync<TMessage>(TMessage message, PublishOptions? options = null,
        CancellationToken cancellationToken = default);
}

public interface IQueueConsumer<TMessage>
{
    Task StartAsync(Func<MessageEnvelope<TMessage>, CancellationToken, Task> handler,
        CancellationToken cancellationToken);
}

public sealed record MessageEnvelope<TMessage>(
    string MessageId, string CorrelationId, IReadOnlyDictionary<string,string> Headers,
    TMessage Payload, int DeliveryAttempt);
```

## Regras de abstração do provider
- Tipos do SDK do provider aparecem **apenas** dentro de `Infrastructure/Messaging/<Provider>/` — nunca acima.
- Seleção via configuração `Messaging:Provider` (`Kafka` | `Sqs` | `RabbitMq` | `Mqtt`); apenas o provider escolhido
  é registrado no composition root da Api.
- **Serialização** é agnóstica de provider (JSON por padrão); o envelope carrega headers + contexto de correlação/trace.
- **Propagação de trace context**: injete no publish, extraia no consume (W3C traceparent nos headers).

## Regras de confiabilidade (todos os providers)
- Entrega **at-least-once** → **consumidores idempotentes** (dedup por `MessageId`/chave de negócio).
- **Tratamento de mensagens venenosas**: retries limitados (Polly `queue-consume`) e então dead-letter/parking. Sem reentrega infinita.
- Faça acknowledge/commit **somente após** o processamento bem-sucedido.
- Valide todo payload consumido; nunca confie em claims de autorização fornecidos pelo produtor.
- Emita spans + logs estruturados + métricas (fila, tópico, id da mensagem, tentativa, resultado).

## Notas específicas por provider
| Provider | Ordenação | Modelo de ack | DLQ | Notas |
|---|---|---|---|---|
| **Kafka** | por partição (key) | commit manual após sucesso | dead-letter topic | consumer groups, partition keys |
| **SQS** | apenas filas FIFO | delete após sucesso; visibility timeout | redrive policy → DLQ | batch receive/delete |
| **RabbitMQ** | por fila | `basic.ack`/`nack`, prefetch | dead-letter exchange | topologia de exchange/fila |
| **MQTT** | nenhuma | QoS 1/2 | parking topic | retained msgs, clean session, hierarquia de tópicos |

Veja [ADR-0008](../adr/0008-pluggable-queue-providers.md). Adicione um provider via `/create-queue-provider`.
