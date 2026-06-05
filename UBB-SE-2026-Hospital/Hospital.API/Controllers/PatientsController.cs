using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse")]
[Route("api/patients")]
public class PatientsController(IPatientService patientService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Patient>>> GetAll(CancellationToken cancellationToken)
        => Ok(await patientService.GetPatientsAsync(cancellationToken));
}
