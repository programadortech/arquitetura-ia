# Arquitetura: <Feature Name>

- **Documento da feature:** [`../features/<feature>.md`](../features/<feature>.md)
- **Status:** Proposta | Aprovada
- **Autor:** <nome> · **Data:** <YYYY-MM-DD>
- **ADRs:** <liste ADRs novos/afetados>

## 1. Resumo
<Um parágrafo: a abordagem técnica.>

## 2. Casos de uso
| Caso de uso | Tipo (Command/Query) | Request → Response | Efeitos colaterais | Critérios de aceite |
|---|---|---|---|---|
| <Name> | Command | `XxxRequest` → `XxxResponse` | escrita em DB, publicação de evento | AC #1, #2 |

## 3. Mapeamento de camadas
- **Domain:** <entidades, value objects, eventos de domínio, invariantes>
- **Application:** <casos de uso, portas (interfaces), behaviors>
- **Infrastructure:** <repos Oracle, adapters de fila, jobs, políticas Polly, OTel>
- **Api:** <endpoints, composição de DI>

```
<component diagram or bullet flow respecting the dependency rule>
```

## 4. Modelo de dados Oracle
- **Tabelas:** <nome, colunas-chave, constraints>
- **Índices:** <nome → colunas → justificativa>
- **Plano de migração:** V<NNNN>__<desc> (+ down). Criado via `/create-oracle-script`.

## 5. Mensageria
- **Provedor:** <plugável; padrão para este projeto>
- **Topics/filas:** <nome, direção, contrato da mensagem>
- **Estratégia de idempotência / DLQ:** <…>
- **Propagação de trace:** <como o contexto flui>

## 6. Jobs (Hangfire)
- <id do job, gatilho (cron/enqueue), caso de uso que executa, guarda de idempotência>

## 7. Resiliência (Polly)
| Chamada | Pipeline | Notas |
|---|---|---|
| <Oracle query> | `oracle` | timeout + retry + breaker |
| <external HTTP> | `http-outbound` | … |

## 8. Contrato de observabilidade
- **Spans:** <nome → atributos>
- **Métricas:** <nome → tipo → labels>
- **Eventos de log:** <template da mensagem → propriedades → nível>

## 9. Segurança
- **AuthN/AuthZ:** <…> · **Validação de entrada:** <…> · **Exposição de dados:** <masking/PII>

## 10. Riscos e trade-offs
- <…>

---
> Próximo: implemente os casos de uso com `/create-usecase`, scripts com `/create-oracle-script`, testes com `/create-tests`.
