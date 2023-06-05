using System.Collections.ObjectModel;

namespace moddingSuite.Model.Ndfbin.ChangeManager;

public class NdfChangeManager
{
    private readonly ObservableCollection<ChangeEntryBase> _changes = new();

    public NdfChangeManager()
    {
    }

    public ObservableCollection<ChangeEntryBase> Changes
    {
        get { return _changes; }
    }

    public bool HasChanges
    {
        get { return Changes.Count > 0; }
    }

    public void AddChange(ChangeEntryBase change)
    {
        Changes.Add(change);
    }
}