using FluentAssertions;
using NSubstitute;
using Plataforma2A.Auth.Application.Ports.Authentication;
using Plataforma2A.Auth.Application.Ports.Email;
using Plataforma2A.Auth.Application.UseCases.Auth.ForgotPassword;

namespace Plataforma2A.Auth.UnitTests.Auth;

public class ForgotPasswordHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();

    private ForgotPasswordHandler CreateHandler() => new(_identity, _email);

    [Fact]
    public async Task Email_existente_gera_token_e_envia_email()
    {
        var user = new IdentityUserInfo(Guid.NewGuid(), "user@2a.com", []);
        _identity.FindByEmailAsync("user@2a.com", Arg.Any<CancellationToken>()).Returns(user);
        _identity.GeneratePasswordResetTokenAsync(user.UserId, Arg.Any<CancellationToken>()).Returns("reset-token");

        var result = await CreateHandler().HandleAsync(new ForgotPasswordRequest("user@2a.com"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _email.Received(1).SendAsync("user@2a.com", Arg.Any<string>(), Arg.Is<string>(b => b.Contains("reset-token")), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Email_inexistente_responde_sucesso_generico_sem_enviar_email()
    {
        _identity.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((IdentityUserInfo?)null);

        var result = await CreateHandler().HandleAsync(new ForgotPasswordRequest("nao-existe@2a.com"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _email.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
