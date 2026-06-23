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
- **Infrastructure:** <repos do banco (provider EF Core), adapters de fila, jobs, políticas Polly, OTel>
- **Api:** <endpoints, composição de DI>

```
<component diagram or bullet flow respecting the dependency rule>
```

## 4. Modelo de dados (banco selecionado)
- **Provider:** <oracle | sqlserver | postgresql | mysql>
- **Tabelas:** <nome, colunas-chave, constraints>
- **Índices:** <nome → colunas → justificativa>
- **Plano de migração:** `db/<provider>/migrations/V<NNNN>__<desc>.sql` (+ down). Criado via `/create-db-script`.

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
| <query no banco> | `database` | timeout + retry + breaker |
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
> Próximo: implemente os casos de uso com `/create-usecase`, scripts com `/create-db-script`, testes com `/create-tests`.
