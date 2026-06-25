namespace Plataforma2ASmart.Auth.Application.Ports.Authentication;

/// <summary>Dados mínimos de um usuário, sem expor tipos do ASP.NET Identity.</summary>
public sealed record IdentityUserInfo(Guid UserId, string Email, IReadOnlyCollection<string> Roles);

/// <summary>Porta para o provedor de identidade (ASP.NET Core Identity na Infrastructure).</summary>
public interface IIdentityService
{
    Task<IdentityUserInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);
    Task<IdentityUserInfo?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<IdentityUserInfo?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
    Task<string?> GeneratePasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);
}
