using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize(Roles = "Admin")]
public class SubstancesController : Controller
{
    private readonly IAdminService adminService;

    public SubstancesController(IAdminService adminService)
    {
        this.adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        IReadOnlyList<Substance> substances = await adminService.GetSubstancesAsync(cancellationToken);
        return View(substances.ToList());
    }

    [HttpGet]
    public async Task<IActionResult> Details(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return NotFound();
        }

        Substance? substance = await adminService.GetSubstanceByNameAsync(name, cancellationToken);
        return substance is null ? NotFound() : View(substance);
    }

    [HttpGet]
    public IActionResult Create()
        => View(new SubstanceViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubstanceViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            await adminService.CreateSubstanceAsync(
                viewModel.Name,
                viewModel.LethalDose,
                viewModel.Description,
                cancellationToken);

            TempData["SuccessMessage"] = "Substance created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException argumentException)
        {
            ModelState.AddModelError(string.Empty, argumentException.Message);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return NotFound();
        }

        Substance? substance = await adminService.GetSubstanceByNameAsync(name, cancellationToken);
        if (substance is null)
        {
            return NotFound();
        }

        var viewModel = new SubstanceViewModel
        {
            Name = substance.Name,
            LethalDose = substance.LethalDose,
            Description = substance.Description,
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string name, SubstanceViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var updatedSubstance = new Substance
        {
            Name = viewModel.Name,
            LethalDose = viewModel.LethalDose,
            Description = viewModel.Description,
        };

        try
        {
            if (!string.Equals(name, viewModel.Name, StringComparison.OrdinalIgnoreCase))
            {
                await adminService.DeleteSubstanceAsync(name, cancellationToken);
                await adminService.CreateSubstanceAsync(
                    viewModel.Name,
                    viewModel.LethalDose,
                    viewModel.Description,
                    cancellationToken);
            }
            else
            {
                await adminService.UpdateSubstanceAsync(updatedSubstance, cancellationToken);
            }

            TempData["SuccessMessage"] = "Substance updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException argumentException)
        {
            ModelState.AddModelError(string.Empty, argumentException.Message);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return NotFound();
        }

        Substance? substance = await adminService.GetSubstanceByNameAsync(name, cancellationToken);
        return substance is null ? NotFound() : View(substance);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string name, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            await adminService.DeleteSubstanceAsync(name, cancellationToken);
            TempData["SuccessMessage"] = "Substance deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}
