using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Plataforma2ASmart.Auth.Application.Ports.Users;
using Plataforma2ASmart.Auth.Infrastructure.Persistence;

namespace Plataforma2ASmart.Auth.Infrastructure.Identity;

/// <summary>
/// Adapter do ASP.NET Core Identity para <see cref="IUserAdminService"/>. Pré-valida unicidade/roles para
/// mensagens claras e envolve "criar usuário + associar roles" numa transação (atomicidade — AZ-12114).
/// </summary>
public sealed class UserAdminService(
    UserManager<ApplicationUser> users,
    RoleManager<IdentityRole<Guid>> roles,
    AppDbContext db,
    IOptions<IdentityOptions> identityOptions) : IUserAdminService
{
    private PasswordOptions PasswordPolicy => identityOptions.Value.Password;

    public async Task<UserCreateOutcome> CreateAsync(CreateUserSpec spec, CancellationToken cancellationToken)
    {
        if (await FirstInvalidRoleAsync(spec.Roles) is { } roleFault)
        {
            return UserCreateOutcome.Fail(roleFault);
        }
        if (await users.FindByEmailAsync(spec.Email) is not null)
        {
            return UserCreateOutcome.Fail(new UserAdminFault(UserAdminErrorCode.DuplicateEmail, "E-mail já cadastrado"));
        }
        if (await users.FindByNameAsync(spec.UserName) is not null)
        {
            return UserCreateOutcome.Fail(new UserAdminFault(UserAdminErrorCode.DuplicateUserName, "Usuário já cadastrado"));
        }

        var temporaryPassword = spec.Password is null ? GenerateTemporaryPassword() : null;
        var password = spec.Password ?? temporaryPassword!;
        var user = new ApplicationUser
        {
            UserName = spec.UserName,
            Email = spec.Email,
            Name = spec.Name,
            IsActive = spec.IsActive,
        };

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return UserCreateOutcome.Fail(MapCreateErrors(created));
        }

        if (spec.Roles.Count > 0)
        {
            var rolesAssigned = await users.AddToRolesAsync(user, spec.Roles);
            if (!rolesAssigned.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return UserCreateOutcome.Fail(new UserAdminFault(UserAdminErrorCode.Unknown, "Falha ao associar perfis ao usuário"));
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return new UserCreateOutcome(true, user.Id, temporaryPassword is not null, temporaryPassword, []);
    }

    public async Task<UserUpdateOutcome> UpdateAsync(UpdateUserSpec spec, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(spec.UserId.ToString());
        if (user is null)
        {
            return UserUpdateOutcome.Fail(new UserAdminFault(UserAdminErrorCode.UserNotFound, "Usuário não encontrado"));
        }
        if (await FirstInvalidRoleAsync(spec.Roles) is { } roleFault)
        {
            return UserUpdateOutcome.Fail(roleFault);
        }
        if (await users.FindByEmailAsync(spec.Email) is { } byEmail && byEmail.Id != user.Id)
        {
            return UserUpdateOutcome.Fail(new UserAdminFault(UserAdminErrorCode.DuplicateEmail, "E-mail já cadastrado"));
        }
        if (await users.FindByNameAsync(spec.UserName) is { } byName && byName.Id != user.Id)
        {
            return UserUpdateOutcome.Fail(new UserAdminFault(UserAdminErrorCode.DuplicateUserName, "Usuário já cadastrado"));
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        user.Name = spec.Name;
        user.IsActive = spec.IsActive;
        await users.SetEmailAsync(user, spec.Email);
        await users.SetUserNameAsync(user, spec.UserName);
        var updated = await users.UpdateAsync(user);
        if (!updated.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return UserUpdateOutcome.Fail(new UserAdminFault(UserAdminErrorCode.Unknown, "Falha ao atualizar o usuário"));
        }

        if (await ReconcileRolesAsync(user, spec.Roles) is { } reconcileFault)
        {
            await transaction.RollbackAsync(cancellationToken);
            return UserUpdateOutcome.Fail(reconcileFault);
        }

        await transaction.CommitAsync(cancellationToken);
        return UserUpdateOutcome.Ok();
    }

    private async Task<UserAdminFault?> ReconcileRolesAsync(ApplicationUser user, IReadOnlyCollection<string> wanted)
    {
        var current = await users.GetRolesAsync(user);
        var toRemove = current.Except(wanted).ToArray();
        var toAdd = wanted.Except(current).ToArray();

        if (toRemove.Length > 0 && !(await users.RemoveFromRolesAsync(user, toRemove)).Succeeded)
        {
            return new UserAdminFault(UserAdminErrorCode.Unknown, "Falha ao remover perfis do usuário");
        }
        if (toAdd.Length > 0 && !(await users.AddToRolesAsync(user, toAdd)).Succeeded)
        {
            return new UserAdminFault(UserAdminErrorCode.Unknown, "Falha ao associar perfis ao usuário");
        }
        return null;
    }

    private async Task<UserAdminFault?> FirstInvalidRoleAsync(IReadOnlyCollection<string> wanted)
    {
        foreach (var role in wanted)
        {
            if (!await roles.RoleExistsAsync(role))
            {
                return new UserAdminFault(UserAdminErrorCode.RoleNotFound, $"Perfil informado não existe: {role}");
            }
        }
        return null;
    }

    private static UserAdminFault MapCreateErrors(IdentityResult result)
    {
        var error = result.Errors.FirstOrDefault();
        var code = error?.Code switch
        {
            "DuplicateEmail" => UserAdminErrorCode.DuplicateEmail,
            "DuplicateUserName" => UserAdminErrorCode.DuplicateUserName,
            _ when error?.Code?.StartsWith("Password", StringComparison.Ordinal) == true => UserAdminErrorCode.PasswordPolicy,
            _ => UserAdminErrorCode.Unknown,
        };
        var message = string.Join(" ", result.Errors.Select(e => e.Description));
        return new UserAdminFault(code, string.IsNullOrWhiteSpace(message) ? "Falha ao criar o usuário" : message);
    }

    private string GenerateTemporaryPassword()
    {
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string digits = "23456789";
        const string special = "!@#$%*?-_";

        var policy = PasswordPolicy;
        var length = Math.Max(policy.RequiredLength, 12);

        var required = new List<char>();
        if (policy.RequireLowercase) required.Add(Pick(lower));
        if (policy.RequireUppercase) required.Add(Pick(upper));
        if (policy.RequireDigit) required.Add(Pick(digits));
        if (policy.RequireNonAlphanumeric) required.Add(Pick(special));

        var alphabet = lower + upper + digits + special;
        while (required.Count < length)
        {
            required.Add(Pick(alphabet));
        }

        return new string(Shuffle(required).ToArray());
    }

    private static char Pick(string set) => set[RandomNumberGenerator.GetInt32(set.Length)];

    private static IEnumerable<char> Shuffle(IList<char> chars)
    {
        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return chars;
    }
}
