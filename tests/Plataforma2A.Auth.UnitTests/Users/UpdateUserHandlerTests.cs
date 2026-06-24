using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Plataforma2A.Auth.Application.Common;
using Plataforma2A.Auth.Application.Ports.Persistence;
using Plataforma2A.Auth.Application.Ports.Users;
using Plataforma2A.Auth.Application.UseCases.Users.UpdateUser;

namespace Plataforma2A.Auth.UnitTests.Users;

public class UpdateUserHandlerTests
{
    private readonly IUserAdminService _users = Substitute.For<IUserAdminService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ILogger<UpdateUserHandler> _logger = Substitute.For<ILogger<UpdateUserHandler>>();

    private UpdateUserHandler CreateHandler() => new(_users, _uow, _logger);

    private static UpdateUserRequest Req(Guid id) =>
        new(id, "João Silva", "joao@2a.com", "joao", ["Supervisor"], true);

    [Fact]
    public async Task Usuario_inexistente_retorna_not_found_sem_transacao()
    {
        var id = Guid.NewGuid();
        _users.FindByIdAsync(id, Arg.Any<CancellationToken>()).Returns((UserAdminInfo?)null);

        var result = await CreateHandler().HandleAsync(Req(id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        await _uow.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Edicao_bem_sucedida_comita()
    {
        var id = Guid.NewGuid();
        _users.FindByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(new UserAdminInfo(id, "Antigo", "joao@2a.com", "joao", ["Operador"], true));
        _users.UpdateAsync(id, "João Silva", "joao@2a.com", "joao", Arg.Any<IReadOnlyCollection<string>>(), true, Arg.Any<CancellationToken>())
            .Returns(new UpdateUserOutcome(true, false, []));

        var result = await CreateHandler().HandleAsync(Req(id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Outcome_not_found_faz_rollback_e_retorna_not_found()
    {
        var id = Guid.NewGuid();
        _users.FindByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(new UserAdminInfo(id, "Antigo", "joao@2a.com", "joao", [], true));
        _users.UpdateAsync(id, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new UpdateUserOutcome(false, true, []));

        var result = await CreateHandler().HandleAsync(Req(id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        await _uow.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Falha_por_duplicidade_retorna_conflict()
    {
        var id = Guid.NewGuid();
        _users.FindByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(new UserAdminInfo(id, "Antigo", "joao@2a.com", "joao", [], true));
        _users.UpdateAsync(id, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new UpdateUserOutcome(false, false, ["E-mail duplicado."]));

        var result = await CreateHandler().HandleAsync(Req(id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Conflict);
        await _uow.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}
