using moddingSuite.Util;
using moddingSuite.ViewModel.Base;
using moddingSuite.ViewModel.Trad;

namespace moddingSuite.Model.Trad;

public class TradEntry : ViewModelBase
{
    private uint _contLen;
    private string _content;
    private byte[] _hash;
    private string _hashView;
    private uint _offsetCont;
    private uint _offsetDic;

    private bool _userCreated = false;

    public string HashView
    {
        get => _hashView;
        set
        {
            _hashView = value;
            OnPropertyChanged(() => HashView);
        }
    }

    public byte[] Hash
    {
        get => _hash;
        set
        {
            _hash = value;

            HashView = Utils.ByteArrayToBigEndianHexByteString(_hash);

            OnPropertyChanged(() => Hash);
        }
    }

    public uint OffsetDic
    {
        get => _offsetDic;
        set
        {
            _offsetDic = value;
            OnPropertyChanged(() => OffsetDic);
        }
    }

    public uint OffsetCont
    {
        get => _offsetCont;
        set
        {
            _offsetCont = value;
            OnPropertyChanged(() => OffsetCont);
        }
    }

    public uint ContLen
    {
        get => _contLen;
        set
        {
            _contLen = value;
            OnPropertyChanged(() => ContLen);
        }
    }

    public string Content
    {
        get => _content;
        set
        {
            _content = value;

            if (UserCreated)
                TradFileViewModel.CalculateHash(this);

            OnPropertyChanged(() => Content);
        }
    }

    public bool UserCreated
    {
        get => _userCreated;
        set
        {
            _userCreated = value;
            OnPropertyChanged(() => UserCreated);
        }
    }
}