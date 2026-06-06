namespace Hospital.Desktop.Views.Doctor
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using Hospital.Desktop.Services;
    using Hospital.Shared.Services;
    using Hospital.Data.Models;
    using Hospital.Desktop.ViewModels.Doctor;
    using Hospital.Desktop.Views.Shell;

    public sealed partial class DoctorSchedulePage : Page
    {
        private readonly DoctorScheduleViewModel viewModel;
        private readonly DialogPresenter dialogPresenter;
        private bool initialized;

        public DoctorSchedulePage()
        {
            this.InitializeComponent();

            this.viewModel = App.Services.GetRequiredService<DoctorScheduleViewModel>();
            this.dialogPresenter = App.Services.GetRequiredService<DialogPresenter>();
            this.DataContext = this.viewModel;

            this.Loaded += DoctorSchedulePage_Loaded;
        }

        private void DoctorSchedulePage_Loaded(object sender, RoutedEventArgs e)
        {
            this.dialogPresenter.SetXamlRoot(this.XamlRoot);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (this.initialized)
            {
                return;
            }

            this.initialized = true;

            await this.viewModel.InitializeAsync();
        }

        private void DateCalendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs eventArgs)
        {
            if (sender.SelectedDates == null || sender.SelectedDates.Count == 0)
            {
                return;
            }

            var picked = sender.SelectedDates[0].Date;

            if (picked < new System.DateTimeOffset(1753, 1, 1, 0, 0, 0, System.TimeSpan.Zero))
            {
                return;
            }

            this.viewModel.SelectedDate = picked;
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.DataContext as AppointmentItemViewModel;

            if (item != null && this.viewModel != null)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Code-behind clicked! Sending ID {item.AppointmentItemId} to ViewModel.");
                this.viewModel.OpenDetails(item);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: Click failed. Either the item or the viewModel was null.");
            }
        }
    }
}