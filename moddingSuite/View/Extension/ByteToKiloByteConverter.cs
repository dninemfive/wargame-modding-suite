using System;
using System.Windows.Data;

namespace moddingSuite.View.Extension;

public class ByteToKiloByteConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        long val = (long)value;

        if (val < 1000)
            return string.Format("{0} B", val);
        else
            return string.Format("{0} kB", (val / 1000));
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
