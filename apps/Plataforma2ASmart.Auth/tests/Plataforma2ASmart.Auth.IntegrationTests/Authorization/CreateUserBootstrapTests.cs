using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using Plataforma2ASmart.Auth.Api.Authorization;
using Plataforma2ASmart.Auth.Application.Ports.Users;

namespace Plataforma2ASmart.Auth.IntegrationTests.Authorization;

/// <summary>
/// Bootstrap do primeiro usuário (AZ-12114): POST /api/users é liberado sem admin enquanto não há usuários;
/// ao existir o primeiro, volta a exigir a role administrativa.
/// </summary>
public class CreateUserBootstrapTests
{
    private static AuthorizationHandlerContext ContextFor(bool isAdmin)
    {
        var identity = new ClaimsIdentity(authenticationType: "test", nameType: "name", roleType: JwtRole);
        if (isAdmin)
        {
            identity.AddClaim(new Claim(JwtRole, UserPolicies.AdminRole));
        }
        var requirement = new CreateUserRequirement();
        return new AuthorizationHandlerContext([requirement], new ClaimsPrincipal(identity), resource: null);
    }

    private const string JwtRole = "role";

    [Fact]
    public async Task Admin_pode_criar_mesmo_com_usuarios_existentes()
    {
        var users = Substitute.For<IUserAdminService>();
        users.AnyUserExistsAsync(Arg.Any<CancellationToken>()).Returns(true);
        var context = ContextFor(isAdmin: true);

        await new CreateUserAuthorizationHandler(users).HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Sem_usuarios_libera_bootstrap_para_anonimo()
    {
        var users = Substitute.For<IUserAdminService>();
        users.AnyUserExistsAsync(Arg.Any<CancellationToken>()).Returns(false);
        var context = ContextFor(isAdmin: false);

        await new CreateUserAuthorizationHandler(users).HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Com_usuarios_e_sem_admin_nega()
    {
        var users = Substitute.For<IUserAdminService>();
        users.AnyUserExistsAsync(Arg.Any<CancellationToken>()).Returns(true);
        var context = ContextFor(isAdmin: false);

        await new CreateUserAuthorizationHandler(users).HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}
