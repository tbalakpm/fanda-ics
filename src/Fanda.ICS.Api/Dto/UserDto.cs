namespace Fanda.ICS.Api.Dto;

// Note: Excluded PasswordHash and PasswordSalt for security reasons
public record class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = default!;
    public string? ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
