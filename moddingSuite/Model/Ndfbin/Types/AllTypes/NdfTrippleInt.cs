using System;
using System.Collections.Generic;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfTrippleInt : NdfFlatValueWrapper
{
    private int _value2;
    private int _value3;

    public NdfTrippleInt(int value1, int value2, int value3)
        : base(NdfType.TrippleInt, value1)
    {
        Value2 = value2;
        Value3 = value3;
    }

    public int Value2
    {
        get => _value2;
        set
        {
            _value2 = value;
            OnPropertyChanged("Value2");
        }
    }

    public int Value3
    {
        get => _value3;
        set
        {
            _value3 = value;
            OnPropertyChanged("Value3");
        }
    }

    public override byte[] GetBytes()
    {
        List<byte> value = new();
        value.AddRange(BitConverter.GetBytes(Convert.ToInt32(Value)));
        value.AddRange(BitConverter.GetBytes(Convert.ToInt32(Value2)));
        value.AddRange(BitConverter.GetBytes(Convert.ToInt32(Value3)));
        return value.ToArray();
    }

    public override byte[] GetNdfText() => throw new NotImplementedException();

    public override string ToString() => string.Format("Int tripplet: {0} : {1} : {2}", Value, Value2, Value3);

}
