using Fanda.UserManagement.Api.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fanda.UserManagement.Api.Services;

public interface IUserService
{
    public Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid id);
    public Task<ApiResponse<PaginatedUsersResponse>> GetUsersAsync(UserQueryParams queryParams);
    public Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request);
    public Task<ApiResponse<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request);
    public Task<ApiResponse<object>> DeleteUserAsync(Guid id);
    public Task<ApiResponse<object>> AssignRoleAsync(Guid userId, string roleName);
    public Task<ApiResponse<object>> RemoveRoleAsync(Guid userId, string roleName);
    public Task<ApiResponse<List<string>>> GetUserRolesAsync(Guid userId);
}

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return ApiResponse<UserDto>.ErrorResult("User not found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = MapToUserDto(user, roles.ToList());

        return ApiResponse<UserDto>.SuccessResult(userDto);
    }

    public async Task<ApiResponse<PaginatedUsersResponse>> GetUsersAsync(UserQueryParams queryParams)
    {
        var query = _userManager.Users.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower) ||
                u.Email!.ToLower().Contains(searchLower));
        }

        // Apply active filter
        if (queryParams.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == queryParams.IsActive.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply sorting
        switch (queryParams.OrderBy.ToLower())
        {
            case "email":
                query = queryParams.OrderDescending
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email);
                break;
            case "firstname":
                query = queryParams.OrderDescending
                    ? query.OrderByDescending(u => u.FirstName)
                    : query.OrderBy(u => u.FirstName);
                break;
            case "lastname":
                query = queryParams.OrderDescending
                    ? query.OrderByDescending(u => u.LastName)
                    : query.OrderBy(u => u.LastName);
                break;
            default:
                query = queryParams.OrderDescending
                    ? query.OrderByDescending(u => u.CreatedAt)
                    : query.OrderBy(u => u.CreatedAt);
                break;
        }

        // Apply pagination
        var users = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(MapToUserDto(user, roles.ToList()));
        }

        var response = new PaginatedUsersResponse
        {
            Users = userDtos,
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };

        return ApiResponse<PaginatedUsersResponse>.SuccessResult(response);
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponse<UserDto>.ErrorResult("User with this email already exists");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            ProfilePictureUrl = request.ProfilePictureUrl,
            IsActive = request.IsActive,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<UserDto>.ErrorResult("User creation failed", errors);
        }

        // Assign roles
        foreach (var roleName in request.RoleNames)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = MapToUserDto(user, roles.ToList());

        return ApiResponse<UserDto>.SuccessResult(userDto, "User created successfully");
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return ApiResponse<UserDto>.ErrorResult("User not found");
        }

        // Update properties
        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;

        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return ApiResponse<UserDto>.ErrorResult("Email already exists");
            }
            user.Email = request.Email;
            user.UserName = request.Email;
        }

        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;

        if (request.ProfilePictureUrl != null)
            user.ProfilePictureUrl = request.ProfilePictureUrl;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<UserDto>.ErrorResult("User update failed", errors);
        }

        // Update roles if specified
        if (request.RoleNames != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            foreach (var roleName in request.RoleNames)
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = MapToUserDto(user, roles.ToList());

        return ApiResponse<UserDto>.SuccessResult(userDto, "User updated successfully");
    }

    public async Task<ApiResponse<object>> DeleteUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return ApiResponse.Error("User not found");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse.Error("User deletion failed", errors);
        }

        return ApiResponse.Success("User deleted successfully");
    }

    public async Task<ApiResponse<object>> AssignRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponse.Error("User not found");
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            return ApiResponse.Error("Role not found");
        }

        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            return ApiResponse.Error("User already has this role");
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse.Error("Role assignment failed", errors);
        }

        return ApiResponse.Success("Role assigned successfully");
    }

    public async Task<ApiResponse<object>> RemoveRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponse.Error("User not found");
        }

        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            return ApiResponse.Error("User does not have this role");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse.Error("Role removal failed", errors);
        }

        return ApiResponse.Success("Role removed successfully");
    }

    public async Task<ApiResponse<List<string>>> GetUserRolesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponse<List<string>>.ErrorResult("User not found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return ApiResponse<List<string>>.SuccessResult(roles.ToList());
    }

    private UserDto MapToUserDto(ApplicationUser user, List<string> roles)
    {
        return new UserDto
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
            Roles = roles
        };
    }
}
