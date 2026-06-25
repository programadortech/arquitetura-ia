namespace Plataforma2ASmart.Auth.Api.Authorization;

/// <summary>Nomes das policies e da role administrativa de usuários (AZ-12114 / ADR-P0002).</summary>
public static class UserPolicies
{
    public const string AdminRole = "Administrador";

    /// <summary>Editar usuários — exige a role administrativa.</summary>
    public const string ManageUsers = "Users.Manage";

    /// <summary>Criar usuário — role administrativa OU bootstrap (sistema sem nenhum usuário).</summary>
    public const string CreateUser = "Users.Create";
}
