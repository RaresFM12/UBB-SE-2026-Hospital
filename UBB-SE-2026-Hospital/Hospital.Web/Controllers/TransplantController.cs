using System;
using System.Collections.Generic;
using System.Text;
using Hospital.Web.Models.Transplant;
using Hospital.Shared.Services;
using Hospital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital.Services;
using ITransplantService = Hospital.Shared.Services.ITransplantService;

namespace Hospital.Web.Controllers;

[Authorize]
public class TransplantController : Controller
{
    private readonly ITransplantService transplantService;
    private readonly IPatientService patientService;

    public TransplantController(ITransplantService transplantService, IPatientService patientService)
    {
        this.transplantService = transplantService;
        this.patientService = patientService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Request(int patientId, CancellationToken cancellationToken)
    {
        var patient = await patientService.GetByIdAsync(patientId, cancellationToken);
        if (patient is null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction("Index", "Admin");
        }

        bool isUrgent;
        string? warning;

        try
        {
            isUrgent = await transplantService.IsUrgentAsync(patientId);
            warning = await transplantService.GetChronicWarningAsync(patientId);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            isUrgent = false;
            warning = null;
        }

        var model = new TransplantRequestViewModel
        {
            PatientId = patientId,
            PatientName = patient.FullName,
            IsUrgent = isUrgent,
            WarningMessage = warning,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Request(TransplantRequestViewModel model, CancellationToken cancellationToken)
    {
        async Task<IActionResult> ReturnWithErrors()
        {
            var patient = await patientService.GetByIdAsync(model.PatientId, cancellationToken);
            model.PatientName = patient?.FullName ?? string.Empty;

            try
            {
                model.IsUrgent = await transplantService.IsUrgentAsync(model.PatientId);
                model.WarningMessage = await transplantService.GetChronicWarningAsync(model.PatientId);
            }
            catch
            {
            }

            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return await ReturnWithErrors();
        }

        try
        {
            await transplantService.CreateWaitlistRequestAsync(model.PatientId, model.SelectedOrgan!);

            TempData["SuccessMessage"] =
                "The patient has been successfully added to the Organ Transplant Waitlist.";

            return RedirectToAction("Index", "Admin", new { selectedId = model.PatientId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await ReturnWithErrors();
        }
    }
}
