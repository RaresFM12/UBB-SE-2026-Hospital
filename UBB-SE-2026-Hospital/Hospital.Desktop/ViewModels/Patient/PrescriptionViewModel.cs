using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Desktop.Proxy;
using Hospital.Data.Models;

namespace Hospital.Desktop.ViewModels.Patient;

public partial class PrescriptionViewModel : ObservableObject
{
    private readonly HttpPrescriptionProxy prescriptionService;

    [ObservableProperty] private ObservableCollection<Prescription> prescriptions = new ObservableCollection<Prescription>();
    [ObservableProperty] private string statusMessage = string.Empty;

    public PrescriptionViewModel(HttpPrescriptionProxy prescriptionService)
    {
        this.prescriptionService = prescriptionService;
    }

    [RelayCommand]
    private async Task LoadPrescriptionsAsync()
    {
        Prescriptions.Clear();
        StatusMessage = string.Empty;
        var latestPrescriptions = await prescriptionService.GetLatestPrescriptionsAsync();
        foreach (var prescription in latestPrescriptions)
        {
            Prescriptions.Add(prescription);
        }

        StatusMessage = $"Loaded {Prescriptions.Count} prescription(s).";
    }
}
