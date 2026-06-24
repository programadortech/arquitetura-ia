# ADR-0023: Estratégia de branches e fluxo de PR

- **Status:** Aceito
- **Data:** 2026-06-24

## Contexto
Precisamos de um fluxo de branches previsível e automatizável, que a IA siga sempre — decidindo o tipo de
trabalho, criando a branch com nomenclatura padrão e abrindo o PR para o alvo correto.

## Decisão
**Branches de longa duração:** `main` (estável/produção), `staging` (homologação), `dev` (integração de desenvolvimento).

**Todo novo trabalho começa a partir da `main`:**
- **Feature** → `feature/{id-atividade}-{nome-atividade}` → **PR para `dev`**.
- **Hotfix** → `hotfix/{id-atividade}-{nome-atividade}` → **PR para `staging`**.

`{id-atividade}` = id do work item (ex.: `12094`); `{nome-atividade}` = slug em kebab-case.

**Responsabilidades da IA:**
- O **agente de dev** (`backend-developer`), **antes de codar**, decide feature vs hotfix, e **cria a branch
  a partir da `main`** com o padrão acima.
- A skill `/review-pr` abre o **PR no alvo correto** por tipo: feature → `dev`; hotfix → `staging`.
- Nunca commitar direto em `main`/`staging`/`dev`; sempre via branch + PR.

Regras em [`docs/standards/branching.md`](../standards/branching.md).

## Consequências
- (+) Fluxo único, automatizável; rastreável pelo id no nome da branch e no PR.
- (+) Hotfix vai para homologação primeiro (staging), feature integra no dev.
- (−) Exige `dev` e `staging` existirem e uma política de promoção (dev → staging → main).

## Alternativas consideradas
- Trunk-based (commit direto na main + flags): ágil, mas o time pediu fluxo com dev/staging e PR por tipo.
- GitFlow clássico (feature a partir de develop): aqui o padrão é **partir sempre da main** (atividade isolada),
  conforme decidido.
