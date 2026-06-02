using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Patient;

public partial class PrescriptionViewModel : ObservableObject
{
    private readonly IPrescriptionService prescriptionService;

    [ObservableProperty] private ObservableCollection<Prescription> prescriptions = new ObservableCollection<Prescription>();
    [ObservableProperty] private string statusMessage = string.Empty;

    public PrescriptionViewModel(IPrescriptionService prescriptionService)
    {
        this.prescriptionService = prescriptionService;
    }

    [RelayCommand]
    private async Task LoadPrescriptionsAsync()
    {
        Prescriptions.Clear();
        StatusMessage = string.Empty;
        // The current IPrescriptionService only has sync methods for pharmacy items.
        // For a full prescription list we'd need an async method; stub for now.
        await Task.CompletedTask;
        StatusMessage = "Prescription list loaded (stub).";
    }
}
