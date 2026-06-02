#if false
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Pharmacist","Nurse")]
[Route("api/shift-swaps")]
public class ShiftSwapsController(IShiftSwapService shiftSwapService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShiftSwapSummary>>> GetAll(CancellationToken cancellationToken = default)
    {
        var requests = await shiftSwapService.GetAllShiftSwapRequestsAsync(cancellationToken);
        var result = requests.Select(r => new ShiftSwapSummary(
            r.SwapId, r.RequestedAt, r.Status,
            r.Shift?.Id ?? 0,
            r.Requester?.StaffId ?? 0,
            r.Colleague?.StaffId ?? 0)).ToList();
        return Ok(result);
    }

    [HttpGet("{swapId:int}")]
    public async Task<IActionResult> GetById(int swapId, CancellationToken cancellationToken = default)
    {
        var swap = await shiftSwapService.GetShiftSwapByIdAsync(swapId, cancellationToken);
        if (swap is null) return NotFound();
        return Ok(new ShiftSwapSummary(
            swap.SwapId, swap.RequestedAt, swap.Status,
            swap.Shift?.Id ?? 0,
            swap.Requester?.StaffId ?? 0,
            swap.Colleague?.StaffId ?? 0));
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateShiftSwapRequest request, CancellationToken cancellationToken = default)
    {
        var requestedAt = request.RequestedAt == default ? DateTime.UtcNow : request.RequestedAt;
        var id = await shiftSwapService.CreateShiftSwapRequestAsync(
            request.ShiftId, request.RequesterId, request.ColleagueId,
            requestedAt, request.Status, cancellationToken);
        return Ok(id);
    }

    [HttpPatch("{swapId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int swapId, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken = default)
    {
        await shiftSwapService.UpdateShiftSwapStatusAsync(swapId, request.Status, cancellationToken);
        return NoContent();
    }

    public record ShiftSwapSummary(int SwapId, DateTime RequestedAt, ShiftSwapRequestStatus Status, int ShiftId, int RequesterId, int ColleagueId);
    public record CreateShiftSwapRequest(int ShiftId, int RequesterId, int ColleagueId, DateTime RequestedAt, ShiftSwapRequestStatus Status);
    public record UpdateStatusRequest(string Status);
}
#endif
