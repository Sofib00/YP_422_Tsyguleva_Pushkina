using System;
using System.Windows;
using System.Windows.Data;

namespace AdApp_Pushkina_Tciguleeva_422
{
    public class OwnAdVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int adUserId)
            {
                var currentUser = App.Current.Properties["CurrentUser"] as Users;
                return (currentUser != null && currentUser.user_id == adUserId) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OtherAdVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int adUserId)
            {
                var currentUser = App.Current.Properties["CurrentUser"] as Users;
                return (currentUser != null && currentUser.user_id != adUserId) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
