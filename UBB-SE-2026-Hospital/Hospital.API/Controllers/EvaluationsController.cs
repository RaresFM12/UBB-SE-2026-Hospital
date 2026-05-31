using Hospital.Data.Models;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[Authorize]
[Route("api/evaluations")]
public class EvaluationsController(IMedicalEvaluationService evaluationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MedicalEvaluation>>> GetAll(CancellationToken cancellationToken = default)
        => Ok(await evaluationService.GetAllEvaluationsAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEvaluationRequest request, CancellationToken cancellationToken = default)
    {
        await evaluationService.CreateEvaluationAsync(
            request.DoctorId, request.PatientId, request.Diagnosis,
            request.Notes, request.Medications, request.AssumedRisk, cancellationToken);
        return NoContent();
    }

    [HttpPut("{evaluationId:int}")]
    public async Task<IActionResult> Update(int evaluationId, [FromBody] UpdateEvaluationRequest request, CancellationToken cancellationToken = default)
    {
        await evaluationService.UpdateEvaluationAsync(evaluationId, request.Diagnosis, request.Notes, request.Medications, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{evaluationId:int}")]
    public async Task<IActionResult> Delete(int evaluationId, CancellationToken cancellationToken = default)
    {
        await evaluationService.DeleteEvaluationAsync(evaluationId, cancellationToken);
        return NoContent();
    }

    public record CreateEvaluationRequest(int DoctorId, int PatientId, string Diagnosis, string Notes, string Medications, bool AssumedRisk);
    public record UpdateEvaluationRequest(string Diagnosis, string Notes, string Medications);
}
