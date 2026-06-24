using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plataforma2A.Auth.Api.Common;
using Plataforma2A.Auth.Api.Contracts.Users;
using Plataforma2A.Auth.Application.Abstractions;

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
    [HttpPost]
    [AllowAnonymous]
    public async Task<IResult> Create([FromBody] CreateUserRequest body, CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(body.ToUseCase(), ct);

        // 201 Created + Location quando criado (ver docs/standards/http-status-codes.md).
        return result.IsSuccess
            ? result.ToApiResult(HttpContext, StatusCodes.Status201Created, location: $"/api/users/{result.Value!.Id}")
            : result.ToApiResult(HttpContext);
    }

    [HttpPut("{id:guid}")]
    [Authorize("users:manage")]
    public async Task<IResult> Update(Guid id, [FromBody] UpdateUserRequest body, CancellationToken ct)
        => (await dispatcher.SendAsync(body.ToUseCase(id), ct)).ToApiResult(HttpContext);
}
