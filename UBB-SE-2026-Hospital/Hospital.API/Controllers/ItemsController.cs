#if false
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Pharmacist")]
[Route("api/items")]
public class ItemsController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Item>>> GetAll([FromQuery] string? name = null, CancellationToken cancellationToken = default)
        => Ok(await adminService.GetItemsAsync(name, cancellationToken));

    [HttpGet("{itemId:int}")]
    public async Task<ActionResult<Item>> GetById(int itemId, CancellationToken cancellationToken = default)
    {
        var item = await adminService.GetItemByIdAsync(itemId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("{itemId:int}/exists")]
    public async Task<ActionResult<bool>> Exists(int itemId, CancellationToken cancellationToken = default)
        => Ok(await adminService.ItemExistsAsync(itemId, cancellationToken));

    [HttpGet("top")]
    public async Task<ActionResult<IReadOnlyList<ItemPopularitySummary>>> GetTop(CancellationToken cancellationToken = default)
    {
        var top = await adminService.GetTopItemsAsync(cancellationToken);
        var summaries = top.Select(t => new ItemPopularitySummary(t.ItemId, t.ItemName, t.OrderCount)).ToList();
        return Ok(summaries);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemRequest request, CancellationToken cancellationToken = default)
    {
        await adminService.CreateItemAsync(
            request.Name, request.Producer, request.Category, request.Price,
            request.NumberOfPills, request.Label, request.Description, request.ImagePath,
            request.Discount, cancellationToken);
        return NoContent();
    }

    [HttpPost("with-quantity")]
    public async Task<IActionResult> CreateWithQuantity([FromBody] CreateItemWithQuantityRequest request, CancellationToken cancellationToken = default)
    {
        await adminService.CreateItemWithQuantityAsync(
            request.Name, request.Producer, request.Category, request.Price,
            request.NumberOfPills, request.Quantity, request.ActiveSubstances, request.Batches,
            request.Label, request.Description, request.ImagePath, request.Discount, cancellationToken);
        return NoContent();
    }

    [HttpPut("{itemId:int}")]
    public async Task<IActionResult> Update(int itemId, [FromBody] Item item, CancellationToken cancellationToken = default)
    {
        item.Id = itemId;
        await adminService.UpdateItemAsync(item, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{itemId:int}")]
    public async Task<IActionResult> Delete(int itemId, CancellationToken cancellationToken = default)
    {
        await adminService.DeleteItemAsync(itemId, cancellationToken);
        return NoContent();
    }

    public record ItemPopularitySummary(int ItemId, string ItemName, int OrderCount);

    public record CreateItemRequest(
        string Name, string Producer, string Category, float Price, int NumberOfPills,
        string Label, string Description, string ImagePath, float Discount);

    public record CreateItemWithQuantityRequest(
        string Name, string Producer, string Category, float Price, int NumberOfPills,
        int Quantity, Dictionary<string, float> ActiveSubstances, Dictionary<DateOnly, int> Batches,
        string Label, string Description, string ImagePath, float Discount);
}
#endif
