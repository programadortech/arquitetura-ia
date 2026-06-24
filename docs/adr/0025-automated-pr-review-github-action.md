# ADR-0025: Gate de PR gratuito (CI determinístico) + revisão de IA local sob demanda

- **Status:** Aceita
- **Data:** 2026-06-24
- **Decisores:** Acaciano (tech lead), Claude

## Contexto
O fluxo de PR (ADR-0023) precisa de um portão de revisão consistente. Duas formas de automatizar:
- **CI determinístico** (build/test/validação de arquitetura) — gratuito em repositório público.
- **Revisão por IA** — a `anthropics/claude-code-action` na nuvem **cobra por execução** da API da Anthropic
  (~US$ 0,40–2,80 por PR no Sonnet) e dispararia a cada push (`synchronize`), multiplicando o custo.

A skill `/review-pr` já roda localmente (agentes `tech-lead-reviewer`, `security-reviewer`,
`oracle-dba-reviewer`, `observability-engineer`) dentro da assinatura do Claude Code — **sem custo por PR**.

Limitação relevante: o `GITHUB_TOKEN` (`github-actions[bot]`) **não pode dar `Approve`** (só `REQUEST CHANGES`
e `COMMENT`); um `Approved` formal exigiria identidade separada do autor.

## Decisão
Vamos adotar o gate em **duas camadas, com custo zero por padrão**:

1. **CI determinístico obrigatório e gratuito** — `.github/workflows/ci.yml`: a cada PR para `dev`/`staging`,
   `dotnet build -warnaserror` + `dotnet test` + `scripts/validate-clean-architecture.ps1`. Sem IA.
2. **Revisão de IA local sob demanda** — `/review-pr` no Claude Code (assinatura existente), antes de abrir/mergear.

**Não** manteremos a Action de IA na nuvem ligada por padrão (evita conta de API por PR). Ela pode ser
reintroduzida no futuro como **opt-in** (trigger `@claude` em comentário) para PRs específicos.

**Merge manual (decisão do usuário):** o ruleset `protect-dev-staging` exige PR + check `build-test`/CI verde;
o botão de merge só habilita com o CI verde. O **auto-merge fica desligado** — quando o CI passa, o agente
**avisa o usuário** e **ele** faz o merge na plataforma. O agente não mergeia por conta própria.

## Consequências
- (+) Gate automático **gratuito** em todo PR (build/test/arquitetura) — barra os erros estruturais que mais importam.
- (+) Revisão de IA continua disponível, **sem custo por PR** (local via `/review-pr`).
- (+) Sem gestão de secret pago nem risco de conta de API surpresa.
- (−) A revisão de IA não é automática em todo PR — depende de rodar `/review-pr` (decisão consciente de custo).
- (−) Sem `Approved` verde automático de bot (exigiria identidade separada); o merge é destravado pelo CI verde + revisão local.

## Alternativas consideradas
- **Action de IA na nuvem em todo PR:** revisão automática, mas **cobra por execução** e por push — custo alto e recorrente.
- **IA na nuvem opt-in (`@claude`):** viável; paga só nos PRs marcados. Fica como evolução futura, não default.
- **Ferramentas externas (CodeRabbit etc.):** não conhecem os padrões/ADRs específicos deste repositório.

## Referências
- [ADR-0023 — Estratégia de branches e fluxo de PR](0023-git-branching-strategy.md)
- [docs/standards/pr-review-automation.md](../standards/pr-review-automation.md)
- `.claude/skills/review-pr/SKILL.md`
- [.github/workflows/ci.yml](../../.github/workflows/ci.yml)
