using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using PatientModel = Hospital.Data.Models.Patient;

namespace Hospital.Desktop.ViewModels.Patient;

public partial class BloodDonorsViewModel : ObservableObject
{
    private readonly IBloodCompatibilityService bloodCompatibilityService;
    private readonly IPatientService patientService;

    [ObservableProperty] private ObservableCollection<PatientModel> patients = new ObservableCollection<PatientModel>();
    [ObservableProperty] private ObservableCollection<DonorMatchRow> donors = new ObservableCollection<DonorMatchRow>();
    [ObservableProperty] private PatientModel? selectedPatient;
    [ObservableProperty] private string statusMessage = string.Empty;

    public BloodDonorsViewModel(IBloodCompatibilityService bloodCompatibilityService, IPatientService patientService)
    {
        this.bloodCompatibilityService = bloodCompatibilityService;
        this.patientService = patientService;
    }

    [RelayCommand]
    private async Task LoadPatientsAsync()
    {
        Patients.Clear();
        StatusMessage = string.Empty;
        try
        {
            var result = await patientService.GetPatientsAsync();
            foreach (var patient in result)
            {
                Patients.Add(patient);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoadCompatibleDonorsAsync()
    {
        Donors.Clear();
        StatusMessage = string.Empty;

        if (SelectedPatient == null)
        {
            StatusMessage = "Please select a patient first.";
            return;
        }

        try
        {
            var topDonors = await bloodCompatibilityService.GetTopCompatibleDonorsAsync(SelectedPatient.PatientId);
            foreach (var donor in topDonors)
            {
                Donors.Add(new DonorMatchRow
                {
                    PatientId = donor.PatientId,
                    FullName = $"{donor.FirstName} {donor.LastName}",
                    Cnp = donor.Cnp,
                });
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}

public class DonorMatchRow
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Cnp { get; set; } = string.Empty;
}
