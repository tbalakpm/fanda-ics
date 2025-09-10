namespace Fanda.ICS.Api.Models;

public class Unit
{
    public Guid Id { get; set; }
    public string? ShortName { get; set; }
    public string UnitName { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Item> Items { get; set; } = default!;
}
