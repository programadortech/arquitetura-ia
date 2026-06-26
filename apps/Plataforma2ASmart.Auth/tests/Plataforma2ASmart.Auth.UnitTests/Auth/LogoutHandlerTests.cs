using BuildingBlocks.Application.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Plataforma2ASmart.Auth.Application.Ports.Authentication;
using Plataforma2ASmart.Auth.Application.UseCases.Auth.Logout;

namespace Plataforma2ASmart.Auth.UnitTests.Auth;

public class LogoutHandlerTests
{
    private readonly IRefreshTokenStore _refreshTokens = Substitute.For<IRefreshTokenStore>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ILogger<LogoutHandler> _logger = Substitute.For<ILogger<LogoutHandler>>();

    [Fact]
    public async Task Logout_revoga_refresh_token_e_comita()
    {
        var handler = new LogoutHandler(_refreshTokens, _uow, _logger);

        var result = await handler.HandleAsync(new LogoutRequest("token-atual"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _refreshTokens.Received(1).RevokeAsync("token-atual", Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
