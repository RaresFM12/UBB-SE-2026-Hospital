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
        var id = await orderService.CreateOrderAsync(request.ClientId, request.PickUpDate, request.IsCompleted, request.IsExpired, cancellationToken);
        return Ok(id);
    }

    [HttpPut("{orderId:int}")]
    public async Task<IActionResult> Update(int orderId, [FromBody] Order order, CancellationToken cancellationToken = default)
    {
        order.Id = orderId;
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
}
