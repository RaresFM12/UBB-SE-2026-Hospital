using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin")]
[Route("api/salaries")]
public class SalariesController(ISalaryComputationService salaryComputationService) : ControllerBase
{
    [HttpPost("doctor")]
    public async Task<ActionResult<double>> ComputeDoctor([FromBody] ComputeDoctorSalaryRequest request, CancellationToken cancellationToken = default)
        => Ok(await salaryComputationService.ComputeSalaryDoctorAsync(request.Doctor, request.MonthlyShifts, request.Month, request.Year, cancellationToken));

    [HttpPost("pharmacist")]
    public async Task<ActionResult<double>> ComputePharmacist([FromBody] ComputePharmacistSalaryRequest request, CancellationToken cancellationToken = default)
        => Ok(await salaryComputationService.ComputeSalaryPharmacistAsync(request.Pharmacist, request.MonthlyShifts, request.Month, request.Year, cancellationToken));
}
