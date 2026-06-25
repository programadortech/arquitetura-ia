using BuildingBlocks.Application.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Plataforma2ASmart.Auth.Application.Ports.Email;
using Plataforma2ASmart.Auth.Application.Ports.Users;
using Plataforma2ASmart.Auth.Application.UseCases.Users.CreateUser;

namespace Plataforma2ASmart.Auth.UnitTests.Users;

public class CreateUserHandlerTests
{
    private readonly IUserAdminService _users = Substitute.For<IUserAdminService>();
    private readonly IUserWelcomeEmailSender _welcomeEmail = Substitute.For<IUserWelcomeEmailSender>();
    private readonly ILogger<CreateUserHandler> _logger = Substitute.For<ILogger<CreateUserHandler>>();

    private CreateUserHandler CreateHandler() => new(_users, _welcomeEmail, _logger);

    private static CreateUserRequest Request(string? password) =>
        new("João Silva", "joao@2a.com", "joao", password, ["Operador"], IsActive: true);

    [Fact]
    public async Task Com_senha_informada_cria_e_nao_prepara_email()
    {
        var id = Guid.NewGuid();
        _users.CreateAsync(Arg.Any<CreateUserSpec>(), Arg.Any<CancellationToken>())
            .Returns(new UserCreateOutcome(true, id, false, null, []));

        var result = await CreateHandler().HandleAsync(Request("Senha@123"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(id);
        result.Value.TemporaryPasswordGenerated.Should().BeFalse();
        await _welcomeEmail.DidNotReceive().SendWelcomeEmailAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sem_senha_gera_temporaria_e_prepara_email_de_boas_vindas()
    {
        _users.CreateAsync(Arg.Any<CreateUserSpec>(), Arg.Any<CancellationToken>())
            .Returns(new UserCreateOutcome(true, Guid.NewGuid(), true, "Temp@1234", []));

        var result = await CreateHandler().HandleAsync(Request(password: null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TemporaryPasswordGenerated.Should().BeTrue();
        await _welcomeEmail.Received(1).SendWelcomeEmailAsync(
            "joao@2a.com", "joao", "Temp@1234", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Email_duplicado_retorna_conflict()
    {
        _users.CreateAsync(Arg.Any<CreateUserSpec>(), Arg.Any<CancellationToken>())
            .Returns(UserCreateOutcome.Fail(new UserAdminFault(UserAdminErrorCode.DuplicateEmail, "E-mail já cadastrado")));

        var result = await CreateHandler().HandleAsync(Request("Senha@123"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Role_inexistente_retorna_validation_sem_preparar_email()
    {
        _users.CreateAsync(Arg.Any<CreateUserSpec>(), Arg.Any<CancellationToken>())
            .Returns(UserCreateOutcome.Fail(new UserAdminFault(UserAdminErrorCode.RoleNotFound, "Perfil informado não existe: X")));

        var result = await CreateHandler().HandleAsync(Request(password: null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        await _welcomeEmail.DidNotReceive().SendWelcomeEmailAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
