---
name: create-oracle-script
description: Escreve um script de migração Oracle (e rollback) versionado, seguro e reversível, seguindo os padrões de nomenclatura e segurança do repositório. Use para qualquer solicitação de mudança de schema/dados no banco.
---

# Skill: create-oracle-script

Cria scripts SQL Oracle ordenados sob `db/oracle/`.

## Inputs
- Propósito da mudança (nova tabela, coluna, índice, dados de seed, etc.) e a feature afetada.

## Steps
1. Adote a mentalidade do `oracle-dba-reviewer`; leia `docs/standards/oracle.md`.
2. Determine o próximo número de versão `NNNN` (preenchido com zeros, sequencial) em `db/oracle/migrations/`.
3. Escreva o script **up**: `db/oracle/migrations/V<NNNN>__<description>.sql`.
   - Nomes em UPPER_SNAKE_CASE; PK em toda tabela; FK/índices nomeados conforme o padrão.
   - Colunas de auditoria onde relevante; tipos de dados e constraints apropriados.
   - Aditivo e retrocompatível sempre que possível.
4. Escreva o script **down** quando viável: `db/oracle/migrations/U<NNNN>__<description>.sql`.
   Se um rollback limpo for impossível, explique o porquê em um comentário de cabeçalho.
5. Para dados de referência/seed use `db/oracle/seed/` com instruções `MERGE` idempotentes.
6. Execute `scripts/validate-oracle-scripts.ps1`.

## Safety rules
- Sem `DROP`/`TRUNCATE`/remoção de colunas sem um plano explícito, protegido e reversível.
- Prefira operações online e com pouco travamento em tabelas grandes; justifique cada índice.

## Suggested agents
`oracle-dba-reviewer` → `security-reviewer` (para exposição de dados) → `tech-lead-reviewer`.

## Done when
Os scripts seguem os padrões de nomenclatura + segurança e `validate-oracle-scripts.ps1` passa.
