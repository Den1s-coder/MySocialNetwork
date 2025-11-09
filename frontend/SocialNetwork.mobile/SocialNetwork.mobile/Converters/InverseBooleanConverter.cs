using System;
using System.Globalization;
using Xamarin.Forms;

namespace SocialNetwork.mobile.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            if (value == null) return true;
            try
            {
                return !(bool)System.Convert.ChangeType(value, typeof(bool));
            }
            catch
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            if (value == null) return true;
            try
            {
                return !(bool)System.Convert.ChangeType(value, typeof(bool));
            }
            catch
            {
                return true;
            }
        }
    }
}
