using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Patient;

public partial class ConsultationsViewModel : ObservableObject
{
    private readonly IPatientService patientService;
    private readonly IBillingService billingService;

    [ObservableProperty] private string patientIdInput = string.Empty;
    [ObservableProperty] private string recordIdInput = string.Empty;
    [ObservableProperty] private string patientName = string.Empty;
    [ObservableProperty] private string basePrice = string.Empty;
    [ObservableProperty] private string prescriptionInfo = string.Empty;
    [ObservableProperty] private string statusMessage = string.Empty;

    public ConsultationsViewModel(IPatientService patientService, IBillingService billingService)
    {
        this.patientService = patientService;
        this.billingService = billingService;
    }

    [RelayCommand]
    private async Task LoadConsultationAsync()
    {
        StatusMessage = string.Empty;
        PatientName = string.Empty;
        BasePrice = string.Empty;
        PrescriptionInfo = string.Empty;

        if (!int.TryParse(PatientIdInput, out int patientId))
        {
            StatusMessage = "Please enter a valid Patient Id.";
            return;
        }

        if (!int.TryParse(RecordIdInput, out int recordId))
        {
            StatusMessage = "Please enter a valid Record Id.";
            return;
        }

        try
        {
            Data.Models.Patient patient = await patientService.GetPatientDetailsAsync(patientId);
            PatientName = patient.FullName;

            decimal computed = await billingService.ComputeBasePriceAsync(patientId, recordId);
            BasePrice = computed.ToString("C");

            Prescription? prescription = await patientService.GetPrescriptionByRecordIdAsync(recordId);
            PrescriptionInfo = prescription is null
                ? "No prescription for this record."
                : $"Prescription #{prescription.PrescriptionId}";

            StatusMessage = "Consultation loaded.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
