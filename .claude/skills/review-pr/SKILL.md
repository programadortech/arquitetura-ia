---
name: review-pr
description: Executa a revisão automatizada e multiperspectiva de PR — arquitetura, qualidade de código, Oracle, testes, observabilidade e segurança — contra os padrões do repositório e produz um veredito. Use quando o usuário disser "revise o PR".
---

# Skill: review-pr

Realiza uma revisão minuciosa e orientada por padrões das mudanças pendentes e emite um veredito estruturado.

## Inputs
- O diff/branch/PR a revisar (padrão: mudanças do working atual / branch atual).

## Steps
1. Reúna o diff (`git diff`), os arquivos alterados e o doc de arquitetura relevante + ADRs.
2. Execute os gates automatizados:
   - `scripts/validate-clean-architecture.ps1`
   - `scripts/validate-architecture.ps1`
   - `scripts/validate-oracle-scripts.ps1`
   - `scripts/validate-tests.ps1`
   - `scripts/validate-pr.ps1`
3. Execute revisões por perspectiva (delegue aos agentes):
   - `tech-lead-reviewer` — correção, conformidade arquitetural, manutenibilidade.
   - `oracle-dba-reviewer` — quaisquer mudanças em `*.sql` / `db/oracle/**`.
   - `observability-engineer` — qualidade de logging/tracing/métricas.
   - `security-reviewer` — segredos, injection, authz, exposição de dados.
4. Cruze com `docs/standards/quality-checklist.md` e `templates/pr-template.md`.
5. Consolide os achados como **Blocking / Should-fix / Nit**, cada um com file:line e uma correção concreta.

## Output
Um único relatório de revisão terminando com **APPROVE** ou **REQUEST CHANGES** e os achados priorizados.
Se `--fix` for solicitado, aplique as correções seguras; caso contrário, apenas reporte.

## Done when
Todos os gates foram executados, todas as perspectivas estão cobertas, e um veredito claro com achados acionáveis é produzido.
