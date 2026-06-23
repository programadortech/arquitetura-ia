# Projeto: <ProjectName>

> Gerado por `/create-project`. Mantenha este arquivo no `docs/` do projeto.

## Visão geral
- **Nome:** <ProjectName>
- **Propósito:** <uma linha>
- **Responsáveis:** <time / pessoas>
- **Criado em:** <YYYY-MM-DD>

## Stack (herdada do template)
.NET 10 · Clean Architecture · dispatcher de UseCase customizado (sem MediatR pago) · banco plugável · Serilog +
OpenTelemetry · Polly · Hangfire · filas plugáveis (Kafka/SQS/RabbitMQ/MQTT).

- **Banco de dados:** <Oracle | SqlServer | PostgreSql | MySql>
- **Provedor de fila padrão:** <Kafka | SQS | RabbitMQ | MQTT>

## Estrutura da solução
```
<ProjectName>/
├── src/
│   ├── <ProjectName>.Domain/
│   ├── <ProjectName>.Application/
│   ├── <ProjectName>.Infrastructure/
│   └── <ProjectName>.Api/
├── tests/
│   ├── <ProjectName>.UnitTests/
│   ├── <ProjectName>.IntegrationTests/
│   └── <ProjectName>.ArchitectureTests/
├── db/<provider>/
├── docs/
└── <ProjectName>.sln
```

## Build e execução
```
dotnet restore
dotnet build -warnaserror
dotnet test
dotnet run --project src/<ProjectName>.Api
```

## Configuração
- Connection strings, endpoint OTLP, provedor de mensageria, Hangfire — via `appsettings.*` / variáveis de ambiente.
- Nenhum segredo no código-fonte.

## Quality gates
- `pwsh scripts/validate-clean-architecture.ps1`
- `pwsh scripts/validate-tests.ps1`
- Gate completo: `bash .claude/hooks/pre-pr-check.sh`

## Links
- Padrões: `../docs/standards/`
- ADRs: `../docs/adr/`
