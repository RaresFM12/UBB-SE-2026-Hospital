using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Hospital.Desktop.ViewModels.Admin;

public record ModuleRow(string Title, string Description);

public partial class ModulesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ModuleRow> modules = new();
    [ObservableProperty] private string statusMessage = string.Empty;

    public ModulesViewModel()
    {
    }

    [RelayCommand]
    private void Load()
    {
        Modules = new ObservableCollection<ModuleRow>
        {
            new("Statistics", "View aggregate hospital metrics and patient distribution charts."),
            new("Pharmacy", "Manage medication stock, prescriptions and dispensing."),
            new("Patient Registration", "Register new patients and capture their personal details."),
            new("Queue", "Monitor and manage the live patient waiting queue."),
            new("Triage", "Assess incoming patients and assign clinical priority."),
            new("Room Assignment", "Allocate patients to available rooms and beds."),
            new("Examination", "Record clinical examinations, diagnoses and notes."),
            new("Transfer Log", "Track patient transfers between departments and facilities."),
            new("Room Management", "Configure rooms, capacities and availability status."),
        };
    }
}
