# Features

Um documento por feature/história. Crie de duas formas:
- **Do zero:** `/create-feature` a partir de [`templates/feature-template.md`](../../templates/feature-template.md).
- **Importando do GitHub Issues:** `/import-story <número>` → gera `docs/features/GH-<número>-<slug>.md`.

Uma feature deve ter critérios de aceite concretos e testáveis antes de avançar para a arquitetura
(`/approve-architecture`). Esta tabela é o **backlog** do repositório.

| História | Issue | Status | Arquitetura | Responsável |
|---|---|---|---|---|
| _nenhuma ainda_ | — | — | — | — |

> Coluna **Issue**: link `#<número>` da issue do GitHub quando a história foi importada (rastreabilidade).
>
> Fluxo: história → arquitetura → casos de uso → testes → PR (o PR referencia `#<número>` e fecha a issue).
