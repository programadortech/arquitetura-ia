# ADR-0010: Trackers de histórias plugáveis (GitHub / Azure DevOps / GitLab)

- **Status:** Aceito
- **Data:** 2026-06-23

## Contexto
As histórias/requisitos podem residir em diferentes ferramentas conforme o time/cliente: GitHub Issues,
Azure DevOps Boards ou GitLab Issues. O fluxo de geração de código (feature → arquitetura → use case →
testes → PR) não deve acoplar-se a um tracker específico, e trocar de origem não deve mudar o processo.

## Decisão
Adotar uma **abstração de tracker plugável**, análoga aos providers de fila ([ADR-0008](0008-pluggable-queue-providers.md)):
um **contrato normalizado** de história (key, title, body, acceptanceCriteriaRaw, labels, milestone,
assignees, url, state) e um adapter por provider. O provider é selecionado em
`.claude/tracker.config.json` (ou via `-Provider`). Segredos somente em variáveis de ambiente
(`GITHUB_TOKEN` / `AZDO_PAT` / `GITLAB_TOKEN`). A skill `/import-story` consome o contrato e gera sempre
`docs/features/<KEY>-<slug>.md` no mesmo formato. Implementação em `scripts/import-tracker-issue.ps1`.
Regras completas em [`docs/standards/issue-trackers.md`](../standards/issue-trackers.md).

## Consequências
- (+) Trocar de tracker é mudança de configuração; o fluxo e os agentes permanecem iguais.
- (+) Rastreabilidade preservada via `key` canônica e referência no PR.
- (+) Entrada padronizada de critérios de aceite (Given/When/Then) alimenta arquitetura e testes.
- (−) O contrato é um denominador comum; campos muito específicos de um provider ficam fora ou via extensão.
- (−) Cada provider exige sua credencial/escopo de leitura configurados no ambiente.

## Alternativas consideradas
- Acoplar só ao GitHub: simples, mas inviabiliza times em Azure DevOps/GitLab.
- Sincronização bidirecional automática com o tracker: complexidade e risco altos; optamos por importação
  sob demanda (pull), versionando a história como documento no repositório.
- Manter histórias apenas no tracker (sem documento no repo): perde a fonte da verdade versionada que os
  agentes leem para implementar.
