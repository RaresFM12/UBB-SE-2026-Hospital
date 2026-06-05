using Hospital.Data.Models;
using Hospital.Desktop.ViewModels.Patient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class PatientProfileWindow : Window
{
    public PatientViewModel ViewModel { get; }

    public PatientProfileWindow()
    {
        ViewModel = App.Services.GetRequiredService<PatientViewModel>();
        InitializeComponent();
        Title = "Patient Medical Information";
        RootGrid.DataContext = ViewModel;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e) => Close();

    private void ConsultationList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (ConsultationList.SelectedItem is MedicalRecord record)
        {
            ViewModel.SelectedMedicalRecord = record;
        }
    }

    private async void ViewPrescription_Click(object sender, RoutedEventArgs e)
    {
        PrescriptionDetailsDialog dialog = new();
        dialog.XamlRoot = RootGrid.XamlRoot;
        dialog.LoadPrescription(await ViewModel.GetSelectedPrescriptionAsync());
        await dialog.ShowAsync();
    }

    private async void ApplyDiscount_Click(object sender, RoutedEventArgs e)
    {
        DiscountRouletteDialog dialog = new(ViewModel.BasePrice);
        dialog.XamlRoot = RootGrid.XamlRoot;
        await dialog.ShowAsync();

        if (dialog.SelectedDiscountPercentage.HasValue)
        {
            await ViewModel.ApplyDiscountAsync(dialog.SelectedDiscountPercentage.Value);
        }
    }

    public async Task InitializeAsync(int patientId)
    {
        await ViewModel.LoadPatientProfileAsync(patientId);
    }
}
