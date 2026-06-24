using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Plataforma2A.Auth.Application.Common;
using Plataforma2A.Auth.Application.Ports.Authentication;
using Plataforma2A.Auth.Application.UseCases.Auth.ChangePassword;

namespace Plataforma2A.Auth.UnitTests.Auth;

public class ChangePasswordHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly ILogger<ChangePasswordHandler> _logger = Substitute.For<ILogger<ChangePasswordHandler>>();

    private ChangePasswordHandler CreateHandler() => new(_identity, _logger);

    [Fact]
    public async Task Confirmacao_divergente_retorna_validation_sem_chamar_identity()
    {
        var result = await CreateHandler().HandleAsync(
            new ChangePasswordRequest(Guid.NewGuid(), "atual", "nova123", "diferente"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        await _identity.DidNotReceive().ChangePasswordAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Senha_atual_invalida_retorna_unauthorized()
    {
        var userId = Guid.NewGuid();
        _identity.ChangePasswordAsync(userId, "atual-errada", "nova12345", Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await CreateHandler().HandleAsync(
            new ChangePasswordRequest(userId, "atual-errada", "nova12345", "nova12345"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Troca_bem_sucedida_retorna_success()
    {
        var userId = Guid.NewGuid();
        _identity.ChangePasswordAsync(userId, "atual", "nova12345", Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await CreateHandler().HandleAsync(
            new ChangePasswordRequest(userId, "atual", "nova12345", "nova12345"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
