using Fanda.ICS.Api.Helpers;
using Fanda.ICS.Api.Models;

namespace Fanda.ICS.Api.Dto;

public class CreateUser
{
    public string Username { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string? ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public User ToModel()
    {
        var (hashedPassword, salt) = CryptoHelper.HashPassword(this.Password);
        return new User
        {
            Id = Guid.CreateVersion7(),
            Username = this.Username,
            FirstName = this.FirstName,
            LastName = this.LastName,
            Email = this.Email,
            Phone = this.Phone,
            Role = this.Role,
            PasswordHash = hashedPassword,
            PasswordSalt = salt,
            ProfilePictureUrl = this.ProfilePictureUrl,
            IsActive = this.IsActive
        };
    }
}
