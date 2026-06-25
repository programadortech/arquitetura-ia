using AppChangePassword = Plataforma2ASmart.Auth.Application.UseCases.Auth.ChangePassword;
using AppForgotPassword = Plataforma2ASmart.Auth.Application.UseCases.Auth.ForgotPassword;
using AppLogin = Plataforma2ASmart.Auth.Application.UseCases.Auth.Login;
using AppResetPassword = Plataforma2ASmart.Auth.Application.UseCases.Auth.ResetPassword;

namespace Plataforma2ASmart.Auth.Api.Contracts.Auth;

public sealed record LoginRequest(string Email, string Password)
{
    public AppLogin.LoginRequest ToUseCase() => new(Email, Password);
}

// O refresh token NÃO trafega no corpo: vai/volta por cookie httpOnly (ADR-P0003). Por isso não há RefreshTokenRequest.

/// <summary>Corpo devolvido por login/refresh: só o access token (refresh fica no cookie httpOnly).</summary>
public sealed record AuthAccessResponse(string AccessToken, DateTimeOffset ExpiresAt);

// UserId não vem do corpo (segurança) — é injetado a partir do token no controller.
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmNewPassword)
{
    public AppChangePassword.ChangePasswordRequest ToUseCase(Guid userId) => new(userId, CurrentPassword, NewPassword, ConfirmNewPassword);
}

public sealed record ForgotPasswordRequest(string Email)
{
    public AppForgotPassword.ForgotPasswordRequest ToUseCase() => new(Email);
}

public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword, string ConfirmNewPassword)
{
    public AppResetPassword.ResetPasswordRequest ToUseCase() => new(Email, Token, NewPassword, ConfirmNewPassword);
}
