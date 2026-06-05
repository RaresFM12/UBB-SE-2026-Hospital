using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Pharmacist","Client")]
[Route("api/baskets")]
public class BasketsController(IBasketService basketService) : ControllerBase
{
    [HttpGet("{userId:int}")]
    public async Task<ActionResult<Dictionary<int, BasketEntryDto>>> GetBasket(int userId, CancellationToken cancellationToken = default)
        => Ok(await basketService.GetBasketAsync(userId, cancellationToken));

    [HttpPut("{userId:int}")]
    public async Task<IActionResult> SaveBasket(int userId, [FromBody] Dictionary<int, BasketEntryDto>? basket, CancellationToken cancellationToken = default)
    {
        await basketService.SaveBasketAsync(userId, basket ?? [], cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> ClearBasket(int userId, CancellationToken cancellationToken = default)
    {
        await basketService.ClearBasketAsync(userId, cancellationToken);
        return NoContent();
    }
}
