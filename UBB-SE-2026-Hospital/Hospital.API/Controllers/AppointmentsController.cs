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

    [HttpGet("{appointmentId:int}")]
    public async Task<ActionResult<Appointment>> GetById(int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentService.GetAppointmentByIdAsync(appointmentId, cancellationToken);
        if (appointment is null) return NotFound();
        return Ok(appointment);
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<IReadOnlyList<Appointment>>> GetUpcoming(
        [FromQuery] int doctorUserId, [FromQuery] DateTime fromDate,
        [FromQuery] int skipCount, [FromQuery] int takeCount,
        CancellationToken cancellationToken = default)
        => Ok(await appointmentService.GetUpcomingAppointmentsAsync(doctorUserId, fromDate, skipCount, takeCount, cancellationToken));

    [HttpGet("range")]
    public async Task<ActionResult<IReadOnlyList<Appointment>>> GetInRange(
        [FromQuery] int doctorId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate,
        CancellationToken cancellationToken = default)
        => Ok(await appointmentService.GetAppointmentsInRangeAsync(doctorId, fromDate, toDate, cancellationToken));

    [HttpPost("{appointmentId:int}/book")]
    public async Task<IActionResult> Book(int appointmentId, CancellationToken cancellationToken = default)
    {
        await appointmentService.BookAppointmentAsync(appointmentId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{appointmentId:int}/finish")]
    public async Task<IActionResult> Finish(int appointmentId, CancellationToken cancellationToken = default)
    {
        await appointmentService.FinishAppointmentAsync(appointmentId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{appointmentId:int}/cancel")]
    public async Task<IActionResult> Cancel(int appointmentId, CancellationToken cancellationToken = default)
    {
        await appointmentService.CancelAppointmentAsync(appointmentId, cancellationToken);
        return NoContent();
    }

    [HttpPut("{appointmentId:int}/status")]
    public async Task<IActionResult> UpdateStatusPut(int appointmentId, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken = default)
    {
        await appointmentService.UpdateAppointmentStatusAsync(appointmentId, request.Status, cancellationToken);
        return NoContent();
    }

    public record CreateAppointmentRequest(int PatientId, int DoctorId, DateTime StartTime, DateTime EndTime, string Status);
    public record UpdateStatusRequest(string Status);
}
