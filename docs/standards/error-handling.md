# Padrão: Tratamento de Erros (Result/Notification + Envelope + Middleware)

Falhas **esperadas/de negócio** são **dados** (Result/Notification), não exceções. `throw` fica só para o
**inesperado**. A Api responde sempre com um **envelope** padronizado, e um **middleware global** captura
o que escapar. Ver [ADR-0014](../adr/0014-error-handling-result-notification.md).

## 1. Result / Notification (Application)
Casos de uso retornam `Result` / `Result<T>` em vez de lançar exceção para regras de negócio.

```csharp
public enum ErrorType { Validation, NotFound, Conflict, Unauthorized, Forbidden }

public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Validation);

public class Result
{
    public bool IsSuccess { get; }
    public IReadOnlyList<Error> Errors { get; }
    public bool IsFailure => !IsSuccess;

    public static Result Success();
    public static Result Failure(params Error[] errors);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }
    public static Result<T> Success(T value);
    public static new Result<T> Failure(params Error[] errors);
}
```

`Notification` acumula erros durante a validação/orquestração e vira um `Result.Failure(notification.Errors)`:

```csharp
var notification = new Notification();
if (string.IsNullOrWhiteSpace(request.Email))
    notification.Add(new Error("email.required", "E-mail é obrigatório"));
if (notification.HasErrors)
    return Result<XptoResponse>.Failure(notification.Errors);
```

**Regras**
- Handlers (`IUseCase<,>`) retornam `Result<T>` (ou `Result`). Nada de `throw` para falha de negócio.
- `throw` só para o **inesperado** (bug, falha de infra) — vai para o middleware global.
- `Error.Code` é estável e em `kebab.dot` (ex.: `credito.insuficiente`) — o front pode mapear i18n.

## 2. Envelope de resposta (Api)
Toda resposta (sucesso ou falha) sai no mesmo formato.

```jsonc
{
  "success": true,
  "data": { /* payload em caso de sucesso, senão null */ },
  "errors": [ { "code": "email.required", "message": "E-mail é obrigatório", "type": "Validation" } ],
  "traceId": "0HN...",
  "timestamp": "2026-06-23T12:00:00Z"
}
```

```csharp
public sealed record ApiError(string Code, string Message, string Type);
public sealed record ApiResponse<T>(bool Success, T? Data, IReadOnlyList<ApiError> Errors, string TraceId, DateTimeOffset Timestamp);
```

**Mapeamento `Result` → HTTP** (helper compartilhado, ex.: `result.ToApiResult()`):
| Resultado | Status |
|---|---|
| Success (com dado) | 200 / 201 |
| Failure `Validation` | 400 |
| Failure `Unauthorized` | 401 · `Forbidden` 403 |
| Failure `NotFound` | 404 |
| Failure `Conflict` | 409 |

`traceId` vem do `Activity.Current?.Id ?? HttpContext.TraceIdentifier` (correlaciona com a telemetria).

## 3. Middleware global de exceções
Para o **inesperado**, um `IExceptionHandler` (.NET 10) registra o erro (Serilog + traceId) e responde em
**ProblemDetails**/envelope, **sem vazar** stack trace/PII ao cliente.

```csharp
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// ...
app.UseExceptionHandler();
```

O `GlobalExceptionHandler` devolve `success:false` + um erro genérico (`code: "erro.inesperado"`) + `traceId`,
e loga o detalhe internamente.

## Resumo do fluxo
```
Falha de negócio  → Result.Failure → endpoint → ApiResponse (4xx)
Sucesso           → Result.Success → endpoint → ApiResponse (2xx)
Exceção inesperada→ throw          → middleware (IExceptionHandler) → ApiResponse/ProblemDetails (500)
```

Use cases criados via `/create-usecase` já seguem este padrão. Envelope/middleware são gerados pelo
`/create-project`.
