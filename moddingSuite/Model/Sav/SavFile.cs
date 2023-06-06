namespace moddingSuite.Model.Sav;

public class SavFile
{
    public byte[] Checksum { get; set; } = new byte[16];

    public uint FileSize { get; set; }

    public uint ContentSize1 { get; set; }
    public uint ContentSize2 { get; set; }

    public ulong SaveDate { get; set; }
}
