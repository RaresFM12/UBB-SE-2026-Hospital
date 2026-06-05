using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Admin;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Admin;

public sealed partial class MedicalStaffPage : Page
{
    public MedicalStaffViewModel ViewModel { get; private set; } = null!;

    public MedicalStaffPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new MedicalStaffViewModel(App.Services.GetRequiredService<IShiftManagementService>());
        DataContext = ViewModel;
        ViewModel.LoadCommand.Execute(null);
    }
}
