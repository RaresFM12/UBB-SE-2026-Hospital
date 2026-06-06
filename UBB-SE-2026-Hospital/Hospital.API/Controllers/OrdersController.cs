using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Pharmacist","Client")]
[Route("api/orders")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Order>>> GetAll([FromQuery] int? clientId = null, CancellationToken cancellationToken = default)
    {
        if (clientId.HasValue && !CanAccessClientData(clientId.Value))
        {
            return Forbid();
        }

        var orders = clientId.HasValue
            ? await orderService.GetOrdersByClientAsync(clientId.Value, cancellationToken)
            : await orderService.GetAllOrdersAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{orderId:int}")]
    public async Task<ActionResult<Order>> GetById(int orderId, CancellationToken cancellationToken = default)
    {
        if (!await orderService.OrderExistsAsync(orderId, cancellationToken))
            return NotFound();
        return Ok(await orderService.GetOrderByIdAsync(orderId, cancellationToken));
    }

    [HttpGet("{orderId:int}/exists")]
    public async Task<ActionResult<bool>> Exists(int orderId, CancellationToken cancellationToken = default)
        => Ok(await orderService.OrderExistsAsync(orderId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (!CanAccessClientData(request.ClientId))
        {
            return Forbid();
        }

        var id = await orderService.CreateOrderAsync(request.ClientId, request.PickUpDate, request.IsCompleted, request.IsExpired, cancellationToken);
        return Ok(id);
    }

    [HttpPost("place-from-basket")]
    public async Task<IActionResult> PlaceFromBasket([FromBody] PlaceOrderFromBasketRequest request, CancellationToken cancellationToken = default)
    {
        if (!CanAccessClientData(request.UserId))
        {
            return Forbid();
        }

        await orderService.PlaceOrderFromBasketAsync(request.UserId, request.ChosenPickUpDate, cancellationToken);
        return NoContent();
    }

    [HttpPut("{orderId:int}")]
    public async Task<IActionResult> Update(int orderId, [FromBody] Order order, CancellationToken cancellationToken = default)
    {
        order.Id = orderId;
        await orderService.UpdateOrderAsync(order, cancellationToken);
        return NoContent();
    }

    [HttpPut("{orderId:int}/modify")]
    public async Task<IActionResult> Modify(
        int orderId,
        [FromBody] ModifyOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        Order? order = await orderService.GetOrderByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (!CanAccessClientData(order.Client.Id))
        {
            return Forbid();
        }

        order.PickUpDate = request.PickUpDate;
        order.ItemQuantitiesWithFinalPrice = request.UpdatedItems;
        await orderService.UpdateOrderAsync(order, cancellationToken);
        return NoContent();
    }

    [AuthorizeRole("Admin")]
    [HttpPost("{orderId:int}/complete")]
    public async Task<IActionResult> Complete(
        int orderId,
        [FromBody] CompleteOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        Dictionary<int, (int Quantity, float Discount)> updatedItems = request.UpdatedItems.ToDictionary(
            pair => pair.Key,
            pair => (pair.Value.Item1, pair.Value.Item2));
        await orderService.CompleteOrderAsync(orderId, updatedItems, cancellationToken);
        return NoContent();
    }

    [HttpPost("{orderId:int}/cancel")]
    public async Task<IActionResult> Cancel(int orderId, CancellationToken cancellationToken = default)
    {
        Order? order = await orderService.GetOrderByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (!CanAccessClientData(order.Client.Id))
        {
            return Forbid();
        }

        await orderService.CancelOrderAsync(orderId, cancellationToken);
        return NoContent();
    }

    [HttpPost("expire-overdue")]
    public async Task<IActionResult> ExpireOverdue(CancellationToken cancellationToken = default)
    {
        await orderService.ExpireOverdueOrdersAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{orderId:int}/resubmit")]
    public async Task<IActionResult> Resubmit(
        int orderId,
        [FromBody] ResubmitOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        Order? order = await orderService.GetOrderByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (!CanAccessClientData(order.Client.Id))
        {
            return Forbid();
        }

        order.PickUpDate = request.PickUpDate;
        order.IsExpired = false;
        await orderService.UpdateOrderAsync(order, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{orderId:int}")]
    public async Task<IActionResult> Delete(int orderId, CancellationToken cancellationToken = default)
    {
        await orderService.DeleteOrderAsync(orderId, cancellationToken);
        return NoContent();
    }

    public record CreateOrderRequest(int ClientId, DateOnly PickUpDate, bool IsCompleted, bool IsExpired);

    public record PlaceOrderFromBasketRequest(int UserId, DateOnly ChosenPickUpDate);

    public record ModifyOrderRequest(Dictionary<int, Tuple<int, float>> UpdatedItems, DateOnly PickUpDate);

    public record CompleteOrderRequest(Dictionary<int, Tuple<int, float>> UpdatedItems);

    public record ResubmitOrderRequest(DateOnly PickUpDate);

    private bool CanAccessClientData(int userId)
    {
        UserPrincipal currentUser = this.GetCurrentUser();
        return currentUser.Id == userId
            || string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(currentUser.Role, "Pharmacist", StringComparison.OrdinalIgnoreCase);
    }
}
