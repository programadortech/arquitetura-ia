---
name: sync-tasks
description: Gera as tasks/atividades de implementação de uma história e as cria no tracker (Azure DevOps/GitHub/GitLab) como itens-filho/checklist. Use quando o usuário disser "crie as tasks da história X no azure" ou após aprovar a arquitetura.
---

# Skill: sync-tasks

Faz o **write-back** do plano de implementação para o tracker: transforma o que será feito em
tasks rastreáveis ligadas à história. Plugável por provider
(ver [`docs/standards/issue-trackers.md`](../../docs/standards/issue-trackers.md)).

## Entradas
- **StoryId** no tracker (work item id / issue number / iid).
- A fonte do plano: o `docs/architecture/<feature>.md` (preferido) ou o `docs/features/<KEY>.md`.

## Passos
1. Ler o documento de arquitetura/feature da história. Derivar a **lista de tasks** a partir de:
   - cada caso de uso (Domain → Application → Infrastructure → Api),
   - scripts Oracle/migrações, providers de fila, jobs,
   - testes (unit/integração/arquitetura),
   - observabilidade e resiliência exigidas pelos NFRs.
   Cada task = um passo objetivo e verificável (título curto + descrição opcional).
2. Verificar `taskSync.enabled` em `.claude/tracker.config.json`. Se desabilitado, apenas listar as tasks
   no documento e parar.
3. Escrever as tasks em um JSON temporário no formato:
   `[ { "title": "...", "description": "..." }, ... ]`
4. Criar no tracker:
   `pwsh scripts/push-tracker-tasks.ps1 -StoryId <id> -TasksFile <arquivo.json>`
   - Azure: cria work items `Task` filhos da User Story (link Hierarchy-Reverse).
   - GitHub/GitLab: anexa checklist `- [ ]` no corpo/descrição da issue.
5. Registrar as tasks também no documento (seção "Plano de tasks") para rastreabilidade local.

## Boas práticas de granularidade
- Uma task por unidade de trabalho entregável (não "fazer tudo"); evite tasks gigantes ou triviais demais.
- Ordem sugerida: Domain → Application (use case) → Infrastructure (adapters/Oracle/fila/job) →
  Api (endpoint/DI) → Testes → Observabilidade/Resiliência → Revisão.
- Não duplicar tasks já criadas (em re-sync, criar apenas as novas).

## Agentes sugeridos
`solution-architect` / `tech-lead-reviewer` (validam o recorte das tasks).

## Concluído quando
As tasks existem no tracker ligadas à história e estão refletidas no documento.
Próximo passo a sugerir: **"implemente o use case <nome>"**.
