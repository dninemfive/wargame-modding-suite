using moddingSuite.ViewModel.Base;

namespace moddingSuite.Model.Textures;

public class TgvMipMap : ViewModelBase
{
    private uint _offset;
    private uint _size;
    private int _mipWidth;
    private byte[] _content;

    public TgvMipMap()
    {
    }

    public TgvMipMap(uint offset, uint size, ushort mipWidth)
    {
        Offset = offset;
        Size = size;
        MipWidth = mipWidth;
    }

    public uint Offset
    {
        get => _offset;
        set { _offset = value; OnPropertyChanged(() => Offset); }
    }

    public uint Size
    {
        get => _size;
        set { _size = value; OnPropertyChanged(() => Size); }
    }

    public int MipWidth
    {
        get => _mipWidth;
        set { _mipWidth = value; OnPropertyChanged(() => MipWidth); }
    }

    public byte[] Content
    {
        get => _content;
        set { _content = value; OnPropertyChanged(() => Content); }
    }
}
