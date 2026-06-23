# ADR-0019: Produto no monorepo (solução em src/) com a fábrica embutida

- **Status:** Aceito
- **Data:** 2026-06-23
- **Refina:** [ADR-0001](0001-record-architecture-decisions.md) e o CLAUDE.md

## Contexto
O produto vai **evoluir continuamente** e a IA precisa do **contexto persistente** (decisões, features,
estado) para gerar/evoluir sempre com base no que já existe. Gerar o projeto em pasta-irmã desconexa
perde esse contexto entre sessões.

## Decisão
Este repositório passa a ser o **monorepo do produto**: a solução .NET vive na **raiz** em `src/` + `tests/`
+ `<Produto>.sln`, e a **fábrica** (`.claude/`, `templates/`, `scripts/`, `docs/standards`, `docs/adr`)
**convive** no mesmo repo. Os docs do produto usam as pastas existentes (`docs/features`,
`docs/architecture`, `docs/usecases`, `docs/database`). Um arquivo **`docs/PRODUCT.md`** mantém o
**contexto vivo** (visão, estado, decisões-chave, convenções) que a IA carrega para evoluir o produto.

Os skills (`/create-project`, `/create-usecase`, etc.) operam **dentro de `src/`**. Para iniciar um produto
diferente, clona-se este repo como *starter* e limpa-se `src/`/`tests/`.

## Consequências
- (+) Contexto do produto persiste e versiona junto; a IA evolui com base no estado real.
- (+) Fábrica + produto no mesmo lugar; um único histórico.
- (−) O repo deixa de ser um template "genérico"; vira o repo do produto (mitigado pelo uso como starter).
- (−) Multi-produto exigiria `projects/<nome>/` (decisão futura, se necessário).

## Alternativas consideradas
- Projeto em pasta-irmã/repos separados: perde contexto e exige sincronizar padrões manualmente.
- `projects/<nome>/` (multi-produto): mais flexível, porém mais cerimônia; adiável.
