using System;
using System.Windows.Data;

namespace VRCSTT.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class SubtractValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return ((double)value) - 5;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
