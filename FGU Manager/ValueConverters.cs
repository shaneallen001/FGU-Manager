using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;

namespace FGU_Manager
{
    public class StringTruncatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringLength = System.Convert.ToInt32(parameter);
            var str = value as string;
            if (str != null && str.Length > stringLength)
            {
                return str.Substring(0, stringLength);
            }
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AttunementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Assuming the value is a string like "True" or "False"
            var attunementStatus = value as string;

            // Checking if the string value is "True" (or "true", if you want case-insensitive comparison)
            bool isAttuned = attunementStatus != null && attunementStatus.Equals("True", StringComparison.OrdinalIgnoreCase);

            return isAttuned ? " (A)" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
