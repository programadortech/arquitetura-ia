---
name: create-queue-provider
description: Adiciona ou conecta um provedor de mensageria plugável (Kafka, SQS, RabbitMQ, MQTT) por trás da abstração comum de fila, selecionável via configuração. Use para adicionar um novo provedor ou configurar a mensageria de um projeto.
---

# Skill: create-queue-provider

Implementa um provedor para a abstração comum de mensageria sem vazar detalhes do provedor para cima.

## Inputs
- **Provider**: Kafka | SQS | RabbitMQ | MQTT.
- Se você precisa de um **publisher**, **consumer**, ou ambos, e os contratos de mensagem/topics.

## Steps
1. Leia `docs/standards/queue-providers.md`. A abstração vive em Application como portas:
   `IQueuePublisher`, `IQueueConsumer<TMessage>`, contrato de envelope de mensagem + serialização.
2. Implemente o adaptador do provedor em Infrastructure (`Infrastructure/Messaging/<Provider>/`):
   - Mapeie o envelope para a API do provedor; preserve o contexto de correlação/trace através da fronteira.
   - Implemente semântica at-least-once com **consumers idempotentes** e **tratamento de poison-message** (DLQ/parking).
   - Envolva publish/consume em políticas Polly; emita spans OTel + logs estruturados (queue, topic, message id, tentativa).
3. Seleção do provedor via configuração (`Messaging:Provider`), conectada no composition root da Api.
   Apenas o provedor selecionado é registrado; a troca é só por configuração.
4. Valide as mensagens de input no consume; nunca confie nas alegações do producer (preocupação do security-reviewer).
5. Tests: testes de integração para o round-trip de publish/consume (Testcontainers quando disponível); testes unitários para o mapeamento.

## Provider notes
- **Kafka**: consumer groups, partition keys para ordenação, commit manual após sucesso.
- **SQS**: visibility timeout, DLQ redrive, tratamento em batch, FIFO vs standard.
- **RabbitMQ**: topologia de exchange/queue, acks, prefetch, dead-letter exchange.
- **MQTT**: nível de QoS, retained messages, clean session, hierarquia de topics.

## Suggested agents
`backend-developer` → `observability-engineer` → `security-reviewer` → `tech-lead-reviewer`.

## Done when
O provedor é selecionável por configuração, idempotente, resiliente, observável e testado — sem nenhum tipo
do provedor referenciado fora de Infrastructure.
