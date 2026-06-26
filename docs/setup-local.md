# Setup local — rodar a API na sua máquina

Passo a passo para rodar o `Plataforma2A.Auth.Api` localmente. **Segredos não vão para o git** — em dev
ficam no `user-secrets` (ADR-0022 / [`standards/configuration.md`](standards/configuration.md)).

## Pré-requisitos
- **.NET SDK 10** (ver `global.json`).
- **SQL Server** acessível em `localhost` (LocalDB, Express, Docker ou instância).
- (Opcional) servidor SMTP de dev (ex.: papercut/mailhog) — só é usado no fluxo de "esqueci a senha".

## 1) Restaurar e compilar
```powershell
dotnet restore Plataforma2A.Auth.slnx
dotnet build Plataforma2A.Auth.slnx
```

## 2) Configurar segredos de dev (user-secrets — fora do git)
A `Jwt:Key` foi removida do `appsettings.Development.json` de propósito (é segredo). Sem ela, a API
**aborta no startup** com `InvalidOperationException: Jwt:Key não configurado…` (fail-fast intencional).

Defina a chave de assinatura (qualquer string com **32+ caracteres**):
```powershell
dotnet user-secrets set "Jwt:Key" "<sua-chave-de-dev-com-32+-caracteres>" --project src/Plataforma2A.Auth.Api
```

Se a sua **connection string tiver usuário/senha**, ela também é segredo — coloque no user-secrets em vez de
versionar no `appsettings.Development.json`:
```powershell
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=Plataforma2A.Auth;User Id=sa;Password=********;TrustServerCertificate=True" --project src/Plataforma2A.Auth.Api
```
> O `appsettings.Development.json` versionado deve ficar **sem senha** (ex.: `Trusted_Connection=True`).
> Conferir o que está salvo: `dotnet user-secrets list --project src/Plataforma2A.Auth.Api`.

## 3) Criar o schema do banco
Rode o script de migração (Identity + REFRESH_TOKEN) na sua instância:
```
db/sqlserver/migrations/0001_az-12094_identity_and_refresh_token.sql
```

## 4) Rodar
```powershell
dotnet run --project src/Plataforma2A.Auth.Api
```
- API em `http://localhost:5080` (Development).
- Documentação: `/scalar` (ou `/swagger`). Endpoints de auth em `/api/auth/*`.

## 5) Testes
```powershell
dotnet test Plataforma2A.Auth.slnx
```

## 6) Observabilidade — ver traces, métricas e logs (ADR-0033)
`http://localhost:4317` é o endpoint de **ingestão** OTLP (gRPC), **não** uma tela — abrir no navegador não mostra
nada. Para visualizar, suba o **dashboard** (precisa de Docker):
```powershell
docker compose -f docker-compose.observability.yml up -d
```
- **UI:** `http://localhost:18888` (traces + métricas + logs).
- Ingestão OTLP em `localhost:4317` (a API exporta para lá por padrão — `OpenTelemetry:Otlp:Endpoint`).
- Rode a API e chame alguns endpoints (`/health`, login) para gerar telemetria. Os **logs** também aparecem no
  dashboard (sink OTLP do Serilog) **além** do console.
- Parar: `docker compose -f docker-compose.observability.yml down`.

## Ambientes superiores (staging/production)
Os `appsettings.{Staging,Production}.json` trazem **placeholders** (`#{JWT_SIGNING_KEY}#`, `#{DB_CONNECTION}#`,
`#{SMTP_*}#`) que o **pipeline de deploy substitui** por variáveis de ambiente / secret store. Nada de segredo
no repositório.
