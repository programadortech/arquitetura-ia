# Banco de Dados (plugável)

Documentação do modelo de dados e log de migração. O banco é **plugável** — Oracle, SQL Server,
PostgreSQL ou MySQL (ver [`../standards/database.md`](../standards/database.md) e
[ADR-0013](../adr/0013-pluggable-database-providers.md)). Os scripts ficam em `db/<provider>/` de cada
projeto. Notas do Oracle: [`../standards/oracle.md`](../standards/oracle.md). Crie scripts com `/create-db-script`.

## Provider do projeto
- Definido em `Database:Provider` (`Oracle | SqlServer | PostgreSql | MySql`) e no `/create-project`.

## Log de migração
| Versão | Provider | Descrição | Up | Down | Data |
|---|---|---|---|---|---|
| _nenhum ainda_ | — | — | — | — | — |

## Notas do modelo de dados
Documente aqui tabelas, relacionamentos, índices principais e justificativas (ou referencie um diagrama ER).
Mantenha sincronizado com os scripts de migração em `db/<provider>/migrations`.
