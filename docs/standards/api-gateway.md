# Padrão: API Gateway opcional (YARP)

Quando há mais de uma API, oferecer **um único ponto de entrada** ao front via **YARP** (reverse proxy da
Microsoft). Opcional no `/create-project` (`gateway: yarp | none`, default `none`). Ver
[ADR-0017](../adr/0017-optional-api-gateway-yarp.md).

## Forma
- Projeto `src/<Produto>.Gateway` (ASP.NET Core + `Yarp.ReverseProxy`).
- Rotas/clusters por **configuração** (`appsettings`), apontando para as APIs internas.
- Responsabilidades **só de borda**: roteamento, CORS, rate limit de borda, encaminhar auth. **Sem lógica
  de negócio** no gateway.

```jsonc
// appsettings.json (gateway)
{
  "ReverseProxy": {
    "Routes": {
      "api": { "ClusterId": "api", "Match": { "Path": "/api/{**catch-all}" } }
    },
    "Clusters": {
      "api": { "Destinations": { "d1": { "Address": "http://localhost:5080/" } } }
    }
  }
}
```

```csharp
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
app.MapReverseProxy();
```

## Regras
- O front conhece **apenas o host do gateway**.
- Observabilidade no gateway (Serilog + OTel) com propagação de trace para as APIs.
- Em produção, o gateway pode ser substituído por um gerenciado (APIM/NGINX) mantendo o contrato de rotas.

Gerado pelo `/create-project` quando `gateway: yarp`.
