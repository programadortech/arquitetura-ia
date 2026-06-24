# ADR-0022: Configuração por ambiente (Development / Staging / Production)

- **Status:** Aceito
- **Data:** 2026-06-23

## Contexto
Todo serviço executável (Api, Gateway, workers) precisa de configuração diferente por ambiente — local
(localhost), homologação (staging) e produção — sem espalhar valores no código nem vazar segredos.

## Decisão
Cada projeto **executável** tem a pilha padrão de configuração do ASP.NET Core, selecionada por
`ASPNETCORE_ENVIRONMENT`:
- `appsettings.json` — base (valores não sensíveis e comuns).
- `appsettings.Development.json` — **local / localhost**.
- `appsettings.Staging.json` — **homologação**.
- `appsettings.Production.json` — **produção**.
- `Properties/launchSettings.json` — perfis de execução local (Development/Staging).

**Ordem de precedência** (maior vence): `appsettings.json` → `appsettings.{Env}.json` → **user-secrets**
(dev) → **variáveis de ambiente** → linha de comando.

**Segredos NUNCA** nesses arquivos: em dev use `dotnet user-secrets`; em staging/produção use **variáveis de
ambiente / secret store** (Key Vault, etc.). Os `appsettings.*.json` contêm apenas **placeholders** (ex.:
`#{...}#`) e valores não sensíveis. Regras em [`docs/standards/configuration.md`](../standards/configuration.md).

## Consequências
- (+) Mesma binária roda em qualquer ambiente, mudando só `ASPNETCORE_ENVIRONMENT` + segredos do ambiente.
- (+) Convenção .NET padrão; `IsDevelopment()` controla docs/exceções só no local.
- (−) Disciplina para manter os arquivos por ambiente em paridade (mesmas chaves).

## Alternativas consideradas
- Um único appsettings com if no código: frágil e propenso a vazar config de prod.
- Ambiente literal `Localhost`: perde os atalhos de `IsDevelopment()` do framework; preferimos `Development`.
