---
name: create-project
description: Estrutura um novo projeto C# corporativo a partir deste template — solução em Clean Architecture, banco plugável (Oracle/SQL Server/PostgreSQL/MySQL), Serilog, OpenTelemetry, Polly, Hangfire, filas plugáveis e os três projetos de teste. Use quando o usuário disser "crie um projeto com nome X".
---

# Skill: create-project

Gera um esqueleto de solução completo e compilável, nomeado conforme o projeto que o usuário fornecer.

## Inputs
- **ProjectName** (obrigatório) — PascalCase, ex: `Billing`, `OrderManagement`.
- Opcional: **banco de dados** (oracle | sqlserver | postgresql | mysql), padrão `oracle`.
- Opcional: **acesso a dados** (efcore | dapper), padrão `efcore` — ambos com Unit of Work (ver `docs/standards/database.md`).
- Opcional: provedor de fila padrão (Kafka | SQS | RabbitMQ | MQTT), padrão `RabbitMQ`.
- Opcional: **jobs** (hangfire | none), padrão `none` · **apidocs** (scalar,swagger) · **gateway** (yarp | none), padrão `none`.
- Opcional: **api** (controllers | minimal), padrão `controllers` — estilo da camada de API (ver `docs/standards/api-layer.md` / ADR-0028).

Se ProjectName estiver faltando, peça antes de prosseguir. Confirme as opções (ou use os defaults).

## Steps
1. Confirme o ProjectName. O produto é criado em **`apps/<ProjectName>/`** (monorepo multi-produto — ADR-0030).
2. Crie o layout em `apps/<ProjectName>/` (ver `docs/standards/monorepo-layout.md`):
   - `src/<ProjectName>.Domain`, `.Application`, `.Infrastructure`, `.Api`
   - `tests/<ProjectName>.UnitTests`, `.IntegrationTests`, `.ArchitectureTests`
   - `db/<provider>/`, `docs/` (PRODUCT.md · features · architecture), `<ProjectName>.slnx` (provider = banco escolhido)
3. Adicione a estrutura base (sem lógica de negócio), **reutilizando o `BuildingBlocks`** (não re-scaffoldar):
   - Referencie `building-blocks/BuildingBlocks.Application` (dispatcher `IUseCase`/`IUseCaseDispatcher`,
     `Result`/`Notification`, `IUnitOfWork`, behaviors) e `building-blocks/BuildingBlocks.Api` (envelope `ApiResponse`
     + `ToApiResult`, `GlobalExceptionHandler`) via `ProjectReference`. Registre com `AddBuildingBlocksApplication(<Assembly do produto>)`.
   - Application do produto: ports específicos + `Result/Notification` (vindo do BuildingBlocks).
   - Infrastructure: Serilog + OpenTelemetry, políticas Polly, e o **acesso a dados escolhido**:
     - **efcore** → `AppDbContext` (provider do banco) + `EfUnitOfWork`;
     - **dapper** → `IDbConnection` + `DapperUnitOfWork` (transação) — ambos com Unit of Work (`docs/standards/database.md`).
     Mais: **Hangfire só se `jobs=hangfire`**, abstração de fila plugável (`IQueuePublisher`/`IQueueConsumer`),
     e adapters de integração conforme o catálogo (`docs/integrations/`).
   - Api: **estilo conforme `api`** (default `controllers`: `[ApiController]`+rotas por atributo; `minimal`: `MapGroup`/`Map*`).
     **Controllers/endpoints finos** (só despacham via `IUseCaseDispatcher` e mapeiam `Result`→envelope; **sem lógica**).
     **`Program.cs` enxuto**: composição em `Extensions/` (`AddObservability`/`AddApiServices`/`AddApiDocumentation`/
     `AddJwtAuthentication`/`AddAuthorizationPolicies`/`AddRateLimiting` + `UseApiPipeline`/`MapApiDocumentation`/`MapApiEndpoints`).
     **Status codes** por `docs/standards/http-status-codes.md` (201 no create, 204 sem corpo, etc.). Ver `docs/standards/api-layer.md` (ADR-0028).
   - Api: host ASP.NET Core, DI, health checks, OTLP, **envelope `ApiResponse` + middleware global de exceções**,
     **OpenAPI** (Scalar + Swagger) — Swagger apontando para o OpenAPI **nativo** `/openapi/v1.json` (NÃO o
     default `/swagger/v1/swagger.json`), a **base URL `/` redireciona para `/scalar`** fora de produção, e os
     endpoints utilitários (`/`, `/health`) usam `.ExcludeFromDescription()` para **não** poluir o OpenAPI
     (projeto recém-criado vem com `paths: {}`) — ver `docs/standards/api-documentation.md`. `gateway` YARP opcional.
   - **Configuração por ambiente** em TODO projeto executável (Api, Gateway, workers): `appsettings.json` +
     `appsettings.Development.json` (localhost) + `appsettings.Staging.json` + `appsettings.Production.json` +
     `Properties/launchSettings.json` (perfis Development/Staging). Sem segredos nos arquivos — placeholders +
     env/secret store. Ver `docs/standards/configuration.md`.
   - Api: host mínimo ASP.NET Core, composition root de DI, health checks, configuração do exportador OTLP.
4. Adicione `global.json` (fixe o SDK do .NET 10), `Directory.Build.props`, `Directory.Packages.props`
   (gerenciamento central de pacotes), `.editorconfig`, `.gitignore`.
5. Adicione testes de arquitetura (NetArchTest) aplicando a regra de dependência.
6. Inicialize os docs por projeto a partir de `templates/` (arquitetura, cópia do ADR-0001, README).
7. Execute `dotnet build -warnaserror`, `scripts/validate-clean-architecture.ps1` e `scripts/validate-api-conventions.ps1`. Corrija até ficar verde.

## Standards to read first
- `docs/standards/architecture.md`, `docs/standards/usecase-dispatcher.md`,
  `docs/standards/observability.md`, `docs/standards/queue-providers.md`,
  `docs/standards/database.md` (+ `oracle.md` se Oracle), `docs/standards/testing.md`.

## Suggested agents
`solution-architect` (design do esqueleto) → `backend-developer` (estruturação) → `devops-engineer`
(build/CI) → `tech-lead-reviewer` (verificação de sanidade).

## Done when
A solução compila com warnings-as-errors, os testes de arquitetura passam e a estrutura corresponde ao `CLAUDE.md`.
**Não** adicione funcionalidades de negócio aqui — isso é `/create-feature`.
