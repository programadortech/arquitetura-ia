using Microsoft.AspNetCore.Authorization;
using Plataforma2ASmart.Auth.Application.Ports.Users;

namespace Plataforma2ASmart.Auth.Api.Authorization;

/// <summary>Requisito de criação de usuário: administrador OU bootstrap do primeiro usuário.</summary>
public sealed class CreateUserRequirement : IAuthorizationRequirement;

/// <summary>
/// Libera o cadastro quando o chamador é administrador; ou, enquanto o sistema não tem nenhum usuário,
/// libera o bootstrap do primeiro usuário (AZ-12114). Ao existir o primeiro, volta a exigir a role.
/// </summary>
public sealed class CreateUserAuthorizationHandler(IUserAdminService users)
    : AuthorizationHandler<CreateUserRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CreateUserRequirement requirement)
    {
        if (context.User.IsInRole(UserPolicies.AdminRole) || !await users.AnyUserExistsAsync(CancellationToken.None))
        {
            context.Succeed(requirement);
        }
    }
}
