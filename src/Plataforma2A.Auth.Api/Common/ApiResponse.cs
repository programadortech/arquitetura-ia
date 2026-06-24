using System.Diagnostics;
using Plataforma2A.Auth.Application.Common;

namespace Plataforma2A.Auth.Api.Common;

/// <summary>Erro no envelope de resposta.</summary>
public sealed record ApiError(string Code, string Message, string Type);

/// <summary>Envelope padronizado de resposta da API (ver docs/standards/error-handling.md).</summary>
public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    IReadOnlyList<ApiError> Errors,
    string TraceId,
    DateTimeOffset Timestamp)
{
    public static ApiResponse<T> Ok(T data, string traceId) => new(true, data, [], traceId, DateTimeOffset.UtcNow);
    public static ApiResponse<T> Fail(IReadOnlyList<ApiError> errors, string traceId) => new(false, default, errors, traceId, DateTimeOffset.UtcNow);
}

/// <summary>Mapeia <see cref="Result"/> para o envelope + status HTTP correto.</summary>
public static class ResultExtensions
{
    public static IResult ToApiResult<T>(this Result<T> result, HttpContext http)
        => result.IsSuccess
            ? Results.Json(ApiResponse<T>.Ok(result.Value!, TraceId(http)))
            : Failure<T>(result.Errors, http);

    public static IResult ToApiResult(this Result result, HttpContext http)
        => result.IsSuccess
            ? Results.Json(ApiResponse<object?>.Ok(null, TraceId(http)))
            : Failure<object?>(result.Errors, http);

    private static IResult Failure<T>(IReadOnlyList<Error> errors, HttpContext http)
    {
        var status = errors.Count > 0 ? StatusFor(errors[0].Type) : StatusCodes.Status400BadRequest;
        var apiErrors = errors.Select(e => new ApiError(e.Code, e.Message, e.Type.ToString())).ToList();
        return Results.Json(ApiResponse<T>.Fail(apiErrors, TraceId(http)), statusCode: status);
    }

    private static int StatusFor(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status400BadRequest,
    };

    private static string TraceId(HttpContext http) => Activity.Current?.Id ?? http.TraceIdentifier;
}
