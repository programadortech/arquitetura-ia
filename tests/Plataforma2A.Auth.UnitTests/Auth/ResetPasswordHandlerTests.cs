using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Plataforma2A.Auth.Application.Common;
using Plataforma2A.Auth.Application.Ports.Authentication;
using Plataforma2A.Auth.Application.Ports.Persistence;
using Plataforma2A.Auth.Application.UseCases.Auth.ResetPassword;

namespace Plataforma2A.Auth.UnitTests.Auth;

public class ResetPasswordHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IRefreshTokenStore _refreshTokens = Substitute.For<IRefreshTokenStore>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<ResetPasswordHandler> _logger = Substitute.For<ILogger<ResetPasswordHandler>>();

    private ResetPasswordHandler CreateHandler() => new(_identity, _refreshTokens, _unitOfWork, _logger);

    [Fact]
    public async Task Confirmacao_divergente_retorna_validation()
    {
        var result = await CreateHandler().HandleAsync(
            new ResetPasswordRequest("user@2a.com", "tok", "nova12345", "diferente"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Token_invalido_retorna_unauthorized()
    {
        _identity.ResetPasswordAsync("user@2a.com", "tok-ruim", "nova12345", Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await CreateHandler().HandleAsync(
            new ResetPasswordRequest("user@2a.com", "tok-ruim", "nova12345", "nova12345"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Reset_bem_sucedido_revoga_todos_os_refresh_tokens_do_usuario()
    {
        var user = new IdentityUserInfo(Guid.NewGuid(), "user@2a.com", []);
        _identity.ResetPasswordAsync("user@2a.com", "tok-ok", "nova12345", Arg.Any<CancellationToken>())
            .Returns(true);
        _identity.FindByEmailAsync("user@2a.com", Arg.Any<CancellationToken>()).Returns(user);

        var result = await CreateHandler().HandleAsync(
            new ResetPasswordRequest("user@2a.com", "tok-ok", "nova12345", "nova12345"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _refreshTokens.Received(1).RevokeAllForUserAsync(user.UserId, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
