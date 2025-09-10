

/*
    Goods or Service
    Taxable or not
    Price including or excluding tax
    Tax rates for purchase and sales
    Tag or Batch number generation options
    Is expiry dated
    Barcoded or not
    SKU - Stock Keeping Unit
    Economic order quantity details
    Pricing details - purchase price, wholesale price, retail price, MRP, margin, discountable or not
    Attributes - brand, model, size, color, weight, dimensions, material, country of origin, warranty, notes
*/

using Fanda.ICS.Api.Enums;

namespace Fanda.ICS.Api.Models;

public class Item
{
    public Guid Id { get; set; }
    public string SKU { get; set; } = default!;
    public string ItemName { get; set; } = default!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public Guid UnitId { get; set; }
    public GTNGenerations GTNGeneration { get; set; }  // SKU, Batch, Tag
    public ItemOfferings Offering { get; set; } // Goods or Service
    public TaxTreatments TaxTreatment { get; set; } // Taxable, Non-Taxable, Exempt, Nil-Rated
    public bool IsSKUGenerated { get; set; }
    public bool IsBatchTracked { get; set; }
    public bool IsBarcoded { get; set; }
    public bool IsExpiryDated { get; set; }
    public bool IsReturnable { get; set; }
    public bool IsDiscontinued { get; set; }
    public string? Warranty { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // One-to-one relationships
    public virtual ItemAttributes Attributes { get; set; } = default!;
    public virtual ItemEOQ EOQ { get; set; } = default!;
    public virtual ItemPricing Pricing { get; set; } = default!;
    public virtual ItemTaxation Taxation { get; set; } = default!;

    // Navigation properties
    public virtual ItemCategory Category { get; set; } = default!;
    public virtual Unit Unit { get; set; } = default!;
}
