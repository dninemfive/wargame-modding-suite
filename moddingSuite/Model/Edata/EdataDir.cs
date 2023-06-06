using System.Collections.Generic;

namespace moddingSuite.Model.Edata;

/// <summary>
///     struct dictGroupEntry {
///     DWORD groupId;
///     DWORD entrySize;
///     zstring name;
///     };
/// </summary>
public class EdataDir : EdataEntity
{
    private EdataDir _parent;
    private int _subTreeSize;

    public EdataDir()
        : base(string.Empty)
    {
    }

    public EdataDir(string name, EdataDir parent = null)
        : base(name)
    {
        Parent = parent;
        parent?.Children.Add(this);
    }

    public int SubTreeSize
    {
        get => _subTreeSize;
        set
        {
            _subTreeSize = value;
            OnPropertyChanged("GroupId");
        }
    }

    public EdataDir Parent
    {
        get => _parent;
        set
        {
            _parent = value;
            OnPropertyChanged("Parent");
        }
    }

    public List<EdataDir> Children { get; } = new();

    public List<EdataContentFile> Files { get; } = new();
}