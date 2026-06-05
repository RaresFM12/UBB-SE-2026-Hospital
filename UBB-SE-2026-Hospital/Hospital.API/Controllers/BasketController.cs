using Hospital.API.Auth;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin", "Pharmacist", "Client")]
[Route("api/basket")]
public class BasketController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BasketItemDto>>> GetBasket(
        [FromQuery] int userId,
        CancellationToken cancellationToken = default)
    {
        List<BasketItemViewModel> items = await orderService.GetBasketItemsAsync(userId, cancellationToken);
        return Ok(items.Select(BasketItemDto.FromViewModel).ToList());
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddBasketItemRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await orderService.AddItemToBasketAsync(
                request.UserId,
                request.ItemId,
                request.Quantity,
                request.ExtraDiscountPercentage,
                cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateQuantity(
        [FromBody] UpdateBasketItemRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await orderService.UpdateBasketItemQuantityAsync(
                request.UserId,
                request.ItemId,
                request.Quantity,
                cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{itemId:int}")]
    public async Task<IActionResult> RemoveItem(
        int itemId,
        [FromQuery] int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await orderService.RemoveFromBasketAsync(userId, itemId, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("apply-prescription")]
    public async Task<IActionResult> ApplyPrescription(
        [FromBody] ApplyPrescriptionToBasketRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await orderService.ApplyPrescriptionToBasketAsync(
                request.UserId,
                request.PrescriptionId,
                cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public record AddBasketItemRequest(int UserId, int ItemId, int Quantity, float ExtraDiscountPercentage = 0f);
    public record UpdateBasketItemRequest(int UserId, int ItemId, int Quantity);
    public record ApplyPrescriptionToBasketRequest(int UserId, string PrescriptionId);
}
