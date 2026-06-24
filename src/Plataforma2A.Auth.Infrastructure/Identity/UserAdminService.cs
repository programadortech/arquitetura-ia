using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Plataforma2A.Auth.Application.Ports.Users;

namespace Plataforma2A.Auth.Infrastructure.Identity;

/// <summary>Adapter de administração de usuários sobre o ASP.NET Core Identity (AZ-12114 / ADR-0026).</summary>
public sealed class UserAdminService(
    UserManager<ApplicationUser> users,
    RoleManager<IdentityRole<Guid>> roles) : IUserAdminService
{
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
        => await users.FindByEmailAsync(email) is not null;

    public async Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken)
        => await users.FindByNameAsync(userName) is not null;

    public async Task<CreateUserOutcome> CreateAsync(
        string name, string email, string userName, string? password,
        IReadOnlyCollection<string> roles, bool isActive, CancellationToken cancellationToken)
    {
        var temporary = string.IsNullOrWhiteSpace(password);
        var effectivePassword = temporary ? GenerateTemporaryPassword() : password!;

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = email,
            Name = name,
            IsActive = isActive,
            EmailConfirmed = true,
        };

        var created = await users.CreateAsync(user, effectivePassword);
        if (!created.Succeeded)
        {
            return new CreateUserOutcome(false, Guid.Empty, false, null, Describe(created));
        }

        var roleResult = await AssignRolesAsync(user, roles);
        if (!roleResult.Succeeded)
        {
            return new CreateUserOutcome(false, Guid.Empty, false, null, Describe(roleResult));
        }

        return new CreateUserOutcome(true, user.Id, temporary, temporary ? effectivePassword : null, []);
    }

    public async Task<UserAdminInfo?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return null;
        }
        var userRoles = await users.GetRolesAsync(user);
        return new UserAdminInfo(user.Id, user.Name, user.Email ?? string.Empty, user.UserName ?? string.Empty, userRoles.ToArray(), user.IsActive);
    }

    public async Task<UpdateUserOutcome> UpdateAsync(
        Guid id, string name, string email, string userName,
        IReadOnlyCollection<string> roles, bool isActive, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return new UpdateUserOutcome(false, true, []);
        }

        user.Name = name;
        user.IsActive = isActive;
        var setEmail = await users.SetEmailAsync(user, email);
        if (!setEmail.Succeeded) { return new UpdateUserOutcome(false, false, Describe(setEmail)); }
        var setUserName = await users.SetUserNameAsync(user, userName);
        if (!setUserName.Succeeded) { return new UpdateUserOutcome(false, false, Describe(setUserName)); }

        var updated = await users.UpdateAsync(user);
        if (!updated.Succeeded) { return new UpdateUserOutcome(false, false, Describe(updated)); }

        var current = await users.GetRolesAsync(user);
        var toRemove = current.Except(roles, StringComparer.OrdinalIgnoreCase).ToArray();
        if (toRemove.Length > 0)
        {
            var removed = await users.RemoveFromRolesAsync(user, toRemove);
            if (!removed.Succeeded) { return new UpdateUserOutcome(false, false, Describe(removed)); }
        }
        var toAdd = roles.Except(current, StringComparer.OrdinalIgnoreCase).ToArray();
        var added = await AssignRolesAsync(user, toAdd);
        if (!added.Succeeded) { return new UpdateUserOutcome(false, false, Describe(added)); }

        return new UpdateUserOutcome(true, false, []);
    }

    private async Task<IdentityResult> AssignRolesAsync(ApplicationUser user, IReadOnlyCollection<string> roleNames)
    {
        if (roleNames.Count == 0)
        {
            return IdentityResult.Success;
        }
        // Garante que as roles existam antes de vincular (admin cadastra usuário com perfis).
        foreach (var role in roleNames)
        {
            if (!await roles.RoleExistsAsync(role))
            {
                var createRole = await roles.CreateAsync(new IdentityRole<Guid>(role) { Id = Guid.NewGuid() });
                if (!createRole.Succeeded) { return createRole; }
            }
        }
        return await users.AddToRolesAsync(user, roleNames);
    }

    private static IReadOnlyList<string> Describe(IdentityResult result)
        => result.Errors.Select(e => e.Description).ToArray();

    /// <summary>Senha temporária forte (garante maiúscula, minúscula, dígito e símbolo).</summary>
    private static string GenerateTemporaryPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnpqrstuvwxyz";
        const string digit = "23456789";
        const string special = "!@#$%*";
        const string all = upper + lower + digit + special;

        var chars = new List<char>
        {
            Pick(upper), Pick(lower), Pick(digit), Pick(special),
        };
        for (var i = 0; i < 8; i++)
        {
            chars.Add(Pick(all));
        }
        // Embaralha (Fisher–Yates) para não fixar a ordem das categorias.
        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return new string(chars.ToArray());

        static char Pick(string set) => set[RandomNumberGenerator.GetInt32(set.Length)];
    }
}
