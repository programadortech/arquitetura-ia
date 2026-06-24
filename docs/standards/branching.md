# Padrão: Branches e Fluxo de PR

Fluxo único que a IA **sempre** segue. Ver [ADR-0023](../adr/0023-git-branching-strategy.md).

## Branches de longa duração
- **`main`** — estável / produção.
- **`staging`** — homologação.
- **`dev`** — integração de desenvolvimento.

## Regra de ouro
**Todo novo trabalho começa a partir da `main`.** Nunca commitar direto em `main`/`staging`/`dev`.

| Tipo | Branch (a partir de `main`) | PR para |
|---|---|---|
| **Feature** | `feature/{id-atividade}-{nome-atividade}` | **`dev`** |
| **Hotfix** | `hotfix/{id-atividade}-{nome-atividade}` | **`staging`** |

- `{id-atividade}` = id do work item (ex.: `12094`).
- `{nome-atividade}` = slug kebab-case do título (ex.: `autenticacao-e-gerenciamento-de-senha`).

Exemplos:
```
feature/12094-autenticacao-e-gerenciamento-de-senha   -> PR para dev
hotfix/13510-corrige-calculo-de-juros                  -> PR para staging
```

## Quem faz o quê (IA)
1. **Início da implementação** — o agente **`backend-developer`** decide **feature** ou **hotfix**, então:
   ```bash
   git checkout main && git pull
   git checkout -b feature/{id}-{slug}     # ou hotfix/{id}-{slug}
   ```
2. **Durante** — commits pequenos e descritivos na branch; referenciar o id no commit/PR.
3. **Fim** — `/review-pr` roda os gates e **abre o PR no alvo por tipo**: feature → `dev`; hotfix → `staging`.
   O PR referencia o item do tracker (ex.: `#12094`).
4. **Gate + merge manual** — ao abrir o PR, o **CI gratuito** (`ci.yml`: build/test/arquitetura) roda como
   check obrigatório. Quando fica **verde**, o agente **avisa o usuário**, que faz o **merge manualmente** na
   plataforma (o merge é decisão dele; o agente não mergeia). A revisão de IA é sob demanda via `/review-pr`
   (local, sem custo). Ver [`pr-review-automation.md`](pr-review-automation.md) / [ADR-0025](../adr/0025-automated-pr-review-github-action.md).

## Como decidir feature × hotfix
- **Hotfix:** correção urgente de algo já em produção/homologação (vai para `staging` para validar rápido).
- **Feature:** todo o resto (novo comportamento, melhoria, refactor) — integra no `dev`.
- Na dúvida, **pergunte ao usuário**.

## Promoção (resumo)
`feature/* → dev` · `hotfix/* → staging` · e a promoção entre `dev → staging → main` segue o processo de
release do time (fora do escopo da IA, salvo instrução).
