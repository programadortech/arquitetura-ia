# Padrão: Banco de Dados Oracle

## Drivers e acesso
- `Oracle.ManagedDataAccess.Core` para o managed driver; `Oracle.EntityFrameworkCore` ao usar EF Core.
- Todo acesso através de **repositórios** que implementam portas de Application. Nenhum tipo de DB vaza para Application/Domain.
- **Sempre parametrizado** (bind variables). Nunca concatene entrada do usuário em SQL.
- Conexões a partir de um pool; descarte de forma determinística (`await using`). Passe `CancellationToken`.

## Organização dos scripts
```
db/oracle/
├── migrations/
│   ├── V0001__create_schema.sql        # up
│   ├── U0001__create_schema.sql        # down (rollback) when feasible
│   ├── V0002__add_order_table.sql
│   └── U0002__add_order_table.sql
├── seed/
│   └── reference_data.sql              # idempotent MERGE statements
└── README.md
```
- **Versionado e ordenado**: `V<NNNN>__<snake_description>.sql` (4 dígitos, sequencial).
- **Reversível**: script de down `U<NNNN>__...` correspondente, ou um comentário de cabeçalho explicando por que o rollback é impossível.
- As migrações são aditivas e retrocompatíveis por padrão (expand/contract para mudanças que quebram compatibilidade).

## Convenções de nomenclatura
| Objeto | Convenção | Exemplo |
|---|---|---|
| Table | UPPER_SNAKE_CASE | `CUSTOMER_ORDER` |
| Column | UPPER_SNAKE_CASE | `ORDER_TOTAL` |
| Primary key | `PK_<TABLE>` | `PK_CUSTOMER_ORDER` |
| Foreign key | `FK_<CHILD>_<PARENT>` | `FK_ORDER_ITEM_ORDER` |
| Index | `IX_<TABLE>_<COLS>` | `IX_ORDER_CUSTOMER_ID` |
| Unique | `UQ_<TABLE>_<COLS>` | `UQ_CUSTOMER_EMAIL` |
| Sequence | `SEQ_<TABLE>` | `SEQ_CUSTOMER_ORDER` |
| Check | `CK_<TABLE>_<RULE>` | `CK_ORDER_TOTAL_POS` |

## Regras de modelagem de dados
- Toda tabela tem uma chave primária.
- Tipos de dados: `NUMBER(p,s)` para numéricos, `VARCHAR2(n CHAR)` para texto (semântica CHAR para i18n),
  `TIMESTAMP WITH TIME ZONE` para instantes, `CLOB`/`BLOB` apenas quando justificado.
- Colunas de auditoria quando relevante: `CREATED_AT`, `CREATED_BY`, `UPDATED_AT`, `UPDATED_BY`.
- Expresse invariantes como constraints (`NOT NULL`, `UNIQUE`, `CHECK`, `FK`).
- Indexe toda foreign key e predicado quente; justifique cada índice (custo de escrita vs. benefício de leitura).

## Segurança
- Sem `DROP` / `TRUNCATE` / drop de coluna sem um plano explícito, protegido e reversível (aplicado por hooks).
- Operações lock-light/online em tabelas grandes; evite DDL bloqueante e demorado em horário comercial.

## Validação
`scripts/validate-oracle-scripts.ps1` verifica nomenclatura, ordenação de versão, presença de scripts de down e
guardas de DDL destrutivo. Veja [ADR-0004](../adr/0004-oracle-database.md).
