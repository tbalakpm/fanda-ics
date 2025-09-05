using System.Security.Principal;

using Fanda.ICS.Api.Data;
using Fanda.ICS.Api.Dto;
using Fanda.ICS.Api.Models;
using Fanda.ICS.Api.Services;

namespace Fanda.ICS.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/users").WithTags("User");

        group.MapGet("/", async (IUserService userService) => await userService.GetAllUsersAsync())
        .WithName("GetAllUsers")
        .WithOpenApi();

        group.MapGet("/{id:int}", async (int id, IUserService userService) => await userService.GetUserByIdAsync(id))
        .WithName("GetUserById")
        .WithOpenApi();

        group.MapPost("/", async (CreateUser user, IUserService userService) => await userService.CreateUserAsync(user))
        .WithName("CreateUser")
        .WithOpenApi();

        group.MapPut("/{id:int}", async (int id, UserDto inputUser, IUserService userService) => await userService.UpdateUserAsync(id, inputUser))
        .WithName("UpdateUser")
        .WithOpenApi();

        group.MapDelete("/{id:int}", async (int id, IUserService userService) => await userService.DeleteUserAsync(id))
        .WithName("DeleteUser")
        .WithOpenApi();
    }
}
