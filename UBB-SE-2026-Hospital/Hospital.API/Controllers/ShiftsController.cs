using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Pharmacist","Nurse")]
[Route("api/shifts")]
public class ShiftsController(IShiftManagementService shiftManagementService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Shift>>> GetAll(CancellationToken cancellationToken = default)
        => Ok(await shiftManagementService.GetAllShiftsAsync(cancellationToken));

    [HttpGet("{shiftId:int}")]
    public async Task<ActionResult<Shift>> GetById(int shiftId, CancellationToken cancellationToken = default)
    {
        var shift = await shiftManagementService.GetShiftByIdAsync(shiftId, cancellationToken);
        return shift is null ? NotFound() : Ok(shift);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShiftRequest request, CancellationToken cancellationToken = default)
    {
        await shiftManagementService.CreateShiftAsync(
            request.StaffId, request.Location ?? string.Empty,
            request.StartTime, request.EndTime, request.Status, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{shiftId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int shiftId, [FromBody] UpdateShiftStatusRequest request, CancellationToken cancellationToken = default)
    {
        await shiftManagementService.UpdateShiftStatusAsync(shiftId, request.Status, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{shiftId:int}/staff")]
    public async Task<IActionResult> UpdateStaff(int shiftId, [FromBody] UpdateShiftStaffRequest request, CancellationToken cancellationToken = default)
    {
        await shiftManagementService.UpdateShiftStaffAsync(shiftId, request.StaffId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{shiftId:int}")]
    public async Task<IActionResult> Delete(int shiftId, CancellationToken cancellationToken = default)
    {
        await shiftManagementService.DeleteShiftAsync(shiftId, cancellationToken);
        return NoContent();
    }

    public record CreateShiftRequest(int StaffId, string? Location, DateTime StartTime, DateTime EndTime, ShiftStatus Status);
    public record UpdateShiftStatusRequest(ShiftStatus Status);
    public record UpdateShiftStaffRequest(int StaffId);
}
