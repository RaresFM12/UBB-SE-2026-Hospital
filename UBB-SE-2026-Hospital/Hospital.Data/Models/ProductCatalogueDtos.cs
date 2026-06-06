using System.Collections.Generic;

namespace Hospital.Data.Models;

public class ProductPriceRange
{
    public float Minimum { get; set; }
    public float Maximum { get; set; }
}

public class ProductCatalogueQuery
{
    public string? Search { get; set; }
    public List<string>? Categories { get; set; }
    public List<ProductPriceRange>? PriceRanges { get; set; }
    public string? StockFilter { get; set; }
    public bool? Discounted { get; set; }
    public List<string>? Substances { get; set; }
    public bool Ascending { get; set; } = true;
    public int Page { get; set; }
    public int PageSize { get; set; } = 30;
    public string? SortBy { get; set; }
}
