namespace BuildingBlocks.Application.Common;

/// <summary>Categoria do erro — mapeada para status HTTP na Api.</summary>
public enum ErrorType { Validation, NotFound, Conflict, Unauthorized, Forbidden }

/// <summary>Erro de negócio (código estável + mensagem + categoria).</summary>
public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Validation);

/// <summary>Resultado de uma operação (sucesso/falha) sem valor.</summary>
public class Result
{
    protected Result(bool isSuccess, IReadOnlyList<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<Error> Errors { get; }

    public static Result Success() => new(true, []);
    public static Result Failure(params Error[] errors) => new(false, errors);
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
}

/// <summary>Resultado com valor em caso de sucesso.</summary>
public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, IReadOnlyList<Error> errors) : base(isSuccess, errors) => Value = value;

    public T? Value { get; }

    public static Result<T> Success(T value) => new(true, value, []);
    public static new Result<T> Failure(params Error[] errors) => new(false, default, errors);
}

/// <summary>Acumula erros durante validação/orquestração e vira um Result.Failure.</summary>
public sealed class Notification
{
    private readonly List<Error> _errors = [];

    public IReadOnlyList<Error> Errors => _errors;
    public bool HasErrors => _errors.Count > 0;

    public void Add(Error error) => _errors.Add(error);
    public void Add(string code, string message, ErrorType type = ErrorType.Validation)
        => _errors.Add(new Error(code, message, type));
}
