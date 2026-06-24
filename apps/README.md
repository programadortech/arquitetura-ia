# apps/ — produtos do monorepo

Cada produto vive em `apps/<Produto>/`, isolado (Clean Architecture), com sua própria solução, banco e
contexto (`docs/PRODUCT.md`). Todos compartilham a **fábrica** (raiz) e a lib **`BuildingBlocks`**
(`../building-blocks/`). Ver [ADR-0030](../docs/adr/0030-monorepo-multiproduto.md) e
[`docs/standards/monorepo-layout.md`](../docs/standards/monorepo-layout.md).

Crie um produto com a skill: **`/create-project nome <Produto>`** (ex.: `Autenticador`, `FluxoCaixa`).

_(vazio por enquanto — nenhum produto criado)_
