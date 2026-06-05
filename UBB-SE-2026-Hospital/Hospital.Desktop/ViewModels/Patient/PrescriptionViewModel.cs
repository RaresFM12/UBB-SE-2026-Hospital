using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Desktop.Proxy;
using Hospital.Data.Models;
using Hospital.Data.Models;

namespace Hospital.Desktop.ViewModels.Patient;

public partial class PrescriptionViewModel : ObservableObject
{
    private readonly HttpPrescriptionProxy prescriptionService;
    private const int PageSize = 9;
    private int loadVersion;

    [ObservableProperty] private ObservableCollection<Prescription> prescriptions = new ObservableCollection<Prescription>();
    [ObservableProperty] private string infoMessage = string.Empty;
    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private string? searchIdText;
    [ObservableProperty] private string? searchName;
    [ObservableProperty] private string? searchMedication;
    [ObservableProperty] private DateTimeOffset? dateFrom;
    [ObservableProperty] private DateTimeOffset? dateTo;

    public PrescriptionFilter ActiveFilter { get; private set; } = new();

    public PrescriptionViewModel(HttpPrescriptionProxy prescriptionService)
    {
        this.prescriptionService = prescriptionService;
    }

    [RelayCommand]
    private async Task LoadPrescriptionsAsync()
    {
        CurrentPage = 1;
        ActiveFilter = new PrescriptionFilter();
        await UpdatePageDataAsync();
    }

    [RelayCommand]
    private async Task ApplyFilterAsync()
    {
        InfoMessage = string.Empty;
        CurrentPage = 1;
        ActiveFilter = new PrescriptionFilter
        {
            PrescriptionId = TryParseNullableInt(SearchIdText),
            MedicationName = Normalize(SearchMedication),
            DateFrom = DateFrom?.DateTime,
            DateTo = DateTo?.DateTime,
            PatientName = Normalize(SearchName),
            DoctorName = Normalize(SearchName),
        };

        await UpdatePageDataAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (Prescriptions.Count != PageSize)
        {
            return;
        }

        CurrentPage++;
        await UpdatePageDataAsync();
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (CurrentPage <= 1)
        {
            return;
        }

        CurrentPage--;
        await UpdatePageDataAsync();
    }

    private async Task UpdatePageDataAsync()
    {
        int currentLoad = ++loadVersion;
        InfoMessage = string.Empty;

        bool hasFilter =
            ActiveFilter.PrescriptionId.HasValue ||
            !string.IsNullOrWhiteSpace(ActiveFilter.MedicationName) ||
            ActiveFilter.DateFrom.HasValue ||
            ActiveFilter.DateTo.HasValue ||
            !string.IsNullOrWhiteSpace(ActiveFilter.PatientName) ||
            !string.IsNullOrWhiteSpace(ActiveFilter.DoctorName);

        List<Prescription> results =
            hasFilter
                ? (await prescriptionService.ApplyFilterAsync(ActiveFilter))
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList()
                : await prescriptionService.GetLatestPrescriptionsAsync(PageSize, CurrentPage);

        if (currentLoad != loadVersion)
        {
            return;
        }

        Prescriptions.Clear();
        foreach (Prescription prescription in results)
        {
            Prescriptions.Add(prescription);
        }

        if (Prescriptions.Count == 0)
        {
            InfoMessage = hasFilter
                ? "No prescriptions found matching those criteria."
                : "No prescriptions available right now.";
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? TryParseNullableInt(string? value)
        => int.TryParse(value, out int parsed) ? parsed : null;
}
