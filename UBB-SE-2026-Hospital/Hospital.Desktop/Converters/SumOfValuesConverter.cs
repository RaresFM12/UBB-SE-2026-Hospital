namespace Hospital.Desktop.Converters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.UI.Xaml.Data;

    /// <summary>
    /// Sums the integer values of a Dictionary&lt;string,int&gt; (or any
    /// IEnumerable of KeyValuePair&lt;string,int&gt;) for display as a card total.
    /// Returns 0 for null or empty collections.
    /// </summary>
    public partial class SumOfValuesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int total = 0;

            if (value is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    if (item is KeyValuePair<string, int> pair)
                    {
                        total += pair.Value;
                    }
                }
            }

            return total;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
