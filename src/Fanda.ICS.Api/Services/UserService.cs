using Fanda.ICS.Api.Data;
using Fanda.ICS.Api.Dto;
using Fanda.ICS.Api.Models;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fanda.ICS.Api.Services;

public interface IUserService
{
    public Task<Ok<List<UserDto>>> GetAllUsersAsync();
    public Task<Results<Ok<UserDto>, NotFound>> GetUserByIdAsync(int id);
    public Task<Results<Created<UserDto>, BadRequest>> CreateUserAsync(CreateUser user);
    public Task<Results<NoContent, NotFound>> UpdateUserAsync(int id, UserDto inputUser);
    public Task<Results<NoContent, NotFound>> DeleteUserAsync(int id);
}

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Ok<List<UserDto>>> GetAllUsersAsync() =>
          TypedResults.Ok(await _db.Users
              .Select(user => user.ToDto())
              .ToListAsync());

    public async Task<Results<Ok<UserDto>, NotFound>> GetUserByIdAsync(int id) =>
       await _db.Users.FindAsync(id)
          is User user
              ? TypedResults.Ok(user.ToDto())
              : TypedResults.NotFound();

    public async Task<Results<Created<UserDto>, BadRequest>> CreateUserAsync(CreateUser user)
    {
        var model = user.ToModel();
        _db.Users.Add(model);
        await _db.SaveChangesAsync();
        return TypedResults.Created($"/api/users/{model.Id}", model.ToDto());
    }

    public async Task<Results<NoContent, NotFound>> UpdateUserAsync(int id, UserDto inputUser)
    {
        var user = await _db.Users.FindAsync(id);

        if (user is null) return TypedResults.NotFound();

        user.Username = inputUser.Username;
        user.FirstName = inputUser.FirstName;
        user.LastName = inputUser.LastName;
        user.Email = inputUser.Email;
        user.Phone = inputUser.Phone;
        user.Role = inputUser.Role;
        user.ProfilePictureUrl = inputUser.ProfilePictureUrl;
        user.IsActive = inputUser.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, NotFound>> DeleteUserAsync(int id)
    {
        if (await _db.Users.FindAsync(id) is User user)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }
}
