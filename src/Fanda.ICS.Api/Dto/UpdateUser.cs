using System;

namespace Fanda.ICS.Api.Dto;

public class UpdateUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
