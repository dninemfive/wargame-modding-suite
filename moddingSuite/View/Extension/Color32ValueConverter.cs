﻿using moddingSuite.Util;
using System;
using System.Drawing;
using System.Windows.Data;

namespace moddingSuite.View.Extension;

public class Color32ValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        Color col = (Color)value;

        return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", col.R, col.G, col.B, col.A);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        string val = value.ToString().Replace("#", string.Empty);

        byte[] colArr = Utils.StringToByteArrayFastest(val);

        return Color.FromArgb(colArr[3], colArr[0], colArr[1], colArr[2]);
    }
}
