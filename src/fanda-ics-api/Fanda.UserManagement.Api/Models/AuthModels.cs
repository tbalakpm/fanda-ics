using System.ComponentModel.DataAnnotations;

namespace Fanda.UserManagement.Api.Models;

// Request Models
public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    [Required, MinLength(6)]
    public string Password { get; set; } = default!;

    public bool RememberMe { get; set; } = false;
}

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    [Required, MinLength(6)]
    public string Password { get; set; } = default!;

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = default!;

    [Required, StringLength(100)]
    public string FirstName { get; set; } = default!;

    [Required, StringLength(100)]
    public string LastName { get; set; } = default!;

    [Phone]
    public string? PhoneNumber { get; set; }
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = default!;
}

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = default!;
}

public class ResetPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    public string Token { get; set; } = default!;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = default!;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = default!;
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = default!;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = default!;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = default!;
}

// Response Models
public class AuthResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = default!;
}
