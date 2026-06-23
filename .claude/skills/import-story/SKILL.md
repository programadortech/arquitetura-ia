---
name: import-story
description: Importa uma história (issue) do GitHub para docs/features/<KEY>-<slug>.md no formato do template, mapeando critérios de aceite, e atualiza o backlog. Use quando o usuário disser "importe a história 123" / "importe a issue #123" / "importe as histórias do milestone X".
---

# Skill: import-story

Traz histórias do GitHub Issues para o repositório como documentos de feature (1 história = 1 arquivo),
prontos para seguir o fluxo `/approve-architecture` → `/create-usecase`.

## Entradas
- **Issue único:** número da issue (ex.: `123`).
- **Lote:** um filtro — `--label historia`, `--milestone "Sprint 5"`, ou `--state open`.

## Pré-requisitos
- `gh` CLI autenticado **ou** a variável de ambiente `GITHUB_TOKEN` (PAT com escopo `repo`) definida.
  O segredo nunca vai para o código-fonte (ver `docs/standards/quality-checklist.md`).

## Passos (issue único)
1. Buscar a issue via `scripts/import-github-issue.ps1 -Number <n>` (tenta `gh`, senão REST API).
   O script devolve JSON normalizado: `number, title, body, labels, milestone, assignees, url, state`.
2. Derivar o identificador do arquivo: `GH-<number>` + slug do título →
   `docs/features/GH-<number>-<slug>.md`.
3. Preencher `templates/feature-template.md` mapeando:
   | Campo da issue | Seção no documento |
   |---|---|
   | `title` | `# Feature: <title>` |
   | `body` (intro) | Descrição do problema / Contexto de negócio |
   | seção de critérios de aceite no `body` | **Critérios de aceite (Given/When/Then)** |
   | `labels` | metadados / tipo |
   | `milestone` | metadados (sprint/release) |
   | `assignees` | Responsável |
   | `url` | campo "Relacionados" (rastreabilidade) |
   | `number` | id canônico `GH-<number>` (usar em commits/PR: `#<number>`) |
4. **Critérios de aceite**: procurar no corpo uma seção `## Critérios de aceite` / `## Acceptance Criteria`
   ou uma lista de tarefas (`- [ ]`). Se não houver, acionar o `product-planner` para redigir critérios
   Given/When/Then a partir da descrição e **marcá-los como "a confirmar"** (não inventar requisito firme).
5. Gravar o documento e **atualizar o backlog** em `docs/features/README.md` (linha com `GH-<number>`,
   link da issue, status).
6. Rodar `scripts/validate-architecture.ps1` (garante consistência de docs).

## Passos (lote)
- Listar via `scripts/import-github-issue.ps1 -List "<filtro>"`, depois repetir os passos 2–5 por issue.
- Ao final, listar o que foi importado e o que ficou com critérios "a confirmar".

## Agentes sugeridos
`product-planner` (estrutura/critérios quando a issue vier incompleta).

## Concluído quando
Cada história importada existe como `docs/features/GH-<number>-<slug>.md` com critérios de aceite
(confirmados ou sinalizados), o backlog está atualizado e a validação de docs passa.
Próximo passo a sugerir: **"abra arquitetura da feature GH-<number>"**.
