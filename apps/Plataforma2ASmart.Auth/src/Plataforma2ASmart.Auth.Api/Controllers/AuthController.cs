using BuildingBlocks.Api;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Plataforma2ASmart.Auth.Api.Authentication;
using Plataforma2ASmart.Auth.Api.Common;
using Plataforma2ASmart.Auth.Api.Contracts.Auth;
using Plataforma2ASmart.Auth.Application.UseCases.Auth;
using AppLogout = Plataforma2ASmart.Auth.Application.UseCases.Auth.Logout;
using AppRefreshToken = Plataforma2ASmart.Auth.Application.UseCases.Auth.RefreshToken;

namespace Plataforma2ASmart.Auth.Api.Controllers;

/// <summary>Autenticação e gerenciamento de senha. Refresh token trafega por cookie httpOnly (ADR-P0003).</summary>
[ApiController]
[Route("api/auth")]
[Tags("Autenticação")]
public sealed class AuthController(IUseCaseDispatcher dispatcher, IOptions<RefreshCookieOptions> cookieOptions) : ControllerBase
{
    private readonly RefreshCookieOptions _cookie = cookieOptions.Value;

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IResult> Login([FromBody] LoginRequest body, CancellationToken ct)
        => CompleteAuth(await dispatcher.SendAsync(body.ToUseCase(), ct));

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IResult> RefreshToken(CancellationToken ct)
    {
        var token = Request.Cookies[_cookie.Name];
        if (string.IsNullOrEmpty(token))
        {
            return Results.Unauthorized();
        }

        var result = await dispatcher.SendAsync(new AppRefreshToken.RefreshTokenRequest(token), ct);
        if (result.IsFailure)
        {
            ClearRefreshCookie();
        }
        return CompleteAuth(result);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IResult> Logout(CancellationToken ct)
    {
        var token = Request.Cookies[_cookie.Name];
        if (!string.IsNullOrEmpty(token))
        {
            await dispatcher.SendAsync(new AppLogout.LogoutRequest(token), ct);
        }
        ClearRefreshCookie();
        return Results.NoContent();
    }

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

    // Sucesso → grava o refresh no cookie httpOnly e devolve só o access token no corpo.
    private IResult CompleteAuth(Result<AuthTokensResponse> result)
    {
        if (result.IsFailure)
        {
            return result.ToApiResult(HttpContext);
        }
        SetRefreshCookie(result.Value!.RefreshToken);
        var body = new AuthAccessResponse(result.Value.AccessToken, result.Value.ExpiresAt);
        return Result<AuthAccessResponse>.Success(body).ToApiResult(HttpContext);
    }

    private void SetRefreshCookie(string token) =>
        Response.Cookies.Append(_cookie.Name, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = _cookie.Secure,
            SameSite = _cookie.SameSite,
            Path = _cookie.Path,
            Expires = DateTimeOffset.UtcNow.AddDays(_cookie.Days),
            IsEssential = true,
        });

    private void ClearRefreshCookie() =>
        Response.Cookies.Delete(_cookie.Name, new CookieOptions
        {
            Path = _cookie.Path,
            Secure = _cookie.Secure,
            SameSite = _cookie.SameSite,
        });
}
