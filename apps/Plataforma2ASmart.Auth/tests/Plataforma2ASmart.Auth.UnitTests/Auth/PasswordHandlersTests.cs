using BuildingBlocks.Application.Common;
using BuildingBlocks.Application.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Plataforma2ASmart.Auth.Application.Ports.Authentication;
using Plataforma2ASmart.Auth.Application.Ports.Email;
using Plataforma2ASmart.Auth.Application.UseCases.Auth.ChangePassword;
using Plataforma2ASmart.Auth.Application.UseCases.Auth.ForgotPassword;
using Plataforma2ASmart.Auth.Application.UseCases.Auth.ResetPassword;

namespace Plataforma2ASmart.Auth.UnitTests.Auth;

public class PasswordHandlersTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IRefreshTokenStore _refreshTokens = Substitute.For<IRefreshTokenStore>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task ChangePassword_confirmacao_divergente_retorna_validation()
    {
        var handler = new ChangePasswordHandler(_identity, Substitute.For<ILogger<ChangePasswordHandler>>());

        var result = await handler.HandleAsync(new ChangePasswordRequest(Guid.NewGuid(), "atual", "nova123", "diferente"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task ChangePassword_senha_atual_invalida_retorna_unauthorized()
    {
        var id = Guid.NewGuid();
        _identity.ChangePasswordAsync(id, "errada", "nova12345", Arg.Any<CancellationToken>()).Returns(false);
        var handler = new ChangePasswordHandler(_identity, Substitute.For<ILogger<ChangePasswordHandler>>());

        var result = await handler.HandleAsync(new ChangePasswordRequest(id, "errada", "nova12345", "nova12345"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task ForgotPassword_email_inexistente_responde_generico_sem_enviar()
    {
        _identity.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((IdentityUserInfo?)null);
        var handler = new ForgotPasswordHandler(_identity, _email, Substitute.For<ILogger<ForgotPasswordHandler>>());

        var result = await handler.HandleAsync(new ForgotPasswordRequest("nao@2a.com"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _email.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResetPassword_sucesso_revoga_refresh_tokens_e_comita()
    {
        var user = new IdentityUserInfo(Guid.NewGuid(), "user@2a.com", []);
        _identity.FindByEmailAsync("user@2a.com", Arg.Any<CancellationToken>()).Returns(user);
        _identity.ResetPasswordAsync("user@2a.com", "tok", "nova12345", Arg.Any<CancellationToken>()).Returns(true);
        var handler = new ResetPasswordHandler(_identity, _refreshTokens, _uow, Substitute.For<ILogger<ResetPasswordHandler>>());

        var result = await handler.HandleAsync(new ResetPasswordRequest("user@2a.com", "tok", "nova12345", "nova12345"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _refreshTokens.Received(1).RevokeAllForUserAsync(user.UserId, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResetPassword_token_invalido_retorna_unauthorized()
    {
        var user = new IdentityUserInfo(Guid.NewGuid(), "user@2a.com", []);
        _identity.FindByEmailAsync("user@2a.com", Arg.Any<CancellationToken>()).Returns(user);
        _identity.ResetPasswordAsync("user@2a.com", "ruim", "nova12345", Arg.Any<CancellationToken>()).Returns(false);
        var handler = new ResetPasswordHandler(_identity, _refreshTokens, _uow, Substitute.For<ILogger<ResetPasswordHandler>>());

        var result = await handler.HandleAsync(new ResetPasswordRequest("user@2a.com", "ruim", "nova12345", "nova12345"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unauthorized);
        await _refreshTokens.DidNotReceive().RevokeAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
