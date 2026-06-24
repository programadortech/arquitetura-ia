using FluentAssertions;
using NSubstitute;
using Plataforma2A.Auth.Application.Common;
using Plataforma2A.Auth.Application.Ports.Authentication;
using Plataforma2A.Auth.Application.UseCases.Auth.RefreshToken;

namespace Plataforma2A.Auth.UnitTests.Auth;

public class RefreshTokenHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IJwtTokenGenerator _jwt = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenStore _refreshTokens = Substitute.For<IRefreshTokenStore>();

    private RefreshTokenHandler CreateHandler() => new(_identity, _jwt, _refreshTokens);

    [Fact]
    public async Task Refresh_valido_rotaciona_revogando_o_anterior_e_emitindo_novo()
    {
        var userId = Guid.NewGuid();
        var user = new IdentityUserInfo(userId, "user@2a.com", []);
        _refreshTokens.ValidateAsync("refresh-antigo", Arg.Any<CancellationToken>()).Returns(userId);
        _identity.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _jwt.Generate(userId, user.Email, user.Roles)
            .Returns(new AccessToken("novo-access", DateTimeOffset.UtcNow.AddMinutes(15)));
        _refreshTokens.IssueAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new RefreshTokenIssued("novo-refresh", DateTimeOffset.UtcNow.AddDays(7)));

        var result = await CreateHandler().HandleAsync(
            new RefreshTokenRequest("access-expirado", "refresh-antigo"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RefreshToken.Should().Be("novo-refresh");
        await _refreshTokens.Received(1).RevokeAsync("refresh-antigo", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Refresh_invalido_retorna_unauthorized()
    {
        _refreshTokens.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Guid?)null);

        var result = await CreateHandler().HandleAsync(
            new RefreshTokenRequest("a", "invalido"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unauthorized);
        await _refreshTokens.DidNotReceive().IssueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
