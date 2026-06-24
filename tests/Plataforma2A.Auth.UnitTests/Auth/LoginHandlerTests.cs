using FluentAssertions;
using NSubstitute;
using Plataforma2A.Auth.Application.Common;
using Plataforma2A.Auth.Application.Ports.Authentication;
using Plataforma2A.Auth.Application.UseCases.Auth.Login;

namespace Plataforma2A.Auth.UnitTests.Auth;

public class LoginHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IJwtTokenGenerator _jwt = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenStore _refreshTokens = Substitute.For<IRefreshTokenStore>();

    private LoginHandler CreateHandler() => new(_identity, _jwt, _refreshTokens);

    [Fact]
    public async Task Credenciais_validas_emitem_par_de_tokens()
    {
        var user = new IdentityUserInfo(Guid.NewGuid(), "user@2a.com", ["admin"]);
        _identity.ValidateCredentialsAsync("user@2a.com", "senha-ok", Arg.Any<CancellationToken>())
            .Returns(user);
        _jwt.Generate(user.UserId, user.Email, user.Roles)
            .Returns(new AccessToken("access-jwt", DateTimeOffset.UtcNow.AddMinutes(15)));
        _refreshTokens.IssueAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(new RefreshTokenIssued("refresh-raw", DateTimeOffset.UtcNow.AddDays(7)));

        var result = await CreateHandler().HandleAsync(new LoginRequest("user@2a.com", "senha-ok"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-jwt");
        result.Value.RefreshToken.Should().Be("refresh-raw");
    }

    [Fact]
    public async Task Credenciais_invalidas_retornam_unauthorized_sem_emitir_tokens()
    {
        _identity.ValidateCredentialsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((IdentityUserInfo?)null);

        var result = await CreateHandler().HandleAsync(new LoginRequest("x@2a.com", "errada"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unauthorized);
        await _refreshTokens.DidNotReceive().IssueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
