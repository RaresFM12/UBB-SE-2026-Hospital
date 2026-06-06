namespace Hospital.Desktop.Converters
{
    using System;
    using Microsoft.UI.Xaml.Data;
    using Windows.UI.Text;

    public partial class StrikethroughConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool hasDiscount && hasDiscount)
            {
                return TextDecorations.Strikethrough;
            }

            return TextDecorations.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
