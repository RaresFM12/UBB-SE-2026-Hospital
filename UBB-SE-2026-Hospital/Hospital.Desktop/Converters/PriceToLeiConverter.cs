namespace Hospital.Desktop.Converters
{
    using System;
    using Microsoft.UI.Xaml.Data;

    public partial class PriceToLeiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is float price)
            {
                return $"{price:F2} Lei";
            }

            return "0.00 Lei";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
