using System;
using System.Windows.Data;
using System.Windows.Markup;
using static System.Resources.ResXFileRef;

namespace VRCSTT.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class SubtractValueConverter : MarkupExtension, IValueConverter
    {
        private SubtractValueConverter _converter;
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

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new SubtractValueConverter();
            return _converter;
        }

        #endregion
    }
}
