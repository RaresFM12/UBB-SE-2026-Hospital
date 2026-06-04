namespace Hospital.Desktop.Converters
{
    using System;
    using System.Collections;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Data;

    /// <summary>
    /// Returns Visible when the bound collection is null or empty, otherwise Collapsed.
    /// Used to show a friendly "No data" placeholder on empty statistics cards.
    /// Pass "Invert" as the ConverterParameter to flip the result.
    /// </summary>
    public partial class CollectionEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isEmpty = true;

            if (value is ICollection collection)
            {
                isEmpty = collection.Count == 0;
            }
            else if (value is IEnumerable enumerable)
            {
                isEmpty = !enumerable.GetEnumerator().MoveNext();
            }

            bool invert = parameter is string s && string.Equals(s, "Invert", StringComparison.OrdinalIgnoreCase);
            if (invert)
            {
                isEmpty = !isEmpty;
            }

            return isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
