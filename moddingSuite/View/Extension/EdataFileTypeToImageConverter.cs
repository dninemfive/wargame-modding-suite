using moddingSuite.Model.Edata;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace moddingSuite.View.Extension;

public class EdataFileTypeToImageConverter : IValueConverter
{
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (EdataFileType)value switch
    {
        EdataFileType.Ndfbin => Application.Current.Resources["ScriptIcon"] as BitmapImage,
        EdataFileType.Dictionary => Application.Current.Resources["OpenDictionayIcon"] as BitmapImage,
        EdataFileType.Package => Application.Current.Resources["PackageFileIcon"] as BitmapImage,
        EdataFileType.Image => Application.Current.Resources["TextureIcon"] as BitmapImage,
        EdataFileType.Mesh => Application.Current.Resources["MeshFileIcon"] as BitmapImage,
        EdataFileType.Scenario => Application.Current.Resources["ScenarioIcon"] as BitmapImage,
        _ => Application.Current.Resources["UnknownFileIcon"] as BitmapImage,
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion
}