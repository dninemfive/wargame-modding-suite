using moddingSuite.BL.Ndf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfMap : NdfFlatValueWrapper
{
    private MapValueHolder _key;

    private NdfType _keyType = NdfType.Unset;
    private NdfType _valueType = NdfType.Unset;

    public NdfMap(MapValueHolder key, MapValueHolder value, NdfBinary mgr)
        : base(NdfType.Map, value)
    {
        Key = key;
        Manager = mgr;

        TypeSelection.AddRange(NdfTypeManager.GetTypeSelection());
    }

    public NdfBinary Manager { get; protected set; }

    public NdfType KeyType
    {
        get => _keyType;
        set
        {
            _keyType = value;
            GetValueForType(true);
            OnPropertyChanged(() => KeyType);
        }
    }

    public NdfType ValueType
    {
        get => _valueType;
        set
        {
            _valueType = value;
            GetValueForType(false);
            OnPropertyChanged(() => ValueType);
        }
    }

    public List<NdfType> TypeSelection { get; } = new();

    public bool IsKeyNull => Key.Value == null;

    public bool IsValueNull => ((MapValueHolder)Value).Value == null;

    public MapValueHolder Key
    {
        get => _key;
        set
        {
            _key = value;
            OnPropertyChanged("Key");
        }
    }

    private void GetValueForType(bool keyOrValue)
    {
        if (keyOrValue)
        {
            Key =
                new MapValueHolder(
                    NdfTypeManager.GetValue(new byte[NdfTypeManager.SizeofType(KeyType)], KeyType, Manager), Manager);
        }
        else
        {
            Value =
                new MapValueHolder(
                    NdfTypeManager.GetValue(new byte[NdfTypeManager.SizeofType(ValueType)], ValueType, Manager), Manager);
        }

        OnPropertyChanged("IsKeyNull");
        OnPropertyChanged("IsValueNull");
    }

    public override byte[] GetBytes()
    {

        if (Key.Value == null || ((MapValueHolder)Value).Value == null)
            return new byte[0];

        List<byte> mapdata = new();

        List<byte> key = Key.Value.GetBytes().ToList();
        List<byte> value = ((MapValueHolder) Value).Value.GetBytes().ToList();

        if (Key.Value.Type is NdfType.ObjectReference or NdfType.TransTableReference)
            mapdata.AddRange(BitConverter.GetBytes((uint)NdfType.Reference));

        mapdata.AddRange(BitConverter.GetBytes((uint)Key.Value.Type));
        mapdata.AddRange(key);

        if (((MapValueHolder)Value).Value.Type is NdfType.ObjectReference or
            NdfType.TransTableReference)
        {
            mapdata.AddRange(BitConverter.GetBytes((uint)NdfType.Reference));
        }

        mapdata.AddRange(BitConverter.GetBytes((uint)((MapValueHolder)Value).Value.Type));
        mapdata.AddRange(value);

        return mapdata.ToArray();
    }

    public override byte[] GetNdfText()
    {
        Encoding end = NdfTextWriter.NdfTextEncoding;
        List<byte> data = new();

        data.AddRange(end.GetBytes("(\n"));

        data.AddRange(Key.Value.GetNdfText());

        data.AddRange(end.GetBytes(",\n"));

        data.AddRange(end.GetBytes(")\n"));

        return data.ToArray();
    }

    public override string ToString() => string.Format("Map: {0} : {1}", Key.Value, ((MapValueHolder)Value).Value);
}