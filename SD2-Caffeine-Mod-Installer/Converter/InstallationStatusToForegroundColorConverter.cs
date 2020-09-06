using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SD2_Caffeine_Mod_Installer.Converter
{
    public class InstallationStatusToForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;
            Color foregroundColor = status == "Installed" ? Colors.Green : Colors.Red;
            return new SolidColorBrush(foregroundColor);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
