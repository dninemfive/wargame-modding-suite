using moddingSuite.Model.Ndfbin;
using moddingSuite.Model.Ndfbin.Types;
using moddingSuite.Model.Ndfbin.Types.AllTypes;
using moddingSuite.View.DialogProvider;
using moddingSuite.ViewModel.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace moddingSuite.ViewModel.Ndf;

public class NdfObjectViewModel : ObjectWrapperViewModel<NdfObject>
{
    public NdfObjectViewModel(NdfObject obj, ViewModelBase parentVm)
        : base(obj, parentVm)
    {
        List<NdfPropertyValue> propVals = new();

        propVals.AddRange(obj.PropertyValues);

        foreach (NdfPropertyValue source in propVals.OrderBy(x => x.Property.Id))
            PropertyValues.Add(source);

        DetailsCommand = new ActionCommand(DetailsCommandExecute);
        AddPropertyCommand = new ActionCommand(AddPropertyExecute, AddPropertyCanExecute);
        RemovePropertyCommand = new ActionCommand(RemovePropertyExecute, RemovePropertyCanExecute);
        CopyToInstancesCommand = new ActionCommand(CopyToInstancesExecute);
    }

    public uint Id
    {
        get => Object.Id;
        set
        {
            Object.Id = value;
            OnPropertyChanged("Name");
        }
    }

    public ObservableCollection<NdfPropertyValue> PropertyValues { get; } =
        new ObservableCollection<NdfPropertyValue>();

    public ICommand DetailsCommand { get; protected set; }
    public ICommand AddPropertyCommand { get; protected set; }
    public ICommand RemovePropertyCommand { get; protected set; }
    public ICommand CopyToInstancesCommand { get; protected set; }

    /// <summary>
    /// Easy property indexing by name for scripts.
    /// </summary>
    public NdfValueWrapper this[string property]
    {
        get => GetPropertyValueByName(property)?.Value;
        set
        {
            NdfPropertyValue prop = GetPropertyValueByName(property);
            if (prop == null)
                throw new KeyNotFoundException("unknown property");

            prop.BeginEdit();
            prop.Value = value;
            prop.EndEdit();
        }
    }

    private NdfPropertyValue GetPropertyValueByName(string name) => PropertyValues.FirstOrDefault(pv => pv.Property.Name == name);

    private void AddPropertyExecute(object obj)
    {
        System.ComponentModel.ICollectionView cv = CollectionViewSource.GetDefaultView(PropertyValues);

        if (cv.CurrentItem is not NdfPropertyValue item)
            return;

        NdfType type = NdfType.Unset;

        foreach (NdfObject instance in Object.Class.Instances)
        {
            foreach (NdfPropertyValue propertyValue in instance.PropertyValues)
            {
                if (propertyValue.Property.Id == item.Property.Id)
                {
                    if (propertyValue.Type != NdfType.Unset)
                        type = propertyValue.Type;
                }
            }
        }

        if (type is NdfType.Unset or NdfType.Unknown)
            return;

        item.Value = NdfTypeManager.GetValue(new byte[NdfTypeManager.SizeofType(type)], type, item.Manager);
    }

    private bool AddPropertyCanExecute()
    {
        System.ComponentModel.ICollectionView cv = CollectionViewSource.GetDefaultView(PropertyValues);

        return cv.CurrentItem is NdfPropertyValue item && item.Type == NdfType.Unset;
    }

    private void RemovePropertyExecute(object obj)
    {
        System.ComponentModel.ICollectionView cv = CollectionViewSource.GetDefaultView(PropertyValues);

        if (cv.CurrentItem is not NdfPropertyValue item || item.Type == NdfType.Unset || item.Type == NdfType.Unknown)
            return;

        MessageBoxResult result = MessageBox.Show("Do you want to set this property to null?", "Confirmation",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
            item.Value = NdfTypeManager.GetValue(new byte[0], NdfType.Unset, item.Manager);
    }

    private bool RemovePropertyCanExecute()
    {
        System.ComponentModel.ICollectionView cv = CollectionViewSource.GetDefaultView(PropertyValues);

        return cv.CurrentItem is NdfPropertyValue item && item.Type != NdfType.Unset;
    }

    private void CopyToInstancesExecute(object obj)
    {
        System.ComponentModel.ICollectionView cv = CollectionViewSource.GetDefaultView(PropertyValues);

        NdfPropertyValue item = cv.CurrentItem as NdfPropertyValue;
        foreach (NdfObject instance in item.Instance.Class.Instances)
        {
            NdfPropertyValue property = instance.PropertyValues.First(x => x.Property == item.Property);
            property.BeginEdit();
            property.Value = item.Value;
            property.EndEdit();
        }
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
        if (prop?.Value == null)
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

        NdfClassViewModel vm = new(refe.Class, ParentVm);

        NdfObjectViewModel inst = vm.Instances.SingleOrDefault(x => x.Id == refe.InstanceId);

        if (inst == null)
            return;

        _ = vm.InstancesCollectionView.MoveCurrentTo(inst);

        DialogProvider.ProvideView(vm, ParentVm);
    }

    private void FollowList(IValueHolder prop)
    {
        if (prop.Value is not NdfCollection refe)
            return;

        //if (IsTable(refe))
        //{
        ListEditorViewModel editor = new(refe, Object.Class.Manager);
        DialogProvider.ProvideView(editor, ParentVm);
        //}
        //else
        //{
        //var editor = new ListEditorViewModel(refe, Object.Class.Manager);
        //DialogProvider.ProvideView(editor, ParentVm);
        //}
    }

    private bool IsTable(NdfCollection collection)
    {
        NdfMap map = collection.First().Value as NdfMap;

        if (collection == null)
            return false;

        MapValueHolder valHolder = map.Value as MapValueHolder;
        return valHolder.Value is NdfCollection;
    }
}