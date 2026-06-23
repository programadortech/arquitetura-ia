---
name: import-story
description: Importa uma história de um tracker plugável (GitHub Issues, Azure DevOps ou GitLab) para docs/features/<KEY>-<slug>.md no formato do template, mapeando critérios de aceite, e atualiza o backlog. Use quando o usuário disser "importe a história 123" / "importe a issue #123" / "importe o work item 1234".
---

# Skill: import-story

Traz histórias de qualquer tracker suportado para o repositório como documentos de feature
(1 história = 1 arquivo), prontos para `/approve-architecture` → `/create-usecase`.
O tracker é **plugável** (ver [`docs/standards/issue-trackers.md`](../../docs/standards/issue-trackers.md)).

## Entradas
- **Id** do item no tracker (número da issue / work item id / iid).
- (opcional) **provider** explícito: `github | azure | gitlab` (senão usa `.claude/tracker.config.json`).

## Pré-requisitos
- `.claude/tracker.config.json` com o `provider` e seus parâmetros não sensíveis preenchidos.
- A credencial do provider definida em variável de ambiente (nunca no código):
  `GITHUB_TOKEN` (ou `gh` autenticado) · `AZDO_PAT` · `GITLAB_TOKEN`.

## Passos
1. Buscar o item normalizado:
   `pwsh scripts/import-tracker-issue.ps1 -Id <id> [-Provider <p>]`
   → devolve JSON único no contrato comum: `provider, key, number, title, body,
   acceptanceCriteriaRaw, labels, milestone, assignees, url, state`.
2. Derivar o caminho do arquivo: `docs/features/<key>-<slug>.md` onde `key` é `GH-/AZ-/GL-<number>`
   e `slug` vem do título.
3. Preencher `templates/feature-template.md`:
   | Campo normalizado | Seção no documento |
   |---|---|
   | `title` | `# Feature: <title>` |
   | `body` (intro) | Descrição do problema / Contexto de negócio |
   | `acceptanceCriteriaRaw` **ou** seção parseada do body | **Critérios de aceite (Given/When/Then)** |
   | `labels` | metadados / tipo |
   | `milestone` | metadados (sprint/iteration) |
   | `assignees` | Responsável |
   | `url` | campo "Relacionados" (rastreabilidade) |
   | `key` | id canônico (usar em commits/PR) |
4. **Critérios de aceite**:
   - Azure: usar `acceptanceCriteriaRaw` (vem em HTML → converter para Given/When/Then em texto).
   - GitHub/GitLab: procurar `## Critérios de aceite` / `## Acceptance Criteria` ou task list no `body`.
   - Ausente: acionar `product-planner` para redigir a partir da descrição e marcar **"a confirmar"**
     (não inventar requisito firme).
5. **Detectar o tipo da história**: se algum `label`/tag estiver em `storyKinds.technicalLabels`
   (`.claude/tracker.config.json`), tratar como **técnica** → usar
   `templates/historia-tecnica-template.md` e encaminhar para `/create-project` e/ou
   `/approve-architecture` (sem use cases de negócio). Caso contrário, **negócio** → `feature-template.md`.
6. Gravar o documento e **atualizar o backlog** em `docs/features/README.md` (linha com `key`,
   link do item, status, tipo).
7. Rodar `scripts/validate-architecture.ps1` (consistência de docs).
8. Se `taskSync.enabled`, sugerir `/sync-tasks` para criar as atividades no tracker após o planejamento.

## Lote
Para vários itens, repetir os passos por id. (Filtros de listagem por sprint/label dependem do provider;
quando não houver suporte direto, peça ao usuário a lista de ids.)

## Agentes sugeridos
`product-planner` (estrutura/critérios quando o item vier incompleto).

## Concluído quando
Cada história existe como `docs/features/<KEY>-<slug>.md` com critérios de aceite (confirmados ou
sinalizados), o backlog está atualizado e a validação de docs passa.
Próximo passo a sugerir: **"abra arquitetura da feature <KEY>"**.
