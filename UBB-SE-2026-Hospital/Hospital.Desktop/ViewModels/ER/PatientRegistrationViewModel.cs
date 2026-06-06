using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Microsoft.UI.Xaml.Controls;
using PatientModel = Hospital.Data.Models.Patient;

namespace Hospital.Desktop.ViewModels.ER;

public partial class PatientRegistrationViewModel : ObservableObject
{
    private const int CnpLength = 13;
    private const string CnpPattern = @"^\d{13}$";
    private const string PhonePattern = @"^07\d{8}$";
    private const string EmergencyContactPattern = @"^[A-Za-z\s'-]+(?:\s-\s|\s)07\d{8}$";

    private readonly IPatientService patientService;
    private readonly IERVisitService erVisitService;

    [ObservableProperty] private string patientCnp = string.Empty;
    [ObservableProperty] private string firstName = string.Empty;
    [ObservableProperty] private string lastName = string.Empty;
    [ObservableProperty] private DateTimeOffset? dateOfBirth;
    [ObservableProperty] private string gender = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string emergencyContact = string.Empty;
    [ObservableProperty] private string chiefComplaint = string.Empty;

    [ObservableProperty] private string patientCnpError = string.Empty;
    [ObservableProperty] private string firstNameError = string.Empty;
    [ObservableProperty] private string lastNameError = string.Empty;
    [ObservableProperty] private string dateOfBirthError = string.Empty;
    [ObservableProperty] private string genderError = string.Empty;
    [ObservableProperty] private string phoneError = string.Empty;
    [ObservableProperty] private string emergencyContactError = string.Empty;
    [ObservableProperty] private string chiefComplaintError = string.Empty;

    private bool submitAttempted;

    public PatientRegistrationViewModel(IPatientService patientService, IERVisitService erVisitService)
    {
        this.patientService = patientService;
        this.erVisitService = erVisitService;
    }

    partial void OnPatientCnpChanged(string value) => ValidateAll();
    partial void OnFirstNameChanged(string value) => ValidateAll();
    partial void OnLastNameChanged(string value) => ValidateAll();
    partial void OnDateOfBirthChanged(DateTimeOffset? value) => ValidateAll();
    partial void OnGenderChanged(string value) => ValidateAll();
    partial void OnPhoneChanged(string value) => ValidateAll();
    partial void OnEmergencyContactChanged(string value) => ValidateAll();
    partial void OnChiefComplaintChanged(string value) => ValidateAll();

    private bool ValidateAll()
    {
        bool valid = true;

        if (string.IsNullOrWhiteSpace(PatientCnp))
        {
            if (submitAttempted) PatientCnpError = "Patient ID (CNP) is required.";
            valid = false;
        }
        else if (!Regex.IsMatch(PatientCnp.Trim(), CnpPattern))
        {
            if (submitAttempted) PatientCnpError = $"CNP must be exactly {CnpLength} digits.";
            valid = false;
        }
        else { PatientCnpError = string.Empty; }

        if (string.IsNullOrWhiteSpace(FirstName))
        {
            if (submitAttempted) FirstNameError = "First name is required.";
            valid = false;
        }
        else { FirstNameError = string.Empty; }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            if (submitAttempted) LastNameError = "Last name is required.";
            valid = false;
        }
        else { LastNameError = string.Empty; }

        if (DateOfBirth is null)
        {
            if (submitAttempted) DateOfBirthError = "Date of birth is required.";
            valid = false;
        }
        else if (DateOfBirth >= DateTimeOffset.Now)
        {
            if (submitAttempted) DateOfBirthError = "Date of birth must be in the past.";
            valid = false;
        }
        else { DateOfBirthError = string.Empty; }

        if (string.IsNullOrWhiteSpace(Gender))
        {
            if (submitAttempted) GenderError = "Gender is required.";
            valid = false;
        }
        else if (!string.Equals(Gender, "Male", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(Gender, "Female", StringComparison.OrdinalIgnoreCase))
        {
            if (submitAttempted) GenderError = "Gender must be Male or Female.";
            valid = false;
        }
        else { GenderError = string.Empty; }

        if (string.IsNullOrWhiteSpace(Phone))
        {
            if (submitAttempted) PhoneError = "Phone number is required.";
            valid = false;
        }
        else if (!Regex.IsMatch(Phone.Trim(), PhonePattern))
        {
            if (submitAttempted) PhoneError = "Phone must be in format 07XXXXXXXX.";
            valid = false;
        }
        else { PhoneError = string.Empty; }

        if (string.IsNullOrWhiteSpace(EmergencyContact))
        {
            if (submitAttempted) EmergencyContactError = "Emergency contact is required.";
            valid = false;
        }
        else if (!Regex.IsMatch(EmergencyContact.Trim(), EmergencyContactPattern))
        {
            if (submitAttempted) EmergencyContactError = "Format: Firstname Lastname - 07XXXXXXXX or Firstname Lastname 07XXXXXXXX";
            valid = false;
        }
        else { EmergencyContactError = string.Empty; }

        if (string.IsNullOrWhiteSpace(ChiefComplaint))
        {
            if (submitAttempted) ChiefComplaintError = "Chief complaint is required.";
            valid = false;
        }
        else { ChiefComplaintError = string.Empty; }

        return valid;
    }

    [RelayCommand]
    private async Task RegisterPatientAndVisit()
    {
        submitAttempted = true;
        if (!ValidateAll())
        {
            await ShowDialog("Invalid Data", "Some fields are missing or incorrect.\nPlease check the highlighted fields and try again.");
            return;
        }

        try
        {
            string patientCnp = PatientCnp.Trim();
            string firstName = FirstName.Trim();
            string lastName = LastName.Trim();
            string phone = Phone.Trim();
            string emergencyContact = EmergencyContact.Trim();
            string chiefComplaint = ChiefComplaint.Trim();
            Sex sex = Gender.Trim().Equals("Female", StringComparison.OrdinalIgnoreCase) ? Sex.F : Sex.M;

            bool patientExists = await patientService.ExistsAsync(patientCnp);
            int patientId = 0;
            PatientModel? patient = null;
            if (!patientExists)
            {
                patient = await patientService.CreatePatientAsync(new CreatePatientRequest
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Cnp = patientCnp,
                    DateOfBirth = DateOfBirth!.Value.DateTime,
                    Sex = sex,
                    PhoneNumber = phone,
                    EmergencyContact = emergencyContact,
                    IsDonor = false,
                });
                patientId = patient.PatientId;
            }
            else
            {
                patient = (await patientService.SearchPatientsAsync(new SearchPatientsRequest { Cnp = patientCnp })).FirstOrDefault();
                patientId = patient?.PatientId ?? 0;
            }

            if (patient is null || patientId <= 0)
            {
                throw new InvalidOperationException("Patient could not be loaded after registration.");
            }

            var visit = new ERVisit
            {
                Patient = patient,
                ChiefComplaint = chiefComplaint,
                ArrivalDateTime = DateTime.Now,
                Status = ERVisit.VisitStatus.REGISTERED,
            };

            var createdVisit = await erVisitService.CreateAsync(visit);
            await ShowDialog("Registration Successful",
                $"Patient ID: {patientCnp}\nVisit ID: {createdVisit.VisitId}\nStatus: {createdVisit.Status}");
            ClearForm();
        }
        catch (Exception ex)
        {
            await ShowDialog("Registration Failed", ex.Message);
        }
    }

    [RelayCommand]
    public void ClearForm()
    {
        PatientCnp = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        DateOfBirth = null;
        Gender = string.Empty;
        Phone = string.Empty;
        EmergencyContact = string.Empty;
        ChiefComplaint = string.Empty;
        PatientCnpError = string.Empty;
        FirstNameError = string.Empty;
        LastNameError = string.Empty;
        DateOfBirthError = string.Empty;
        GenderError = string.Empty;
        PhoneError = string.Empty;
        EmergencyContactError = string.Empty;
        ChiefComplaintError = string.Empty;
        submitAttempted = false;
    }

    private async Task ShowDialog(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = ((App)Application.Current).CurrentWindow?.Content?.XamlRoot,
        };
        await dialog.ShowAsync();
    }
}
