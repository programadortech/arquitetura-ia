using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Plataforma2A.Auth.Application.Common;
using Plataforma2A.Auth.Application.Ports.Email;
using Plataforma2A.Auth.Application.Ports.Persistence;
using Plataforma2A.Auth.Application.Ports.Users;
using Plataforma2A.Auth.Application.UseCases.Users.CreateUser;

namespace Plataforma2A.Auth.UnitTests.Users;

public class CreateUserHandlerTests
{
    private readonly IUserAdminService _users = Substitute.For<IUserAdminService>();
    private readonly IUserWelcomeEmailSender _welcome = Substitute.For<IUserWelcomeEmailSender>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ILogger<CreateUserHandler> _logger = Substitute.For<ILogger<CreateUserHandler>>();

    private CreateUserHandler CreateHandler() => new(_users, _welcome, _uow, _logger);

    private static CreateUserRequest Req(string? password) =>
        new("João Silva", "joao@2a.com", "joao", password, ["Operador"], true);

    [Fact]
    public async Task Email_duplicado_retorna_conflict_sem_criar()
    {
        _users.EmailExistsAsync("joao@2a.com", Arg.Any<CancellationToken>()).Returns(true);

        var result = await CreateHandler().HandleAsync(Req("Senha@123"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Conflict);
        await _users.DidNotReceive().CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_duplicado_retorna_conflict()
    {
        _users.EmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _users.UserNameExistsAsync("joao", Arg.Any<CancellationToken>()).Returns(true);

        var result = await CreateHandler().HandleAsync(Req("Senha@123"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Com_senha_informada_cria_comita_e_nao_envia_email()
    {
        var id = Guid.NewGuid();
        _users.EmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _users.UserNameExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _users.CreateAsync("João Silva", "joao@2a.com", "joao", "Senha@123", Arg.Any<IReadOnlyCollection<string>>(), true, Arg.Any<CancellationToken>())
            .Returns(new CreateUserOutcome(true, id, false, null, []));

        var result = await CreateHandler().HandleAsync(Req("Senha@123"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TemporaryPasswordGenerated.Should().BeFalse();
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _welcome.DidNotReceive().SendWelcomeEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sem_senha_gera_temporaria_e_envia_email_de_boas_vindas()
    {
        var id = Guid.NewGuid();
        _users.EmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _users.UserNameExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _users.CreateAsync("João Silva", "joao@2a.com", "joao", null, Arg.Any<IReadOnlyCollection<string>>(), true, Arg.Any<CancellationToken>())
            .Returns(new CreateUserOutcome(true, id, true, "Tmp@9aZx1!", []));

        var result = await CreateHandler().HandleAsync(Req(null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TemporaryPasswordGenerated.Should().BeTrue();
        await _welcome.Received(1).SendWelcomeEmailAsync("joao@2a.com", "joao", "Tmp@9aZx1!", Arg.Any<CancellationToken>());
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Falha_de_criacao_faz_rollback_e_retorna_validation()
    {
        _users.EmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _users.UserNameExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _users.CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new CreateUserOutcome(false, Guid.Empty, false, null, ["A senha deve ter ao menos 8 caracteres."]));

        var result = await CreateHandler().HandleAsync(Req("123"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        await _uow.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}
