using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hospital.Data.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class PrescriptionDetailsDialog : ContentDialog, INotifyPropertyChanged
{
    private string doctorNotes = "No notes provided.";

    public ObservableCollection<PrescriptionItem> MedicationItems { get; } = new();

    public string DoctorNotes
    {
        get => doctorNotes;
        private set
        {
            doctorNotes = value;
            OnPropertyChanged();
        }
    }

    public Visibility NoMedicationVisibility
        => MedicationItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public PrescriptionDetailsDialog()
    {
        InitializeComponent();
        MedicationItems.CollectionChanged += (_, _) => OnPropertyChanged(nameof(NoMedicationVisibility));
    }

    public void LoadPrescription(Prescription? prescription)
    {
        MedicationItems.Clear();

        if (prescription is null)
        {
            DoctorNotes = "No prescription data available for this consultation.";
            return;
        }

        DoctorNotes = string.IsNullOrWhiteSpace(prescription.DoctorNotes)
            ? "No notes provided."
            : prescription.DoctorNotes;

        foreach (PrescriptionItem item in prescription.MedicationList)
        {
            MedicationItems.Add(item);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
