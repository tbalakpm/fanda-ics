using Microsoft.AspNetCore.Identity;

namespace Fanda.UserManagement.Api.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Predefined roles
    public static class Names
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Supervisor = "Supervisor";
        public const string Staff = "Staff";
        public const string Member = "Member";
        public const string User = "User";
        public const string Guest = "Guest";

        public static readonly string[] All = 
        {
            Admin, Manager, Supervisor, Staff, Member, User, Guest
        };
    }
}
