using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor")]
[Route("api/appointments")]
public class AppointmentsController(IDoctorAppointmentService appointmentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Appointment>>> GetAll(CancellationToken cancellationToken = default)
        => Ok(await appointmentService.GetAllAppointmentsAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        await appointmentService.CreateAppointmentAsync(
            request.PatientId, request.DoctorId,
            request.StartTime, request.EndTime, request.Status, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{appointmentId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int appointmentId, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken = default)
    {
        await appointmentService.UpdateAppointmentStatusAsync(appointmentId, request.Status, cancellationToken);
        return NoContent();
    }

    public record CreateAppointmentRequest(int PatientId, int DoctorId, DateTime StartTime, DateTime EndTime, string Status);
    public record UpdateStatusRequest(string Status);
}
