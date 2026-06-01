using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin")]
[Route("api/staff")]
public class StaffController(IShiftManagementService shiftManagementService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Staff>>> GetAll(CancellationToken cancellationToken = default)
        => Ok(await shiftManagementService.GetAllStaffAsync(cancellationToken));

    [HttpGet("{staffId:int}")]
    public async Task<ActionResult<Staff>> GetById(int staffId, CancellationToken cancellationToken = default)
    {
        var staff = await shiftManagementService.GetStaffByIdAsync(staffId, cancellationToken);
        return staff is null ? NotFound() : Ok(staff);
    }

    [HttpGet("doctors")]
    public async Task<ActionResult<IReadOnlyList<DoctorSummary>>> GetDoctors(CancellationToken cancellationToken = default)
    {
        var doctors = await shiftManagementService.GetDoctorsAsync(cancellationToken);
        var summaries = doctors
            .Select(d => new DoctorSummary(d.StaffId, d.FirstName, d.LastName))
            .ToList();
        return Ok(summaries);
    }

    [HttpGet("pharmacists")]
    public async Task<ActionResult<IReadOnlyList<Pharmacyst>>> GetPharmacists(CancellationToken cancellationToken = default)
        => Ok(await shiftManagementService.GetPharmacistsAsync(cancellationToken));

    [HttpPatch("{staffId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int staffId, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken = default)
    {
        await shiftManagementService.UpdateStaffStatusAsync(staffId, request.Status, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{staffId:int}/availability")]
    public async Task<IActionResult> UpdateAvailability(int staffId, [FromBody] UpdateAvailabilityRequest request, CancellationToken cancellationToken = default)
    {
        await shiftManagementService.UpdateStaffAvailabilityAsync(staffId, request.IsAvailable, request.Status, cancellationToken);
        return NoContent();
    }

    public record DoctorSummary(int DoctorId, string FirstName, string LastName);
    public record UpdateStatusRequest(string Status);
    public record UpdateAvailabilityRequest(bool IsAvailable, DoctorStatus Status);
}
