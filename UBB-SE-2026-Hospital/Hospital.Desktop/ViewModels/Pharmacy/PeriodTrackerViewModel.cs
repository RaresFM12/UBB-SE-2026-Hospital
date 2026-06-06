using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Desktop.Views.Shell;
using Hospital.Shared.Proxies;
using Hospital.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Hospital.Desktop.ViewModels.Pharmacy
{
    public partial class PeriodTrackerNoteItemViewModel : ObservableObject
    {
        public int NoteId { get; set; }

        [ObservableProperty]
        private string body = string.Empty;

        [ObservableProperty]
        private bool? isDone = false;
    }

    public partial class PeriodTrackerViewModel : ObservableObject
    {
        private readonly IPeriodTrackerApiClient _trackerService;
        private readonly ICurrentUserService _userService;
        private int _currentMonthOffset = 0;
        private readonly DialogPresenter _dialogPresenter;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DashboardVisibility))]
        [NotifyPropertyChangedFor(nameof(EmptyStateVisibility))]
        private bool hasPeriodTracker;

        public Visibility DashboardVisibility => HasPeriodTracker ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EmptyStateVisibility => HasPeriodTracker ? Visibility.Collapsed : Visibility.Visible;

        [ObservableProperty] private DateTimeOffset? startPeriodDate = DateTimeOffset.Now;

        [ObservableProperty] private double cycleDays = 28;
        [ObservableProperty] private double periodLasts = 7;
        [ObservableProperty] private int pmsOption = 0;

        [ObservableProperty] private string currentPhaseString = string.Empty;
        [ObservableProperty] private string nextPeriodDistanceString = string.Empty;
        [ObservableProperty] private string ovulationDistanceString = string.Empty;
        [ObservableProperty] private string nextPeriodDateString = string.Empty;
        [ObservableProperty] private string currentMonthName = string.Empty;

        [ObservableProperty] private string periodIntervalText = string.Empty;
        [ObservableProperty] private string lowFertilityIntervalText = string.Empty;
        [ObservableProperty] private string ovulationIntervalText = string.Empty;
        [ObservableProperty] private string pmsIntervalText = string.Empty;

        [ObservableProperty] private bool canAddNote = true;
        [ObservableProperty] private string newNoteText = string.Empty;

        public ObservableCollection<PeriodTrackerNoteItemViewModel> Notes { get; } = new();
        public ObservableCollection<object> ShopItems { get; } = new();

        public PeriodTrackerViewModel(IPeriodTrackerApiClient trackerService, ICurrentUserService userService, DialogPresenter dialogPresenter)
        {
            _trackerService = trackerService;
            _userService = userService;
            _dialogPresenter = dialogPresenter;
        }

        public async Task InitializeAsync()
        {
            await LoadDashboardAsync();
            await LoadNotesAsync();
        }

        private async Task LoadDashboardAsync()
        {
            var userId = _userService.UserId;
            if (userId == 0) return;

            try
            {
                var snapshot = await Task.Run(() => _trackerService.GetDashboardSnapshot(userId, _currentMonthOffset));

                if (snapshot != null)
                {
                    HasPeriodTracker = snapshot.HasPeriodTracker;
                    CurrentPhaseString = snapshot.CurrentPhaseString ?? "Unknown";
                    NextPeriodDistanceString = snapshot.NextPeriodDistanceString ?? "";
                    OvulationDistanceString = snapshot.OvulationDistanceString ?? "";
                    NextPeriodDateString = snapshot.NextPeriodDateString ?? "";
                    CurrentMonthName = snapshot.CurrentMonthName ?? DateTime.Today.ToString("MMMM");

                    PeriodIntervalText = snapshot.PeriodIntervalText ?? "";
                    LowFertilityIntervalText = snapshot.LowFertilityIntervalText ?? "";
                    OvulationIntervalText = snapshot.OvulationIntervalText ?? "";
                    PmsIntervalText = snapshot.PmsIntervalText ?? "";

                    StartPeriodDate = snapshot.StartPeriodDate;
                    CycleDays = snapshot.CycleDays >= 20 ? snapshot.CycleDays : 28;
                    PeriodLasts = snapshot.PeriodLasts >= 1 ? snapshot.PeriodLasts : 7;
                    PmsOption = snapshot.PMSOption;

                    ShopItems.Clear();
                    if (snapshot.ShopItems != null)
                    {
                        foreach (var item in snapshot.ShopItems) ShopItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard Load Error: {ex.Message}");
            }
        }

        private async Task LoadNotesAsync()
        {
            var userId = _userService.UserId;
            if (userId == 0) return;

            var rawNotes = await _trackerService.GetNotesAsync(userId);
            Notes.Clear();
            foreach (var kvp in rawNotes)
            {
                Notes.Add(new PeriodTrackerNoteItemViewModel { NoteId = kvp.Key, Body = kvp.Value.Body, IsDone = kvp.Value.IsDone });
            }
            CanAddNote = Notes.Count < 4;
        }

        [RelayCommand]
        public async Task SaveCycleAsync()
        {
            var userId = _userService.UserId;
            try
            {
                await _trackerService.UpdatePeriodTrackerAsync(userId, StartPeriodDate ?? DateTimeOffset.Now, CycleDays, PeriodLasts, PmsOption);

                HasPeriodTracker = true;

                OnPropertyChanged(nameof(DashboardVisibility));
                OnPropertyChanged(nameof(EmptyStateVisibility));

                await LoadDashboardAsync();
                await _dialogPresenter.ShowMessageAsync("Success", "Cycle configuration saved.");
            }
            catch (Exception ex)
            {
                await _dialogPresenter.ShowMessageAsync("Save Failed", ex.Message);
            }
        }

        public async Task AddNoteAsync()
        {
            if (string.IsNullOrWhiteSpace(NewNoteText) || !CanAddNote) return;
            try
            {
                await _trackerService.AddNoteAsync(_userService.UserId, NewNoteText);
                NewNoteText = string.Empty;
                await LoadNotesAsync();
            }
            catch (Exception ex) { await _dialogPresenter.ShowMessageAsync("Note Failed", ex.Message); }
        }

        public async Task RemoveNoteAsync(PeriodTrackerNoteItemViewModel note)
        {
            if (note == null) return;
            try
            {
                await _trackerService.DeleteNoteAsync(_userService.UserId, note.NoteId);
                await LoadNotesAsync();
            }
            catch (Exception ex)
            {
                await _dialogPresenter.ShowMessageAsync("Delete Failed", ex.Message);
            }
        }

        public async Task ToggleNoteAsync(PeriodTrackerNoteItemViewModel note)
        {
            if (note == null) return;
            await _trackerService.UpdateNoteAsync(_userService.UserId, note.NoteId, note.Body, note.IsDone ?? false);
            await LoadNotesAsync();
        }

        public async Task ChangeMonthAsync(string direction)
        {
            _currentMonthOffset += direction == "Next" ? 1 : -1;
            await LoadDashboardAsync();
        }
    }
}