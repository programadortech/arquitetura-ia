using Plataforma2A.Auth.Application.Ports.Users;

namespace Plataforma2A.Auth.Application.UseCases.Users;

/// <summary>Resposta de cadastro/edição de usuário.</summary>
public sealed record UserResponse(
    Guid Id,
    string Name,
    string Email,
    string UserName,
    IReadOnlyCollection<string> Roles,
    bool IsActive,
    bool TemporaryPasswordGenerated);

/// <summary>Mappers estáticos (sem AutoMapper — ADR-0021).</summary>
public static class UserMappers
{
    public static UserResponse ToResponse(this UserAdminInfo user, bool temporaryPasswordGenerated = false)
        => new(user.Id, user.Name, user.Email, user.UserName, user.Roles, user.IsActive, temporaryPasswordGenerated);
}
