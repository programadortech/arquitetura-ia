using Plataforma2A.Auth.Api.Common;
using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.UseCases.Users.CreateUser;
using Plataforma2A.Auth.Application.UseCases.Users.UpdateUser;

namespace Plataforma2A.Auth.Api.Endpoints;

/// <summary>Endpoints de administração de usuários (AZ-12114). Exigem a policy "users:manage".</summary>
public static class UserEndpoints
{
    public sealed record CreateUserBody(
        string Name, string Email, string UserName, string? Password, string[] Roles, bool IsActive);

    public sealed record UpdateUserBody(
        string Name, string Email, string UserName, string[] Roles, bool IsActive);

    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Usuários")
            .RequireAuthorization("users:manage");

        group.MapPost("/", async (CreateUserBody body, IUseCaseDispatcher dispatcher, HttpContext http, CancellationToken ct) =>
        {
            var result = await dispatcher.SendAsync(
                new CreateUserRequest(body.Name, body.Email, body.UserName, body.Password, body.Roles ?? [], body.IsActive), ct);
            return result.ToApiResult(http);
        })
        .WithSummary("Cadastra um usuário (senha opcional — gera temporária se ausente).");

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
