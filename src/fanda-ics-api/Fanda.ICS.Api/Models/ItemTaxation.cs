using System.ComponentModel.DataAnnotations;

namespace Fanda.ICS.Api.Models;

public class ItemTaxation
{
    [Key]
    public Guid ItemId { get; set; }
    // HSN - Harmonized System of Nomenclature - Goods
    // SAC - Service Accounting Code - Services
    public string? HSNSAC { get; set; }
    public decimal? PurchaseTaxRate { get; set; }
    public decimal? SalesTaxRate { get; set; }
    public decimal? CessRate { get; set; }
    public bool IsInclusiveOfTax { get; set; }

    public virtual Item Item { get; set; } = default!;
}
