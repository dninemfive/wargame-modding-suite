using System;
using System.Collections.Generic;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfEugInt2 : NdfFlatValueWrapper
{
    private int _value2;

    public NdfEugInt2(int value1, int value2)
        : base(NdfType.EugInt2, value1)
    {
        Value2 = value2;
    }

    public int Value2
    {
        get { return _value2; }
        set
        {
            _value2 = value;
            OnPropertyChanged("Value2");
        }
    }

    public override byte[] GetBytes()
    {
        List<byte> value = new();
        value.AddRange(BitConverter.GetBytes(Convert.ToInt32(Value)));
        value.AddRange(BitConverter.GetBytes(Convert.ToInt32(Value2)));
        return value.ToArray();
    }

    public override byte[] GetNdfText()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return string.Format("Int pair: {0} : {1}", Value, Value2);
    }
}
