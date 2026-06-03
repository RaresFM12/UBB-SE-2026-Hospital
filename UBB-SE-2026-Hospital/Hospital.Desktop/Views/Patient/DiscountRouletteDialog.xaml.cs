using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class DiscountRouletteDialog : ContentDialog
{
    private static readonly int[] DiscountOptions = [0, 10, 25, 50, 100];

    public decimal BasePrice { get; }

    public int? SelectedDiscountPercentage { get; private set; }

    public decimal SelectedFinalPrice { get; private set; }

    public string BasePriceText => $"{BasePrice:0.00} lei";

    public string DiscountResultText
        => SelectedDiscountPercentage.HasValue
            ? $"You won {SelectedDiscountPercentage.Value}% off!"
            : string.Empty;

    public string FinalPriceText => $"{SelectedFinalPrice:0.00} lei";

    public DiscountRouletteDialog(decimal basePrice)
    {
        BasePrice = basePrice;
        InitializeComponent();
        SpinButton.IsEnabled = true;
        ResultPanel.Visibility = Visibility.Collapsed;
    }

    private async void SpinButton_Click(object sender, RoutedEventArgs e)
    {
        SpinButton.IsEnabled = false;
        Storyboard storyboard = new();
        DoubleAnimation spinAnimation = new()
        {
            From = 0,
            To = 3600,
            Duration = new Duration(TimeSpan.FromSeconds(3)),
        };

        Storyboard.SetTarget(spinAnimation, WheelRotate);
        Storyboard.SetTargetProperty(spinAnimation, "Angle");
        storyboard.Children.Add(spinAnimation);
        storyboard.Begin();

        await Task.Delay(3000);

        int discount = DiscountOptions[RandomNumberGenerator.GetInt32(DiscountOptions.Length)];
        SelectedDiscountPercentage = discount;
        SelectedFinalPrice = BasePrice * (1 - (discount / 100m));
        DiscountResultTextBlock.Text = DiscountResultText;
        FinalPriceTextBlock.Text = FinalPriceText;
        ResultPanel.Visibility = Visibility.Visible;

        await Task.Delay(1200);
        Hide();
    }
}
