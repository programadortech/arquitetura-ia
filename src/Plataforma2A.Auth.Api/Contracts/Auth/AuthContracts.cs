using AppChangePassword = Plataforma2A.Auth.Application.UseCases.Auth.ChangePassword;
using AppForgotPassword = Plataforma2A.Auth.Application.UseCases.Auth.ForgotPassword;
using AppLogin = Plataforma2A.Auth.Application.UseCases.Auth.Login;
using AppRefreshToken = Plataforma2A.Auth.Application.UseCases.Auth.RefreshToken;
using AppResetPassword = Plataforma2A.Auth.Application.UseCases.Auth.ResetPassword;

namespace Plataforma2A.Auth.Api.Contracts.Auth;

public sealed record LoginRequest(string Email, string Password)
{
    public AppLogin.LoginRequest ToUseCase() => new(Email, Password);
}

public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken)
{
    public AppRefreshToken.RefreshTokenRequest ToUseCase() => new(AccessToken, RefreshToken);
}

// UserId NÃO vem do corpo (segurança) — é injetado a partir do token no controller.
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
