namespace Hospital.Desktop.Converters;

using System;
using Microsoft.UI.Xaml.Data;

public sealed class DateToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            DateTime dateTime when dateTime != default => dateTime.ToString("dd/MM/yyyy"),
            DateTimeOffset dateTimeOffset when dateTimeOffset != default => dateTimeOffset.ToString("dd/MM/yyyy"),
            _ => "N/A",
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
