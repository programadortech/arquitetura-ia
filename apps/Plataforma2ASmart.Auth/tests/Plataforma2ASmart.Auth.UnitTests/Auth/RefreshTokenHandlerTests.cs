using BuildingBlocks.Application.Common;
using BuildingBlocks.Application.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Plataforma2ASmart.Auth.Application.Ports.Authentication;
using Plataforma2ASmart.Auth.Application.UseCases.Auth.RefreshToken;

namespace Plataforma2ASmart.Auth.UnitTests.Auth;

public class RefreshTokenHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IJwtTokenGenerator _jwt = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenStore _refreshTokens = Substitute.For<IRefreshTokenStore>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ILogger<RefreshTokenHandler> _logger = Substitute.For<ILogger<RefreshTokenHandler>>();

    private RefreshTokenHandler CreateHandler() => new(_identity, _jwt, _refreshTokens, _uow, _logger);

    [Fact]
    public async Task Refresh_valido_rotaciona_atomicamente()
    {
        var userId = Guid.NewGuid();
        var user = new IdentityUserInfo(userId, "user@2a.com", []);
        _refreshTokens.ValidateAsync("antigo", Arg.Any<CancellationToken>()).Returns(userId);
        _identity.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _jwt.Generate(userId, user.Email, user.Roles).Returns(new AccessToken("novo", DateTimeOffset.UtcNow.AddMinutes(15)));
        _refreshTokens.IssueAsync(userId, Arg.Any<CancellationToken>()).Returns(new RefreshTokenIssued("novo-r", DateTimeOffset.UtcNow.AddDays(7)));

        var result = await CreateHandler().HandleAsync(new RefreshTokenRequest("acc", "antigo"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _refreshTokens.Received(1).RevokeAsync("antigo", Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Refresh_invalido_retorna_unauthorized()
    {
        _refreshTokens.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Guid?)null);

        var result = await CreateHandler().HandleAsync(new RefreshTokenRequest("a", "invalido"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unauthorized);
        await _refreshTokens.DidNotReceive().IssueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
