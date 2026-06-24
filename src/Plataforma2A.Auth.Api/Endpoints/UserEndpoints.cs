using Plataforma2A.Auth.Api.Common;
using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.UseCases.Users.CreateUser;
using Plataforma2A.Auth.Application.UseCases.Users.UpdateUser;

namespace Plataforma2A.Auth.Api.Endpoints;

/// <summary>
/// Endpoints de usuários (AZ-12114). O cadastro (POST) é **público/anônimo** (ADR-0027): não aceita roles
/// nem status — cria com a role padrão e ativo, evitando escalada de privilégio. A edição (PUT) exige "users:manage".
/// </summary>
public static class UserEndpoints
{
    /// <summary>Role padrão atribuída no cadastro público (sem elevação).</summary>
    private const string DefaultRole = "Usuario";

    // Cadastro público: NÃO recebe roles nem isActive (definidos pelo servidor).
    public sealed record CreateUserBody(string Name, string Email, string UserName, string? Password);

    public sealed record UpdateUserBody(
        string Name, string Email, string UserName, string[] Roles, bool IsActive);

    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Usuários")
            .RequireAuthorization("users:manage");

        // Cadastro ABERTO (sem login). Servidor fixa role padrão + ativo — anônimo não escolhe perfil (ADR-0027).
        group.MapPost("/", async (CreateUserBody body, IUseCaseDispatcher dispatcher, HttpContext http, CancellationToken ct) =>
        {
            var result = await dispatcher.SendAsync(
                new CreateUserRequest(body.Name, body.Email, body.UserName, body.Password, [DefaultRole], IsActive: true), ct);
            return result.ToApiResult(http);
        })
        .WithSummary("Cadastra um usuário (público; senha opcional — gera temporária se ausente).")
        .AllowAnonymous();

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserBody body, IUseCaseDispatcher dispatcher, HttpContext http, CancellationToken ct) =>
        {
            var result = await dispatcher.SendAsync(
                new UpdateUserRequest(id, body.Name, body.Email, body.UserName, body.Roles ?? [], body.IsActive), ct);
            return result.ToApiResult(http);
        })
        .WithSummary("Edita os dados básicos e as roles do usuário (não altera senha).");

        return app;
    }
}
