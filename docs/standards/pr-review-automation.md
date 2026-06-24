# Padrão: Revisão e gate de PR (custo zero por padrão)

> Vinculante. Codifica [ADR-0025](../adr/0025-automated-pr-review-github-action.md). Complementa o fluxo de
> branches/PR de [`branching.md`](branching.md) e a skill `.claude/skills/review-pr/SKILL.md`.

## Estratégia (sem custo de API)
A revisão de PR tem duas camadas — a obrigatória é **gratuita**:

1. **Gate determinístico em CI (grátis, obrigatório)** — `.github/workflows/ci.yml`.
   A cada PR para `dev`/`staging` roda: `dotnet build -warnaserror`, `dotnet test`
   (unit + arquitetura + integração) e `scripts/validate-clean-architecture.ps1`.
   Não usa IA. O repositório é **público** → minutos de GitHub Actions são gratuitos.
   Pega de graça o que mais importa: build quebrado, teste falhando e violação da regra de dependência.

2. **Revisão de IA sob demanda (local, sem custo por PR)** — a skill `/review-pr`, executada no Claude Code
   (dentro da assinatura existente), delega aos agentes `tech-lead-reviewer`, `security-reviewer`,
   `oracle-dba-reviewer`, `observability-engineer` e produz um veredito. Roda quando você pede, antes
   de abrir/mergear o PR. **Não chama a API por PR — não gera conta avulsa.**

## Por que não a Action de IA na nuvem por padrão
A `anthropics/claude-code-action` chama a **API da Anthropic e cobra por execução** (~US$ 0,40–2,80 por PR
no Sonnet) — e dispararia a cada push (`synchronize`), multiplicando o custo. Por isso ela **não fica ligada
por padrão**. Pode ser reintroduzida como **opt-in** (trigger `@claude` num comentário) se algum dia se quiser
revisão de IA na nuvem em PRs específicos — aí paga só nesses casos.

## Limitação do GitHub (se um dia usar a Action de nuvem)
O `GITHUB_TOKEN` (`github-actions[bot]`) **não pode dar `Approve`** — só `REQUEST CHANGES` e `COMMENT`.
Um `Approved` formal (estado verde) exige **identidade separada do autor** (PAT de outra conta/machine user
ou GitHub App). O PAT do próprio autor também não aprova o próprio PR.

## Merge é MANUAL (decisão do usuário)
O **merge final é do usuário** — o agente **não mergeia** e **não liga auto-merge**. O agente abre o PR,
acompanha o **CI** e, quando **tudo fica verde**, **avisa o usuário** (link do PR + status dos checks) para
que **ele** entre no GitHub e faça o merge para `dev`. "Verde" = o CI passou (build/test/arquitetura).
A revisão de IA local (`/review-pr`) é qualidade sob demanda — não bloqueia nem dispara o merge.

```bash
gh pr create --base dev --head feature/{id}-{slug} --title "…" --body "…"
# (sem auto-merge) — quando o CI ficar verde, o agente avisa; o usuário clica em "Merge" no GitHub.
```

## Fluxo recomendado por feature
1. Implementar na branch `feature/{id}-{slug}` (a partir de `main`).
2. Build/test/validate locais verdes (o hook `pre-pr-check` ajuda).
3. (Opcional, recomendado) Rodar `/review-pr` localmente para a revisão de IA — custo zero.
4. Abrir o PR para `dev`. O **CI grátis** roda como check obrigatório.
5. CI **verde** → o agente **avisa o usuário** → **o usuário faz o merge** na plataforma. CI vermelho → corrigir e avisar de novo.

## Setup (já configurado neste repo)
- **Ruleset `protect-dev-staging`** (Settings → Rules → Rulesets): exige PR + check `build-test` (CI) verde;
  bloqueia push direto/force-push/deleção em `dev`/`staging`. O botão de merge só habilita com o CI verde. Admin pode bypass.
- **Auto-merge desligado** no repo (merge é ato manual do usuário); *delete branch on merge* fica ligado (limpeza).
- Nada de secrets pagos: o gate grátis usa só o `GITHUB_TOKEN` padrão.
