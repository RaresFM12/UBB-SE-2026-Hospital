using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Admin;

public partial class SubstancesViewModel : ObservableObject
{
    private readonly IAdminService adminService;

    [ObservableProperty] private ObservableCollection<Substance> substances = new();
    [ObservableProperty] private string newName = string.Empty;
    [ObservableProperty] private string newLethalDose = string.Empty;
    [ObservableProperty] private string newDescription = string.Empty;
    [ObservableProperty] private string statusMessage = string.Empty;

    public SubstancesViewModel(IAdminService adminService)
    {
        this.adminService = adminService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            var result = await adminService.GetSubstancesAsync();
            Substances = new ObservableCollection<Substance>(result);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading substances: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            if (string.IsNullOrWhiteSpace(NewName))
            {
                StatusMessage = "Name is required.";
                return;
            }

            if (!float.TryParse(NewLethalDose, out float lethalDose))
            {
                StatusMessage = "Lethal dose must be a valid number.";
                return;
            }

            await adminService.CreateSubstanceAsync(NewName.Trim(), lethalDose, NewDescription ?? string.Empty);

            NewName = string.Empty;
            NewLethalDose = string.Empty;
            NewDescription = string.Empty;

            await LoadAsync();
            StatusMessage = "Substance added.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding substance: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(Substance? substance)
    {
        StatusMessage = string.Empty;
        if (substance is null)
        {
            return;
        }

        try
        {
            await adminService.DeleteSubstanceAsync(substance.Name);
            await LoadAsync();
            StatusMessage = $"Deleted '{substance.Name}'.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting substance: {ex.Message}";
        }
    }
}
