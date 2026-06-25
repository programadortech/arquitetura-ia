namespace Plataforma2ASmart.Auth.Application.UseCases.Users;

/// <summary>Dados do usuário retornados pelos casos de uso (a senha temporária nunca é exposta).</summary>
public sealed record UserResponse(
    Guid Id,
    string Name,
    string Email,
    string UserName,
    IReadOnlyCollection<string> Roles,
    bool IsActive,
    bool TemporaryPasswordGenerated);
