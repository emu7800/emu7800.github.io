namespace EMU7800.Core;

/// <summary>
/// Atari 7800 Bankset Bankswitched cartridge - 2x52K
/// </summary>
public sealed class Cart78BB52K : Cart78BB
{
    // Address Space    Cart/Device
    // 0x3000:0xd000    0x0000:0xd000 ROM CPU readable
    // 0x3000:0xd000    0xd000:0xd000 ROM Maria readable

    #region IDevice Members

    const int
       ROM_SHIFT = 17,
       ROM_SIZE  = 1 << ROM_SHIFT // 128 KB, 0x20000
       ;

    public override byte this[ushort addr]
    {
        get => ROM[(M.Mem.MariaRead << (ROM_SHIFT - 1)) | addr];
        set {}
    }

    #endregion

    public override bool Map()
    {
        M.Mem.Map(0x3000, 0xd000, this);
        return true;
    }

    public Cart78BB52K(byte[] romBytes)
    {
        LoadRom(romBytes, ROM_SIZE);
    }

    #region Serialization Members

    public Cart78BB52K(DeserializationContext input) : base(input)
    {
        _ = input.CheckVersion(1);
        LoadRom(input.ReadBytes());
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);
        output.WriteVersion(1);
        output.Write(ROM);
    }

    #endregion
}