using System;

namespace moddingSuite.ViewModel.Base;

public abstract class ObjectWrapperViewModel<T> : ViewModelBase
    where T : ViewModelBase
{
    private ViewModelBase _parentVm;

    protected ObjectWrapperViewModel(T obj, ViewModelBase parentVm)
    {
        //if (parentVm == null)
        //    throw new ArgumentException("parentVm");

        Object = obj ?? throw new ArgumentException("obj");
        ParentVm = parentVm;
    }

    public T Object { get; protected set; }

    public ViewModelBase ParentVm
    {
        get => _parentVm;
        set { _parentVm = value; OnPropertyChanged("ParentVm"); }
    }
}