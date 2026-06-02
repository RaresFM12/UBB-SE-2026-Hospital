using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.Web.Models;

namespace Hospital.Web.Controllers
{
    [Authorize(Roles = "Client")]
    public class PeriodTrackerController : Controller
    {
        private const int MinimumCycleDays = 20;
        private const int MaximumCycleDays = 45;
        private const int MinimumPeriodDays = 1;
        private const int MaximumPeriodDays = 9;
        private const int MinimumPmsOption = 0;
        private const int MaximumPmsOption = 3;
        private const int DefaultBasketQuantity = 1;

        private readonly IPeriodTrackerService _periodTrackerService;
        private readonly IBasketService _basketService;

        public PeriodTrackerController(
            IPeriodTrackerService periodTrackerService,
            IBasketService basketService)
        {
            _periodTrackerService = periodTrackerService;
            _basketService = basketService;
        }

        [HttpGet]
        public IActionResult Index(int monthOffset = 0)
        {
            return View(ToViewModel(_periodTrackerService.GetDashboardSnapshot(monthOffset)));
        }

        [HttpGet]
        public IActionResult Details(int id = 0)
        {
            var state = _periodTrackerService.GetTrackerState();
            if (!state.HasPeriodTracker)
            {
                return RedirectToAction(nameof(Create));
            }

            return View(ToViewModel(_periodTrackerService.GetDashboardSnapshot(0)));
        }

        [HttpGet]
        public IActionResult Create()
        {
            var state = _periodTrackerService.GetTrackerState();
            if (state.HasPeriodTracker) return RedirectToAction(nameof(Index));

            return View(new PeriodTrackerViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PeriodTrackerInputModel inputModel)
        {

            if (IsInvalidTrackerInput(inputModel))
            {
                return View(new PeriodTrackerViewModel
                {
                    StartPeriodDate = DateOnly.FromDateTime(inputModel.StartPeriodDate),
                    CycleDays = inputModel.CycleDays,
                    PeriodLasts = inputModel.PeriodLasts,
                    PMSOption = inputModel.PMSOption
                });
            }

            _periodTrackerService.UpdatePeriodTracker(inputModel.StartPeriodDate, inputModel.CycleDays, inputModel.PeriodLasts, inputModel.PMSOption);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id = 0)
        {
            var state = _periodTrackerService.GetTrackerState();
            if (!state.HasPeriodTracker)
            {
                return RedirectToAction(nameof(Create));
            }

            return View(ToViewModel(state));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PeriodTrackerInputModel inputModel)
        {
            if (IsInvalidTrackerInput(inputModel))
            {
                return View(new PeriodTrackerViewModel
                {
                    StartPeriodDate = DateOnly.FromDateTime(inputModel.StartPeriodDate),
                    CycleDays = inputModel.CycleDays,
                    PeriodLasts = inputModel.PeriodLasts,
                    PMSOption = inputModel.PMSOption
                });
            }

            _periodTrackerService.UpdatePeriodTracker(inputModel.StartPeriodDate, inputModel.CycleDays, inputModel.PeriodLasts, inputModel.PMSOption);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id = 0)
        {
            var state = _periodTrackerService.GetTrackerState();
            if (!state.HasPeriodTracker)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(ToViewModel(state));
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id = 0)
        {
            _periodTrackerService.UpdatePeriodTracker(DateTimeOffset.MinValue, 0, 0, 0);

            var existingNotes = _periodTrackerService.GetNotes();
            foreach (var noteId in existingNotes.Keys.ToList())
            {
                _periodTrackerService.DeleteNote(noteId);
            }

            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateNote([FromForm] string noteBody)
        {
            _periodTrackerService.AddNote(noteBody ?? "New Entry");
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditNote([FromForm] int noteId, [FromForm] string noteBody, [FromForm] bool isDone = false)
        {
            _periodTrackerService.UpdateNote(noteId, noteBody ?? string.Empty, isDone);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveNote([FromForm] int noteId)
        {
            _periodTrackerService.DeleteNote(noteId);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductToBasket(int itemId, float discountPercentage)
        {
            string? idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out int userId))
            {
                return Forbid();
            }

            await _basketService.AddToBasketAsync(userId, itemId, DefaultBasketQuantity, discountPercentage);
            return RedirectToAction(nameof(Index));
        }

        private static bool IsInvalidTrackerInput(PeriodTrackerInputModel inputModel)
        {
            return inputModel.StartPeriodDate == default ||
                inputModel.CycleDays < MinimumCycleDays ||
                inputModel.CycleDays > MaximumCycleDays ||
                inputModel.PeriodLasts < MinimumPeriodDays ||
                inputModel.PeriodLasts > MaximumPeriodDays ||
                inputModel.PMSOption < MinimumPmsOption ||
                inputModel.PMSOption > MaximumPmsOption;
        }

        private static PeriodTrackerViewModel ToViewModel(PeriodTrackerState state)
        {
            return new PeriodTrackerViewModel
            {
                HasPeriodTracker = state.HasPeriodTracker,
                StartPeriodDate = DateOnly.FromDateTime(state.StartPeriodDate.DateTime),
                CycleDays = (int)state.CycleDays,
                PeriodLasts = (int)state.PeriodLasts,
                PMSOption = state.PremenstrualSyndromeOption,
            };
        }

        private static PeriodTrackerViewModel ToViewModel(PeriodTrackerDashboardSnapshot snapshot)
        {
            return new PeriodTrackerViewModel
            {
                HasPeriodTracker = snapshot.HasPeriodTracker,
                StartPeriodDate = DateOnly.FromDateTime(snapshot.StartPeriodDate),
                CycleDays = snapshot.CycleDays,
                PeriodLasts = snapshot.PeriodLasts,
                PMSOption = snapshot.PMSOption,
                MonthOffset = snapshot.MonthOffset,
                CurrentMonthName = snapshot.CurrentMonthName,
                PeriodIntervalText = snapshot.PeriodIntervalText,
                LowFertilityIntervalText = snapshot.LowFertilityIntervalText,
                OvulationIntervalText = snapshot.OvulationIntervalText,
                PmsIntervalText = snapshot.PmsIntervalText,
                CurrentPhaseString = snapshot.CurrentPhaseString,
                NextPeriodDateString = snapshot.NextPeriodDateString,
                NextPeriodDistanceString = snapshot.NextPeriodDistanceString,
                IsInMenstrualPhase = snapshot.IsInMenstrualPhase,
                CurrentDayOfCycle = snapshot.CurrentDayOfCycle,
                DaysUntilOvulation = snapshot.DaysUntilOvulation,
                OvulationDistanceString = snapshot.OvulationDistanceString,
                Notes = snapshot.Notes.Select(note => new WebNoteItemViewModel
                {
                    NoteId = note.NoteId,
                    NoteBody = note.NoteBody,
                    IsDone = note.IsDone,
                }).ToList(),
                ShopItems = snapshot.ShopItems.Select(item => new WebShopItemViewModel
                {
                    RawItem = item.RawItem,
                    DisplayPrice = item.DisplayPrice,
                    HasDiscountApplied = item.HasDiscountApplied,
                }).ToList(),
            };
        }
    }
}
