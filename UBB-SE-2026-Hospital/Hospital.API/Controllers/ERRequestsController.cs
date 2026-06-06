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
    [HttpPut("{requestId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int requestId, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken = default)
    {
        await erDispatchService.UpdateRequestStatusAsync(requestId, request.Status, request.AssignedDoctorId, request.AssignedDoctorName, cancellationToken);
        return NoContent();
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<int>>> GetPending(CancellationToken cancellationToken = default)
        => Ok(await erDispatchService.GetPendingRequestIdsAsync(cancellationToken));

    [HttpPost("{requestId:int}/dispatch")]
    public async Task<ActionResult<ERDispatchResult>> Dispatch(int requestId, CancellationToken cancellationToken = default)
    {
        var result = await erDispatchService.DispatchERRequestAsync(requestId, cancellationToken);
        result.Request ??= await erDispatchService.GetRequestByIdAsync(result.RequestId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("dispatch-all")]
    public async Task<ActionResult<IReadOnlyList<ERDispatchResult>>> DispatchAll(CancellationToken cancellationToken = default)
    {
        var results = await erDispatchService.DispatchAllPendingAsync(cancellationToken);
        foreach (var result in results)
        {
            result.Request ??= await erDispatchService.GetRequestByIdAsync(result.RequestId, cancellationToken);
        }

        return Ok(results);
    }

    [HttpPost("simulate")]
    public async Task<ActionResult<IReadOnlyList<int>>> Simulate([FromBody] SimulateRequest request, CancellationToken cancellationToken = default)
        => Ok(await erDispatchService.SimulateIncomingRequestsAsync(request.Count, cancellationToken));

    [HttpGet("{requestId:int}/candidates")]
    public async Task<ActionResult<IReadOnlyList<DoctorProfile>>> GetCandidates(int requestId, [FromQuery] int nearEndMinutes, CancellationToken cancellationToken = default)
        => Ok(await erDispatchService.GetManualOverrideCandidatesAsync(requestId, nearEndMinutes, cancellationToken));

    [HttpPost("{requestId:int}/override")]
    public async Task<ActionResult<ERDispatchResult>> Override(int requestId, [FromBody] OverrideRequest request, CancellationToken cancellationToken = default)
    {
        var result = await erDispatchService.ManualOverrideAsync(requestId, request.DoctorId, request.NearEndMinutes, cancellationToken);
        result.Request ??= await erDispatchService.GetRequestByIdAsync(result.RequestId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-visit/{visitId:int}")]
    public async Task<ActionResult<ERRequest>> GetByVisit(int visitId)
    {
        var request = await erDispatchService.GetRequestByVisitIdAsync(visitId);
        return request is null ? NotFound() : Ok(request);
    }

    public record CreateERRequest(string Specialization, string Location, string Status);
    public record UpdateStatusRequest(string Status, int? AssignedDoctorId, string? AssignedDoctorName);
    public record SimulateRequest(int Count);
    public record OverrideRequest(int DoctorId, int NearEndMinutes);
}
