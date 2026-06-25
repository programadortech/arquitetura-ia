namespace Plataforma2ASmart.Auth.Application.Ports.Users;

/// <summary>Dados para criar um usuário. <c>Password</c> nulo → o provedor gera uma senha temporária.</summary>
public sealed record CreateUserSpec(
    string Name,
    string Email,
    string UserName,
    string? Password,
    IReadOnlyCollection<string> Roles,
    bool IsActive);

/// <summary>Dados para editar um usuário (sem senha — fluxo de senha é separado).</summary>
public sealed record UpdateUserSpec(
    Guid UserId,
    string Name,
    string Email,
    string UserName,
    IReadOnlyCollection<string> Roles,
    bool IsActive);

/// <summary>Categoria de falha do provedor de identidade, mapeada para <c>ErrorType</c> pelo caso de uso.</summary>
public enum UserAdminErrorCode { DuplicateEmail, DuplicateUserName, PasswordPolicy, RoleNotFound, UserNotFound, Unknown }

/// <summary>Falha de uma operação administrativa (código + mensagem já apresentável ao cliente).</summary>
public sealed record UserAdminFault(UserAdminErrorCode Code, string Message);

/// <summary>
/// Resultado da criação. Em sucesso traz o <c>UserId</c>; quando a senha foi gerada,
/// <c>TemporaryPassword</c> existe apenas em memória para o e-mail de boas-vindas (nunca persistido/logado).
/// </summary>
public sealed record UserCreateOutcome(
    bool Succeeded,
    Guid UserId,
    bool TemporaryPasswordGenerated,
    string? TemporaryPassword,
    IReadOnlyList<UserAdminFault> Faults)
{
    public static UserCreateOutcome Fail(params UserAdminFault[] faults) => new(false, Guid.Empty, false, null, faults);
}

/// <summary>Resultado da edição.</summary>
public sealed record UserUpdateOutcome(bool Succeeded, IReadOnlyList<UserAdminFault> Faults)
{
    public static UserUpdateOutcome Fail(params UserAdminFault[] faults) => new(false, faults);
    public static UserUpdateOutcome Ok() => new(true, []);
}

/// <summary>
/// Porta de administração de usuários — encapsula o ASP.NET Core Identity (UserManager/RoleManager) sem vazar
/// seus tipos. Garante atomicidade de "criar usuário + associar roles" na Infrastructure.
/// </summary>
public interface IUserAdminService
{
    Task<UserCreateOutcome> CreateAsync(CreateUserSpec spec, CancellationToken cancellationToken);
    Task<UserUpdateOutcome> UpdateAsync(UpdateUserSpec spec, CancellationToken cancellationToken);

    /// <summary>Há algum usuário no sistema? Usado pelo bootstrap do primeiro usuário (AZ-12114).</summary>
    Task<bool> AnyUserExistsAsync(CancellationToken cancellationToken);
}
