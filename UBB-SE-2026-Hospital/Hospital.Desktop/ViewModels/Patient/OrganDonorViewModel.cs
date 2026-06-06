using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Patient;

public sealed class DonorRow
{
    public int PatientId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string BloodType { get; init; } = "-";
    public string Rh { get; init; } = "-";
    public string Score { get; init; } = string.Empty;
}

public partial class OrganDonorViewModel : ObservableObject
{
    private readonly IBloodCompatibilityService bloodCompatibilityService;
    private readonly ITransplantService transplantService;

    [ObservableProperty] private int recipientPatientId;
    [ObservableProperty] private string selectedOrgan = "Kidney";
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private ObservableCollection<DonorRow> donors = new ObservableCollection<DonorRow>();

    public ObservableCollection<string> Organs { get; } = new ObservableCollection<string>
    {
        "Kidney", "Heart", "Liver", "Lung", "Pancreas", "Cornea",
    };

    public OrganDonorViewModel(
        IBloodCompatibilityService bloodCompatibilityService,
        ITransplantService transplantService)
    {
        this.bloodCompatibilityService = bloodCompatibilityService;
        this.transplantService = transplantService;
    }

    [RelayCommand]
    private async Task FindDonorsAsync()
    {
        Donors.Clear();
        StatusMessage = string.Empty;

        if (RecipientPatientId <= 0)
        {
            StatusMessage = "Please enter a valid recipient patient id.";
            return;
        }

        try
        {
            var compatible = await bloodCompatibilityService.GetTopCompatibleDonorsAsync(RecipientPatientId);
            foreach (var patient in compatible)
            {
                Donors.Add(new DonorRow
                {
                    PatientId = patient.PatientId,
                    FullName = $"{patient.FirstName} {patient.LastName}",
                    BloodType = patient.MedicalHistory?.BloodType?.ToString() ?? "-",
                    Rh = patient.MedicalHistory?.Rh?.ToString() ?? "-",
                    Score = string.Empty,
                });
            }

            StatusMessage = $"Found {Donors.Count} compatible donor(s) for patient {RecipientPatientId}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddToWaitlistAsync()
    {
        StatusMessage = string.Empty;

        if (RecipientPatientId <= 0)
        {
            StatusMessage = "Please enter a valid recipient patient id.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedOrgan))
        {
            StatusMessage = "Please choose an organ first.";
            return;
        }

        try
        {
            await transplantService.CreateWaitlistRequestAsync(RecipientPatientId, SelectedOrgan);
            StatusMessage = $"Added patient {RecipientPatientId} to the {SelectedOrgan} waitlist.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AssignDonorAsync(DonorRow? donor)
    {
        StatusMessage = string.Empty;

        if (donor is null)
        {
            return;
        }

        if (RecipientPatientId <= 0)
        {
            StatusMessage = "Please enter a valid recipient patient id.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedOrgan))
        {
            StatusMessage = "Please choose an organ first.";
            return;
        }

        try
        {
            // Mirror the web flow: ensure a pending transplant exists for this
            // recipient + organ, then resolve its id and assign the donor.
            var pending = await ResolvePendingTransplantAsync();
            if (pending is null)
            {
                await transplantService.CreateWaitlistRequestAsync(RecipientPatientId, SelectedOrgan);
                pending = await ResolvePendingTransplantAsync();
            }

            if (pending is null)
            {
                StatusMessage = "Assignment requires a pending transplant id, but none could be prepared.";
                return;
            }

            await transplantService.AssignDonorAsync(pending.TransplantId, donor.PatientId, donor.Score.AsScore());
            StatusMessage = $"Assigned donor {donor.FullName} to transplant {pending.TransplantId}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task<Transplant?> ResolvePendingTransplantAsync()
    {
        var existing = await transplantService.GetByReceiverIdAsync(RecipientPatientId);
        return existing
            .Where(transplant => transplant.Status == TransplantStatus.Pending
                && string.Equals(transplant.OrganType, SelectedOrgan, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(transplant => transplant.RequestDate)
            .FirstOrDefault();
    }
}

internal static class DonorScoreExtensions
{
    public static float AsScore(this string value)
        => float.TryParse(value, out var parsed) ? parsed : 0f;
}
