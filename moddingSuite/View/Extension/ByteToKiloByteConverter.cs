using System;
using System.Windows.Data;

namespace moddingSuite.View.Extension;

public class ByteToKiloByteConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        long val = (long)value;

        return val < 1000 ? string.Format("{0} B", val) : (object)string.Format("{0} kB", val / 1000);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
}
