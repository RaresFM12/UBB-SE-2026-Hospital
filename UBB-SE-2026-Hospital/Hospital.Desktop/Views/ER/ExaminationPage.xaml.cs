using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.ER;
using Microsoft.Extensions.DependencyInjection;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.ER;

public sealed partial class ExaminationPage : Page
{
    public ExaminationViewModel ViewModel { get; private set; } = null!;

    public ExaminationPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = App.Services.GetRequiredService<ExaminationViewModel>();
        ViewModel.XamlRoot = XamlRoot;
        DataContext = ViewModel;
        ViewModel.LoadDataCommand.Execute(null);
    }
}
