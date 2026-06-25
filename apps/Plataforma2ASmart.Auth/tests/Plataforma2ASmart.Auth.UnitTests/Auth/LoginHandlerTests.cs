using BuildingBlocks.Application.Common;
using BuildingBlocks.Application.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Plataforma2ASmart.Auth.Application.Ports.Authentication;
using Plataforma2ASmart.Auth.Application.UseCases.Auth.Login;

namespace Plataforma2ASmart.Auth.UnitTests.Auth;

public class LoginHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IJwtTokenGenerator _jwt = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenStore _refreshTokens = Substitute.For<IRefreshTokenStore>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ILogger<LoginHandler> _logger = Substitute.For<ILogger<LoginHandler>>();

    private LoginHandler CreateHandler() => new(_identity, _jwt, _refreshTokens, _uow, _logger);

    [Fact]
    public async Task Credenciais_validas_emitem_par_de_tokens_e_comitam()
    {
        var user = new IdentityUserInfo(Guid.NewGuid(), "user@2a.com", ["admin"]);
        _identity.ValidateCredentialsAsync("user@2a.com", "ok", Arg.Any<CancellationToken>()).Returns(user);
        _jwt.Generate(user.UserId, user.Email, user.Roles).Returns(new AccessToken("jwt", DateTimeOffset.UtcNow.AddMinutes(15)));
        _refreshTokens.IssueAsync(user.UserId, Arg.Any<CancellationToken>()).Returns(new RefreshTokenIssued("refresh", DateTimeOffset.UtcNow.AddDays(7)));

        var result = await CreateHandler().HandleAsync(new LoginRequest("user@2a.com", "ok"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("jwt");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Credenciais_invalidas_retornam_unauthorized_sem_emitir()
    {
        _identity.ValidateCredentialsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((IdentityUserInfo?)null);

        var result = await CreateHandler().HandleAsync(new LoginRequest("x@2a.com", "errada"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unauthorized);
        await _refreshTokens.DidNotReceive().IssueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
