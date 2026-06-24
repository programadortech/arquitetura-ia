namespace Plataforma2A.Auth.Application.Ports.Users;

/// <summary>Dados de um usuário para administração (sem expor tipos do ASP.NET Identity).</summary>
public sealed record UserAdminInfo(
    Guid Id,
    string Name,
    string Email,
    string UserName,
    IReadOnlyCollection<string> Roles,
    bool IsActive);

/// <summary>Resultado de criação: id, se gerou senha temporária (e qual, só em memória) e erros do provedor.</summary>
public sealed record CreateUserOutcome(
    bool Succeeded,
    Guid UserId,
    bool TemporaryPasswordGenerated,
    string? TemporaryPassword,
    IReadOnlyList<string> Errors);

/// <summary>Resultado de edição: sucesso, se o usuário não existe, e erros do provedor.</summary>
public sealed record UpdateUserOutcome(
    bool Succeeded,
    bool NotFound,
    IReadOnlyList<string> Errors);

/// <summary>
/// Porta de administração de usuários (implementada na Infrastructure sobre UserManager/RoleManager).
/// Não comita transação — o caso de uso controla via <see cref="Persistence.IUnitOfWork"/> (ADR-0026).
/// </summary>
public interface IUserAdminService
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken);

    /// <summary>Cria o usuário. Se <paramref name="password"/> for nulo/vazio, gera uma senha temporária válida.</summary>
    Task<CreateUserOutcome> CreateAsync(
        string name,
        string email,
        string userName,
        string? password,
        IReadOnlyCollection<string> roles,
        bool isActive,
        CancellationToken cancellationToken);

    Task<UserAdminInfo?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Atualiza dados básicos + roles + status. Não altera senha.</summary>
    Task<UpdateUserOutcome> UpdateAsync(
        Guid id,
        string name,
        string email,
        string userName,
        IReadOnlyCollection<string> roles,
        bool isActive,
        CancellationToken cancellationToken);
}
