using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Plataforma2A.Auth.Api.Common;
using Plataforma2A.Auth.Api.Contracts.Auth;
using Plataforma2A.Auth.Application.Abstractions;

namespace Plataforma2A.Auth.Api.Controllers;

/// <summary>Autenticação e gerenciamento de senha (AZ-12094). Controller fino: despacha e mapeia Result → envelope.</summary>
[ApiController]
[Route("api/auth")]
[Tags("Autenticação")]
public sealed class AuthController(IUseCaseDispatcher dispatcher) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IResult> Login([FromBody] LoginRequest body, CancellationToken ct)
        => (await dispatcher.SendAsync(body.ToUseCase(), ct)).ToApiResult(HttpContext);

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IResult> RefreshToken([FromBody] RefreshTokenRequest body, CancellationToken ct)
        => (await dispatcher.SendAsync(body.ToUseCase(), ct)).ToApiResult(HttpContext);

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IResult> ChangePassword([FromBody] ChangePasswordRequest body, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }
        return (await dispatcher.SendAsync(body.ToUseCase(userId.Value), ct)).ToApiResult(HttpContext);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IResult> ForgotPassword([FromBody] ForgotPasswordRequest body, CancellationToken ct)
        => (await dispatcher.SendAsync(body.ToUseCase(), ct)).ToApiResult(HttpContext);

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IResult> ResetPassword([FromBody] ResetPasswordRequest body, CancellationToken ct)
        => (await dispatcher.SendAsync(body.ToUseCase(), ct)).ToApiResult(HttpContext);
}
