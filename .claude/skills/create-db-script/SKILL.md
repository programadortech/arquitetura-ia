---
name: create-db-script
description: Cria um script de migração (e rollback) versionado, seguro e reversível para o banco do projeto — Oracle, SQL Server, PostgreSQL ou MySQL — seguindo os padrões de nomenclatura e segurança. Use para qualquer mudança de schema/dados (inclui pedidos como "crie um script Oracle...").
---

# Skill: create-db-script

Cria scripts SQL ordenados em `db/<provider>/` para o banco selecionado no projeto.

## Entradas
- Propósito da mudança (nova tabela, coluna, índice, seed, etc.) e a feature afetada.
- **Provider** do projeto: `oracle | sqlserver | postgresql | mysql` (de `Database:Provider` /
  parâmetro do `/create-project`). Se não estiver claro, perguntar.

## Passos
1. Adotar a mentalidade do `oracle-dba-reviewer` (revisor de banco); ler
   [`docs/standards/database.md`](../../docs/standards/database.md) e, para Oracle,
   [`docs/standards/oracle.md`](../../docs/standards/oracle.md).
2. Determinar o próximo número `NNNN` (sequencial, zero-padded) em `db/<provider>/migrations/`.
3. Escrever o script **up**: `db/<provider>/migrations/V<NNNN>__<descricao>.sql`, no **dialeto do provider**:
   - Nomenclatura e tipos conforme a tabela do provider em `database.md`.
   - PK em toda tabela; FK/índices nomeados; colunas de auditoria quando fizer sentido.
   - Aditivo e compatível sempre que possível.
4. Escrever o script **down** quando viável: `db/<provider>/migrations/U<NNNN>__<descricao>.sql`.
   Se rollback limpo for impossível, explicar no cabeçalho (`-- no-rollback: <motivo>`).
5. Seed/dados de referência em `db/<provider>/seed/` com upsert idempotente (`MERGE` / `INSERT ... ON CONFLICT` /
   `INSERT ... ON DUPLICATE KEY`, conforme o provider).
6. Rodar `scripts/validate-db-scripts.ps1`.

## Regras de segurança
- Sem `DROP`/`TRUNCATE`/drop de coluna sem plano explícito, guardado (`-- destructive-ok: <motivo>`) e reversível.
- Operações leves de lock em tabelas grandes; justificar cada índice.
- SQL específico do dialeto — não misturar sintaxe de outro provider.

## Agentes sugeridos
`oracle-dba-reviewer` (revisor de banco) → `security-reviewer` → `tech-lead-reviewer`.

## Concluído quando
Os scripts seguem nomenclatura + segurança do provider e `validate-db-scripts.ps1` passa.
