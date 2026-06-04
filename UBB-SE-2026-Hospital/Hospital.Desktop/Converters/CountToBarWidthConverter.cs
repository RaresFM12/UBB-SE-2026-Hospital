namespace Hospital.Desktop.Converters
{
    using System;
    using System.Globalization;
    using Microsoft.UI.Xaml.Data;

    /// <summary>
    /// Converts an integer count into a pixel width for a small horizontal bar.
    /// Counts in this dashboard are absolute (typically 0-20), so a fixed
    /// pixels-per-unit scale keeps bars comparable across cards. An optional
    /// ConverterParameter overrides the scale factor.
    /// </summary>
    public partial class CountToBarWidthConverter : IValueConverter
    {
        private const double DefaultPixelsPerUnit = 9.0;
        private const double MinWidth = 4.0;
        private const double MaxWidth = 180.0;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double count = value switch
            {
                int i => i,
                long l => l,
                double d => d,
                _ => 0
            };

            double scale = DefaultPixelsPerUnit;
            if (parameter is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed) && parsed > 0)
            {
                scale = parsed;
            }

            double width = count * scale;
            if (width < MinWidth)
            {
                width = count <= 0 ? 0 : MinWidth;
            }

            return Math.Min(width, MaxWidth);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
