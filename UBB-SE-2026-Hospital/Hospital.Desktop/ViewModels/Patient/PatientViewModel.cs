using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;
using Microsoft.UI.Xaml;
using PatientModel = Hospital.Data.Models.Patient;

namespace Hospital.Desktop.ViewModels.Patient;

public partial class PatientViewModel : ObservableObject
{
    private readonly IPatientService patientService;
    private readonly IBillingService billingService;
    private bool isRefreshingSelectedPatient;

    [ObservableProperty] private ObservableCollection<PatientModel> patients = new ObservableCollection<PatientModel>();
    [ObservableProperty] private PatientModel? selectedPatient;
    [ObservableProperty] private bool showDeceasedArchive;
    [ObservableProperty] private DateTimeOffset selectedDeathDate = DateTimeOffset.Now;
    [ObservableProperty] private MedicalHistory? medicalHistory;
    [ObservableProperty] private ObservableCollection<MedicalRecord> medicalRecords = new ObservableCollection<MedicalRecord>();
    [ObservableProperty] private ObservableCollection<string> allergies = new ObservableCollection<string>();
    [ObservableProperty] private MedicalRecord? selectedMedicalRecord;
    [ObservableProperty] private string searchQuery = string.Empty;
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private decimal basePrice;
    [ObservableProperty] private decimal finalPrice;
    [ObservableProperty] private bool discountApplied;
    [ObservableProperty] private int discountPercentage;
    [ObservableProperty] private string billingStatusMessage = string.Empty;

    public string ArchiveSectionTitle => ShowDeceasedArchive ? "Deceased" : "Patients";

    public Visibility ActivePatientActionsVisibility
        => HasSelectedPatient && !ShowDeceasedArchive ? Visibility.Visible : Visibility.Collapsed;

    public Visibility DeceasedArchiveActionsVisibility
        => HasSelectedPatient && ShowDeceasedArchive ? Visibility.Visible : Visibility.Collapsed;

    public bool HasSelectedMedicalRecord => SelectedMedicalRecord is not null;

    public bool HasNoSelectedMedicalRecord => SelectedMedicalRecord is null;

    public bool HasSelectedPatient => SelectedPatient is not null;

    public bool HasAllergies => Allergies.Count > 0;

    public bool HasNoAllergies => Allergies.Count == 0;

    public string ChronicConditionsFormatted
        => MedicalHistory?.ChronicConditions is { Count: > 0 }
            ? string.Join(", ", MedicalHistory.ChronicConditions)
            : "None";

    public Visibility SelectedRecordVisibility
        => SelectedMedicalRecord is null ? Visibility.Collapsed : Visibility.Visible;

    public Visibility EmptySelectionVisibility
        => SelectedMedicalRecord is null ? Visibility.Visible : Visibility.Collapsed;

    public Visibility DiscountAppliedVisibility
        => DiscountApplied ? Visibility.Visible : Visibility.Collapsed;

    public Visibility DiscountActionVisibility
        => SelectedMedicalRecord is not null && !DiscountApplied ? Visibility.Visible : Visibility.Collapsed;

    public string BasePriceDisplay => $"{BasePrice:0.00} lei";

    public string FinalPriceDisplay => $"{FinalPrice:0.00} lei";

    public string DiscountSummary => DiscountApplied
        ? $"Discount applied: {DiscountPercentage}%"
        : "No discount applied yet.";

    public string StaffDisplay
        => SelectedMedicalRecord?.StaffMember is null
            ? "Unavailable"
            : $"{SelectedMedicalRecord.StaffMember.FirstName} {SelectedMedicalRecord.StaffMember.LastName}";

    public PatientViewModel(IPatientService patientService, IBillingService billingService)
    {
        this.patientService = patientService;
        this.billingService = billingService;
    }

    partial void OnSelectedPatientChanged(PatientModel? value)
    {
        OnPropertyChanged(nameof(SelectedRecordVisibility));
        OnPropertyChanged(nameof(EmptySelectionVisibility));
        OnPropertyChanged(nameof(HasSelectedPatient));
        OnPropertyChanged(nameof(ActivePatientActionsVisibility));
        OnPropertyChanged(nameof(DeceasedArchiveActionsVisibility));

        if (isRefreshingSelectedPatient)
        {
            return;
        }

        if (value != null)
        {
            _ = LoadSelectedPatientDetailsAsync(value.PatientId);
        }
        else
        {
            MedicalHistory = null;
            MedicalRecords.Clear();
            Allergies.Clear();
            SelectedMedicalRecord = null;
        }
    }

    partial void OnSelectedMedicalRecordChanged(MedicalRecord? value)
    {
        OnPropertyChanged(nameof(SelectedRecordVisibility));
        OnPropertyChanged(nameof(EmptySelectionVisibility));
        OnPropertyChanged(nameof(StaffDisplay));
        OnPropertyChanged(nameof(HasSelectedMedicalRecord));
        OnPropertyChanged(nameof(HasNoSelectedMedicalRecord));

        if (value == null)
        {
            BasePrice = 0;
            FinalPrice = 0;
            DiscountApplied = false;
            DiscountPercentage = 0;
            BillingStatusMessage = string.Empty;
            RaiseBillingPropertyChanges();
            return;
        }

        _ = LoadBillingForRecordAsync(value);
    }

    partial void OnMedicalHistoryChanged(MedicalHistory? value)
        => OnPropertyChanged(nameof(ChronicConditionsFormatted));

    partial void OnBasePriceChanged(decimal value) => OnPropertyChanged(nameof(BasePriceDisplay));

    partial void OnFinalPriceChanged(decimal value) => OnPropertyChanged(nameof(FinalPriceDisplay));

    partial void OnDiscountAppliedChanged(bool value)
    {
        OnPropertyChanged(nameof(DiscountAppliedVisibility));
        OnPropertyChanged(nameof(DiscountActionVisibility));
        OnPropertyChanged(nameof(DiscountSummary));
    }

    partial void OnDiscountPercentageChanged(int value)
        => OnPropertyChanged(nameof(DiscountSummary));

    partial void OnShowDeceasedArchiveChanged(bool value)
    {
        OnPropertyChanged(nameof(ArchiveSectionTitle));
        OnPropertyChanged(nameof(ActivePatientActionsVisibility));
        OnPropertyChanged(nameof(DeceasedArchiveActionsVisibility));
        _ = LoadPatientsAsync();
    }

    [RelayCommand]
    private async Task LoadPatientsAsync()
    {
        Patients.Clear();
        StatusMessage = string.Empty;
        try
        {
            IReadOnlyList<PatientModel> result = string.IsNullOrWhiteSpace(SearchQuery)
                ? await patientService.GetPatientsAsync()
                : await patientService.SearchPatientsAsync(new SearchPatientsRequest { NamePart = SearchQuery.Trim() });

            foreach (var patient in result.Where(p => ShowDeceasedArchive ? p.IsDeceased : !p.IsDeceased))
            {
                Patients.Add(patient);
            }

            if (Patients.Count == 0)
            {
                StatusMessage = "No patients found matching your search.";
            }
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    public async Task LoadPatientProfileAsync(int patientId)
    {
        SelectedMedicalRecord = null;
        DiscountApplied = false;
        DiscountPercentage = 0;
        BasePrice = 0;
        FinalPrice = 0;
        BillingStatusMessage = string.Empty;

        PatientModel details = await patientService.GetPatientDetailsAsync(patientId);

        // Set the patient without triggering OnSelectedPatientChanged's auto-load,
        // otherwise the records load twice concurrently and the list shows duplicates.
        isRefreshingSelectedPatient = true;
        SelectedPatient = details;
        isRefreshingSelectedPatient = false;

        await LoadSelectedPatientDetailsAsync(patientId);
    }

    public async Task UpdateSelectedPatientAsync()
    {
        if (SelectedPatient is null)
        {
            return;
        }

        try
        {
            await patientService.UpdatePatientAsync(SelectedPatient.PatientId, new UpdatePatientRequest
            {
                FirstName = SelectedPatient.FirstName,
                LastName = SelectedPatient.LastName,
                Cnp = SelectedPatient.Cnp,
                DateOfBirth = SelectedPatient.DateOfBirth,
                Sex = SelectedPatient.Sex,
                PhoneNumber = SelectedPatient.PhoneNumber,
                EmergencyContact = SelectedPatient.EmergencyContact,
                IsDonor = SelectedPatient.IsDonor,
                Transferred = SelectedPatient.Transferred,
                DateOfDeath = SelectedPatient.DateOfDeath,
                IsArchived = SelectedPatient.IsArchived,
            });

            StatusMessage = "Patient updated successfully.";
            await LoadPatientsAsync();
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error updating patient: {ex.Message}";
        }
    }

    public async Task ArchiveSelectedPatientAsync()
    {
        if (SelectedPatient is null)
        {
            return;
        }

        try
        {
            await patientService.ArchivePatientAsync(SelectedPatient.PatientId);
            StatusMessage = "Patient archived successfully.";
            await LoadPatientsAsync();
            MedicalHistory = null;
            MedicalRecords.Clear();
            Allergies.Clear();
            SelectedPatient = null;
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error archiving patient: {ex.Message}";
        }
    }

    public async Task MarkSelectedPatientAsDeceasedAsync()
    {
        if (SelectedPatient is null)
        {
            return;
        }

        try
        {
            await patientService.ArchiveAsDeceasedAsync(SelectedPatient.PatientId, new ArchiveAsDeceasedRequest
            {
                DeathDate = SelectedDeathDate.DateTime,
            });
            StatusMessage = "Patient marked as deceased and moved to archive.";
            await LoadPatientsAsync();
            MedicalHistory = null;
            MedicalRecords.Clear();
            Allergies.Clear();
            SelectedPatient = null;
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task LoadSelectedPatientDetailsAsync(int patientId)
    {
        try
        {
            PatientModel details = await patientService.GetPatientDetailsAsync(patientId);
            MedicalHistory? history = await patientService.GetMedicalHistoryAsync(patientId);
            List<string> allergies = await patientService.GetPatientAllergiesAsync(patientId);

            if (SelectedPatient?.PatientId != patientId)
            {
                return;
            }

            isRefreshingSelectedPatient = true;
            details.MedicalHistory = history ?? details.MedicalHistory;
            SelectedPatient = details;
            isRefreshingSelectedPatient = false;

            MedicalHistory = details.MedicalHistory;
            Allergies.Clear();
            foreach (string allergy in allergies.OrderBy(a => a))
            {
                Allergies.Add(allergy);
            }
            OnPropertyChanged(nameof(HasAllergies));
            OnPropertyChanged(nameof(HasNoAllergies));

            await LoadMedicalRecordsAsync(details.MedicalHistory?.MedicalHistoryId ?? 0);
        }
        catch (System.Exception ex)
        {
            isRefreshingSelectedPatient = false;
            StatusMessage = $"Error loading patient details: {ex.Message}";
        }
    }

    private async Task LoadMedicalRecordsAsync(int medicalHistoryId)
    {
        MedicalRecords.Clear();
        SelectedMedicalRecord = null;

        if (medicalHistoryId <= 0)
        {
            return;
        }

        try
        {
            List<MedicalRecord> records = await patientService.GetMedicalRecordsAsync(medicalHistoryId);
            foreach (var record in records.OrderByDescending(record => record.ConsultationDate))
            {
                MedicalRecords.Add(record);
            }
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error loading medical records: {ex.Message}";
        }
    }

    private async Task LoadBillingForRecordAsync(MedicalRecord record)
    {
        if (SelectedPatient is null)
        {
            return;
        }

        BillingStatusMessage = string.Empty;

        try
        {
            BasePrice = await billingService.ComputeBasePriceAsync(SelectedPatient.PatientId, record.RecordId);
            FinalPrice = record.FinalPrice > 0 ? record.FinalPrice : BasePrice;
            DiscountPercentage = record.DiscountApplied ?? 0;
            DiscountApplied = record.DiscountApplied.HasValue;
            RaiseBillingPropertyChanges();
        }
        catch (System.Exception ex)
        {
            BillingStatusMessage = $"Error loading billing info: {ex.Message}";
        }
    }

    public async Task<Prescription?> GetSelectedPrescriptionAsync()
    {
        if (SelectedMedicalRecord is null)
        {
            return null;
        }

        return await patientService.GetPrescriptionByRecordIdAsync(SelectedMedicalRecord.RecordId);
    }

    public async Task ApplyDiscountAsync(int discountPercent)
    {
        if (SelectedMedicalRecord is null)
        {
            return;
        }

        try
        {
            decimal discountedPrice = await billingService.ApplyDiscountAsync(BasePrice, discountPercent);
            SelectedMedicalRecord.DiscountApplied = discountPercent;
            SelectedMedicalRecord.FinalPrice = discountedPrice;
            FinalPrice = discountedPrice;
            DiscountPercentage = discountPercent;
            DiscountApplied = true;
            BillingStatusMessage = "Discount applied successfully.";
            RaiseBillingPropertyChanges();
        }
        catch (System.Exception ex)
        {
            BillingStatusMessage = $"Error applying discount: {ex.Message}";
        }
    }

    private void RaiseBillingPropertyChanges()
    {
        OnPropertyChanged(nameof(BasePriceDisplay));
        OnPropertyChanged(nameof(FinalPriceDisplay));
        OnPropertyChanged(nameof(DiscountSummary));
        OnPropertyChanged(nameof(DiscountAppliedVisibility));
        OnPropertyChanged(nameof(DiscountActionVisibility));
    }
}
