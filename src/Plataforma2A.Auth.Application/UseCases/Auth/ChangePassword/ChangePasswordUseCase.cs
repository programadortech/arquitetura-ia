using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.Common;
using Plataforma2A.Auth.Application.Ports.Authentication;

namespace Plataforma2A.Auth.Application.UseCases.Auth.ChangePassword;

/// <summary>Troca de senha do usuário autenticado. AC #6–#7.</summary>
public sealed record ChangePasswordRequest(Guid UserId, string CurrentPassword, string NewPassword, string ConfirmNewPassword)
    : IUseCaseRequest<Result<Unit>>;

public sealed class ChangePasswordHandler(IIdentityService identity) : IUseCase<ChangePasswordRequest, Result<Unit>>
{
    public async Task<Result<Unit>> HandleAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return Result<Unit>.Failure(new Error("senha.confirmacao_invalida", "A nova senha e a confirmação não conferem", ErrorType.Validation));
        }

        var changed = await identity.ChangePasswordAsync(request.UserId, request.CurrentPassword, request.NewPassword, cancellationToken);
        return changed
            ? Result<Unit>.Success(Unit.Value)
            : Result<Unit>.Failure(new Error("senha.atual_invalida", "Senha atual inválida", ErrorType.Unauthorized));
    }
}
