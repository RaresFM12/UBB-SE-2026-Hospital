using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Hospital.Desktop.ViewModels.Pharmacy;
using Hospital.Desktop.Views.Shell;
using System;

namespace Hospital.Desktop.Views.Pharmacy
{
    public class InlineBoolToVisConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }

    public sealed partial class PeriodTrackerPage : Page
    {
        public PeriodTrackerViewModel ViewModel { get; }
        private readonly DialogPresenter dialogPresenter;

        public PeriodTrackerPage()
        {
            this.ViewModel = App.Services.GetRequiredService<PeriodTrackerViewModel>();
            this.dialogPresenter = App.Services.GetRequiredService<DialogPresenter>();

            this.InitializeComponent();
            this.DataContext = this.ViewModel;

            this.Loaded += async (s, e) =>
            {
                this.dialogPresenter.SetXamlRoot(this.XamlRoot);
                await this.ViewModel.InitializeAsync();
            };
        }

        public Visibility GetDashboardVisibility(bool hasTracker) => hasTracker ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GetEmptyStateVisibility(bool hasTracker) => hasTracker ? Visibility.Collapsed : Visibility.Visible;

        private async void ChangeMonth_Prev_Click(object sender, RoutedEventArgs e) => await ViewModel.ChangeMonthAsync("Prev");
        private async void ChangeMonth_Next_Click(object sender, RoutedEventArgs e) => await ViewModel.ChangeMonthAsync("Next");
        private async void SaveCycle_Click(object sender, RoutedEventArgs e) => await ViewModel.SaveCycleAsync();
        private async void AddNote_Click(object sender, RoutedEventArgs e) => await ViewModel.AddNoteAsync();

        private async void NoteCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is PeriodTrackerNoteItemViewModel note)
            {
                await ViewModel.ToggleNoteAsync(note);
            }
        }

        private async void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is PeriodTrackerNoteItemViewModel note)
            {
                await ViewModel.RemoveNoteAsync(note);
            }
        }
    }
}