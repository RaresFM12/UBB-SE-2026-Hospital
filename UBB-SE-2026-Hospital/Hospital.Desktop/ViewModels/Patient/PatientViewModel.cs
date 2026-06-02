using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Patient;

public partial class PatientViewModel : ObservableObject
{
    private readonly IPatientService patientService;

    [ObservableProperty] private ObservableCollection<Patient> patients = new ObservableCollection<Patient>();
    [ObservableProperty] private Patient? selectedPatient;
    [ObservableProperty] private MedicalHistory? medicalHistory;
    [ObservableProperty] private ObservableCollection<MedicalRecord> medicalRecords = new ObservableCollection<MedicalRecord>();
    [ObservableProperty] private string searchQuery = string.Empty;
    [ObservableProperty] private string statusMessage = string.Empty;

    public PatientViewModel(IPatientService patientService)
    {
        this.patientService = patientService;
    }

    partial void OnSelectedPatientChanged(Patient? value)
    {
        if (value != null)
        {
            MedicalHistory = value.MedicalHistory;
            _ = LoadMedicalRecordsAsync(value.PatientId);
        }
    }

    [RelayCommand]
    private async Task LoadPatientsAsync()
    {
        Patients.Clear();
        StatusMessage = string.Empty;
        try
        {
            var result = await patientService.SearchPatientsAsync(new SearchPatientsRequest { NamePart = SearchQuery });
            foreach (var patient in result)
            {
                Patients.Add(patient);
            }
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task LoadMedicalRecordsAsync(int patientId)
    {
        MedicalRecords.Clear();
        try
        {
            var history = await patientService.GetMedicalHistoryAsync(patientId);
            if (history?.MedicalRecords != null)
            {
                foreach (var record in history.MedicalRecords)
                {
                    MedicalRecords.Add(record);
                }
            }
        }
        catch { }
    }
}
