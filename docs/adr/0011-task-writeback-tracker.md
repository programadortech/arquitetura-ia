# ADR-0011: Write-back de tasks para o tracker

- **Status:** Aceito
- **Data:** 2026-06-23

## Contexto
Ao planejar a implementação de uma história, o time gera um conjunto de atividades (tasks). Hoje esse
plano vive só no documento de arquitetura; o tracker (Azure DevOps/GitHub/GitLab) fica sem as tasks,
prejudicando acompanhamento, divisão de trabalho e rastreabilidade.

## Decisão
Estender a abstração de tracker plugável ([ADR-0010](0010-pluggable-issue-trackers.md)) com **escrita**:
após o planejamento (tipicamente em `/approve-architecture`), a skill `/sync-tasks` deriva as tasks e as
cria no tracker como itens-filho da história. Comportamento por provider:
- **Azure DevOps:** work items do tipo configurado (`taskSync.azureTaskType`, default `Task`) com link
  de parentesco `System.LinkTypes.Hierarchy-Reverse`.
- **GitHub/GitLab:** checklist `- [ ]` anexada ao corpo/descrição da issue (modo configurável).
Controlado por `taskSync.enabled` em `.claude/tracker.config.json`. Implementação em
`scripts/push-tracker-tasks.ps1`. Segredos apenas via variável de ambiente.

## Consequências
- (+) O tracker reflete o plano real; acompanhamento e rastreabilidade ponta a ponta.
- (+) Mesmo fluxo para qualquer provider; o recorte de tasks é revisável.
- (−) Escrita no tracker exige credencial com permissão de escrita (PAT/token com escopo adequado).
- (−) Re-sync precisa evitar duplicar tasks já criadas (a skill cria apenas as novas).

## Alternativas consideradas
- Manter o plano só no documento: perde visibilidade no tracker usado pelo time.
- Sincronização bidirecional contínua: complexidade/risco altos; optamos por push sob demanda.
