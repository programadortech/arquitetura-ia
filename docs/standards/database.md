# Padrão: Banco de Dados Relacional Plugável

O banco relacional é **plugável**: **Oracle, SQL Server, PostgreSQL ou MySQL**, selecionável por
configuração. Mesma filosofia dos [providers de fila](queue-providers.md): o motor é detalhe de
Infrastructure, atrás de **ports** na Application. Trocar de banco não muda Domain/Application nem o fluxo.
Ver [ADR-0013](../adr/0013-pluggable-database-providers.md). Padrão recomendado: **Oracle**.

## Seleção do provider
- No projeto gerado: `Database:Provider` em `appsettings` = `Oracle | SqlServer | PostgreSql | MySql`.
- Em `/create-project`: parâmetro de banco (default `Oracle`).
- Apenas o **pacote EF Core do provider escolhido** é referenciado (central em `Directory.Packages.props`).

| Provider | Pacote EF Core | Driver/Notas |
|---|---|---|
| **Oracle** | `Oracle.EntityFrameworkCore` | `Oracle.ManagedDataAccess.Core` |
| **SQL Server** | `Microsoft.EntityFrameworkCore.SqlServer` | `Microsoft.Data.SqlClient` |
| **PostgreSQL** | `Npgsql.EntityFrameworkCore.PostgreSQL` | `Npgsql` |
| **MySQL** | `Pomelo.EntityFrameworkCore.MySql` | `MySqlConnector` |

## Regras de acesso (todos os providers)
- Acesso somente por **repositórios** que implementam ports da Application. Nenhum tipo de banco
  vaza para Application/Domain.
- **Sempre parametrizado** (bind variables / parâmetros). Nunca concatenar input em SQL.
- Conexões de pool; `await using`; passar `CancellationToken`.
- Resiliência: chamadas ao banco usam a pipeline Polly `database` (timeout + retry transitório + breaker)
  — ver [resilience.md](resilience.md).

## Organização dos scripts (por provider)
```
db/
└── <provider>/                 # oracle | sqlserver | postgresql | mysql
    ├── migrations/
    │   ├── V0001__create_schema.sql   # up
    │   ├── U0001__create_schema.sql   # down (quando viável)
    │   └── ...
    ├── seed/
    │   └── reference_data.sql         # idempotente
    └── README.md
```
- **Versionados e ordenados**: `V<NNNN>__<descricao>.sql` (4 dígitos). `U<NNNN>__...` para rollback.
- Aditivos e compatíveis por padrão (expand/contract para mudanças quebráveis).
- Validados por `scripts/validate-db-scripts.ps1`.

## Convenções de nomenclatura
| Objeto | Oracle | SQL Server | PostgreSQL | MySQL |
|---|---|---|---|---|
| Tabela | `CUSTOMER_ORDER` | `CustomerOrder` ou `customer_order` | `customer_order` | `customer_order` |
| PK | `PK_<TABELA>` | `PK_<Tabela>` | `pk_<tabela>` | `pk_<tabela>` |
| FK | `FK_<FILHA>_<PAI>` | idem | `fk_<filha>_<pai>` | idem |
| Índice | `IX_<TABELA>_<COLS>` | idem | `ix_<tabela>_<cols>` | idem |

> Mantenha **uma convenção por projeto** conforme o provider escolhido. Oracle: UPPER_SNAKE_CASE;
> PostgreSQL/MySQL: snake_case minúsculo (evita problemas de case-folding); SQL Server: PascalCase ou
> snake_case (consistente). Detalhes do Oracle em [`oracle.md`](oracle.md).

## Tipos de dados (equivalências comuns)
| Conceito | Oracle | SQL Server | PostgreSQL | MySQL |
|---|---|---|---|---|
| Texto | `VARCHAR2(n CHAR)` | `NVARCHAR(n)` | `varchar(n)` / `text` | `VARCHAR(n)` |
| Numérico exato | `NUMBER(p,s)` | `DECIMAL(p,s)` | `numeric(p,s)` | `DECIMAL(p,s)` |
| Inteiro/identidade | `NUMBER` + `SEQ`/identity | `INT IDENTITY` | `bigint generated ... as identity` | `BIGINT AUTO_INCREMENT` |
| Instante (UTC) | `TIMESTAMP WITH TIME ZONE` | `DATETIMEOFFSET` | `timestamptz` | `DATETIME(6)` / `TIMESTAMP` |
| Booleano | `NUMBER(1)`/`CHAR(1)` | `BIT` | `boolean` | `TINYINT(1)` |
| Grande texto/binário | `CLOB`/`BLOB` | `NVARCHAR(MAX)`/`VARBINARY(MAX)` | `text`/`bytea` | `LONGTEXT`/`LONGBLOB` |

## Design e segurança
- Toda tabela tem PK; invariantes como constraints (`NOT NULL`, `UNIQUE`, `CHECK`, `FK`).
- Colunas de auditoria quando fizer sentido: `CREATED_AT/BY`, `UPDATED_AT/BY`.
- Indexar FKs e predicados quentes; justificar cada índice.
- Sem `DROP`/`TRUNCATE`/drop de coluna sem plano explícito, guardado e reversível (imposto por hooks).

## Testes
- Integração com o provider real via **Testcontainers** (imagens para Oracle, SQL Server, PostgreSQL e MySQL)
  ou schema de teste descartável. Ver [testing.md](testing.md).

Crie scripts com `/create-db-script`. Notas específicas do Oracle: [`oracle.md`](oracle.md).
