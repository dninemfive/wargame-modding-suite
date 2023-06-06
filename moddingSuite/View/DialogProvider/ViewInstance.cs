using moddingSuite.ViewModel.Base;
using System;
using System.Windows;

namespace moddingSuite.View.DialogProvider;

public class ViewInstance
{
    public Window View { get; protected set; }
    public ViewModelBase ViewModel { get; protected set; }

    public ViewInstance(Window view, ViewModelBase vm)
    {
        View = view ?? throw new ArgumentException("view");
        ViewModel = vm ?? throw new ArgumentException("vm");
    }
}
