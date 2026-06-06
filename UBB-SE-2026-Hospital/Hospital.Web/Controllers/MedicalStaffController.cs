using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Web.Models.MedicalStaff;
using Hospital.Web.Services;
using Hospital.Shared.Proxies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Hospital.Web.Controllers;

[Authorize]
public class MedicalStaffController : Controller
{
    private const int NoSearchResultsCount = 0;
    private const int CnpLength = 13;

    private readonly IPatientApiClient patientService;

    public MedicalStaffController(IPatientApiClient patientService)
    {
        this.patientService = patientService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View("Dashboard", new MedicalStaffDashboardViewModel
        {
            HasSearched = false,
        });
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(
        string? searchQuery,
        int? selectedId,
        CancellationToken cancellationToken = default)
    {
        var model = new MedicalStaffDashboardViewModel
        {
            SearchQuery = searchQuery,
            HasSearched = true,
            SelectedPatientId = selectedId
        };

        try
        {
            SearchPatientsDto dto = BuildSearchDto(searchQuery);
            List<Patient> results = await patientService.SearchPatientsAsync(dto, cancellationToken);

            if (results.Count == NoSearchResultsCount)
            {
                model.ErrorMessage = string.IsNullOrWhiteSpace(searchQuery)
                    ? "There are no patients."
                    : "There are no patients with this name or CNP.";
            }
            else
            {
                model.SearchResults = results.Select(p => new PatientSearchResultViewModel
                {
                    Id = p.PatientId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Cnp = p.Cnp,
                    Dob = p.DateOfBirth
                }).ToList();
            }
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            model.ErrorMessage = "Database connection error: " + ex.Message;
        }

        return View(model);
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening the medical staff dashboard.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }

    private static SearchPatientsDto BuildSearchDto(string? searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return new SearchPatientsDto();
        }

        string trimmed = searchQuery.Trim();
        return trimmed.Length == CnpLength && trimmed.All(char.IsDigit)
            ? new SearchPatientsDto { Cnp = trimmed }
            : new SearchPatientsDto { NamePart = trimmed };
    }
}

