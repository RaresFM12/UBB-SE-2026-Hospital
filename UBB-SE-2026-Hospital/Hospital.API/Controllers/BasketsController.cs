using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Pharmacist","Client")]
[Route("api/baskets")]
public class BasketsController(IBasketService basketService, IOrderService orderService) : ControllerBase
{
    [HttpGet("{userId:int}")]
    public async Task<ActionResult<Dictionary<int, BasketEntryDto>>> GetBasket(int userId, CancellationToken cancellationToken = default)
    {
        if (!CanAccessClientData(userId))
        {
            return Forbid();
        }

        return Ok(await basketService.GetBasketAsync(userId, cancellationToken));
    }

    [HttpPut("{userId:int}")]
    public async Task<IActionResult> SaveBasket(int userId, [FromBody] Dictionary<int, BasketEntryDto>? basket, CancellationToken cancellationToken = default)
    {
        if (!CanAccessClientData(userId))
        {
            return Forbid();
        }

        await basketService.SaveBasketAsync(userId, basket ?? [], cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> ClearBasket(int userId, CancellationToken cancellationToken = default)
    {
        if (!CanAccessClientData(userId))
        {
            return Forbid();
        }

        await basketService.ClearBasketAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpGet("/api/basket")]
    public async Task<ActionResult<List<BasketItemViewModel>>> GetBasketItems([FromQuery] int userId, CancellationToken cancellationToken = default)
    {
        if (!CanAccessClientData(userId))
        {
            return Forbid();
        }

        return Ok(await orderService.GetBasketItemsAsync(userId, cancellationToken));
    }

    [HttpPost("/api/basket/add")]
    public async Task<IActionResult> AddBasketItem([FromBody] BasketItemRequest request, CancellationToken cancellationToken = default)
    {
        if (!CanAccessClientData(request.UserId))
        {
            return Forbid();
        }

        await orderService.AddItemToBasketAsync(
            request.UserId,
            request.ItemId,
            request.Quantity,
            request.ExtraDiscountPercentage,
            cancellationToken);
        return NoContent();
    }

    [HttpPut("/api/basket/update")]
    public async Task<IActionResult> UpdateBasketItem([FromBody] BasketItemRequest request, CancellationToken cancellationToken = default)
    {
        if (!CanAccessClientData(request.UserId))
        {
            return Forbid();
        }

        await orderService.UpdateBasketItemQuantityAsync(request.UserId, request.ItemId, request.Quantity, cancellationToken);
        return NoContent();
    }

    [HttpDelete("/api/basket/{itemId:int}")]
    public async Task<IActionResult> RemoveBasketItem(int itemId, [FromQuery] int userId, CancellationToken cancellationToken = default)
    {
        if (!CanAccessClientData(userId))
        {
            return Forbid();
        }

        await orderService.RemoveFromBasketAsync(userId, itemId, cancellationToken);
        return NoContent();
    }

    [HttpPost("/api/basket/apply-prescription")]
    public async Task<IActionResult> ApplyPrescription([FromBody] ApplyPrescriptionRequest request, CancellationToken cancellationToken = default)
    {
        if (!CanAccessClientData(request.UserId))
        {
            return Forbid();
        }

        await orderService.ApplyPrescriptionToBasketAsync(request.UserId, request.PrescriptionId, cancellationToken);
        return NoContent();
    }

    public record BasketItemRequest(int UserId, int ItemId, int Quantity, float ExtraDiscountPercentage = 0f);

    public record ApplyPrescriptionRequest(int UserId, string PrescriptionId);

    private bool CanAccessClientData(int userId)
    {
        UserPrincipal currentUser = this.GetCurrentUser();
        return currentUser.Id == userId
            || string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(currentUser.Role, "Pharmacist", StringComparison.OrdinalIgnoreCase);
    }
}
