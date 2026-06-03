using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class DiscountRouletteDialog : ContentDialog
{
    private static readonly int[] DiscountOptions = [0, 10, 25, 50, 100];

    public decimal BasePrice { get; }

    public int? SelectedDiscountPercentage { get; private set; }

    public decimal SelectedFinalPrice { get; private set; }

    public bool IsSpinEnabled { get; private set; } = true;

    public string BasePriceText => $"{BasePrice:0.00} lei";

    public string DiscountResultText
        => SelectedDiscountPercentage.HasValue
            ? $"You won {SelectedDiscountPercentage.Value}% off!"
            : string.Empty;

    public string FinalPriceText => $"{SelectedFinalPrice:0.00} lei";

    public Visibility ResultVisibility
        => SelectedDiscountPercentage.HasValue ? Visibility.Visible : Visibility.Collapsed;

    public DiscountRouletteDialog(decimal basePrice)
    {
        BasePrice = basePrice;
        InitializeComponent();
    }

    private async void SpinButton_Click(object sender, RoutedEventArgs e)
    {
        IsSpinEnabled = false;
        Bindings.Update();
        SpinAnimation.Begin();

        await Task.Delay(3000);

        int discount = DiscountOptions[RandomNumberGenerator.GetInt32(DiscountOptions.Length)];
        SelectedDiscountPercentage = discount;
        SelectedFinalPrice = BasePrice * (1 - (discount / 100m));
        Bindings.Update();

        await Task.Delay(1200);
        Hide();
    }
}
