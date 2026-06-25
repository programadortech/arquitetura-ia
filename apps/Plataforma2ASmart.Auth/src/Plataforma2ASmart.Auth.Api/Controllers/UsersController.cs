using BuildingBlocks.Api;
using BuildingBlocks.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plataforma2ASmart.Auth.Api.Contracts.Users;
using Plataforma2ASmart.Auth.Api.Extensions;

namespace Plataforma2ASmart.Auth.Api.Controllers;

/// <summary>Administração de usuários (AZ-12114). Controller fino: despacha e mapeia Result → envelope.</summary>
[ApiController]
[Route("api/users")]
[Tags("Usuários")]
[Authorize(Policy = ServiceCollectionExtensions.ManageUsersPolicy)]
public sealed class UsersController(IUseCaseDispatcher dispatcher) : ControllerBase
{
    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateUserRequest body, CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(body.ToUseCase(), ct);
        var location = result.IsSuccess ? $"/api/users/{result.Value!.Id}" : null;
        return result.ToApiResult(HttpContext, StatusCodes.Status201Created, location);
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> Update(Guid id, [FromBody] UpdateUserRequest body, CancellationToken ct)
        => (await dispatcher.SendAsync(body.ToUseCase(id), ct)).ToApiResult(HttpContext);
}
