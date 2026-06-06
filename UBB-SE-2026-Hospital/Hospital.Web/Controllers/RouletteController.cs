using Hospital.Data.Models;
using Hospital.Web.Models.Consultations;
using Hospital.Web.Services;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class RouletteController : Controller
{
    private const int NoDiscount = 0;
    private const int SmallDiscount = 10;
    private const int DoubledSmallDiscount = 20;
    private const int MediumDiscount = 25;
    private const int LargeDiscount = 50;
    private const int FullDiscount = 100;

    private static readonly HashSet<int> AllowedSubmittedDiscounts = new ()
    {
        NoDiscount,
        SmallDiscount,
        DoubledSmallDiscount,
        MediumDiscount,
        LargeDiscount,
        FullDiscount
    };

    private readonly IPatientService patientService;
    private readonly IBillingService billingService;

    public RouletteController(IPatientService patientService, IBillingService billingService)
    {
        this.patientService = patientService;
        this.billingService = billingService;
    }

    [HttpGet]
    public async Task<IActionResult> Spin(int patientId, int recordId)
    {
        Patient patient;
        try
        {
            patient = await patientService.GetPatientDetailsAsync(patientId, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Consultation", new { patientId, recordId });
        }

        MedicalRecord? record = patient.MedicalHistory?.MedicalRecords?
            .FirstOrDefault(record => record.RecordId == recordId);
        if (record is null)
        {
            TempData["ErrorMessage"] = "Consultation record not found.";
            return RedirectToAction("Details", "Admin", new { id = patientId });
        }

        decimal basePrice;
        try
        {
            basePrice = await billingService.ComputeBasePriceAsync(patientId, recordId);
        }
        catch (InvalidOperationException)
        {
            basePrice = record.BasePrice;
        }

        var model = new DiscountRouletteViewModel
        {
            PatientId = patientId,
            RecordId = recordId,
            PatientFullName = $"{patient.FirstName} {patient.LastName}",
            BasePrice = basePrice
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Spin(int patientId, int recordId, decimal basePrice, int discount)
    {
        if (!AllowedSubmittedDiscounts.Contains(discount))
        {
            TempData["ErrorMessage"] = "Invalid discount value.";
            return RedirectToAction("Details", "Consultation", new { patientId, recordId });
        }

        try
        {
            await billingService.PersistDiscountAsync(recordId, basePrice, discount);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Consultation", new { patientId, recordId });
        }

        TempData["SuccessMessage"] = discount == NoDiscount
            ? "Unlucky - no discount this time."
            : $"You won a {discount}% discount!";

        return RedirectToAction("Details", "Consultation", new { patientId, recordId });
    }
}

