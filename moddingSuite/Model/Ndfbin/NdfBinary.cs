using moddingSuite.Model.Ndfbin.ChangeManager;
using moddingSuite.Model.Ndfbin.Types.AllTypes;
using moddingSuite.ViewModel.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace moddingSuite.Model.Ndfbin;

public class NdfBinary : ViewModelBase
{
    private NdfHeader _header;
    private NdfFooter _footer;

    public NdfHeader Header
    {
        get => _header;
        set
        {
            _header = value;
            OnPropertyChanged("Header");
        }
    }

    public NdfFooter Footer
    {
        get => _footer;
        set
        {
            _footer = value;
            OnPropertyChanged("Footer");
        }
    }

    public ObservableCollection<NdfClass> Classes { get; set; }
    public ObservableCollection<NdfStringReference> Strings { get; set; }
    public ObservableCollection<NdfTranReference> Trans { get; set; }
    public List<NdfObject> Instances { get; set; }

    public HashSet<uint> TopObjects { get; set; }
    public List<uint> Import { get; set; }
    public List<uint> Export { get; set; }

    public NdfChangeManager ChangeManager { get; protected set; }

    public NdfBinary()
    {
        ChangeManager = new NdfChangeManager();
    }

    public NdfObject CreateInstanceOf(NdfClass cls, bool isTopLevelInstance = true)
    {
        uint newId = (uint)Instances.Count();

        NdfObject inst = new()
        { Class = cls, Id = newId };

        AddEmptyProperties(inst);

        Instances.Add(inst);

        if (isTopLevelInstance)
        {
            _ = TopObjects.Add(inst.Id);
            inst.IsTopObject = true;
        }

        return inst;
    }

    public void DeleteInstance(NdfObject inst)
    {
        _ = Instances.Remove(inst);
        NdfClass cls = inst.Class;
        _ = cls.Instances.Remove(inst);

        if (TopObjects.Contains(inst.Id))
            _ = TopObjects.Remove(inst.Id);
    }

    public void AddEmptyProperties(NdfObject instance)
    {
        foreach (NdfProperty property in instance.Class.Properties)
        {
            if (instance.PropertyValues.All(x => x.Property != property))
            {
                instance.PropertyValues.Add(new NdfPropertyValue(instance)
                {
                    Property = property,
                    Value = new NdfNull()
                });
            }
        }
    }
}