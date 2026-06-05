using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Pharmacist","Nurse")]
[Route("api/hangouts")]
public class HangoutsController(IHangoutService hangoutService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Hangout>>> GetAll(CancellationToken cancellationToken = default)
        => Ok(await hangoutService.GetAllHangoutsAsync(cancellationToken));

    [HttpGet("{hangoutId:int}")]
    public async Task<ActionResult<Hangout>> GetById(int hangoutId, CancellationToken cancellationToken = default)
    {
        var hangout = await hangoutService.GetHangoutByIdAsync(hangoutId, cancellationToken);
        return hangout is null ? NotFound() : Ok(hangout);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateHangoutRequest request, CancellationToken cancellationToken = default)
    {
        var id = await hangoutService.CreateHangoutAsync(request.Title, request.Description, request.Date, request.MaxParticipants, cancellationToken);
        return Ok(id);
    }

    public record CreateHangoutRequest(string Title, string Description, DateTime Date, int MaxParticipants);
}
