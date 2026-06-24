# Padrão: Documentação de API (OpenAPI plugável)

Documento OpenAPI **nativo do .NET 10** como fonte única, com UI **plugável**. Ver
[ADR-0015](../adr/0015-pluggable-api-documentation.md). Default do template: **Scalar + Swagger**.

## Base — OpenAPI nativo
```csharp
builder.Services.AddOpenApi();   // Microsoft.AspNetCore.OpenApi
// ...
app.MapOpenApi();                // expõe /openapi/v1.json
```

## UIs selecionáveis (parâmetro `apidocs` do /create-project)
| Opção | Pacote | Rota | Notas |
|---|---|---|---|
| **scalar** | `Scalar.AspNetCore` | `/scalar` | UI moderna; default |
| **swagger** | `Swashbuckle.AspNetCore.SwaggerUI` | `/swagger` | Clássico, difundido |
| **redoc** | `Swashbuckle.AspNetCore.ReDoc` | `/redoc` | Documentação estática elegante |

Default `scalar,swagger` (ambas fora de produção). Configuração padrão (gerada pelo `/create-project`):
```csharp
if (!app.Environment.IsProduction())
{
    app.MapOpenApi();                       // documento: /openapi/v1.json
    app.MapScalarApiReference();            // UI Scalar: /scalar
    app.UseSwaggerUI(o =>                   // UI Swagger: /swagger
    {
        o.SwaggerEndpoint("/openapi/v1.json", "<Produto> v1"); // aponta para o OpenAPI NATIVO
        o.RoutePrefix = "swagger";
    });
    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription(); // utilitário, fora do OpenAPI
}
else
{
    app.MapGet("/", () => "<Produto> API").ExcludeFromDescription();
}
app.MapHealthChecks("/health").ExcludeFromDescription(); // health não é contrato de API
```
> **A doc só mostra APIs reais.** Endpoints utilitários (`/`, `/health`, redirect) usam
> `.ExcludeFromDescription()` para **não** poluir o OpenAPI. Num projeto recém-criado o documento vem com
> `"paths": {}` (vazio) — os endpoints aparecem conforme as features são implementadas (`/create-usecase`).
> **Importante (evita o erro "Failed to fetch /swagger/v1/swagger.json"):** o Swagger UI deve apontar para
> o documento do **OpenAPI nativo** (`/openapi/v1.json`), **não** para o caminho default do Swashbuckle
> (`/swagger/v1/swagger.json`), que só existe com `AddSwaggerGen`. Como usamos `AddOpenApi()` (nativo),
> sempre configure `SwaggerEndpoint("/openapi/v1.json", ...)`.

## Regras
- O documento descreve o **envelope** (`ApiResponse<T>`) como o tipo de resposta — ver
  [`error-handling.md`](error-handling.md).
- Toda operação tem `summary`, tags por área e exemplos quando útil; DTOs anotados.
- **Exposição em produção é controlada por configuração** (default: docs só em Development). Se exposta em
  produção, proteger por auth.
- Versionar a API (`/v1`) quando houver quebra de contrato.

Selecionado/gerado pelo `/create-project`.
