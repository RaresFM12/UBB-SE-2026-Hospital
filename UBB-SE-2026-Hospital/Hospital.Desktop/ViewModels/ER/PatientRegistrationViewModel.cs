using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;
using Microsoft.UI.Xaml.Controls;

namespace Hospital.Desktop.ViewModels.ER;

public partial class PatientRegistrationViewModel : ObservableObject
{
    private readonly IPatientService patientService;
    private readonly IERVisitService erVisitService;

    [ObservableProperty] private string patientCnp = string.Empty;
    [ObservableProperty] private string firstName = string.Empty;
    [ObservableProperty] private string lastName = string.Empty;
    [ObservableProperty] private DateTimeOffset dateOfBirth = DateTimeOffset.Now;
    [ObservableProperty] private bool hasDateOfBirth;
    [ObservableProperty] private string gender = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string emergencyContact = string.Empty;
    [ObservableProperty] private string chiefComplaint = string.Empty;

    [ObservableProperty] private string patientCnpError = string.Empty;
    [ObservableProperty] private string firstNameError = string.Empty;
    [ObservableProperty] private string lastNameError = string.Empty;
    [ObservableProperty] private string dateOfBirthError = string.Empty;
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
    partial void OnDateOfBirthChanged(DateTimeOffset value) => ValidateAll();
    partial void OnHasDateOfBirthChanged(bool value) => ValidateAll();
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
        else if (!Regex.IsMatch(PatientCnp, @"^\d{13}$"))
        {
            if (submitAttempted) PatientCnpError = "CNP must be exactly 13 digits.";
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

        if (!HasDateOfBirth)
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

        if (string.IsNullOrWhiteSpace(Phone))
        {
            if (submitAttempted) PhoneError = "Phone number is required.";
            valid = false;
        }
        else if (!Regex.IsMatch(Phone, @"^07\d{8}$"))
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
        else if (!Regex.IsMatch(EmergencyContact, @"^[A-Za-z\s]+ - 07\d{8}$"))
        {
            if (submitAttempted) EmergencyContactError = "Format: Firstname Lastname - 07XXXXXXXX";
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
            bool patientExists = await patientService.ExistsAsync(PatientCnp);
            int patientId = 0;
            if (!patientExists)
            {
                var created = await patientService.CreatePatientAsync(new CreatePatientRequest
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Cnp = PatientCnp,
                    DateOfBirth = DateOfBirth.DateTime,
                    Sex = Gender.Equals("Female", StringComparison.OrdinalIgnoreCase) ? Sex.F : Sex.M,
                    PhoneNumber = Phone,
                    EmergencyContact = EmergencyContact,
                    IsDonor = false,
                });
                patientId = created.PatientId;
            }
            else
            {
                var existing = (await patientService.SearchPatientsAsync(new SearchPatientsRequest { Cnp = PatientCnp })).FirstOrDefault();
                patientId = existing?.PatientId ?? 0;
            }

            var visit = new ERVisit
            {
                Patient = new Patient { PatientId = patientId, Cnp = PatientCnp, FirstName = FirstName, LastName = LastName },
                ChiefComplaint = ChiefComplaint,
                ArrivalDateTime = DateTime.Now,
                Status = ERVisit.VisitStatus.REGISTERED,
            };

            var createdVisit = await erVisitService.CreateAsync(visit);
            await ShowDialog("Registration Successful",
                $"Patient ID: {PatientCnp}\nVisit ID: {createdVisit.VisitId}\nStatus: {createdVisit.Status}");
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
        DateOfBirth = DateTimeOffset.Now;
        HasDateOfBirth = false;
        Gender = string.Empty;
        Phone = string.Empty;
        EmergencyContact = string.Empty;
        ChiefComplaint = string.Empty;
        PatientCnpError = string.Empty;
        FirstNameError = string.Empty;
        LastNameError = string.Empty;
        DateOfBirthError = string.Empty;
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
