using moddingSuite.Model.Ndfbin;
using moddingSuite.Model.Ndfbin.Types;
using moddingSuite.Model.Ndfbin.Types.AllTypes;
using moddingSuite.View.DialogProvider;
using moddingSuite.View.Ndfbin.Viewer;
using moddingSuite.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace moddingSuite.ViewModel.Ndf;

public class ListEditorViewModel : ViewModelBase
{
    private NdfCollection _collection;
    private NdfBinary _ndfbinManager;

    private bool _isInsertMode;

    public NdfBinary NdfbinManager
    {
        get { return _ndfbinManager; }
        set { _ndfbinManager = value; OnPropertyChanged(() => NdfbinManager); }
    }

    public NdfCollection Value
    {
        get { return _collection; }
        set { _collection = value; OnPropertyChanged(() => Value); }
    }

    public ICommand DetailsCommand { get; set; }
    public ICommand AddRowCommand { get; protected set; }
    public ICommand AddRowOfCommonTypeCommand { get; protected set; }
    public ICommand DeleteRowCommand { get; protected set; }

    public bool IsInsertMode
    {
        get { return _isInsertMode; }
        set { _isInsertMode = value; OnPropertyChanged(() => IsInsertMode); }
    }

    public ListEditorViewModel(NdfCollection collection, NdfBinary mgr)
    {
        if (collection == null)
            throw new ArgumentNullException("collection");
        if (mgr == null)
            throw new ArgumentNullException("mgr");

        _collection = collection;
        _ndfbinManager = mgr;
        DetailsCommand = new ActionCommand(DetailsCommandExecute);

        AddRowCommand = new ActionCommand(AddRowExecute);
        AddRowOfCommonTypeCommand = new ActionCommand(AddRowOfCommonTypeExecute, AddRowOfCommonTypeCanExecute);
        DeleteRowCommand = new ActionCommand(DeleteRowExecute, DeleteRowCanExecute);
    }


    private bool AddRowOfCommonTypeCanExecute()
    {
        return Value != null && Value.Count > 0;
    }

    private bool DeleteRowCanExecute()
    {
        ICollectionView cv = CollectionViewSource.GetDefaultView(Value);

        return cv != null && cv.CurrentItem != null;
    }

    private void DeleteRowExecute(object obj)
    {
        ICollectionView cv = CollectionViewSource.GetDefaultView(Value);

        if (cv == null || cv.CurrentItem == null)
            return;


        if (cv.CurrentItem is not CollectionItemValueHolder val)
            return;

        Value.Remove(val);
    }

    private void AddRowOfCommonTypeExecute(object obj)
    {
        ICollectionView cv = CollectionViewSource.GetDefaultView(Value);

        if (cv == null)
            return;

        NdfType type =
            Value.GroupBy(x => x.Value.Type).OrderByDescending(gp => gp.Count()).Select(x => x.First().Value.Type).
                Single();

        CollectionItemValueHolder wrapper =
            new(NdfTypeManager.GetValue(new byte[NdfTypeManager.SizeofType(type)], type, NdfbinManager), NdfbinManager);

        if (IsInsertMode)
        {
            if (cv.CurrentItem == null)
                return;


            if (cv.CurrentItem is not CollectionItemValueHolder val)
                return;

            Value.Insert(cv.CurrentPosition + 1, wrapper);
        }
        else
            Value.Add(wrapper);

        cv.MoveCurrentTo(wrapper);
    }

    private void AddRowExecute(object obj)
    {
        ICollectionView cv = CollectionViewSource.GetDefaultView(Value);

        if (cv == null)
            return;

        AddCollectionItemView view = new();
        AddCollectionItemViewModel vm = new(NdfbinManager, view);

        view.DataContext = vm;

        bool? ret = view.ShowDialog();

        if (!ret.HasValue || !ret.Value)
            return;


        if (IsInsertMode)
        {
            if (cv.CurrentItem == null)
                return;


            if (cv.CurrentItem is not CollectionItemValueHolder val)
                return;

            Value.Insert(cv.CurrentPosition + 1, vm.Wrapper);
        }
        else
            Value.Add(vm.Wrapper);

        cv.MoveCurrentTo(vm.Wrapper);
    }


    public void DetailsCommandExecute(object obj)
    {
        if (obj is not IEnumerable<DataGridCellInfo> item)
            return;

        IValueHolder prop = item.First().Item as IValueHolder;

        FollowDetails(prop);
    }

    private void FollowDetails(IValueHolder prop)
    {
        if (prop == null || prop.Value == null)
            return;

        switch (prop.Value.Type)
        {
            case NdfType.MapList:
            case NdfType.List:
                FollowList(prop);
                break;
            case NdfType.ObjectReference:
                FollowObjectReference(prop);
                break;
            case NdfType.Map:
                if (prop.Value is NdfMap map)
                {
                    FollowDetails(map.Key);
                    FollowDetails(map.Value as IValueHolder);
                }

                break;
            default:
                return;
        }
    }

    private void FollowObjectReference(IValueHolder prop)
    {
        if (prop.Value is not NdfObjectReference refe)
            return;

        NdfClassViewModel vm = new(refe.Class, null);

        NdfObjectViewModel inst = vm.Instances.SingleOrDefault(x => x.Id == refe.InstanceId);

        if (inst == null)
            return;

        vm.InstancesCollectionView.MoveCurrentTo(inst);

        DialogProvider.ProvideView(vm);
    }

    private void FollowList(IValueHolder prop)
    {
        if (prop.Value is not NdfCollection refe)
            return;

        ListEditorViewModel editor = new(refe, NdfbinManager);

        DialogProvider.ProvideView(editor, this);
    }


}
