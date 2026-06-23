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

Default `scalar,swagger` (ambas em Development). Exemplo:
```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();      // /scalar
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "API v1")); // /swagger
}
```

## Regras
- O documento descreve o **envelope** (`ApiResponse<T>`) como o tipo de resposta — ver
  [`error-handling.md`](error-handling.md).
- Toda operação tem `summary`, tags por área e exemplos quando útil; DTOs anotados.
- **Exposição em produção é controlada por configuração** (default: docs só em Development). Se exposta em
  produção, proteger por auth.
- Versionar a API (`/v1`) quando houver quebra de contrato.

Selecionado/gerado pelo `/create-project`.
