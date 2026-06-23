# ADR-0004: Oracle como banco de dados relacional

- **Status:** Aceito
- **Data:** 2026-06-22

## Contexto
O banco de dados padrão da empresa é o Oracle. Precisamos de padrões de acesso consistentes, evolução segura
de schema e disciplina de performance em todos os projetos gerados.

## Decisão
Usar **Oracle** via `Oracle.ManagedDataAccess.Core` (e `Oracle.EntityFrameworkCore` onde EF Core for
usado). Todo acesso através de repositórios que implementam ports da Application; **sempre parametrizado**.
Mudanças de schema são scripts **versionados, ordenados e reversíveis** em `db/oracle/` seguindo
[`docs/standards/oracle.md`](../standards/oracle.md), validados por `scripts/validate-oracle-scripts.ps1`.

## Consequências
- (+) Nomenclatura consistente, migrações seguras, parametrização garantida.
- (+) Tipos do banco isolados na Infrastructure.
- (−) Tuning específico do Oracle necessário para performance; testes de integração precisam de um Oracle (Testcontainers).

## Alternativas consideradas
- PostgreSQL/SQL Server: não são o padrão corporativo aqui.
- Migrações apenas via ORM: preferimos SQL explícito e revisável com scripts de rollback para segurança em produção.
