# Plano — Fábrica de Front-end (Angular) em repositório separado

- **Status:** Proposta (aguardando aprovação para criar o repo)
- **Data:** 2026-06-25
- **Decisão de plataforma:** [ADR-0031](../adr/0031-fabrica-frontend-angular-repo-separado.md)
- **Repo atual:** back-end (C#/.NET, este repositório). **Novo repo:** front-end (Angular/TypeScript).

> Objetivo: replicar o **padrão de fábrica** deste back-end (agentes + skills + standards + ADRs + hooks +
> write-back de tasks + gate de CI/branching) num repositório **apartado, 100% front-end**, reaproveitando o
> que é stack-agnóstico e trocando só o que é específico de back-end por equivalentes de front-end.

## 1. Princípio
O valor desta fábrica **não** é a linguagem — é o **processo versionado e assistido por agentes**:
`import-story → brainstorm → approve-architecture → create-* → tests → PR`, com decisões em ADR, padrões
vinculantes e tasks rastreadas no tracker. Esse DNA é portável. A fábrica de FE mantém o **mesmo fluxo e a
mesma mentalidade**, para o time ter um único modelo mental nos dois repositórios.

## 2. O que é reaproveitado (agnóstico) × o que troca (específico)

| Camada | Reaproveita (copia/adapta leve) | Troca por versão Angular/FE |
|---|---|---|
| **Processo / skills** | `import-story`, `brainstorm-story`, `approve-architecture`, `create-tech-story`, `sync-tasks`, `review-pr`, `create-feature` | `create-project` (workspace Nx+Angular), `create-component`, `create-page` (rota/feature), `create-design-token`, `create-store` (state), `create-integration` (API client/auth/analytics), `create-tests` (unit/componente/e2e) — **substituem** `create-usecase`/`create-db-script`/`create-job`/`create-queue-provider` |
| **Agentes** | `product-planner`, `tech-lead-reviewer`, `security-reviewer`, `devops-engineer` (adaptados) | `frontend-developer` (Angular), `ux-accessibility-reviewer`, `design-system-keeper`, `solution-architect` (FE: arquitetura de UI), `qa-tester` (componente+e2e+a11y) — **substituem** `backend-developer`/`oracle-dba-reviewer`; `observability-engineer` → foco em **RUM/Web Vitals/error tracking** |
| **Scripts** | `import-tracker-issue.ps1`, `push-tracker-tasks.ps1`, `load-dotenv.ps1` (**idênticos** — mesmo Azure) | validadores: `validate-clean-architecture` → `validate-frontend-architecture` (camadas/standalone, sem import de app em lib, etc.); `validate-api-conventions` → `validate-ui-conventions` (componentes smart/dumb, a11y básica) |
| **Docs/standards** | ADR template, feature template, brainstorm, `branching`, `issue-trackers`, `quality-checklist`, `clean-code`, `escrita-de-historias`, `configuration` | `architecture` (componentes, feature/standalone, signals), `state-management`, `styling-design-tokens`, `accessibility` (WCAG), `api-access` (client tipado do OpenAPI), `error-handling` (HTTP interceptors), `testing` (pirâmide FE), `performance` (budgets/lazy/Web Vitals), `security` (CSP/XSS/sem segredo no bundle), `i18n` |
| **Compartilhado** | Conceito do `BuildingBlocks` / monorepo multi-produto (ADR-0030) | `libs/` Nx: **`design-system`** (UI kit + tokens), **`core`** (http/auth/api-client), **`util`** — apps em `apps/<App>/` |
| **CI / governança** | Gate `ci-ok`, ruleset em `dev`/`staging`, fluxo de PR, memória | Matriz por **Nx affected** (lint, typecheck, test unit/componente, build, bundle-budget, a11y, e2e opcional) |

## 3. Layout proposto do repo de front-end (espelha ADR-0030)

```
/ (raiz = fábrica compartilhada de FE)
├── .claude/ (agents · skills · hooks · settings)     # tooling
├── docs/ (standards · adr · guia · integrations)     # transversal
├── templates/ (adr · feature · architecture-ui · component · pr · test-plan)
├── scripts/ (tracker reaproveitado + validate-frontend-*)
├── libs/
│   ├── design-system/   # UI kit + design tokens (analogia ao BuildingBlocks)
│   ├── core/            # http client tipado (gerado do OpenAPI), auth, interceptors
│   └── util/            # helpers puros, testáveis
├── apps/
│   └── <App>/           # cada produto de front-end (Angular) + e2e
├── nx.json · tsconfig.base.json · package.json       # workspace compartilhado
└── CLAUDE.md · README.md
```

**Workspace:** recomendo **Nx + Angular**. O Nx dá `nx affected` (build/test só do que mudou) que **espelha o job
`discover` + matriz** do back-end, além de generators (base para as skills `create-component`/`create-page`) e cache.
Alternativa mais leve: workspace nativo do Angular CLI (multi-project) — fica como item de decisão no `create-project`/ADR do 1º app.

## 4. Agentes do front-end (papéis)
- **frontend-developer** — implementa componentes/páginas/stores/serviços seguindo a arquitetura aprovada e os standards.
- **solution-architect (FE)** — desenha a arquitetura da feature de UI: rotas, componentes smart/dumb, estado, contratos de API, pontos de a11y/perf.
- **ux-accessibility-reviewer** — revisa acessibilidade (WCAG), semântica, foco/teclado, contraste; testabilidade de UX.
- **design-system-keeper** — guardião do `design-system` (tokens, componentes reutilizáveis, consistência visual, evita duplicação).
- **qa-tester (FE)** — estratégia de testes: unit, **componente** (Testing Library), **e2e** (Playwright/Cypress), a11y automatizada.
- **security-reviewer (FE)** — XSS/sanitização, CSP, dados sensíveis no bundle/localStorage, deps, authz no client (apenas UX; a verdade é no BE).
- **product-planner**, **tech-lead-reviewer**, **devops-engineer** — reaproveitados (adaptados ao contexto FE).

## 5. Skills do front-end (fluxo)
Reaproveitadas (agnósticas): `import-story`, `brainstorm-story`, `approve-architecture`, `create-tech-story`,
`create-feature`, `sync-tasks`, `review-pr`. Novas/substitutas:
- **create-project** — workspace Nx+Angular, libs base (`design-system`/`core`/`util`), CI, ruleset, configuração por ambiente.
- **create-page** — nova rota/feature (lazy), com smart component + estado + acesso à API tipada + testes.
- **create-component** — componente reutilizável (preferência: no `design-system` se for genérico; dumb/presentational por padrão) + teste de componente + story (se Storybook).
- **create-store** — fatia de estado (Signals/NgRx) com seletores/efeitos e testes.
- **create-design-token** — token de design (cor/espaçamento/tipografia) propagado ao tema.
- **create-integration** — adapter de integração no client (API tipada do OpenAPI, auth/OIDC, analytics) via porta + provider.
- **create-tests** — suíte unit/componente/e2e/a11y mapeada aos critérios de aceite.

## 6. Contrato com o back-end (ponto de conexão dos dois repos)
- O back-end **já expõe OpenAPI** (ADR-0015). A fábrica de FE **gera um client TypeScript tipado** a partir desse
  contrato (ex.: `openapi-typescript`/`orval`/`ng-openapi-gen`) em `libs/core`. FE e BE ficam em sincronia **sem
  acoplar os repositórios**: o contrato é a fronteira.
- **Obrigação no back-end** (este repo): publicar/versionar o `openapi.json` de cada produto (artefato de CI ou
  endpoint estável). Capturado no [ADR-0031](../adr/0031-fabrica-frontend-angular-repo-separado.md).

## 7. Governança idêntica
Branching/PR (`feature/{id}-{nome}` → PR p/ `dev`; promoção fast-forward p/ `main`/`staging`), gate `ci-ok`,
ruleset em `dev`/`staging`, ADR para toda decisão, `docs/PRODUCT.md` por app, memória do projeto, write-back de
tasks no **mesmo Azure** (org `T-SystemsdoBrasil` / `Yamaha - Rollout`) — reuso direto dos scripts de tracker.

## 8. Decisões em aberto (resolver no scaffolding / 1º app)
- **Nx × Angular CLI workspace** (recomendo Nx) — vira ADR no `create-project`.
- **Estado:** Signals nativo × NgRx SignalStore × NgRx clássico — ADR no 1º app, conforme complexidade.
- **Testes:** Jest × Vitest (unit/componente) e Playwright × Cypress (e2e).
- **Estilo:** CSS/SCSS + tokens × Tailwind × biblioteca (Angular Material/CDK) — define o `design-system`.
- **Storybook** para o design system (sim/não).
- **Geração do client:** ferramenta OpenAPI→TS e onde roda (CI do FE puxando o `openapi.json` publicado pelo BE).

## 9. Próximos passos (após aprovação)
1. Criar o repositório de FE (ex.: `arquitetura-ia-frontend`) e portar o **núcleo agnóstico** (scripts de tracker, templates de ADR/feature, fluxo de skills, branching, hooks, gate de CI, memória).
2. Escrever os **standards de FE** e os **agentes/skills** específicos de Angular.
3. Implementar o `create-project` (Nx+Angular) e gerar o **primeiro app** como prova de ponta a ponta (espelhando o que fizemos aqui com o Plataforma2ASmart.Auth).
4. Fechar o **contrato OpenAPI→TS** (gerador + publicação do `openapi.json` no back-end).

---
> Aprovado o plano, o passo 1–2 pode ser executado como `/create-project` (na nova fábrica) ou portado manualmente a partir deste repo.
