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
   - `scripts/validate-db-scripts.ps1`
   - `scripts/validate-tests.ps1`
   - `scripts/validate-pr.ps1`
3. Execute revisões por perspectiva (delegue aos agentes):
   - `tech-lead-reviewer` — correção, conformidade arquitetural, manutenibilidade.
   - `oracle-dba-reviewer` — quaisquer mudanças em `*.sql` / `db/<provider>/**`.
   - `observability-engineer` — qualidade de logging/tracing/métricas.
   - `security-reviewer` — segredos, injection, authz, exposição de dados.
4. Cruze com `docs/standards/quality-checklist.md` e `templates/pr-template.md`.
5. Consolide os achados como **Blocking / Should-fix / Nit**, cada um com file:line e uma correção concreta.

## Output
Um único relatório de revisão terminando com **APPROVE** ou **REQUEST CHANGES** e os achados priorizados.
Se `--fix` for solicitado, aplique as correções seguras; caso contrário, apenas reporte.

## Abertura do PR + auto-merge (alvo por tipo — ver docs/standards/branching.md)
Ao abrir o PR, escolha o **alvo** pelo tipo da branch:
- `feature/{id}-{slug}` → **PR para `dev`**.
- `hotfix/{id}-{slug}` → **PR para `staging`**.

O PR referencia o item do tracker (ex.: `#12094`). Nunca abrir PR direto para `main`.

**Auto-merge (merge automático ao ficar tudo verde):** depois de abrir o PR, ligue o auto-merge:
```bash
gh pr create --base dev --head feature/{id}-{slug} --title "…" --body "…"
gh pr merge <n> --auto --merge --delete-branch
```
O GitHub então **mergeia sozinho assim que os checks obrigatórios passam** (o workflow `ci.yml`:
build `-warnaserror` + testes unit/arquitetura/integração + validação da regra de dependência) — sem
clique manual. O ruleset `protect-dev-staging` exige esses checks verdes, então nada entra quebrado.
A revisão de IA (`/review-pr`) é a checagem de qualidade **local e sob demanda** antes de ligar o auto-merge
(custo zero); ela não bloqueia o gate automático.

## Done when
Todos os gates foram executados, todas as perspectivas estão cobertas, um veredito claro com achados
acionáveis é produzido, e (se solicitado abrir o PR) ele aponta para o alvo correto por tipo **com auto-merge
ligado** — o merge acontece automaticamente quando o CI fica verde.
