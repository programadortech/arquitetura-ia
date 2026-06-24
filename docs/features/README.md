# Features

Um documento por feature/história. Crie de duas formas:
- **Do zero:** `/create-feature` a partir de [`templates/feature-template.md`](../../templates/feature-template.md).
- **Importando de um tracker:** `/import-story <id>` → gera `docs/features/<KEY>-<slug>.md`.
  O tracker é plugável (GitHub / Azure DevOps / GitLab) — ver
  [`../standards/issue-trackers.md`](../standards/issue-trackers.md).

Uma feature deve ter critérios de aceite concretos e testáveis antes de avançar para a arquitetura
(`/approve-architecture`). Esta tabela é o **backlog** do repositório.

> **POs:** escrevam as histórias no Azure DevOps seguindo o
> [padrão de escrita de histórias](../standards/escrita-de-historias.md) — é o que garante import limpo
> e implementação correta pela IA.

| História | Item (tracker) | Status | Arquitetura | Responsável |
|---|---|---|---|---|
| [Autenticação e Gerenciamento de Senha](AZ-12094-autenticacao-e-gerenciamento-de-senha.md) | [AZ-12094](https://dev.azure.com/T-SystemsdoBrasil/Yamaha%20-%20Rollout/_workitems/edit/12094) | Importada (a refinar) | — | — |

> Coluna **Item (tracker)**: a `key` canônica e o link do item de origem — `GH-<n>` (GitHub),
> `AZ-<n>` (Azure DevOps) ou `GL-<n>` (GitLab) — quando a história foi importada (rastreabilidade).
>
> Fluxo: história → arquitetura → casos de uso → testes → PR (o PR referencia o item e o fecha/relaciona).
