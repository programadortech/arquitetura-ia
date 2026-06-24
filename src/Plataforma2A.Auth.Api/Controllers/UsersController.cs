using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plataforma2A.Auth.Api.Common;
using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.UseCases.Users.CreateUser;
using Plataforma2A.Auth.Application.UseCases.Users.UpdateUser;

namespace Plataforma2A.Auth.Api.Controllers;

/// <summary>
/// Administração de usuários (AZ-12114). Controller fino. O cadastro (POST) é público (ADR-0027) e não aceita
/// roles/status; a edição (PUT) exige a policy "users:manage".
/// </summary>
[ApiController]
[Route("api/users")]
[Tags("Usuários")]
public sealed class UsersController(IUseCaseDispatcher dispatcher) : ControllerBase
{
    /// <summary>Role padrão atribuída no cadastro público (sem elevação).</summary>
    private const string DefaultRole = "Usuario";

    // Cadastro público: NÃO recebe roles nem isActive (definidos pelo servidor).
    public sealed record CreateUserBody(string Name, string Email, string UserName, string? Password);
    public sealed record UpdateUserBody(string Name, string Email, string UserName, string[] Roles, bool IsActive);

    [HttpPost]
    [AllowAnonymous]
    public async Task<IResult> Create([FromBody] CreateUserBody body, CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new CreateUserRequest(body.Name, body.Email, body.UserName, body.Password, [DefaultRole], IsActive: true), ct);

        // 201 Created + Location quando criado (ver docs/standards/http-status-codes.md).
        return result.IsSuccess
            ? result.ToApiResult(HttpContext, StatusCodes.Status201Created, location: $"/api/users/{result.Value!.Id}")
            : result.ToApiResult(HttpContext);
    }

    [HttpPut("{id:guid}")]
    [Authorize("users:manage")]
    public async Task<IResult> Update(Guid id, [FromBody] UpdateUserBody body, CancellationToken ct)
        => (await dispatcher.SendAsync(
                new UpdateUserRequest(id, body.Name, body.Email, body.UserName, body.Roles ?? [], body.IsActive), ct))
            .ToApiResult(HttpContext);
}
