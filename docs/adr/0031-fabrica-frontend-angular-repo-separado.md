# ADR-0031: Fábrica de front-end (Angular) em repositório separado + contrato OpenAPI→TS

- **Status:** Aceita
- **Data:** 2026-06-25
- **Decisores:** Acaciano (tech lead), Claude

## Contexto
Este repositório é uma **fábrica** de back-end (C#/.NET, Clean Architecture) que gera/evolui produtos com um
processo assistido por agentes (`import-story → brainstorm → approve-architecture → create-* → tests → PR`),
padrões vinculantes, ADRs, hooks e write-back de tasks no tracker. O time precisa do **mesmo modelo** para o
front-end, num ambiente **apartado e 100% focado em FE** (toolchain Node/Angular, papéis e cadência próprios).

O valor da fábrica é o **processo versionado e os agentes**, que é stack-agnóstico; o que muda é a camada técnica
(skills/agentes/standards/CI específicos de back-end vs front-end).

## Decisão
Vamos criar a fábrica de front-end como um **repositório separado** (não um monorepo full-stack), **espelhando o
padrão deste back-end**, com stack **Angular + TypeScript**:
- **Reaproveitar o núcleo agnóstico:** fluxo de skills (import/brainstorm/arquitetura/sync-tasks/review/feature),
  scripts de tracker (`import-tracker-issue`, `push-tracker-tasks`, `load-dotenv` — **mesmo Azure**), templates de
  ADR/feature, branching/PR, gate `ci-ok`, ruleset em `dev`/`staging`, memória.
- **Substituir o específico:** agentes (`frontend-developer`, `ux-accessibility-reviewer`, `design-system-keeper`,
  `solution-architect`/`qa-tester` em modo FE), skills (`create-project` Nx+Angular, `create-page`,
  `create-component`, `create-store`, `create-design-token`, `create-integration`, `create-tests`), standards
  (arquitetura de componentes, estado, design tokens, acessibilidade, acesso a API, testes, performance, segurança),
  CI por **Nx affected** (lint/typecheck/test/build/bundle-budget/a11y/e2e).
- **Layout multi-app** análogo ao [ADR-0030](0030-monorepo-multiproduto.md): `apps/<App>/` + libs compartilhadas
  (`design-system`, `core`, `util`) no lugar do `BuildingBlocks`.
- **Contrato entre repos via OpenAPI→TS:** a fronteira FE↔BE é o **contrato OpenAPI** (já exposto pelo back-end —
  [ADR-0015](0015-pluggable-api-documentation.md)). A fábrica de FE **gera um client TypeScript tipado** a partir
  dele. **Obrigação criada para este back-end:** publicar/versionar o `openapi.json` de cada produto (artefato de
  CI ou endpoint estável) para o FE consumir.

O plano detalhado vive em [`docs/platform/frontend-factory-plan.md`](../platform/frontend-factory-plan.md).

## Consequências
- (+) Ambiente de FE limpo e focado; `CLAUDE.md`, lista de agentes e CI sem ruído de toolchain cruzado.
- (+) Mesmo DNA de processo nos dois repos → um único modelo mental para o time.
- (+) Contrato OpenAPI desacopla os repositórios mantendo tipos em sincronia.
- (−) Algum **núcleo agnóstico duplicado** entre os dois repos (scripts/templates/processo) — risco de drift; mitigado
  mantendo-os simples e, se necessário, extraindo um pacote compartilhado no futuro.
- (−) Cria uma obrigação no back-end: **publicar o OpenAPI** de forma estável/versionada.

## Alternativas consideradas
- **Monorepo full-stack único** (FE+BE em `apps/`): contratos no mesmo lugar e mudança atômica cross-stack, mas
  mistura toolchains (.NET+Node), polui agentes/skills/CI e acopla cadências de release. Rejeitada — contraria o
  pedido de ambiente apartado.
- **Repo separado + core de fábrica compartilhado (pacote/submodule):** mais DRY, porém adiciona complexidade de
  versionamento/manutenção do core agora. Adiada — pode ser adotada depois se o drift incomodar.

## Referências
- [`docs/platform/frontend-factory-plan.md`](../platform/frontend-factory-plan.md) — blueprint da fábrica de FE.
- [ADR-0030 monorepo multi-produto](0030-monorepo-multiproduto.md) · [ADR-0015 documentação de API/OpenAPI](0015-pluggable-api-documentation.md) · [ADR-0023 branching](0023-git-branching-strategy.md)
