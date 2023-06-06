using moddingSuite.Model.Ndfbin;
using moddingSuite.Model.Ndfbin.Types;
using moddingSuite.Model.Ndfbin.Types.AllTypes;
using System.Windows;
using System.Windows.Controls;

namespace moddingSuite.View.Extension;

public class EditingControlDataTemplateSelector : DataTemplateSelector
{
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is not IValueHolder || container is not FrameworkElement element)
            return null;

        IValueHolder propVal = item as IValueHolder;

        NdfValueWrapper wrap = propVal.Value;

        return wrap == null
            ? null
            : wrap.Type switch
            {
                NdfType.Float32 or NdfType.Float64 => element.FindResource("FloatEditingTemplate") as DataTemplate,
                NdfType.UInt16 or NdfType.UInt32 => element.FindResource("UInt32EditingTemplate") as DataTemplate,
                NdfType.Int32 or NdfType.Int8 or NdfType.Int16 => element.FindResource("Int32EditingTemplate") as DataTemplate,
                NdfType.Guid => element.FindResource("GuidEditingTemplate") as DataTemplate,
                NdfType.Boolean => element.FindResource("BooleanEditingTemplate") as DataTemplate,
                NdfType.Color32 => element.FindResource("ColorPickerEditingTemplate") as DataTemplate,
                NdfType.Vector => element.FindResource("VectorEditingTemplate") as DataTemplate,
                NdfType.Map => element.FindResource("MapEditingTemplate") as DataTemplate,
                NdfType.ObjectReference => element.FindResource("ObjectReferenceEditingTemplate") as DataTemplate,
                NdfType.LocalisationHash => element.FindResource("LocaleHashEditingTemplate") as DataTemplate,
                NdfType.TableString or NdfType.TableStringFile => element.FindResource("StringTableEditingTemplate") as DataTemplate,
                NdfType.TransTableReference => element.FindResource("TransTableReferenceEditingTemplate") as DataTemplate,
                NdfType.Blob => element.FindResource("BlobEditingTemplate") as DataTemplate,
                NdfType.EugFloat2 => element.FindResource("FloatPairEditingTemplate") as DataTemplate,
                NdfType.List => null,
                _ => null,
            };
    }
}