using Hospital.Shared.Proxies;
namespace Hospital.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Hospital.Data.Models;
    using Hospital.Data.Repositories;
    using Hospital.Shared.Services;
    using Hospital.Web.Models;

    [Authorize(Roles = "Doctor,Admin")]
    public class HangoutsController : Controller
    {
        private readonly IHangoutApiClient hangoutService;
        private readonly IDoctorAppointmentApiClient doctorAppointmentService;
        private readonly IStaffRepository staffRepository;

        public HangoutsController(IHangoutApiClient hangoutService, IDoctorAppointmentApiClient doctorAppointmentService, IStaffRepository staffRepository)
        {
            this.hangoutService = hangoutService;
            this.doctorAppointmentService = doctorAppointmentService;
            this.staffRepository = staffRepository;
        }

        private async Task<int?> GetCurrentStaffIdAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return null;

            var allStaff = await this.staffRepository.GetAllAsync();
            var existing = allStaff.Find(staff => string.Equals(staff.Email, email, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                return existing.StaffId;
            }

           
            var displayName = User.FindFirstValue(ClaimTypes.Name) ?? email;
            var created = await this.staffRepository.CreateAsync(new Staff
            {
                Email = email,
                FirstName = displayName,
                LastName = string.Empty,

                Role = "Staff",
            });

            return created.StaffId;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Hangout> hangouts = this.hangoutService.GetAllHangouts();
            int? currentStaffId = await this.GetCurrentStaffIdAsync();

            HangoutViewModel MapHangoutToViewModel(Hangout hangout) =>
                new HangoutViewModel
                {
                    HangoutId = hangout.HangoutID,
                    Title = hangout.Title,
                    Description = hangout.Description,
                    FormattedDate = hangout.FormattedDate,
                    ParticipantCount = hangout.ParticipantList.Count,
                    MaxParticipants = hangout.MaxParticipants,
                    IsFull = hangout.ParticipantList.Count >= hangout.MaxParticipants,
                    IsAlreadyJoined = currentStaffId.HasValue &&
                        hangout.ParticipantList.Any(participant => participant.StaffId == currentStaffId.Value),
                    ParticipantStaffIds = hangout.ParticipantList.Select(participant => participant.StaffId).ToHashSet(),
                };

            var viewModel = new HangoutsIndexViewModel
            {
                Hangouts = hangouts.ConvertAll(MapHangoutToViewModel),
                CurrentStaffId = currentStaffId,
                Doctors = await this.LoadDoctorOptionsAsync(),
            };

            return this.View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            List<DoctorOptionViewModel> doctors = await this.LoadDoctorOptionsAsync();
            return this.View(new CreateHangoutViewModel { Doctors = doctors });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateHangoutViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                viewModel.Doctors = await this.LoadDoctorOptionsAsync();
                return this.View(viewModel);
            }

            var creator = new Staff { StaffId = viewModel.SelectedDoctorId };

            try
            {
                this.hangoutService.CreateHangout(
                    viewModel.Title,
                    viewModel.Description,
                    viewModel.Date,
                    viewModel.MaxParticipantsCount,
                    creator);

                return this.RedirectToAction(nameof(this.Index));
            }
            catch (ArgumentException argumentException)
            {
                this.ModelState.AddModelError(string.Empty, argumentException.Message);
                viewModel.Doctors = await this.LoadDoctorOptionsAsync();
                return this.View(viewModel);
            }
            catch (InvalidOperationException operationException)
            {
                this.ModelState.AddModelError(string.Empty, operationException.Message);
                viewModel.Doctors = await this.LoadDoctorOptionsAsync();
                return this.View(viewModel);
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            bool HasMatchingId(Hangout hangout) => hangout.HangoutID == id;
            Hangout hangout = this.hangoutService.GetAllHangouts().Find(HasMatchingId);
            if (hangout == null)
            {
                return this.NotFound();
            }

            HangoutViewModel viewModel = new HangoutViewModel
            {
                HangoutId = hangout.HangoutID,
                Title = hangout.Title,
                Description = hangout.Description,
                FormattedDate = hangout.FormattedDate,
                ParticipantCount = hangout.ParticipantList.Count,
                MaxParticipants = hangout.MaxParticipants,
                IsFull = hangout.ParticipantList.Count >= hangout.MaxParticipants,
            };

            return this.View(viewModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, object model)
        {
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            return this.View();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Join(int hangoutId, int staffId)
        {
            var joiningStaff = new Staff { StaffId = staffId };

            try
            {
                this.hangoutService.JoinHangout(hangoutId, joiningStaff);
            }
            catch (ArgumentException argumentException)
            {
                this.TempData["ErrorMessage"] = argumentException.Message;
            }
            catch (InvalidOperationException operationException)
            {
                this.TempData["ErrorMessage"] = operationException.Message;
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        private async Task<List<DoctorOptionViewModel>> LoadDoctorOptionsAsync()
        {
            IReadOnlyList<(int DoctorId, string DoctorName)> doctors =
                await this.doctorAppointmentService.GetAllDoctorsAsync();

            DoctorOptionViewModel MapDoctorToOption((int DoctorId, string DoctorName) doctor) =>
                new DoctorOptionViewModel
                {
                    DoctorId = doctor.DoctorId,
                    DoctorName = doctor.DoctorName,
                };

            var result = new List<DoctorOptionViewModel>();
            foreach ((int DoctorId, string DoctorName) doctor in doctors)
            {
                result.Add(MapDoctorToOption(doctor));
            }

            return result;
        }
    }
}

