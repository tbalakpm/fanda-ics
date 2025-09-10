using System.ComponentModel.DataAnnotations;

namespace Fanda.ICS.Api.Models;

public class ItemPricing
{
    [Key]
    public Guid ItemId { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? RetailPrice { get; set; }
    public decimal? MRP { get; set; } // Maximum Retail Price
    public decimal? Margin { get; set; }
    public bool IsDiscountable { get; set; }

    public virtual Item Item { get; set; } = default!;
}
