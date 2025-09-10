using System.Security.Claims;
using Fanda.UserManagement.Api.Models;
using Fanda.UserManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fanda.UserManagement.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var userGroup = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi()
            .RequireAuthorization();

        // Current user
        userGroup.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Get current authenticated user")
            .Produces<ApiResponse<UserDto>>(200);

        // User CRUD operations
        userGroup.MapGet("/", GetUsers)
            .WithName("GetUsers")
            .WithSummary("Get paginated list of users")
            .Produces<ApiResponse<PaginatedUsersResponse>>(200);

        userGroup.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Get user by ID")
            .Produces<ApiResponse<UserDto>>(200)
            .Produces<ApiResponse<UserDto>>(404);

        userGroup.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create a new user")
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"))
            .Produces<ApiResponse<UserDto>>(201)
            .Produces<ApiResponse<UserDto>>(400);

        userGroup.MapPut("/{id:guid}", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Update an existing user")
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"))
            .Produces<ApiResponse<UserDto>>(200)
            .Produces<ApiResponse<UserDto>>(400)
            .Produces<ApiResponse<UserDto>>(404);

        userGroup.MapDelete("/{id:guid}", DeleteUser)
            .WithName("DeleteUser")
            .WithSummary("Delete a user")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .Produces<ApiResponse<object>>(200)
            .Produces<ApiResponse<object>>(404);

        // Role management
        userGroup.MapGet("/{id:guid}/roles", GetUserRoles)
            .WithName("GetUserRoles")
            .WithSummary("Get user's roles")
            .Produces<ApiResponse<List<string>>>(200)
            .Produces<ApiResponse<List<string>>>(404);

        userGroup.MapPost("/{id:guid}/roles/{roleName}", AssignRole)
            .WithName("AssignRole")
            .WithSummary("Assign a role to user")
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"))
            .Produces<ApiResponse<object>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(404);

        userGroup.MapDelete("/{id:guid}/roles/{roleName}", RemoveRole)
            .WithName("RemoveRole")
            .WithSummary("Remove a role from user")
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"))
            .Produces<ApiResponse<object>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(404);
    }

    private static async Task<IResult> GetCurrentUser(
        ClaimsPrincipal user,
        [FromServices] IUserService userService)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await userService.GetUserByIdAsync(userId);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> GetUsers(
        [AsParameters] UserQueryParams queryParams,
        [FromServices] IUserService userService)
    {
        var result = await userService.GetUsersAsync(queryParams);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetUserById(
        [FromRoute] Guid id,
        [FromServices] IUserService userService)
    {
        var result = await userService.GetUserByIdAsync(id);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> CreateUser(
        [FromBody] CreateUserRequest request,
        [FromServices] IUserService userService)
    {
        var result = await userService.CreateUserAsync(request);
        return result.Success ? Results.Created($"/api/users/{result.Data?.Id}", result) : Results.BadRequest(result);
    }

    private static async Task<IResult> UpdateUser(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRequest request,
        [FromServices] IUserService userService)
    {
        var result = await userService.UpdateUserAsync(id, request);
        return result.Success ? Results.Ok(result) : 
               result.Message.Contains("not found") ? Results.NotFound(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> DeleteUser(
        [FromRoute] Guid id,
        [FromServices] IUserService userService)
    {
        var result = await userService.DeleteUserAsync(id);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> GetUserRoles(
        [FromRoute] Guid id,
        [FromServices] IUserService userService)
    {
        var result = await userService.GetUserRolesAsync(id);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> AssignRole(
        [FromRoute] Guid id,
        [FromRoute] string roleName,
        [FromServices] IUserService userService)
    {
        var result = await userService.AssignRoleAsync(id, roleName);
        return result.Success ? Results.Ok(result) : 
               result.Message.Contains("not found") ? Results.NotFound(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> RemoveRole(
        [FromRoute] Guid id,
        [FromRoute] string roleName,
        [FromServices] IUserService userService)
    {
        var result = await userService.RemoveRoleAsync(id, roleName);
        return result.Success ? Results.Ok(result) : 
               result.Message.Contains("not found") ? Results.NotFound(result) : Results.BadRequest(result);
    }
}
