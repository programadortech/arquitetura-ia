# Padrão: Configuração por Ambiente

Todo projeto **executável** (Api, Gateway, workers) tem a pilha de configuração padrão do ASP.NET Core,
por ambiente. Ver [ADR-0022](../adr/0022-per-environment-configuration.md).

## Arquivos (gerados em cada projeto executável)
```
src/<Produto>.Api/
├── appsettings.json                 # base — comum, não sensível
├── appsettings.Development.json     # local / localhost
├── appsettings.Staging.json         # homologação
├── appsettings.Production.json      # produção
└── Properties/launchSettings.json   # perfis de execução local
```

## Seleção e precedência
- Ambiente via `ASPNETCORE_ENVIRONMENT` = `Development | Staging | Production`.
- Precedência (maior vence): `appsettings.json` → `appsettings.{Env}.json` → **user-secrets** (dev) →
  **variáveis de ambiente** → linha de comando.
- `IsDevelopment()` liga OpenAPI/Scalar/Swagger e a página de exceção **apenas no local**.

## Segredos — nunca nos arquivos
- **Dev:** `dotnet user-secrets` (não vai para o git).
- **Staging/Produção:** **variáveis de ambiente / secret store** (Key Vault, AWS Secrets Manager, etc.).
- Os `appsettings.*.json` versionados contêm só **placeholders**/valores não sensíveis:
  ```jsonc
  // appsettings.Production.json (sem segredos — injetados por env/secret store)
  {
    "ConnectionStrings": { "Default": "#{DB_CONNECTION}#" },
    "Jwt": { "Issuer": "Plataforma2A", "Key": "#{JWT_KEY}#" },
    "OpenTelemetry": { "Otlp": { "Endpoint": "#{OTLP_ENDPOINT}#" } }
  }
  ```

## O que muda por ambiente (exemplos)
| Chave | Development (localhost) | Staging | Production |
|---|---|---|---|
| Connection string | banco local | banco de homolog | banco de prod (via secret) |
| Logging mínimo | `Debug` | `Information` | `Warning` |
| OpenAPI/Swagger | ligado | ligado (protegido) | **desligado** (ou protegido) |
| OTLP endpoint | local | collector de staging | collector de prod |
| Rate limit | folgado | realista | realista |

## launchSettings.json (execução local)
Perfis para rodar localmente em `Development` (default) e `Staging`:
```jsonc
{
  "profiles": {
    "Development": { "commandName": "Project", "environmentVariables": { "ASPNETCORE_ENVIRONMENT": "Development" }, "applicationUrl": "http://localhost:5080" },
    "Staging":     { "commandName": "Project", "environmentVariables": { "ASPNETCORE_ENVIRONMENT": "Staging" } }
  }
}
```
> Produção **não** tem perfil local; roda com `ASPNETCORE_ENVIRONMENT=Production` + segredos do ambiente.

## Regras
- Mesmas **chaves** em todos os ambientes (paridade) — só os valores mudam.
- Nenhum segredo versionado (o `.gitignore` já ignora `appsettings.Secrets.json`/`*.local.json`).
- Bibliotecas (Domain/Application/Infrastructure) **não** têm appsettings — só projetos executáveis.

Gerado pelo `/create-project` para cada projeto executável.
