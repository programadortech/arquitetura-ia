using AppCreateUser = Plataforma2A.Auth.Application.UseCases.Users.CreateUser;
using AppUpdateUser = Plataforma2A.Auth.Application.UseCases.Users.UpdateUser;

namespace Plataforma2A.Auth.Api.Contracts.Users;

// Cadastro público (ADR-0027): o cliente não envia roles nem isActive — o servidor fixa a role padrão e ativo.
public sealed record CreateUserRequest(string Name, string Email, string UserName, string? Password)
{
    private const string DefaultRole = "Usuario";

    public AppCreateUser.CreateUserRequest ToUseCase() => new(Name, Email, UserName, Password, [DefaultRole], IsActive: true);
}

public sealed record UpdateUserRequest(string Name, string Email, string UserName, string[] Roles, bool IsActive)
{
    public AppUpdateUser.UpdateUserRequest ToUseCase(Guid id) => new(id, Name, Email, UserName, Roles ?? [], IsActive);
}
