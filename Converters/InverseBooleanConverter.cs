using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using static System.Resources.ResXFileRef;

namespace VRCSTT.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : MarkupExtension, IValueConverter
    {
        private InverseBooleanConverter _converter;
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                if ((Visibility)value == Visibility.Visible)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }

            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new InverseBooleanConverter();
            return _converter;
        }

        #endregion
    }
}
