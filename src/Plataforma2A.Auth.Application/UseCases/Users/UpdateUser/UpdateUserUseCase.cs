using Microsoft.Extensions.Logging;
using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.Common;
using Plataforma2A.Auth.Application.Ports.Persistence;
using Plataforma2A.Auth.Application.Ports.Users;

namespace Plataforma2A.Auth.Application.UseCases.Users.UpdateUser;

/// <summary>Edição de usuário (admin). Não altera senha. AC #6–#8.</summary>
public sealed record UpdateUserRequest(
    Guid Id,
    string Name,
    string Email,
    string UserName,
    IReadOnlyCollection<string> Roles,
    bool IsActive) : IUseCaseRequest<Result<UserResponse>>;

public sealed class UpdateUserHandler(
    IUserAdminService users,
    IUnitOfWork unitOfWork,
    ILogger<UpdateUserHandler> logger) : IUseCase<UpdateUserRequest, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> HandleAsync(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var existing = await users.FindByIdAsync(request.Id, cancellationToken);
        if (existing is null)
        {
            return Result<UserResponse>.Failure(new Error("usuario.nao_encontrado", "Usuário não encontrado", ErrorType.NotFound));
        }

        // Atualização de dados + sincronização de roles transacional (ADR-0026).
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        UpdateUserOutcome outcome;
        try
        {
            outcome = await users.UpdateAsync(
                request.Id, request.Name, request.Email, request.UserName, request.Roles, request.IsActive, cancellationToken);

            if (outcome.NotFound)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                return Result<UserResponse>.Failure(new Error("usuario.nao_encontrado", "Usuário não encontrado", ErrorType.NotFound));
            }
            if (!outcome.Succeeded)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                logger.LogWarning("Falha na edição de usuário {UserId}: {Erros}", request.Id, string.Join("; ", outcome.Errors));
                var type = outcome.Errors.Any(e => e.Contains("duplicad", StringComparison.OrdinalIgnoreCase)) ? ErrorType.Conflict : ErrorType.Validation;
                return Result<UserResponse>.Failure(new Error("usuario.edicao_invalida", string.Join("; ", outcome.Errors), type));
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        logger.LogInformation("Usuário editado {UserId}", request.Id);

        var response = new UserResponse(
            request.Id, request.Name, request.Email, request.UserName, request.Roles, request.IsActive, false);
        return Result<UserResponse>.Success(response);
    }
}
