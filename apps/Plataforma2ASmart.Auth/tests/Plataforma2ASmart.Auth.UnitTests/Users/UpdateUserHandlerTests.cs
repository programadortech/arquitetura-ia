using BuildingBlocks.Application.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Plataforma2ASmart.Auth.Application.Ports.Users;
using Plataforma2ASmart.Auth.Application.UseCases.Users.UpdateUser;

namespace Plataforma2ASmart.Auth.UnitTests.Users;

public class UpdateUserHandlerTests
{
    private readonly IUserAdminService _users = Substitute.For<IUserAdminService>();
    private readonly ILogger<UpdateUserHandler> _logger = Substitute.For<ILogger<UpdateUserHandler>>();

    private UpdateUserHandler CreateHandler() => new(_users, _logger);

    private static UpdateUserRequest Request() =>
        new(Guid.NewGuid(), "João Silva", "joao@2a.com", "joao", ["Supervisor"], IsActive: true);

    [Fact]
    public async Task Edicao_valida_atualiza_e_nunca_marca_senha_temporaria()
    {
        _users.UpdateAsync(Arg.Any<UpdateUserSpec>(), Arg.Any<CancellationToken>()).Returns(UserUpdateOutcome.Ok());

        var result = await CreateHandler().HandleAsync(Request(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Roles.Should().Contain("Supervisor");
        result.Value.TemporaryPasswordGenerated.Should().BeFalse();
    }

    [Fact]
    public async Task Usuario_inexistente_retorna_not_found()
    {
        _users.UpdateAsync(Arg.Any<UpdateUserSpec>(), Arg.Any<CancellationToken>())
            .Returns(UserUpdateOutcome.Fail(new UserAdminFault(UserAdminErrorCode.UserNotFound, "Usuário não encontrado")));

        var result = await CreateHandler().HandleAsync(Request(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Email_de_outro_usuario_retorna_conflict()
    {
        _users.UpdateAsync(Arg.Any<UpdateUserSpec>(), Arg.Any<CancellationToken>())
            .Returns(UserUpdateOutcome.Fail(new UserAdminFault(UserAdminErrorCode.DuplicateEmail, "E-mail já cadastrado")));

        var result = await CreateHandler().HandleAsync(Request(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Conflict);
    }
}
