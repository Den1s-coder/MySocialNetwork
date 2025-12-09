using System;
using System.Globalization;
using Xamarin.Forms;

namespace SocialNetwork.mobile.Converters
{
    public class SenderIdToBoolConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public Guid CurrentUserId { get; set; } = Guid.Empty;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Invert ? true : false;

            Guid senderId;
            if (value is Guid g) senderId = g;
            else if (!Guid.TryParse(value.ToString(), out senderId)) return Invert ? true : false;

            bool isCurrent = senderId == CurrentUserId;
            return Invert ? !isCurrent : isCurrent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
