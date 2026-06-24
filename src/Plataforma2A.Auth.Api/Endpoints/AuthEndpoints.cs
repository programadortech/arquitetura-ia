using Plataforma2A.Auth.Api.Common;
using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.UseCases.Auth.ChangePassword;
using Plataforma2A.Auth.Application.UseCases.Auth.ForgotPassword;
using Plataforma2A.Auth.Application.UseCases.Auth.Login;
using Plataforma2A.Auth.Application.UseCases.Auth.RefreshToken;
using Plataforma2A.Auth.Application.UseCases.Auth.ResetPassword;

namespace Plataforma2A.Auth.Api.Endpoints;

/// <summary>Endpoints de autenticação e gerenciamento de senha (AZ-12094).</summary>
public static class AuthEndpoints
{
    // Corpos das requisições da API (o UserId do change-password vem do token, não do corpo).
    public sealed record LoginBody(string Email, string Password);
    public sealed record RefreshTokenBody(string AccessToken, string RefreshToken);
    public sealed record ChangePasswordBody(string CurrentPassword, string NewPassword, string ConfirmNewPassword);
    public sealed record ForgotPasswordBody(string Email);
    public sealed record ResetPasswordBody(string Email, string Token, string NewPassword, string ConfirmNewPassword);

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Autenticação");

        // Rate limit só nos endpoints anônimos (vetores de força bruta) — AC #11.
        group.MapPost("/login", async (LoginBody body, IUseCaseDispatcher dispatcher, HttpContext http, CancellationToken ct) =>
        {
            var result = await dispatcher.SendAsync(new LoginRequest(body.Email, body.Password), ct);
            return result.ToApiResult(http);
        })
        .WithSummary("Autentica por e-mail e senha e devolve os tokens.")
        .AllowAnonymous()
        .RequireRateLimiting("auth");

        group.MapPost("/refresh-token", async (RefreshTokenBody body, IUseCaseDispatcher dispatcher, HttpContext http, CancellationToken ct) =>
        {
            var result = await dispatcher.SendAsync(new RefreshTokenRequest(body.AccessToken, body.RefreshToken), ct);
            return result.ToApiResult(http);
        })
        .WithSummary("Renova o access token rotacionando o refresh token.")
        .AllowAnonymous();

        group.MapPost("/change-password", async (ChangePasswordBody body, IUseCaseDispatcher dispatcher, HttpContext http, CancellationToken ct) =>
        {
            var userId = http.User.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }
            var result = await dispatcher.SendAsync(
                new ChangePasswordRequest(userId.Value, body.CurrentPassword, body.NewPassword, body.ConfirmNewPassword), ct);
            return result.ToApiResult(http);
        })
        .WithSummary("Troca a senha do usuário autenticado.")
        .RequireAuthorization();

        group.MapPost("/forgot-password", async (ForgotPasswordBody body, IUseCaseDispatcher dispatcher, HttpContext http, CancellationToken ct) =>
        {
            var result = await dispatcher.SendAsync(new ForgotPasswordRequest(body.Email), ct);
            return result.ToApiResult(http);
        })
        .WithSummary("Inicia a redefinição de senha (envia token por e-mail).")
        .AllowAnonymous()
        .RequireRateLimiting("auth");

        group.MapPost("/reset-password", async (ResetPasswordBody body, IUseCaseDispatcher dispatcher, HttpContext http, CancellationToken ct) =>
        {
            var result = await dispatcher.SendAsync(
                new ResetPasswordRequest(body.Email, body.Token, body.NewPassword, body.ConfirmNewPassword), ct);
            return result.ToApiResult(http);
        })
        .WithSummary("Redefine a senha com o token recebido por e-mail.")
        .AllowAnonymous()
        .RequireRateLimiting("auth");

        return app;
    }
}
