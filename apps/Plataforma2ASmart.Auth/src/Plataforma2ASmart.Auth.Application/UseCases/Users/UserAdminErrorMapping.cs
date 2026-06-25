using BuildingBlocks.Application.Common;
using Plataforma2ASmart.Auth.Application.Ports.Users;

namespace Plataforma2ASmart.Auth.Application.UseCases.Users;

/// <summary>Traduz as falhas do provedor de identidade em <see cref="Error"/> de negócio (código + categoria).</summary>
internal static class UserAdminErrorMapping
{
    public static Error[] ToErrors(this IEnumerable<UserAdminFault> faults)
        => faults.Select(ToError).ToArray();

    public static Error ToError(this UserAdminFault fault)
        => new(CodeFor(fault.Code), fault.Message, TypeFor(fault.Code));

    private static ErrorType TypeFor(UserAdminErrorCode code) => code switch
    {
        UserAdminErrorCode.DuplicateEmail or UserAdminErrorCode.DuplicateUserName => ErrorType.Conflict,
        UserAdminErrorCode.UserNotFound => ErrorType.NotFound,
        _ => ErrorType.Validation,
    };

    private static string CodeFor(UserAdminErrorCode code) => code switch
    {
        UserAdminErrorCode.DuplicateEmail => "users.email_duplicado",
        UserAdminErrorCode.DuplicateUserName => "users.usuario_duplicado",
        UserAdminErrorCode.PasswordPolicy => "users.senha_politica",
        UserAdminErrorCode.RoleNotFound => "users.perfil_inexistente",
        UserAdminErrorCode.UserNotFound => "users.nao_encontrado",
        _ => "users.erro",
    };
}
