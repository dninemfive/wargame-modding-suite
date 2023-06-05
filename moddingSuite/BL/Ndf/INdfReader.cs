using moddingSuite.Model.Ndfbin;

namespace moddingSuite.BL.Ndf;

public interface INdfReader
{
    NdfBinary Read(byte[] data);

}
