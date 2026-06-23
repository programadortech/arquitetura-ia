---
name: create-tech-story
description: Cria uma história TÉCNICA (arquitetura/infra/setup) — ex.: setup do projeto, arquitetura base, pipeline, observabilidade — como documento e, opcionalmente, item no tracker. Use quando o usuário disser "crie uma história técnica de X" ou "preciso do setup/arquitetura base".
---

# Skill: create-tech-story

Define uma história **técnica**: trabalho de plataforma/arquitetura que constrói ou evolui o sistema como
um todo, em vez de uma feature de negócio.

## Quando usar
- "crie uma história técnica para o setup do projeto / arquitetura base / pipeline / observabilidade"
- Início de um sistema novo, ou trabalho transversal (segurança, migração de plataforma, refactor amplo).

## Diferença para história de negócio
| | Negócio (`/create-feature`) | Técnica (`/create-tech-story`) |
|---|---|---|
| Foco | valor para o usuário | base/plataforma/arquitetura |
| Critérios de aceite | comportamento (Given/When/Then) | técnicos (builda, testes de arquitetura, observabilidade, ADRs) |
| Downstream | `/approve-architecture` → `/create-usecase` | `/create-project` e/ou `/approve-architecture` (sem use cases de negócio) |

## Passos
1. Preencher `templates/historia-tecnica-template.md`: objetivo técnico, motivação, escopo,
   **critérios de aceite técnicos verificáveis**, NFRs transversais, dependências.
2. Gravar em `docs/features/<KEY>-<slug>.md` marcando claramente **Tipo: Técnica**
   (use `KEY` do tracker quando houver, ou `TEC-<slug>` quando criada do zero).
3. Atualizar o backlog em `docs/features/README.md` (marcar tipo técnica).
4. Se houver tracker e `taskSync.enabled`, sugerir `/sync-tasks` para criar as atividades técnicas.
5. Encaminhar:
   - setup inicial do sistema → `/create-project`;
   - arquitetura base / decisões transversais → `/approve-architecture` (gera ADRs base).

## Convenção de tipo no tracker
Uma história importada é tratada como técnica se tiver um dos labels/tags de
`storyKinds.technicalLabels` em `.claude/tracker.config.json`
(default: `historia-tecnica`, `tecnica`, `tech`, `arquitetura`, `infra`).

## Agentes sugeridos
`solution-architect` (lidera) → `devops-engineer` (pipeline/infra) → `observability-engineer`
→ `security-reviewer` → `tech-lead-reviewer`.

## Concluído quando
A história técnica existe com critérios de aceite técnicos verificáveis e está encaminhada para
`/create-project` e/ou `/approve-architecture`.
