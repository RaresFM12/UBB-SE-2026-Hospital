using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Admin;

public partial class MedicalStaffViewModel : ObservableObject
{
    private readonly IShiftManagementService shiftManagementService;

    [ObservableProperty] private ObservableCollection<Staff> staff = new ObservableCollection<Staff>();
    [ObservableProperty] private string statusMessage = string.Empty;

    public MedicalStaffViewModel(IShiftManagementService shiftManagementService)
    {
        this.shiftManagementService = shiftManagementService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            var result = await shiftManagementService.GetAllStaffAsync(CancellationToken.None);
            Staff = new ObservableCollection<Staff>(result);
            StatusMessage = $"Loaded {Staff.Count} staff member(s).";
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
