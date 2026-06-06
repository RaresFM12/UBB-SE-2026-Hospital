namespace Hospital.Desktop.Converters
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Data;

    /// <summary>
    /// Converts a float DiscountPercentage to the inverse Visibility.
    /// Visible when discount == 0 (no discount), Collapsed when discounted.
    /// Used to show the regular (non-discounted) price block.
    /// </summary>
    public partial class NoDiscountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is float discountPercentage && discountPercentage > 0)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
