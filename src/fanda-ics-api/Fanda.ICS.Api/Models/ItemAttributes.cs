using System.ComponentModel.DataAnnotations;

namespace Fanda.ICS.Api.Models;

public class ItemAttributes
{
    [Key]
    public Guid ItemId { get; set; }
    [MaxLength(50)]
    public string? Brand { get; set; }
    [MaxLength(50)]
    public string? Model { get; set; }
    [MaxLength(50)]
    public string? Material { get; set; }
    [MaxLength(50)]
    public string? Color { get; set; }
    [MaxLength(50)]
    public string? Size { get; set; }
    [MaxLength(50)]
    public string? Weight { get; set; }
    [MaxLength(50)]
    public string? Dimensions { get; set; }
    [MaxLength(50)]
    public string? CountryOfOrigin { get; set; }

    public virtual Item Item { get; set; } = default!;
}
