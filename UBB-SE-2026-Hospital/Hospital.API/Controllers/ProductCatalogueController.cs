using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin", "Pharmacist", "Client")]
[Route("api/product-catalogue")]
public class ProductCatalogueController(IProductCatalogueService catalogueService) : ControllerBase
{
    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<Item>>> Search([FromBody] ProductCatalogueQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<(float Minimum, float Maximum)>? priceRanges = query.PriceRanges?
            .Select(priceRange => (priceRange.Minimum, priceRange.Maximum))
            .ToList();

        IReadOnlyList<Item> items = await catalogueService.GetItemsAsync(
            query.Search,
            query.Categories,
            priceRanges,
            query.StockFilter,
            query.Discounted,
            query.Substances,
            query.Ascending,
            query.Page,
            query.PageSize,
            query.SortBy,
            cancellationToken);

        return Ok(items);
    }
}
