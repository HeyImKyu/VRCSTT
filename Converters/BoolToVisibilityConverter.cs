using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using static System.Resources.ResXFileRef;

namespace VRCSTT.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        private BoolToVisibilityConverter _converter;
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new BoolToVisibilityConverter();
            return _converter;
        }

        #endregion
    }
}
