# ADR-0030: Monorepo multi-produto (`apps/<Produto>/`) + biblioteca compartilhada `BuildingBlocks`

- **Status:** Aceita
- **Data:** 2026-06-24
- **Decisores:** Acaciano (tech lead), Claude
- **Substitui:** [ADR-0019](0019-product-monorepo-src-layout.md) (produto único em `src/` na raiz).

## Contexto
O repositório nasceu como **monorepo de um único produto** (código em `src/` na raiz + a fábrica ao lado — ADR-0019).
A necessidade agora é abrigar **vários produtos** no mesmo repositório (ex.: Autenticador, Fluxo de Caixa, …), cada um
isolado com sua própria solução, banco e contexto, **compartilhando a fábrica** (skills, agents, padrões, scripts,
ADRs, CI) e os **blocos transversais** de código (dispatcher, Result, envelope, etc.).

## Decisão
1. **Cada produto vive em `apps/<Produto>/`**, autossuficiente:
   ```
   apps/<Produto>/
   ├── src/  ( <Produto>.Domain · .Application · .Infrastructure · .Api )
   ├── tests/ ( UnitTests · IntegrationTests · ArchitectureTests )
   ├── db/<provider>/
   ├── docs/ ( PRODUCT.md · features/ · architecture/ )
   └── <Produto>.slnx
   ```
2. **A fábrica é compartilhada na raiz** e agnóstica de produto: `.claude/`, `templates/`, `scripts/`,
   `docs/standards/`, `docs/adr/` (decisões transversais), `docs/integrations/`, `docs/guia/`,
   `global.json`, `Directory.Build.props`, `Directory.Packages.props`, CI e ruleset.
3. **Blocos transversais em `building-blocks/`** — biblioteca **`BuildingBlocks`** reutilizada por todos os produtos:
   - `BuildingBlocks.Application` — `IUseCase`/`IUseCaseDispatcher`/`UseCaseDispatcher`, behaviors (`LoggingBehavior`),
     `Unit`, `Result`/`Error`/`Notification`, porta `IUnitOfWork`, `AddBuildingBlocksApplication()`.
   - `BuildingBlocks.Api` — envelope `ApiResponse` + `ToApiResult` (status codes), `GlobalExceptionHandler`.
   Cada produto **referencia** o BuildingBlocks (ProjectReference) em vez de re-scaffoldar esses tipos.
4. **`/create-project` gera em `apps/<nome>/`** referenciando o BuildingBlocks; **CI roda em matriz por produto**
   (descobre `apps/*` + builda o BuildingBlocks); os scripts de validação rodam **por produto**.
5. **Contexto por produto** em `apps/<Produto>/docs/PRODUCT.md` (não mais um único `docs/PRODUCT.md` na raiz).
   Decisões **transversais** continuam em `docs/adr/` (raiz); decisões **específicas do produto** ficam em
   `apps/<Produto>/docs/` (architecture/feature) — e, se quiser ADRs por produto, `apps/<Produto>/docs/adr/`.

## Consequências
- (+) Vários produtos isolados no mesmo repo, com a fábrica e os blocos comuns compartilhados.
- (+) Menos duplicação (dispatcher/Result/envelope vêm do BuildingBlocks); padrões aplicados igualmente a todos.
- (+) Build/CI por produto — mexer num produto não rebuilda os outros (path filters/matriz).
- (−) Mudança estrutural grande; o produto existente (Plataforma2A.Auth) é removido e re-scaffoldado no novo layout.
- (−) BuildingBlocks vira dependência comum: mudanças nele afetam todos os produtos (versionar com cuidado).
- (−) A regra de dependência (validador) precisa permitir `Application/Api` referenciarem `BuildingBlocks.*`.

## Alternativas consideradas
- **Manter um produto em `src/` (ADR-0019):** não acomoda múltiplos produtos. Superada.
- **Cada produto 100% independente (sem BuildingBlocks):** zero acoplamento, mas duplica dispatcher/Result/envelope
  em todo produto. Rejeitada pelo PO em favor da lib compartilhada.
- **Vários repositórios (polyrepo):** perde a fábrica/contexto compartilhados e o fluxo único. Rejeitada.

## Referências
- [`docs/standards/monorepo-layout.md`](../standards/monorepo-layout.md) · [ADR-0019](0019-product-monorepo-src-layout.md) (substituída)
- [ADR-0002](0002-clean-architecture.md) · [ADR-0028](0028-padroes-camada-api.md) · [ADR-0014](0014-error-handling-result-notification.md)
