using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[Authorize]
[Route("api/hangout-participants")]
public class HangoutParticipantsController(IHangoutService hangoutService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HangoutParticipantSummary>>> GetAll(CancellationToken cancellationToken = default)
    {
        var participants = await hangoutService.GetAllParticipantsAsync(cancellationToken);
        var summaries = participants
            .Select(p => new HangoutParticipantSummary(p.Hangout?.HangoutID ?? 0, p.Staff?.StaffId ?? 0))
            .ToList();
        return Ok(summaries);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateParticipantRequest request, CancellationToken cancellationToken = default)
    {
        await hangoutService.AddParticipantAsync(request.HangoutId, request.StaffId, cancellationToken);
        return NoContent();
    }

    public record HangoutParticipantSummary(int HangoutId, int StaffId);
    public record CreateParticipantRequest(int HangoutId, int StaffId);
}
