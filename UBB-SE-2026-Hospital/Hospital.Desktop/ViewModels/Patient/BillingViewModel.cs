using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Patient;

public partial class BillingViewModel : ObservableObject
{
    private readonly IBillingService billingService;

    [ObservableProperty] private int patientId;
    [ObservableProperty] private int recordId;
    [ObservableProperty] private int discountPercent;
    [ObservableProperty] private decimal basePrice;
    [ObservableProperty] private decimal finalPrice;
    [ObservableProperty] private string statusMessage = string.Empty;

    public BillingViewModel(IBillingService billingService)
    {
        this.billingService = billingService;
    }

    [RelayCommand]
    private async Task ComputePriceAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            BasePrice = await billingService.ComputeBasePriceAsync(PatientId, RecordId);
            FinalPrice = await billingService.ApplyDiscountAsync(BasePrice, DiscountPercent);
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
