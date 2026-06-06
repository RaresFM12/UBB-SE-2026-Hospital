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

    [HttpDelete("{orderId:int}")]
    public async Task<IActionResult> Delete(int orderId, CancellationToken cancellationToken = default)
    {
        await orderService.DeleteOrderAsync(orderId, cancellationToken);
        return NoContent();
    }

    public record CreateOrderRequest(int ClientId, DateOnly PickUpDate, bool IsCompleted, bool IsExpired);

    public record PlaceOrderFromBasketRequest(int UserId, DateOnly ChosenPickUpDate);

    private bool CanAccessClientData(int userId)
    {
        UserPrincipal currentUser = this.GetCurrentUser();
        return currentUser.Id == userId
            || string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(currentUser.Role, "Pharmacist", StringComparison.OrdinalIgnoreCase);
    }
}
