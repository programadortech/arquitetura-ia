using AppCreateUser = Plataforma2ASmart.Auth.Application.UseCases.Users.CreateUser;
using AppUpdateUser = Plataforma2ASmart.Auth.Application.UseCases.Users.UpdateUser;

namespace Plataforma2ASmart.Auth.Api.Contracts.Users;

/// <summary>Cadastro de usuário. <c>Password</c> opcional — ausente gera senha temporária no servidor.</summary>
public sealed record CreateUserRequest(
    string Name,
    string Email,
    string UserName,
    string? Password,
    IReadOnlyCollection<string>? Roles,
    bool IsActive)
{
    public AppCreateUser.CreateUserRequest ToUseCase()
        => new(Name, Email, UserName, Password, Roles ?? [], IsActive);
}

// UserId vem da rota (não do corpo) — segue o padrão de não confiar em id do cliente no corpo.
public sealed record UpdateUserRequest(
    string Name,
    string Email,
    string UserName,
    IReadOnlyCollection<string>? Roles,
    bool IsActive)
{
    public AppUpdateUser.UpdateUserRequest ToUseCase(Guid userId)
        => new(userId, Name, Email, UserName, Roles ?? [], IsActive);
}
