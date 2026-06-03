using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class PatientsPage : Page
{
    public PatientViewModel ViewModel { get; private set; } = null!;

    public PatientsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = App.Services.GetRequiredService<PatientViewModel>();
        ViewModel.LoadPatientsCommand.Execute(null);
    }

    private async void ViewPrescription_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        PrescriptionDetailsDialog dialog = new();
        dialog.XamlRoot = XamlRoot;
        dialog.LoadPrescription(await ViewModel.GetSelectedPrescriptionAsync());
        await dialog.ShowAsync();
    }

    private async void ApplyDiscount_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        DiscountRouletteDialog dialog = new(ViewModel.BasePrice);
        dialog.XamlRoot = XamlRoot;
        await dialog.ShowAsync();

        if (dialog.SelectedDiscountPercentage.HasValue)
        {
            await ViewModel.ApplyDiscountAsync(dialog.SelectedDiscountPercentage.Value);
        }
    }
}
