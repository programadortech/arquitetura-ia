# ADR-0025: Revisão automatizada de PR via GitHub Action (Claude)

- **Status:** Aceita
- **Data:** 2026-06-24
- **Decisores:** Acaciano (tech lead), Claude

## Contexto
O fluxo de PR (ADR-0023) precisa de um portão de revisão consistente. A skill `/review-pr` já roda
localmente (gates + agentes `tech-lead-reviewer`, `security-reviewer`, `oracle-dba-reviewer`,
`observability-engineer`), mas depende de alguém executá-la.

Há uma restrição do GitHub relevante: **o autor de um PR não pode dar `APPROVE`/`REQUEST CHANGES` no
próprio PR — apenas comentar**. Logo, uma revisão que rode com a mesma identidade que abriu o PR só
consegue registrar observações, não aprovar/reprovar formalmente.

## Decisão
Vamos adicionar uma **GitHub Action** (`.github/workflows/claude-pr-review.yml`) que, a cada PR para
`dev` ou `staging`, executa o **Claude Code Action** (`anthropics/claude-code-action`) revisando o diff
contra `docs/standards/` e `docs/adr/`, seguindo a skill `review-pr`, e **posta o veredito via
`gh pr review`** (`--approve` / `--request-changes` / `--comment`).

Por rodar como `github-actions[bot]` (identidade separada do autor), a Action **pode aprovar ou reprovar
de fato**. A revisão local on-demand (`/review-pr`) continua disponível para análises mais profundas.

Pré-requisito operacional: secret **`ANTHROPIC_API_KEY`** no repositório. O `GITHUB_TOKEN` padrão da Action
recebe permissão `pull-requests: write`.

## Consequências
- (+) Todo PR para `dev`/`staging` recebe uma revisão padronizada automaticamente, com Approve/Request changes reais.
- (+) Os mesmos padrões/ADRs da revisão local valem na CI — uma fonte de verdade.
- (+) Não depende de o autor lembrar de rodar a revisão.
- (−) Consome créditos da API Anthropic por PR; exige gerenciar o secret.
- (−) A Action lê os padrões e o diff, mas **não roda os gates PowerShell** (build/test/`validate-*.ps1`) —
  esses ficam para um workflow de CI dedicado (a criar) ou para a verificação local.

## Alternativas consideradas
- **Só revisão local (`/review-pr`):** depende de execução manual e não aprova/reprova em PR do próprio autor.
- **Conta de serviço/bot dedicada com PAT:** funciona, mas adiciona gestão de identidade/segredo extra; a
  `github-actions[bot]` já resolve a separação de identidade sem conta adicional.
- **Outras ferramentas de review (CodeRabbit etc.):** não conhecem os padrões/ADRs específicos deste repositório.

## Referências
- [ADR-0023 — Estratégia de branches e fluxo de PR](0023-git-branching-strategy.md)
- [docs/standards/pr-review-automation.md](../standards/pr-review-automation.md)
- [docs/standards/branching.md](../standards/branching.md)
- `.claude/skills/review-pr/SKILL.md`
- [.github/workflows/claude-pr-review.yml](../../.github/workflows/claude-pr-review.yml)
