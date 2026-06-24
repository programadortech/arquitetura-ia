using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Plataforma2A.Auth.Api.Common;
using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.UseCases.Auth.ChangePassword;
using Plataforma2A.Auth.Application.UseCases.Auth.ForgotPassword;
using Plataforma2A.Auth.Application.UseCases.Auth.Login;
using Plataforma2A.Auth.Application.UseCases.Auth.RefreshToken;
using Plataforma2A.Auth.Application.UseCases.Auth.ResetPassword;

namespace Plataforma2A.Auth.Api.Controllers;

/// <summary>Autenticação e gerenciamento de senha (AZ-12094). Controller fino: despacha e mapeia Result → envelope.</summary>
[ApiController]
[Route("api/auth")]
[Tags("Autenticação")]
public sealed class AuthController(IUseCaseDispatcher dispatcher) : ControllerBase
{
    public sealed record LoginBody(string Email, string Password);
    public sealed record RefreshTokenBody(string AccessToken, string RefreshToken);
    public sealed record ChangePasswordBody(string CurrentPassword, string NewPassword, string ConfirmNewPassword);
    public sealed record ForgotPasswordBody(string Email);
    public sealed record ResetPasswordBody(string Email, string Token, string NewPassword, string ConfirmNewPassword);

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IResult> Login([FromBody] LoginBody body, CancellationToken ct)
        => (await dispatcher.SendAsync(new LoginRequest(body.Email, body.Password), ct)).ToApiResult(HttpContext);

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IResult> RefreshToken([FromBody] RefreshTokenBody body, CancellationToken ct)
        => (await dispatcher.SendAsync(new RefreshTokenRequest(body.AccessToken, body.RefreshToken), ct)).ToApiResult(HttpContext);

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IResult> ChangePassword([FromBody] ChangePasswordBody body, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }
        var result = await dispatcher.SendAsync(
            new ChangePasswordRequest(userId.Value, body.CurrentPassword, body.NewPassword, body.ConfirmNewPassword), ct);
        return result.ToApiResult(HttpContext);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IResult> ForgotPassword([FromBody] ForgotPasswordBody body, CancellationToken ct)
        => (await dispatcher.SendAsync(new ForgotPasswordRequest(body.Email), ct)).ToApiResult(HttpContext);

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IResult> ResetPassword([FromBody] ResetPasswordBody body, CancellationToken ct)
        => (await dispatcher.SendAsync(
                new ResetPasswordRequest(body.Email, body.Token, body.NewPassword, body.ConfirmNewPassword), ct))
            .ToApiResult(HttpContext);
}
