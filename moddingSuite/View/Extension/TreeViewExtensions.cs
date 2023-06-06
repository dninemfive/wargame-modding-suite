using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace moddingSuite.View.Extension;

public static class TreeViewExtension
{
    public static readonly DependencyProperty SelectItemOnRightClickProperty = DependencyProperty.RegisterAttached(
       "SelectItemOnRightClick",
       typeof(bool),
       typeof(TreeViewExtension),
       new UIPropertyMetadata(false, OnSelectItemOnRightClickChanged));

    public static bool GetSelectItemOnRightClick(DependencyObject d) => (bool)d.GetValue(SelectItemOnRightClickProperty);

    public static void SetSelectItemOnRightClick(DependencyObject d, bool value) => d.SetValue(SelectItemOnRightClickProperty, value);

    private static void OnSelectItemOnRightClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        bool selectItemOnRightClick = (bool)e.NewValue;

        if (d is TreeView treeView)
        {
            if (selectItemOnRightClick)
                treeView.PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
            else
                treeView.PreviewMouseRightButtonDown -= OnPreviewMouseRightButtonDown;
        }
    }

    private static void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

        if (treeViewItem != null)
        {
            _ = treeViewItem.Focus();
            e.Handled = true;
        }
    }

    public static TreeViewItem VisualUpwardSearch(DependencyObject source)
    {
        while (source is not null and not TreeViewItem)
            source = VisualTreeHelper.GetParent(source);

        return source as TreeViewItem;
    }
}
