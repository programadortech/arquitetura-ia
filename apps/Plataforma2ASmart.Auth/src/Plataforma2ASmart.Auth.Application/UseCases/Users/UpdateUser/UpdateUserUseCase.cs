using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Common;
using Microsoft.Extensions.Logging;
using Plataforma2ASmart.Auth.Application.Ports.Users;

namespace Plataforma2ASmart.Auth.Application.UseCases.Users.UpdateUser;

/// <summary>Edita os dados básicos e perfis de um usuário (não altera senha). AC #6–#8, #10–#12.</summary>
public sealed record UpdateUserRequest(
    Guid UserId,
    string Name,
    string Email,
    string UserName,
    IReadOnlyCollection<string> Roles,
    bool IsActive) : IUseCaseRequest<Result<UserResponse>>;

public sealed class UpdateUserHandler(
    IUserAdminService users,
    ILogger<UpdateUserHandler> logger) : IUseCase<UpdateUserRequest, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> HandleAsync(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Tentativa de edição do usuário {UserId}", request.UserId);

        var spec = new UpdateUserSpec(request.UserId, request.Name, request.Email, request.UserName, request.Roles, request.IsActive);
        var outcome = await users.UpdateAsync(spec, cancellationToken);
        if (!outcome.Succeeded)
        {
            logger.LogWarning("Falha na edição do usuário {UserId}", request.UserId);
            return Result<UserResponse>.Failure(outcome.Faults.ToErrors());
        }

        logger.LogInformation("Usuário {UserId} editado", request.UserId);
        return Result<UserResponse>.Success(new UserResponse(
            request.UserId, request.Name, request.Email, request.UserName, request.Roles, request.IsActive,
            TemporaryPasswordGenerated: false));
    }
}
