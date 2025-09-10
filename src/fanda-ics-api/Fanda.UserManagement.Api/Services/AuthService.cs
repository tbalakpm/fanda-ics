using Fanda.UserManagement.Api.Endpoints;
using Fanda.UserManagement.Api.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fanda.UserManagement.Api.Services;

public interface IAuthService
{
    public Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    public Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    public Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    public Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequest request);
    public Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequest request);
    public Task<ApiResponse<object>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    public Task<ApiResponse<object>> LogoutAsync(string refreshToken);
    public Task<bool> ValidateTokenAsync(ValidateTokenRequest request);
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IEmailService _emailService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        IEmailService emailService,
        JwtSettings jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _emailService = emailService;
        _jwtSettings = jwtSettings;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return ApiResponse<AuthResponse>.ErrorResult("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return ApiResponse<AuthResponse>.ErrorResult("Account is deactivated");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            return ApiResponse<AuthResponse>.ErrorResult("Invalid email or password");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponse<AuthResponse>.ErrorResult("User with this email already exists");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<AuthResponse>.ErrorResult("Registration failed", errors);
        }

        // Assign default User role
        await _userManager.AddToRoleAsync(user, ApplicationRole.Names.User);

        // Send welcome email
        await _emailService.SendWelcomeEmailAsync(user.Email!, user.FirstName);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _refreshTokenService.GetByTokenAsync(request.RefreshToken);
        if (refreshToken == null || !refreshToken.IsValidToken)
        {
            return ApiResponse<AuthResponse>.ErrorResult("Invalid or expired refresh token");
        }

        var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
        if (user == null || !user.IsActive)
        {
            return ApiResponse<AuthResponse>.ErrorResult("User not found or inactive");
        }

        // Revoke the used refresh token
        await _refreshTokenService.RevokeTokenAsync(request.RefreshToken);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user != null && user.IsActive)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendPasswordResetEmailAsync(user.Email!, token);
        }

        // Always return success for security (don't reveal if email exists)
        return ApiResponse.Success("If the email exists, a password reset link has been sent");
    }

    public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return ApiResponse.Error("Invalid reset token or email");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse.Error("Password reset failed", errors);
        }

        // Revoke all refresh tokens
        await _refreshTokenService.RevokeAllUserTokensAsync(user.Id);

        return ApiResponse.Success("Password reset successfully");
    }

    public async Task<ApiResponse<object>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponse.Error("User not found");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse.Error("Password change failed", errors);
        }

        return ApiResponse.Success("Password changed successfully");
    }

    public async Task<ApiResponse<object>> LogoutAsync(string refreshToken)
    {
        await _refreshTokenService.RevokeTokenAsync(refreshToken);
        return ApiResponse.Success("Logged out successfully");
    }

    private async Task<ApiResponse<AuthResponse>> GenerateAuthResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _refreshTokenService.CreateAsync(refreshTokenEntity);

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            ProfilePictureUrl = user.ProfilePictureUrl,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = roles.ToList()
        };

        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            User = userDto
        };

        return ApiResponse<AuthResponse>.SuccessResult(authResponse, "Authentication successful");
    }

    Task<bool> IAuthService.ValidateTokenAsync(ValidateTokenRequest request)
    {
        return Task.FromResult(_tokenService.ValidateToken(request.Token));
    }
}
