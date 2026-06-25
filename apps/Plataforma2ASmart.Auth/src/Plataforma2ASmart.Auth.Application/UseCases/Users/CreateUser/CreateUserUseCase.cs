using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Common;
using Microsoft.Extensions.Logging;
using Plataforma2ASmart.Auth.Application.Ports.Email;
using Plataforma2ASmart.Auth.Application.Ports.Users;

namespace Plataforma2ASmart.Auth.Application.UseCases.Users.CreateUser;

/// <summary>Cadastra um usuário (com senha informada ou temporária gerada). AC #1–#5, #10–#12.</summary>
public sealed record CreateUserRequest(
    string Name,
    string Email,
    string UserName,
    string? Password,
    IReadOnlyCollection<string> Roles,
    bool IsActive) : IUseCaseRequest<Result<UserResponse>>;

public sealed class CreateUserHandler(
    IUserAdminService users,
    IUserWelcomeEmailSender welcomeEmail,
    ILogger<CreateUserHandler> logger) : IUseCase<CreateUserRequest, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> HandleAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Tentativa de cadastro de usuário {UserName}", request.UserName);

        var spec = new CreateUserSpec(request.Name, request.Email, request.UserName, request.Password, request.Roles, request.IsActive);
        var outcome = await users.CreateAsync(spec, cancellationToken);
        if (!outcome.Succeeded)
        {
            logger.LogWarning("Falha no cadastro de usuário {UserName}", request.UserName);
            return Result<UserResponse>.Failure(outcome.Faults.ToErrors());
        }

        if (outcome.TemporaryPasswordGenerated && outcome.TemporaryPassword is not null)
        {
            await PrepareWelcomeEmailAsync(request, outcome.TemporaryPassword, cancellationToken);
        }

        logger.LogInformation("Usuário {UserId} cadastrado", outcome.UserId);
        return Result<UserResponse>.Success(new UserResponse(
            outcome.UserId, request.Name, request.Email, request.UserName, request.Roles, request.IsActive,
            outcome.TemporaryPasswordGenerated));
    }

    // O usuário já está criado; indisponibilidade do e-mail não deve falhar o cadastro (AZ-12114).
    private async Task PrepareWelcomeEmailAsync(CreateUserRequest request, string temporaryPassword, CancellationToken cancellationToken)
    {
        try
        {
            await welcomeEmail.SendWelcomeEmailAsync(request.Email, request.UserName, temporaryPassword, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao preparar e-mail de boas-vindas para {UserName}", request.UserName);
        }
    }
}
