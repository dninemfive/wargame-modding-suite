using moddingSuite.Util;
using System;
using System.Globalization;
using System.Windows.Data;

namespace moddingSuite.View.Extension;

public class LocalisationHashValueConverter : IValueConverter
{
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Utils.ByteArrayToBigEndianHexByteString((byte[])value);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Utils.StringToByteArrayFastest(value.ToString());

    #endregion
}