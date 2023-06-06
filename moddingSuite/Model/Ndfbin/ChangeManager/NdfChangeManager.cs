using System.Collections.ObjectModel;

namespace moddingSuite.Model.Ndfbin.ChangeManager;

public class NdfChangeManager
{
    public NdfChangeManager()
    {
    }

    public ObservableCollection<ChangeEntryBase> Changes { get; } = new();

    public bool HasChanges => Changes.Count > 0;

    public void AddChange(ChangeEntryBase change) => Changes.Add(change);
}