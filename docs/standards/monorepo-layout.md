# Padrão: Layout do monorepo multi-produto

> Vinculante. Codifica o [ADR-0030](../adr/0030-monorepo-multiproduto.md) (substitui o ADR-0019).

## Estrutura
```
/ (raiz = fábrica compartilhada)
├── .claude/                  # agents + skills + hooks (fábrica)
├── templates/                # formatos de artefatos
├── scripts/                  # gates de validação (rodam por produto)
├── docs/
│   ├── standards/            # regras vinculantes (transversais)
│   ├── adr/                  # decisões transversais (0001..)
│   ├── integrations/         # catálogo de provedores
│   └── guia/                 # guia HTML do processo
├── building-blocks/          # CÓDIGO compartilhado entre produtos
│   ├── BuildingBlocks.Application/   # IUseCase/dispatcher, Result/Notification, IUnitOfWork, behaviors
│   └── BuildingBlocks.Api/           # envelope ApiResponse + ToApiResult, GlobalExceptionHandler
├── apps/                     # OS PRODUTOS (um por pasta)
│   └── <Produto>/
│       ├── src/  ( <Produto>.Domain · .Application · .Infrastructure · .Api )
│       ├── tests/ ( UnitTests · IntegrationTests · ArchitectureTests )
│       ├── db/<provider>/
│       ├── docs/ ( PRODUCT.md · features/ · architecture/ )
│       └── <Produto>.slnx
├── global.json · Directory.Build.props · Directory.Packages.props   # MSBuild compartilhado (raiz)
└── CLAUDE.md · README.md
```

## Regras
- **Um produto = uma pasta em `apps/`** com sua própria solução (`.slnx`), banco e `docs/PRODUCT.md`. Nada de produto na raiz.
- **A fábrica é compartilhada** e agnóstica de produto (não referencia código de produto).
- **Blocos transversais vêm do `BuildingBlocks`** (ProjectReference relativo: `../../../building-blocks/...`).
  Não re-scaffoldar dispatcher/Result/envelope dentro do produto.
- **MSBuild compartilhado:** `global.json` + `Directory.Build.props` + `Directory.Packages.props` na raiz valem para
  `building-blocks/` e `apps/*` (o MSBuild sobe a árvore). Versões de pacote centralizadas.
- **Clean Architecture por produto** (Domain ← Application ← {Infrastructure, Api}); `Application`/`Api` podem
  referenciar `BuildingBlocks.*`. O validador (`validate-clean-architecture.ps1`) permite refs a `BuildingBlocks.*`.
- **Contexto:** `apps/<Produto>/docs/PRODUCT.md` (por produto). ADRs transversais na raiz (`docs/adr/`); decisões
  só do produto em `apps/<Produto>/docs/` (architecture/feature).

## Tooling
- **`/create-project nome X`** cria `apps/X/` no layout acima, referenciando o BuildingBlocks.
- **CI** (`.github/workflows/ci.yml`) builda o `BuildingBlocks` e roda **matriz por produto** (`apps/*/*.slnx`):
  build `-warnaserror` + testes + `validate-clean-architecture.ps1` + `validate-api-conventions.ps1` (escopo do produto).
- **Branches/PR** seguem `branching.md`; recomenda-se escopo do produto no commit/PR (ex.: `feat(auth): …`).
