using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Plataforma2ASmart.Auth.Infrastructure.Identity;

namespace Plataforma2ASmart.Auth.IntegrationTests.Identity;

/// <summary>
/// Regressão da AZ-12114: o gate de usuário inativo no fluxo de login da AZ-12094. Usuário inativo não autentica
/// mesmo com a senha correta. Cobre o <see cref="IdentityService.ValidateCredentialsAsync"/>.
/// </summary>
public class InactiveUserLoginGateTests
{
    private static UserManager<ApplicationUser> MockUserManager()
        => Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

    [Fact]
    public async Task Usuario_inativo_com_senha_correta_nao_autentica()
    {
        var users = MockUserManager();
        var user = new ApplicationUser { Email = "inativo@2a.com", IsActive = false };
        users.FindByEmailAsync("inativo@2a.com").Returns(user);
        users.CheckPasswordAsync(user, "correta").Returns(true);

        var result = await new IdentityService(users).ValidateCredentialsAsync("inativo@2a.com", "correta", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Usuario_ativo_com_senha_correta_autentica()
    {
        var users = MockUserManager();
        var user = new ApplicationUser { Email = "ativo@2a.com", IsActive = true };
        users.FindByEmailAsync("ativo@2a.com").Returns(user);
        users.CheckPasswordAsync(user, "correta").Returns(true);
        users.GetRolesAsync(user).Returns(new List<string> { "Operador" });

        var result = await new IdentityService(users).ValidateCredentialsAsync("ativo@2a.com", "correta", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Email.Should().Be("ativo@2a.com");
    }
}
