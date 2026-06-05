using Hospital.Shared.Proxies;
namespace Hospital.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Hospital.Data.Models;
    using Hospital.Shared.Services;
    using Hospital.Web.Models;

    [Authorize(Roles = "Pharmacist")]
    public class PharmacyVacationController : Controller
    {
        private readonly IPharmacyVacationApiClient pharmacyVacationService;

        public PharmacyVacationController(IPharmacyVacationApiClient pharmacyVacationService)
        {
            this.pharmacyVacationService = pharmacyVacationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return this.View(this.BuildViewModel(new PharmacyVacationViewModel()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(PharmacyVacationViewModel viewModel)
        {
            if (viewModel.PharmacistStaffId is null || viewModel.StartDate is null || viewModel.EndDate is null)
            {
                viewModel.StatusMessage = "Select a pharmacist and both dates.";
                viewModel.StatusCssClass = "alert-warning";
                return this.View(this.BuildViewModel(viewModel));
            }

            try
            {
                this.pharmacyVacationService.RegisterVacation(
                    viewModel.PharmacistStaffId.Value,
                    viewModel.StartDate.Value,
                    viewModel.EndDate.Value);

                viewModel.StatusMessage = "Vacation shift added.";
                viewModel.StatusCssClass = "alert-success";
            }
            catch (ArgumentException exception)
            {
                viewModel.StatusMessage = exception.Message;
                viewModel.StatusCssClass = "alert-danger";
            }
            catch (InvalidOperationException exception)
            {
                viewModel.StatusMessage = exception.Message;
                viewModel.StatusCssClass = "alert-danger";
            }

            return this.View(this.BuildViewModel(viewModel));
        }

        private PharmacyVacationViewModel BuildViewModel(PharmacyVacationViewModel viewModel)
        {
            viewModel.Pharmacists = this.BuildPharmacistOptions(viewModel.PharmacistStaffId);
            return viewModel;
        }

        private IReadOnlyList<SelectListItem> BuildPharmacistOptions(int? selectedStaffId)
        {
            return this.pharmacyVacationService
                .GetPharmacists()
                .Select(pharmacist => new SelectListItem
                {
                    Value = pharmacist.StaffId.ToString(),
                    Text = BuildDisplayName(pharmacist),
                    Selected = pharmacist.StaffId == selectedStaffId,
                })
                .ToList();
        }

        private static string BuildDisplayName(Pharmacyst pharmacist)
        {
            bool IsNonEmpty(string? namePart) => !string.IsNullOrWhiteSpace(namePart);
            string displayName = string.Join(
                " ",
                new[] { pharmacist.FirstName?.Trim(), pharmacist.LastName?.Trim() }.Where(IsNonEmpty));

            return string.IsNullOrWhiteSpace(displayName)
                ? $"Pharmacist #{pharmacist.StaffId}"
                : displayName;
        }
    }
}

