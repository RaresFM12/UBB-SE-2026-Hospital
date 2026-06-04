#if false
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse","ERDoctor")]
[Route("api/er-requests")]
public class ERRequestsController(IERDispatchService erDispatchService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ERRequest>>> GetAll(CancellationToken cancellationToken = default)
        => Ok(await erDispatchService.GetAllRequestsAsync(cancellationToken));

    [HttpGet("{requestId:int}")]
    public async Task<ActionResult<ERRequest>> GetById(int requestId, CancellationToken cancellationToken = default)
    {
        var request = await erDispatchService.GetRequestByIdAsync(requestId, cancellationToken);
        return request is null ? NotFound() : Ok(request);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateERRequest request, CancellationToken cancellationToken = default)
    {
        var id = await erDispatchService.CreateRequestAsync(request.Specialization, request.Location, request.Status, cancellationToken);
        return Ok(id);
    }

    [HttpPatch("{requestId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int requestId, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken = default)
    {
        await erDispatchService.UpdateRequestStatusAsync(requestId, request.Status, request.AssignedDoctorId, request.AssignedDoctorName, cancellationToken);
        return NoContent();
    }

    public record CreateERRequest(string Specialization, string Location, string Status);
    public record UpdateStatusRequest(string Status, int? AssignedDoctorId, string? AssignedDoctorName);
}
#endif
