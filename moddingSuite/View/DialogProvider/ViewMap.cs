using moddingSuite.ViewModel.Base;
using System;
using System.Windows;

namespace moddingSuite.View.DialogProvider;

public class ViewMap<TView, TViewModel> : IViewMap
    where TView : Window
    where TViewModel : ViewModelBase
{
    public Type ViewType { get; protected set; }
    public Type ViewModelType { get; protected set; }

    public ViewMap()
    {
        ViewType = typeof(TView);
        ViewModelType = typeof(TViewModel);
    }
}
