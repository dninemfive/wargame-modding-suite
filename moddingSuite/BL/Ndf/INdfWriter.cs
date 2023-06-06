using moddingSuite.Model.Ndfbin;
using System.IO;

namespace moddingSuite.BL.Ndf;

internal interface INdfWriter
{
    void Write(Stream outStrea, NdfBinary ndf, bool compressed);
}
