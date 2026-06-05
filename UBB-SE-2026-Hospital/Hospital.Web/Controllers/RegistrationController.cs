
using Hospital.Web.Models.Registration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class RegistrationController : Controller
{
    private readonly IPatientService patientService;
    private readonly IERVisitService erVisitService;

    public RegistrationController(IPatientService patientService, IERVisitService erVisitService)
    {
        this.patientService = patientService;
        this.erVisitService = erVisitService;
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
            string cnp = model.PatientId.Trim();
            bool patientExists = await patientService.ExistsAsync(cnp);

            if (!patientExists)
            {
                Patient created = await patientService.CreatePatientAsync(new CreatePatientRequest
                {
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    Cnp = cnp,
                    DateOfBirth = model.DateOfBirth,
                    Sex = model.Sex,
                    PhoneNumber = model.Phone.Trim(),
                    EmergencyContact = model.EmergencyContact.Trim(),
                    IsDonor = false
                }, cancellationToken);

                TempData["SuccessMessage"] = $"Patient {created.FullName} was created.";
            }

            Patient patient = (await patientService.SearchPatientsAsync(
                new SearchPatientsRequest { Cnp = cnp },
                cancellationToken)).First();

            ERVisit visit = await erVisitService.CreateAsync(new ERVisit
            {
                Patient = patient,
                ChiefComplaint = model.ChiefComplaint.Trim(),
                ArrivalDateTime = DateTime.Now,
                Status = ERVisit.VisitStatus.REGISTERED
            });

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

