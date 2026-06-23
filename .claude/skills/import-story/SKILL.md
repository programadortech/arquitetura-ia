---
name: import-story
description: Importa uma história (User Story/PBI/Feature) de um tracker plugável (GitHub Issues, Azure DevOps ou GitLab) para docs/features/<KEY>-<slug>.md, navegando a hierarquia — se receber uma task, sobe para a história-pai e traz as tasks-filhas como contexto. Use quando o usuário disser "importe a história 123" / "importe a issue #123" / "importe o work item 1234".
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

## Princípio: a HISTÓRIA manda, a TASK é apoio
A **história** (User Story / PBI / Feature) é a fonte de contexto e dos critérios de aceite — é o que dirige
arquitetura, código e testes. As **tasks** são itens-filho (atividades) e servem para *entender* o recorte,
não como unidade a documentar. Por isso o import é **centrado na história** e navega a hierarquia.

## Passos
1. Buscar o item normalizado (já vem com hierarquia):
   `pwsh scripts/import-tracker-issue.ps1 -Id <id> [-Provider <p>]`
   → JSON no contrato comum: `provider, key, number, workItemType, title, body,
   acceptanceCriteriaRaw, labels, milestone, assignees, url, state, parent, children`.
2. **Resolver o nível certo (a história):**
   - **Tem `children`?** Então o item já é o nível que "manda" (agrupa o trabalho) → use-o como a história,
     mesmo que o tipo seja `Bug`/`Bug Sprint`. (Ex.: um Bug com 9 tasks-filhas é a história.)
   - **É folha** (`workItemType` = `Task`, ou item sem `children` e sem critérios de aceite) e tem `parent`?
     → **suba para o `parent`** (re-rode o script com `-Id <parent.id>`) e cite o item original em "Relacionados".
   - Caso típico de história já no nível certo (PBI/User Story/Feature): siga com ele.
   - Em qualquer dúvida sobre qual é a "história que manda", **confirme com o usuário** mostrando
     `workItemType`, `parent` e a lista de `children`.
3. Derivar o caminho do arquivo: `docs/features/<key>-<slug>.md` onde `key` é `GH-/AZ-/GL-<number>`
   e `slug` vem do título.
4. Preencher `templates/feature-template.md`:
   | Campo normalizado | Seção no documento |
   |---|---|
   | `title` | `# Feature: <title>` |
   | `body` (intro) | Descrição do problema / Contexto de negócio |
   | `acceptanceCriteriaRaw` **ou** seção parseada do body | **Critérios de aceite (Given/When/Then)** |
   | `labels` | metadados / tipo |
   | `milestone` | metadados (sprint/iteration) |
   | `assignees` | Responsável |
   | `url`, `parent`, task original | campo "Relacionados" (rastreabilidade) |
   | `key` | id canônico (usar em commits/PR) |
   | `children` (tasks) | seção **"Tasks existentes no tracker"** (contexto do recorte já planejado) |
5. **Critérios de aceite**:
   - Azure: usar `acceptanceCriteriaRaw` (vem em HTML → converter para Given/When/Then em texto).
   - GitHub/GitLab: procurar `## Critérios de aceite` / `## Acceptance Criteria` ou task list no `body`.
   - Ausente: acionar `product-planner` para redigir a partir da descrição **e dos `children` (tasks)**
     como pistas do escopo, marcando **"a confirmar"** (não inventar requisito firme).
6. **Detectar o tipo da história**: se algum `label`/tag estiver em `storyKinds.technicalLabels`
   (`.claude/tracker.config.json`), tratar como **técnica** → usar
   `templates/historia-tecnica-template.md` e encaminhar para `/create-project` e/ou
   `/approve-architecture` (sem use cases de negócio). Caso contrário, **negócio** → `feature-template.md`.
7. Gravar o documento e **atualizar o backlog** em `docs/features/README.md` (linha com `key`,
   link do item, status, tipo).
8. Rodar `scripts/validate-architecture.ps1` (consistência de docs).
9. Se `taskSync.enabled`, sugerir `/sync-tasks` após o planejamento — mas **não duplicar** as `children`
   que já existem no tracker (criar apenas as novas atividades que faltarem).

## Lote
Para vários itens, repetir os passos por id. (Filtros de listagem por sprint/label dependem do provider;
quando não houver suporte direto, peça ao usuário a lista de ids.)

## Agentes sugeridos
`product-planner` (estrutura/critérios quando o item vier incompleto).

## Concluído quando
Cada história existe como `docs/features/<KEY>-<slug>.md` com critérios de aceite (confirmados ou
sinalizados), o backlog está atualizado e a validação de docs passa.
Próximo passo a sugerir: **"abra arquitetura da feature <KEY>"**.
