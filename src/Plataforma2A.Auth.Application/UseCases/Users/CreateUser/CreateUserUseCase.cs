using Microsoft.Extensions.Logging;
using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.Common;
using Plataforma2A.Auth.Application.Ports.Email;
using Plataforma2A.Auth.Application.Ports.Persistence;
using Plataforma2A.Auth.Application.Ports.Users;

namespace Plataforma2A.Auth.Application.UseCases.Users.CreateUser;

/// <summary>Cadastro de usuário (admin). Senha opcional → gera temporária. AC #1–#5.</summary>
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
    IUnitOfWork unitOfWork,
    ILogger<CreateUserHandler> logger) : IUseCase<CreateUserRequest, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> HandleAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (await users.EmailExistsAsync(request.Email, cancellationToken))
        {
            return Result<UserResponse>.Failure(new Error("usuario.email_duplicado", "E-mail já cadastrado", ErrorType.Conflict));
        }
        if (await users.UserNameExistsAsync(request.UserName, cancellationToken))
        {
            return Result<UserResponse>.Failure(new Error("usuario.login_duplicado", "Usuário já cadastrado", ErrorType.Conflict));
        }

        // Criação + roles transacional: falha em qualquer etapa ⇒ rollback (sem usuário parcial) — ADR-0026.
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        CreateUserOutcome outcome;
        try
        {
            outcome = await users.CreateAsync(
                request.Name, request.Email, request.UserName, request.Password, request.Roles, request.IsActive, cancellationToken);

            if (!outcome.Succeeded)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                logger.LogWarning("Falha no cadastro de usuário {Email}: {Erros}", request.Email, string.Join("; ", outcome.Errors));
                // Erros do Identity aqui são tipicamente política de senha/validação.
                return Result<UserResponse>.Failure(new Error("usuario.criacao_invalida", string.Join("; ", outcome.Errors), ErrorType.Validation));
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        // E-mail de boas-vindas (só quando gerou senha temporária). Indisponibilidade NÃO falha o cadastro — só registra.
        if (outcome.TemporaryPasswordGenerated && outcome.TemporaryPassword is not null)
        {
            try
            {
                await welcomeEmail.SendWelcomeEmailAsync(request.Email, request.UserName, outcome.TemporaryPassword, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao preparar e-mail de boas-vindas para {UserId} (cadastro mantido)", outcome.UserId);
            }
        }

        logger.LogInformation("Usuário cadastrado {UserId} (senha temporária: {Temp})", outcome.UserId, outcome.TemporaryPasswordGenerated);

        var response = new UserResponse(
            outcome.UserId, request.Name, request.Email, request.UserName, request.Roles, request.IsActive, outcome.TemporaryPasswordGenerated);
        return Result<UserResponse>.Success(response);
    }
}
