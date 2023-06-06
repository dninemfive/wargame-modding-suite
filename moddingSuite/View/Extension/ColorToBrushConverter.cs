using System;
using System.Drawing;
using System.Windows.Data;

namespace moddingSuite.View.Extension;

public class ColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        Color col = (Color)value;

        return new SolidBrush(Color.FromArgb(col.A, col.R, col.B, col.G));
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
}
