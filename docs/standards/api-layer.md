# Padrão: Camada de API (estilo, composição e SRP)

> Vinculante. Codifica o [ADR-0028](../adr/0028-padroes-camada-api.md). Complementa
> [arquitetura](architecture.md), [tratamento de erros](error-handling.md) e
> [status codes](http-status-codes.md).

## 1. Estilo de API — plugável (default **Controllers**)
O `/create-project` aceita `api: controllers | minimal` (**default `controllers`**).

### Controllers (default)
- `[ApiController]` + `ControllerBase`, roteamento por **atributo** (`[Route("api/users")]`, `[HttpPost]`).
- Um controller por recurso/agregado (`UsersController`, `AuthController`). **Sem** lógica de negócio.
- Injeta **apenas** `IUseCaseDispatcher` (e, se necessário, helpers de borda). Cada action:
  1. recebe o body/route, 2. monta o `*Request`, 3. `await dispatcher.SendAsync(...)`, 4. `return result.ToApiResult(...)`.

### Minimal (opcional)
- `MapGroup("/api/...")` + `Map{Post,Put,...}`, um arquivo de endpoints por recurso (`UserEndpoints`),
  registrado por extension (`MapUserEndpoints`). Mesmas regras de "fino" e despacho via dispatcher.

> O **estilo não vaza** para Application/Domain/Infrastructure — muda só a borda. As demais camadas são idênticas.

## 2. SRP / SOLID (uma responsabilidade por classe)
- **Controller/endpoint:** fino — traduz HTTP ↔ caso de uso. Nada de regra, acesso a dados ou orquestração.
- **Handler (`IUseCase<,>`):** **um** caso de uso. Não acumula múltiplas operações.
- **Service/adapter (Infrastructure):** uma responsabilidade técnica (ex.: `JwtTokenGenerator` só gera token).
- **Mappers estáticos** (`ToResponse`/`ToEntity`) — sem AutoMapper ([ADR-0021](../adr/0021-no-automapper-static-mappers.md)).
- Métodos curtos e coesos; sem classes "faz-tudo" (`*Manager`/`*Helper` genéricos). Nome diz a responsabilidade.

## 3. `Program.cs` enxuto via extension methods
O `Program.cs` **apenas orquestra**; o registro e o pipeline ficam em extensões agrupadas por preocupação,
em `src/<Produto>.Api/Extensions/`:

- **Serviços** (`IServiceCollection`): `AddApiServices` (controllers/minimal + ProblemDetails + health),
  `AddObservability` (Serilog + OpenTelemetry), `AddApiDocumentation` (OpenAPI + Scalar/Swagger),
  `AddJwtAuthentication`, `AddAuthorizationPolicies`, `AddRateLimiting`.
- **Pipeline** (`WebApplication`): `UseApiPipeline` (exception handler, request logging, rate limiter, authN/Z),
  `MapApiDocumentation`, `MapApiEndpoints` (controllers: `MapControllers()`; minimal: `MapXxxEndpoints()`).

Forma alvo do `Program.cs` (poucas linhas):
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddObservability(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApiServices()
    .AddApiDocumentation()
    .AddJwtAuthentication()
    .AddAuthorizationPolicies()
    .AddRateLimiting();

var app = builder.Build();
app.UseApiPipeline();
app.MapApiDocumentation();
app.MapApiEndpoints();
app.Run();

public partial class Program; // testes de integração
```
Cada extension faz **uma** coisa e retorna `this` (encadeável). Sem blocos gigantes de configuração inline.

## 3.1 Contratos HTTP (request/response) — fora do controller
Os **DTOs de request/response** do HTTP **nunca** ficam aninhados na classe do controller. Eles vivem em
arquivos próprios sob `src/<Produto>.Api/Contracts/<Recurso>/` (ex.: `Contracts/Auth/AuthContracts.cs`,
`Contracts/Users/UserContracts.cs`).

Regras:
- **Request contract = só o que o cliente PODE enviar.** Campos definidos pelo servidor (ex.: `UserId` do token,
  `roles`/`isActive` no cadastro público) **não** entram no corpo — são injetados no controller/mapper. Evita
  *overposting* / escalada de privilégio.
- Cada contrato expõe um mapper **`ToUseCase(...)`** que devolve o `*Request` do caso de uso (Application).
  Dados de servidor entram por parâmetro do mapper (ex.: `ToUseCase(Guid userId)`). Sem AutoMapper (ADR-0021).
- **Response:** quando a forma HTTP é igual ao response do caso de uso, use-o direto no envelope (sem DTO extra).
  Crie um response em `Contracts/` **só** quando a forma divergir — também **nunca** aninhado no controller.

Controller fino consumindo o contrato:
```csharp
[HttpPost("login")]
[AllowAnonymous]
public async Task<IResult> Login([FromBody] LoginRequest body, CancellationToken ct)
    => (await dispatcher.SendAsync(body.ToUseCase(), ct)).ToApiResult(HttpContext);
```

## 4. Borda → envelope + status code
- Toda resposta usa o envelope `ApiResponse` ([error-handling](error-handling.md)).
- Status code conforme [http-status-codes.md](http-status-codes.md): **201** no create (com `Location`),
  **204** sem corpo, 200 nas demais leituras/edições; erros via `ErrorType` no `ToApiResult`.

## 5. Gate
`scripts/validate-api-conventions.ps1` (no CI e no `/review-pr`) verifica, entre outros:
- `Program.cs` enxuto (acima de um limite de linhas → falha);
- camada Api sem lógica indevida (heurística: sem `DbContext`/`UserManager`/`new HttpClient` em controllers/endpoints);
- **controllers não declaram contratos** (sem `record` de request/response dentro de `*Controller.cs` → vão para `Contracts/`);
- consistência do estilo (controllers ⇒ `MapControllers`; minimal ⇒ `Map*Endpoints`);
- presença da pasta `Extensions/` quando há composição.
Itens de julgamento (SRP fino, status code correto caso a caso) são cobertos pela revisão (`tech-lead-reviewer`).
