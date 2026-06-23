# ADR-0020: Acesso a dados plugável — EF Core ou Dapper, com Unit of Work

- **Status:** Aceito
- **Data:** 2026-06-23
- **Refina:** [ADR-0013](0013-pluggable-database-providers.md)

## Contexto
Times têm preferências diferentes de acesso a dados: **EF Core** (produtividade, change tracking,
migrations) ou **Dapper** (SQL explícito, performance, controle fino). A escolha é **ortogonal** ao
provider do banco (Oracle/SQL Server/PostgreSQL/MySQL). Em ambos precisamos de **transações coesas** por
caso de uso (Unit of Work).

## Decisão
O **acesso a dados é selecionável** no `/create-project` (`dataaccess: efcore | dapper`, default `efcore`),
e **ambos** expõem o mesmo contrato de persistência na Application:
- **Ports** (Application/Ports/Persistence): `IUnitOfWork` (Begin/Commit/Rollback/SaveChanges) e os
  repositórios por agregado (`IXxxRepository`). O Domain/Application **não conhecem** EF nem Dapper.
- **Adapters** (Infrastructure):
  - **EF Core:** `AppDbContext` + `EfUnitOfWork` (SaveChanges persiste o tracking; transações via `DbContext.Database`).
  - **Dapper:** `DapperUnitOfWork` mantém `IDbConnection` + `IDbTransaction`; repositórios usam essa conexão/transação;
    `SaveChangesAsync` faz **Commit**.
- Um **`TransactionBehavior`** (pipeline do dispatcher) pode envolver cada caso de uso numa transação UoW
  (begin → handler → commit / rollback no erro), uniforme para EF e Dapper.

Regras em [`docs/standards/database.md`](../standards/database.md).

## Consequências
- (+) Mesmo código de Application/Domain independente de EF ou Dapper; troca sem afetar casos de uso.
- (+) Unit of Work consistente nos dois; transação por caso de uso.
- (−) Dapper não tem change tracking nem migrations — SQL e mapeamento manuais; migrações continuam em `db/<provider>`.
- (−) Manter dois caminhos de adapter (mitigado: a maioria dos projetos escolhe um).

## Alternativas consideradas
- Só EF Core: produtivo, mas times de alta performance/SQL preferem Dapper.
- Só Dapper: controle/perf, porém perde produtividade e migrations do EF.
- Misturar EF e Dapper no mesmo projeto: permitido pontualmente (ex.: Dapper para queries de leitura
  pesadas), mas o **padrão de escrita/UoW** segue o escolhido.
