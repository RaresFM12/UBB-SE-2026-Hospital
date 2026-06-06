using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Services
{
    public class PeriodTrackerService : IPeriodTrackerService
    {
        private const int DefaultCycleLengthInDays = 28;
        private const int DefaultPeriodLengthInDays = 5;
        private const int DefaultPremenstrualSyndromeOption = 0;

        private readonly IUsersRepository usersRepository;
        private readonly IWellnessItemsService? wellnessItemsService;

        public PeriodTrackerService(
            IUsersRepository usersRepository,
            IWellnessItemsService? wellnessItemsService = null)
        {
            this.usersRepository = usersRepository;
            this.wellnessItemsService = wellnessItemsService;
        }

        public async Task<User> GetUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            if (userId <= 0) return null;
            return await this.usersRepository.GetUserByIdAsync(userId);
        }

        public Hospital.Data.Models.PeriodTrackerState GetTrackerState(int userId)
        {
            var user = this.usersRepository.GetUserByIdAsync(userId).GetAwaiter().GetResult();
            bool hasTracker = user != null && user.CycleDays > 0;

            return new Hospital.Data.Models.PeriodTrackerState
            { 
                StartPeriodDate = user != null && user.StartPeriodDate.Year > 2000
                    ? new DateTimeOffset(user.StartPeriodDate.ToDateTime(new TimeOnly(0, 0)))
                    : new DateTimeOffset(DateTime.Today),
                CycleDays = user?.CycleDays ?? DefaultCycleLengthInDays,
                PeriodLasts = user?.PeriodLasts ?? DefaultPeriodLengthInDays,
                PremenstrualSyndromeOption = user?.PremenstrualSyndromeOption ?? DefaultPremenstrualSyndromeOption
            };
        }

        public PeriodTrackerDashboardSnapshot GetDashboardSnapshot(int userId, int monthOffset)
        {
            var state = GetTrackerState(userId);

            var snapshot = new PeriodTrackerDashboardSnapshot
            {
                HasPeriodTracker = state.HasPeriodTracker,
                StartPeriodDate = state.StartPeriodDate.DateTime,
                CycleDays = (int)state.CycleDays,
                PeriodLasts = (int)state.PeriodLasts,
                PMSOption = state.PremenstrualSyndromeOption,
                MonthOffset = monthOffset,
                Notes = GetNotes(userId).Select(n => new PeriodTrackerNoteSnapshot
                {
                    NoteId = n.Key,
                    NoteBody = n.Value.Body,
                    IsDone = n.Value.IsDone
                }).ToList()
            };

            if (snapshot.HasPeriodTracker && snapshot.CycleDays > 0)
            {
                RunCalendarCalculations(snapshot);
                PopulateRecommendedProducts(snapshot);
            }

            return snapshot;
        }

        public Dictionary<int, (string Body, bool IsDone)> GetNotes(int userId)
        {
            if (userId <= 0) return new Dictionary<int, (string, bool)>();

            var user = this.usersRepository.GetUserByIdAsync(userId).GetAwaiter().GetResult();
            if (user == null || user.PeriodNoteEntries == null)
                return new Dictionary<int, (string, bool)>();

            return user.PeriodNoteEntries.ToDictionary(periodNote => periodNote.NoteId, periodNote => (periodNote.NoteBody, periodNote.IsDone));
        }

        public async Task<Dictionary<int, (string Body, bool IsDone)>> GetNotesAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await GetUserAsync(userId, cancellationToken);
            if (user == null || user.PeriodNoteEntries == null)
                return new Dictionary<int, (string, bool)>();

            return user.PeriodNoteEntries.ToDictionary(periodNote => periodNote.NoteId, periodNote => (periodNote.NoteBody, periodNote.IsDone));
        }

        public async Task UpdatePeriodTrackerAsync(int userId, DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption, CancellationToken cancellationToken = default)
        {
            var user = await GetUserAsync(userId, cancellationToken);
            if (user == null) return;

            user.StartPeriodDate = DateOnly.FromDateTime(startPeriodDate.DateTime);
            user.CycleDays = (int)cycleDays;
            user.PeriodLasts = (int)periodLasts;
            user.PremenstrualSyndromeOption = premenstrualSyndromeOption;

            await this.usersRepository.UpdateUserAsync(user);
        }

        public async Task AddNoteAsync(int userId, string noteBody, CancellationToken cancellationToken = default)
        {
            var user = await GetUserAsync(userId, cancellationToken);
            if (user == null) return;

            int nextId = (user.PeriodNoteEntries?.Any() == true) ? user.PeriodNoteEntries.Max(periodNote => periodNote.NoteId) + 1 : 1;

            user.PeriodNoteEntries ??= new List<PeriodNote>();
            user.PeriodNoteEntries.Add(new PeriodNote
            {
                NoteId = nextId,
                NoteBody = noteBody ?? string.Empty,
                IsDone = false,
                User = user
            });

            await this.usersRepository.UpdateUserAsync(user);
        }

        public async Task UpdateNoteAsync(int userId, int noteId, string noteBody, bool isDone, CancellationToken cancellationToken = default)
        {
            var user = await GetUserAsync(userId, cancellationToken);
            var note = user?.PeriodNoteEntries?.FirstOrDefault(periodNote => periodNote.NoteId == noteId);
            if (note != null)
            {
                note.NoteBody = noteBody ?? string.Empty;
                note.IsDone = isDone;
                await this.usersRepository.UpdateUserAsync(user);
            }
        }

        public async Task DeleteNoteAsync(int userId, int noteId, CancellationToken cancellationToken = default)
        {
            var user = await GetUserAsync(userId, cancellationToken);
            var note = user?.PeriodNoteEntries?.FirstOrDefault(periodNote => periodNote.NoteId == noteId);
            if (note != null)
            {
                user.PeriodNoteEntries!.Remove(note);
                await this.usersRepository.UpdateUserAsync(user);
            }
        }

        private void RunCalendarCalculations(PeriodTrackerDashboardSnapshot snapshot)
        {
            DateTime today = DateTime.Today;
            DateTime computedStart = snapshot.StartPeriodDate.Date;

            while (computedStart.AddDays(snapshot.CycleDays) <= today)
            {
                computedStart = computedStart.AddDays(snapshot.CycleDays);
            }

            while (computedStart > today)
            {
                computedStart = computedStart.AddDays(-snapshot.CycleDays);
            }

            computedStart = computedStart.AddDays(snapshot.MonthOffset * snapshot.CycleDays);

            DateTime endPeriod = computedStart.AddDays(snapshot.PeriodLasts);
            DateTime startLowFertility = endPeriod.AddDays(1);
            DateTime endLowFertility = computedStart.AddDays(8);
            DateTime startOvulation = computedStart.AddDays(11);
            DateTime endOvulation = computedStart.AddDays(15);
            DateTime nextPeriod = computedStart.AddDays(snapshot.CycleDays);

            snapshot.CurrentMonthName = computedStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
            snapshot.PeriodIntervalText = $"{computedStart.Day} {computedStart:MMMM} - {endPeriod.Day} {endPeriod:MMMM}";
            snapshot.LowFertilityIntervalText = snapshot.PeriodLasts < 8
                ? $"{startLowFertility.Day} {startLowFertility:MMMM} - {endLowFertility.Day} {endLowFertility:MMMM}"
                : "No low fertility days this month";
            snapshot.OvulationIntervalText = $"{startOvulation.Day} {startOvulation:MMMM} - {endOvulation.Day} {endOvulation:MMMM}";

            if (snapshot.PMSOption > 0)
            {
                int pmsOffset = snapshot.PMSOption == 1 ? 3 : (snapshot.PMSOption == 2 ? 6 : 10);
                DateTime startPms = nextPeriod.AddDays(-pmsOffset);
                snapshot.PmsIntervalText = $"{startPms.Day} {startPms:MMMM} - {nextPeriod.Day} {nextPeriod:MMMM}";
            }
            else
            {
                snapshot.PmsIntervalText = "No registered PMS effects";
            }

            snapshot.IsInMenstrualPhase = today >= computedStart && today <= endPeriod;

            bool isCurrentCycle = snapshot.MonthOffset == 0;

            if (isCurrentCycle)
            {
                if (today >= computedStart && today <= endPeriod)
                {
                    snapshot.CurrentPhaseString = "Menstrual Phase";
                }
                else if (today > endPeriod && today < startOvulation)
                {
                    snapshot.CurrentPhaseString = "Follicular Phase";
                }
                else if (today >= startOvulation && today <= endOvulation)
                {
                    snapshot.CurrentPhaseString = "Ovulation Phase";
                }
                else if (today > endOvulation && today < nextPeriod)
                {
                    snapshot.CurrentPhaseString = "Luteal Phase";
                }
                else
                {
                    snapshot.CurrentPhaseString = "Menstrual Phase";
                }
            }
            else if (computedStart > today)
            {
                snapshot.CurrentPhaseString = $"Will start on {computedStart:dd.MM.yyyy}";
            }
            else
            {
                snapshot.CurrentPhaseString = $"Started on {computedStart:dd.MM.yyyy}";
            }

            snapshot.NextPeriodDateString = nextPeriod.ToString("dd.MM.yyyy");
            snapshot.CurrentDayOfCycle = (int)(today - computedStart).TotalDays + 1;

            double daysDifference = Math.Ceiling((nextPeriod - today).TotalDays);
            if (isCurrentCycle)
            {
                snapshot.NextPeriodDistanceString = $"{Math.Max(0, daysDifference)} days left";
            }
            else if (daysDifference < 0)
            {
                snapshot.NextPeriodDistanceString = $"{(int)Math.Abs(daysDifference)} days ago";
            }
            else
            {
                snapshot.NextPeriodDistanceString = $"In {(int)daysDifference} days";
            }

            if (isCurrentCycle)
            {
                if (today < startOvulation)
                {
                    snapshot.DaysUntilOvulation = (int)Math.Ceiling((startOvulation - today).TotalDays);
                    snapshot.OvulationDistanceString = $"In {snapshot.DaysUntilOvulation} days";
                }
                else if (today >= startOvulation && today <= endOvulation)
                {
                    snapshot.DaysUntilOvulation = 0;
                    snapshot.OvulationDistanceString = "In Progress";
                }
                else
                {
                    DateTime nextMonthOvulation = nextPeriod.AddDays(11);
                    snapshot.DaysUntilOvulation = (int)Math.Ceiling((nextMonthOvulation - today).TotalDays);
                    snapshot.OvulationDistanceString = $"In {snapshot.DaysUntilOvulation} days";
                }
            }
            else
            {
                double ovulationDiff = Math.Ceiling((startOvulation - today).TotalDays);
                snapshot.DaysUntilOvulation = (int)ovulationDiff;
                if (ovulationDiff < 0)
                {
                    snapshot.OvulationDistanceString = $"{(int)Math.Abs(ovulationDiff)} days ago";
                }
                else
                {
                    snapshot.OvulationDistanceString = $"In {(int)ovulationDiff} days";
                }
            }
        }

        private void PopulateRecommendedProducts(PeriodTrackerDashboardSnapshot snapshot)
        {
            if (this.wellnessItemsService == null) return;

            var wellnessItems = this.wellnessItemsService.GetWellnessItems();
            float discountModifier = snapshot.IsInMenstrualPhase ? 20.0f : 0.0f;

            var editableShopItems = new List<PeriodTrackerShopItemSnapshot>();
            if (snapshot.ShopItems != null)
            {
                editableShopItems.AddRange(snapshot.ShopItems);
            }

            foreach (var item in wellnessItems)
            {
                float basePrice = (float)item.Price;
                float netPrice = discountModifier > 0 ? basePrice * 0.8f : basePrice;

                editableShopItems.Add(new PeriodTrackerShopItemSnapshot
                {
                    RawItem = item,
                    DisplayPrice = netPrice,
                    HasDiscountApplied = snapshot.IsInMenstrualPhase,
                });
            }

            snapshot.ShopItems = editableShopItems;
        }

        public void UpdatePeriodTracker(int userId, DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption)
            => UpdatePeriodTrackerAsync(userId, startPeriodDate, cycleDays, periodLasts, premenstrualSyndromeOption).GetAwaiter().GetResult();

        public void AddNote(int userId, string noteBody)
            => AddNoteAsync(userId, noteBody).GetAwaiter().GetResult();

        public void UpdateNote(int userId, int noteId, string noteBody, bool isDone)
            => UpdateNoteAsync(userId, noteId, noteBody, isDone).GetAwaiter().GetResult();

        public void DeleteNote(int userId, int noteId)
            => DeleteNoteAsync(userId, noteId).GetAwaiter().GetResult();
    }
}