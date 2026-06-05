using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Web.Models.Registration;
using Hospital.Web.Services;
using Hospital.Shared.Proxies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class RegistrationController : Controller
{
    private readonly IPatientApiClient patientApiClient;
    private readonly IErWorkflowApiClient erApiClient;

    public RegistrationController(IPatientApiClient patientApiClient, IErWorkflowApiClient erApiClient)
    {
        this.patientApiClient = patientApiClient;
        this.erApiClient = erApiClient;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new RegistrationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegistrationViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        try
        {
            string patientId = model.PatientId.Trim();
            bool patientExists = await patientApiClient.ExistsAsync(patientId, cancellationToken);

            if (!patientExists)
            {
                Patient created = await patientApiClient.CreatePatientAsync(new CreatePatientRequest
                {
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    Cnp = patientId,
                    DateOfBirth = model.DateOfBirth,
                    Sex = model.Sex,
                    PhoneNumber = model.Phone.Trim(),
                    EmergencyContact = model.EmergencyContact.Trim(),
                    IsDonor = false
                }, cancellationToken);

                TempData["SuccessMessage"] = $"Patient {created.FullName} was created.";
            }

            ERVisit visit = await erApiClient.CreateVisitAsync(new ERVisit
            {
                Patient = (await patientApiClient.SearchPatientsAsync(
                    new SearchPatientsRequest { Cnp = patientId },
                    cancellationToken)).First(),
                ChiefComplaint = model.ChiefComplaint.Trim(),
                ArrivalDateTime = DateTime.Now,
                Status = ERVisit.VisitStatus.REGISTERED
            }, cancellationToken);

            TempData["SuccessMessage"] = $"Registration complete. Visit {visit.VisitId} is ready for triage.";
            return RedirectToAction(nameof(Index));
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Index", model);
        }
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before registering ER patients.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}

