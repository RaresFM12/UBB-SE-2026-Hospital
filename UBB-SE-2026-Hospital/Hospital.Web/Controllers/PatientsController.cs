using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class PatientsController(IPatientService patientService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await patientService.GetPatientsAsync(cancellationToken));
}
