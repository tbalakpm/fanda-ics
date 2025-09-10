using System;
using System.ComponentModel.DataAnnotations;

namespace Fanda.ICS.Api.Models;

public class ItemEOQ
{
    [Key]
    public Guid ItemId { get; set; }
    public decimal? EconomicOrderQuantity { get; set; }
    public decimal? ReorderLevel { get; set; }
    public decimal? MinimumStockLevel { get; set; }
    public decimal? MaximumStockLevel { get; set; }

    public virtual Item Item { get; set; } = default!;
}
