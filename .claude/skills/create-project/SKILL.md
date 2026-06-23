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

Se ProjectName estiver faltando, peça antes de prosseguir. Confirme as opções (ou use os defaults).

## Steps
1. Confirme o ProjectName e a pasta de destino (`<ProjectName>/` na raiz do repositório ou um caminho que o usuário fornecer).
2. Crie o layout da solução exatamente como definido em `CLAUDE.md` → "Standard solution layout":
   - `src/<ProjectName>.Domain`, `.Application`, `.Infrastructure`, `.Api`
   - `tests/<ProjectName>.UnitTests`, `.IntegrationTests`, `.ArchitectureTests`
   - `db/<provider>/`, `docs/`, `<ProjectName>.sln` (provider = banco escolhido)
3. Adicione a estrutura base de infraestrutura (sem lógica de negócio):
   - Application: `IUseCase<TRequest,TResponse>`, `IUseCaseDispatcher`, implementação `UseCaseDispatcher`,
     e a extensão de DI `AddApplication()` (veja `docs/standards/usecase-dispatcher.md`).
   - Application: além do dispatcher, as ports de persistência `IUnitOfWork` + repositórios e o
     **Result/Notification** (ver `docs/standards/error-handling.md`).
   - Infrastructure: Serilog + OpenTelemetry, políticas Polly, e o **acesso a dados escolhido**:
     - **efcore** → `AppDbContext` (provider do banco) + `EfUnitOfWork`;
     - **dapper** → `IDbConnection` + `DapperUnitOfWork` (transação) — ambos com Unit of Work (`docs/standards/database.md`).
     Mais: **Hangfire só se `jobs=hangfire`**, abstração de fila plugável (`IQueuePublisher`/`IQueueConsumer`),
     e adapters de integração conforme o catálogo (`docs/integrations/`).
   - Api: host ASP.NET Core, DI, health checks, OTLP, **envelope `ApiResponse` + middleware global de exceções**,
     **OpenAPI** (Scalar + Swagger), e `gateway` YARP opcional.
   - Api: host mínimo ASP.NET Core, composition root de DI, health checks, configuração do exportador OTLP.
4. Adicione `global.json` (fixe o SDK do .NET 10), `Directory.Build.props`, `Directory.Packages.props`
   (gerenciamento central de pacotes), `.editorconfig`, `.gitignore`.
5. Adicione testes de arquitetura (NetArchTest) aplicando a regra de dependência.
6. Inicialize os docs por projeto a partir de `templates/` (arquitetura, cópia do ADR-0001, README).
7. Execute `dotnet build -warnaserror` e `scripts/validate-clean-architecture.ps1`. Corrija até ficar verde.

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
