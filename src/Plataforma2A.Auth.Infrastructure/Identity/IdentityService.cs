using Microsoft.AspNetCore.Identity;
using Plataforma2A.Auth.Application.Ports.Authentication;

namespace Plataforma2A.Auth.Infrastructure.Identity;

/// <summary>Adapter do ASP.NET Core Identity para a porta <see cref="IIdentityService"/>.</summary>
public sealed class IdentityService(UserManager<ApplicationUser> users) : IIdentityService
{
    public async Task<IdentityUserInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email);
        // Usuário inexistente, inativo (AZ-12114 / ADR-0026) ou senha inválida → mesma resposta (não revela o motivo).
        if (user is null || !user.IsActive || !await users.CheckPasswordAsync(user, password))
        {
            return null;
        }
        return await ToInfoAsync(user);
    }

    public async Task<IdentityUserInfo?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email);
        return user is null ? null : await ToInfoAsync(user);
    }

    public async Task<IdentityUserInfo?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        return user is null ? null : await ToInfoAsync(user);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return false;
        }
        var result = await users.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        return user is null ? null : await users.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            return false;
        }
        var result = await users.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    private async Task<IdentityUserInfo> ToInfoAsync(ApplicationUser user)
    {
        var roles = await users.GetRolesAsync(user);
        return new IdentityUserInfo(user.Id, user.Email ?? string.Empty, roles.ToArray());
    }
}
