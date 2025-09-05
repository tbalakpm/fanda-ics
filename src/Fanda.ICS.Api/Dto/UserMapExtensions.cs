using Fanda.ICS.Api.Models;

namespace Fanda.ICS.Api.Dto;

public static class UserMapExtensions
{
    public static UserDto ToDto(this User user) =>
      new()
      {
          Id = user.Id,
          Username = user.Username,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Email = user.Email,
          Phone = user.Phone,
          Role = user.Role,
          ProfilePictureUrl = user.ProfilePictureUrl,
          IsActive = user.IsActive,
          CreatedAt = user.CreatedAt,
          UpdatedAt = user.UpdatedAt
      };

    public static IEnumerable<UserDto> ToDto(this IEnumerable<User> users) =>
      users.Select(user => user.ToDto());

    public static User ToModel(this UserDto userDto) =>
      new()
      {
          Id = userDto.Id,
          Username = userDto.Username,
          FirstName = userDto.FirstName,
          LastName = userDto.LastName,
          Email = userDto.Email,
          Phone = userDto.Phone,
          Role = userDto.Role,
          ProfilePictureUrl = userDto.ProfilePictureUrl,
          IsActive = userDto.IsActive,
          CreatedAt = userDto.CreatedAt,
          UpdatedAt = userDto.UpdatedAt
      };

    public static IEnumerable<User> ToModel(this IEnumerable<UserDto> userDtos) =>
      userDtos.Select(dto => dto.ToModel());
}
