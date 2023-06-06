using moddingSuite.Model.Ndfbin.Types.AllTypes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace moddingSuite.Model.Ndfbin.Types;

public static class NdfTypeManager
{
    public static NdfType GetType(byte[] data)
    {
        if (data.Length != 4)
            return NdfType.Unknown;

        uint value = BitConverter.ToUInt32(data, 0);

        return Enum.IsDefined(typeof(NdfType), value) ? (NdfType)value : NdfType.Unknown;
    }

    public static NdfValueWrapper GetValue(byte[] data, NdfType type, NdfBinary mgr)
    {
        //if (data.Length != SizeofType(type))
        //    return null;

        switch (type)
        {
            case NdfType.Boolean:
                return new NdfBoolean(BitConverter.ToBoolean(data, 0));
            case NdfType.Int8:
                return new NdfInt8(data[0]);
            case NdfType.Int16:
                return new NdfInt16(BitConverter.ToInt16(data, 0));
            case NdfType.UInt16:
                return new NdfUInt16(BitConverter.ToUInt16(data, 0));
            /*case NdfType.Int24:
                var buf = new byte[4];
                
                Array.Copy(data, buf, 3);
                return new NdfInt32(BitConverter.ToInt32(buf, 0));*/
            case NdfType.Int32:
                return new NdfInt32(BitConverter.ToInt32(data, 0));
            case NdfType.UInt32:
                return new NdfUInt32(BitConverter.ToUInt32(data, 0));
            case NdfType.Long:
                return new NdfLong(BitConverter.ToInt64(data, 0));
            case NdfType.Float32:
                return new NdfSingle(BitConverter.ToSingle(data, 0));
            case NdfType.Float64:
                return new NdfDouble(BitConverter.ToDouble(data, 0));

            case NdfType.TableStringFile:
                int id = BitConverter.ToInt32(data, 0);
                return new NdfFileNameString(mgr.Strings[id]);
            case NdfType.TableString:
                int id2 = BitConverter.ToInt32(data, 0);
                return new NdfString(mgr.Strings[id2]);
            case NdfType.Color32:
                return new NdfColor32(Color.FromArgb(data[3], data[0], data[1], data[2]));
            case NdfType.Color128:
                return new NdfColor128(data);
            case NdfType.Vector:
                byte[] px = data.Take(4).ToArray();
                byte[] py = data.Skip(4).Take(4).ToArray();
                byte[] pz = data.Skip(8).ToArray();
                return new NdfVector(new Point3D(BitConverter.ToSingle(px, 0),
                                                 BitConverter.ToSingle(py, 0),
                                                 BitConverter.ToSingle(pz, 0)));

            case NdfType.ObjectReference:
                uint instId = BitConverter.ToUInt32(data.Take(4).ToArray(), 0);
                uint clsId = BitConverter.ToUInt32(data.Skip(4).ToArray(), 0);
                NdfClass cls = null;

                // possible deadrefs here
                if (clsId <= mgr.Classes.Count)
                    cls = mgr.Classes[(int)clsId];

                return new NdfObjectReference(cls, instId, cls == null);

            case NdfType.Map:
                return new NdfMap(new MapValueHolder(null, mgr), new MapValueHolder(null, mgr), mgr);

            case NdfType.Guid:
                return new NdfGuid(new Guid(data));

            case NdfType.WideString:
                return new NdfWideString(Encoding.Unicode.GetString(data));

            case NdfType.TransTableReference:
                int id3 = BitConverter.ToInt32(data, 0);
                return new NdfTrans(mgr.Trans[id3]);

            case NdfType.LocalisationHash:
                return new NdfLocalisationHash(data);

            case NdfType.List:
                return new NdfCollection();
            case NdfType.MapList:
                return new NdfMapList();

            case NdfType.Blob:
                return new NdfBlob(data);

            case NdfType.ZipBlob:
                return new NdfZipBlob(data);

            case NdfType.EugInt2:
                return new NdfEugInt2(BitConverter.ToInt32(data, 0), BitConverter.ToInt32(data, 4));
            case NdfType.EugFloat2:
                return new NdfEugFloat2(BitConverter.ToSingle(data, 0), BitConverter.ToSingle(data, 4));

            case NdfType.TrippleInt:
                return new NdfTrippleInt(BitConverter.ToInt32(data, 0), BitConverter.ToInt32(data, 4), BitConverter.ToInt32(data, 8));

            case NdfType.Hash:
                return new NdfHash(data);

            case NdfType.Time64:
                return new NdfTime64(new DateTime(1970, 1, 1).AddSeconds(BitConverter.ToUInt32(data, 0)));

            case NdfType.Unset:
                return new NdfNull();

            default:
                return null;
        }
    }

    public static uint SizeofType(NdfType type) => type switch
    {
        NdfType.Boolean or NdfType.Int8 => 1,
        NdfType.Int16 or NdfType.UInt16 => 2,
        NdfType.Int32 or NdfType.UInt32 or NdfType.Float32 or NdfType.TableStringFile or NdfType.TableString or NdfType.Color32 or NdfType.WideString => 4,
        NdfType.Float64 or NdfType.Long or NdfType.LocalisationHash or NdfType.ObjectReference or NdfType.EugInt2 or NdfType.EugFloat2 or NdfType.Time64 => 8,
        NdfType.TrippleInt or NdfType.Vector => 12,
        NdfType.Color128 or NdfType.Guid or NdfType.Hash => 16,
        NdfType.Map => 0,
        NdfType.List or NdfType.MapList or NdfType.TransTableReference => 4,
        _ => 0,
    };

    public static IEnumerable<NdfType> GetTypeSelection() => new[]
                   {
                       NdfType.Boolean,
                       NdfType.Int32,
                       NdfType.UInt32,
                       NdfType.Long,
                       NdfType.Float32,
                       NdfType.ObjectReference,
                       NdfType.Map,
                       NdfType.List,
                       NdfType.TableString,
                       NdfType.TableStringFile,
                       NdfType.LocalisationHash,
                       NdfType.WideString,
                       NdfType.TransTableReference,
                       NdfType.Guid,
                       NdfType.Int8,
                       NdfType.Int16,
                       NdfType.UInt16,
                       NdfType.Color32,
                       NdfType.Float64,
                       NdfType.Vector,
                       NdfType.Color128,
                       NdfType.MapList
                   };
}