using System.Security.Claims;

using Fanda.UserManagement.Api.Models;
using Fanda.UserManagement.Api.Services;

using Microsoft.AspNetCore.Mvc;

namespace Fanda.UserManagement.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var authGroup = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // Public endpoints
        authGroup.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register a new user")
            .Produces<ApiResponse<AuthResponse>>(200)
            .Produces<ApiResponse<AuthResponse>>(400);

        authGroup.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login user")
            .Produces<ApiResponse<AuthResponse>>(200)
            .Produces<ApiResponse<AuthResponse>>(400);

        authGroup.MapPost("/refresh-token", RefreshToken)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .Produces<ApiResponse<AuthResponse>>(200)
            .Produces<ApiResponse<AuthResponse>>(400);

        authGroup.MapPost("/forgot-password", ForgotPassword)
            .WithName("ForgotPassword")
            .WithSummary("Request password reset")
            .Produces<ApiResponse<object>>(200)
            .Produces<ApiResponse<object>>(400);

        authGroup.MapPost("/reset-password", ResetPassword)
            .WithName("ResetPassword")
            .WithSummary("Reset password with token")
            .Produces<ApiResponse<object>>(200)
            .Produces<ApiResponse<object>>(400);

        authGroup.MapPost("/validate", ValidateToken)
            .WithName("ValidateToken")
            .WithSummary("Validate JWT token")
            .Produces(200)
            .Produces(401);

        // Protected endpoints
        authGroup.MapPost("/change-password", ChangePassword)
            .WithName("ChangePassword")
            .WithSummary("Change user password")
            .RequireAuthorization()
            .Produces<ApiResponse<object>>(200)
            .Produces<ApiResponse<object>>(400);

        authGroup.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Logout user")
            .RequireAuthorization()
            .Produces<ApiResponse<object>>(200)
            .Produces<ApiResponse<object>>(400);
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        [FromServices] IAuthService authService)
    {
        var result = await authService.RegisterAsync(request);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] IAuthService authService)
    {
        var result = await authService.LoginAsync(request);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        [FromServices] IAuthService authService)
    {
        var result = await authService.RefreshTokenAsync(request);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        [FromServices] IAuthService authService)
    {
        var result = await authService.ForgotPasswordAsync(request);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        [FromServices] IAuthService authService)
    {
        var result = await authService.ResetPasswordAsync(request);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ValidateToken(
        [FromBody] ValidateTokenRequest request,
        [FromServices] IAuthService authService)
    {
        var result = await authService.ValidateTokenAsync(request);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        [FromServices] IAuthService authService,
        ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await authService.ChangePasswordAsync(userId, request);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Logout(
        [FromBody] RefreshTokenRequest request,
        [FromServices] IAuthService authService)
    {
        var result = await authService.LogoutAsync(request.RefreshToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }
}
