# Padrão: Revisão automatizada de PR (GitHub Action + Claude)

> Vinculante. Codifica [ADR-0025](../adr/0025-automated-pr-review-github-action.md). Complementa o fluxo de
> branches/PR de [`branching.md`](branching.md) e a skill `.claude/skills/review-pr/SKILL.md`.

## O que é
Toda Pull Request para `dev` ou `staging` é revisada automaticamente pelo Claude via GitHub Action
(`.github/workflows/claude-pr-review.yml`). A Action lê o diff, confronta com os padrões (`docs/standards/`)
e ADRs (`docs/adr/`), e **posta o veredito** no PR usando `gh pr review`:

- **APPROVE** — nenhum achado *Blocking*.
- **REQUEST CHANGES** — há achado *Blocking* (com `arquivo:linha` e correção).
- **COMMENT** — apenas observações menores.

A Action roda como `github-actions[bot]` (identidade separada do autor), então **aprova/reprova de fato** —
o GitHub não permite que o **autor** aprove o próprio PR (só comentar).

## Quem revisa o quê
| Camada | Onde | Aprova/Reprova? |
|---|---|---|
| Automática (CI) | GitHub Action a cada PR p/ `dev`/`staging` | Sim (bot) |
| Local on-demand | `/review-pr` (agentes `tech-lead-reviewer`, `security-reviewer`, `oracle-dba-reviewer`, `observability-engineer`) | Só comenta se rodar como autor |

## Setup (uma vez por repositório)
1. **Secret** `ANTHROPIC_API_KEY` em *Settings → Secrets and variables → Actions → New repository secret*.
2. (Opcional) Em *Settings → Branches*, exigir o status check da Action e/ou 1 review aprovado em `dev`/`staging`
   como **branch protection** — assim um *REQUEST CHANGES* bloqueia o merge.
3. Nada mais no código: o workflow usa o `GITHUB_TOKEN` padrão com permissão `pull-requests: write`.

## O que a Action cobre e o que não cobre
- **Cobre:** aderência à regra de dependência, Result/Notification + envelope, dispatcher próprio (sem MediatR),
  mappers estáticos (sem AutoMapper), segredos fora do código, logs estruturados, config por ambiente,
  cobertura de testes, reversibilidade de scripts de banco, alvo de PR correto.
- **Não cobre (ainda):** os gates PowerShell (`build`, `dotnet test`, `validate-*.ps1`) — rode-os localmente
  antes do PR (o hook `pre-pr-check` ajuda) ou adicione um workflow de CI dedicado.

## Relação com o fluxo de branches
- `feature/{id}-{slug}` → PR para **`dev`**.
- `hotfix/{id}-{slug}` → PR para **`staging`**.
- Nunca abrir PR direto para `main`. Ver [`branching.md`](branching.md) / [ADR-0023](../adr/0023-git-branching-strategy.md).
