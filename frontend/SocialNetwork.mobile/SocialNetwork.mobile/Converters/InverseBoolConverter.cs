using System;
using System.Globalization;
using Xamarin.Forms;

namespace SocialNetwork.mobile.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            if (value is bool?) return !(bool?)value ?? true;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            if (value is bool?) return !(bool?)value ?? true;
            return true;
        }
    }
}