using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Patient;

public partial class TransplantViewModel : ObservableObject
{
    private readonly ITransplantService transplantService;

    [ObservableProperty] private ObservableCollection<Transplant> transplants = new ObservableCollection<Transplant>();
    [ObservableProperty] private ObservableCollection<TransplantMatch> matches = new ObservableCollection<TransplantMatch>();
    [ObservableProperty] private string statusMessage = string.Empty;

    public TransplantViewModel(ITransplantService transplantService)
    {
        this.transplantService = transplantService;
    }

    [RelayCommand]
    private async Task LoadTransplantsAsync()
    {
        Transplants.Clear();
        StatusMessage = string.Empty;
        try
        {
            var list = await transplantService.GetAllAsync();
            foreach (var t in list)
            {
                Transplants.Add(t);
            }
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task FindMatchesAsync()
    {
        Matches.Clear();
        try
        {
            var list = await transplantService.GetMatchesAsync();
            foreach (var m in list)
            {
                Matches.Add(m);
            }
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
