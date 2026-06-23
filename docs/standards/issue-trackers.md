# Padrão: Trackers de Histórias Plugáveis

As histórias (features) podem vir de **GitHub Issues**, **Azure DevOps Boards** ou **GitLab Issues**.
O tracker é escolhido por **configuração**, atrás de uma abstração comum — mesma filosofia dos
[providers de fila plugáveis](queue-providers.md). Trocar de tracker é mudança de config, não de fluxo.

## Contrato normalizado (a "porta")

Todo adapter de tracker retorna o **mesmo** objeto, independente da origem:

```json
{
  "provider": "github | azure | gitlab",
  "key": "GH-42 | AZ-1234 | GL-7",
  "number": 42,
  "workItemType": "Product Backlog Item | User Story | Feature | Bug | Task | null",
  "title": "...",
  "body": "...",
  "acceptanceCriteriaRaw": "... (campo dedicado quando o provider tem; senão null)",
  "labels": ["..."],
  "milestone": "Sprint 5 / IterationPath",
  "assignees": ["..."],
  "url": "https://...",
  "state": "open | active | closed",
  "parent":   { "id": 11247, "title": "...", "type": "Product Backlog Item" },
  "children": [ { "id": 12080, "title": "...", "type": "Task", "state": "..." } ]
}
```

### Hierarquia: a história manda, a task é apoio
A **história** (User Story / PBI / Feature) carrega os critérios de aceite e dirige tudo; as **tasks** são
itens-filho. O adapter resolve `parent` e `children` (no Azure, via relações de hierarquia). A skill
`/import-story` é **centrada na história**: se receber uma task, sobe para o `parent`; os `children` entram
como contexto do recorte já planejado e evitam que o `/sync-tasks` duplique tasks existentes.

A skill [`/import-story`](../../.claude/skills/import-story/SKILL.md) consome esse contrato e gera
`docs/features/<KEY>-<slug>.md` no formato de `templates/feature-template.md` — sempre igual,
qualquer que seja o tracker.

## Configuração

`.claude/tracker.config.json` seleciona o provider e seus parâmetros **não sensíveis**:

```json
{
  "provider": "github",
  "github": { "repo": "" },
  "azure":  { "org": "minhaorg", "project": "MeuProjeto" },
  "gitlab": { "host": "https://gitlab.com", "projectId": "123" }
}
```

`provider` pode ser sobrescrito por chamada: `-Provider azure`.

## Autenticação (segredos só via variável de ambiente)

| Provider | Mecanismo | Credencial (env) |
|---|---|---|
| **github** | `gh` CLI autenticado, ou REST `api.github.com` | `GITHUB_TOKEN` (escopo `repo`) |
| **azure** | REST `dev.azure.com/_apis/wit/workitems` (Basic auth) | `AZDO_PAT` (escopo *Work Items: Read*) |
| **gitlab** | REST `<host>/api/v4` (header `PRIVATE-TOKEN`) | `GITLAB_TOKEN` (escopo `read_api`) |

> Tokens **nunca** vão para `tracker.config.json` nem para o código-fonte
> (ver [quality-checklist.md](quality-checklist.md)).

## Critérios de aceite por provider
- **Azure DevOps** tem campo dedicado `Microsoft.VSTS.Common.AcceptanceCriteria` (HTML) → vira a seção
  *Critérios de aceite* direto.
- **GitHub / GitLab** não têm campo dedicado: a skill procura uma seção `## Critérios de aceite` /
  `## Acceptance Criteria` (ou task list) no corpo. Para padronizar a entrada, use o template de issue
  do GitHub em [`.github/ISSUE_TEMPLATE/historia.md`](../../.github/ISSUE_TEMPLATE/historia.md) (ou um
  template equivalente no GitLab/Azure).
- Se não houver critérios, o `product-planner` redige a partir da descrição e marca como **"a confirmar"**.

> Para a entrada vir sempre completa, os POs devem seguir o
> [padrão de escrita de histórias](escrita-de-historias.md).

## Rastreabilidade
- O arquivo de feature usa a `key` (`GH-`/`AZ-`/`GL-`) como id canônico.
- Commits e PRs referenciam o item original para fechar/relacionar (ex.: `#42` no GitHub).

## Write-back de tasks (escrita no tracker)
Além de ler histórias, a abstração **escreve** as atividades planejadas de volta no tracker como
itens-filho — ver [ADR-0011](../adr/0011-task-writeback-tracker.md). A skill `/sync-tasks` deriva as tasks
do documento de arquitetura e chama `scripts/push-tracker-tasks.ps1`:
- **Azure DevOps:** work items do tipo `taskSync.azureTaskType` (default `Task`) com link de parentesco.
- **GitHub/GitLab:** checklist `- [ ]` anexada ao corpo/descrição da issue.

Controlado por `taskSync.enabled`. A credencial precisa ter permissão de **escrita** no tracker.

## Tipos de história (negócio / técnica)
Uma história importada é classificada por label/tag — ver
[ADR-0012](../adr/0012-story-types-business-technical.md):
- **Técnica** se tiver algum label de `storyKinds.technicalLabels`
  (default: `historia-tecnica`, `tecnica`, `tech`, `arquitetura`, `infra`) → usa
  `templates/historia-tecnica-template.md` e segue para `/create-project` / `/approve-architecture`.
- **Negócio** caso contrário → `templates/feature-template.md`.

Ver [ADR-0010](../adr/0010-pluggable-issue-trackers.md). Adapters em
`scripts/import-tracker-issue.ps1` (leitura) e `scripts/push-tracker-tasks.ps1` (escrita de tasks).
