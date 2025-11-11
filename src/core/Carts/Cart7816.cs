namespace EMU7800.Core;

/// <summary>
/// Atari 7800 non-bankswitched 16KB cartridge
/// </summary>
public sealed class Cart7816 : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    // 0x0000:0x4000              0xC000:0x4000 (repeated downward to 0x4000)
    //

    #region IDevice Members

    const int
        ROM_SHIFT = 14,  // 8 KB, 0x4000
        ROM_SIZE  = 1 << ROM_SHIFT,
        ROM_MASK  = ROM_SIZE - 1
        ;

    public override byte this[ushort addr]
    {
        get => ROM[addr & ROM_MASK];
        set {}
    }

    #endregion

    public Cart7816(byte[] romBytes)
        => LoadRom(romBytes, ROM_SIZE);

    #region Serialization Members

    public Cart7816(DeserializationContext input) : base(input)
    {
        input.CheckVersion(1);
        LoadRom(input.ReadExpectedBytes(ROM_SIZE), ROM_SIZE);
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);
        output.WriteVersion(1);
        output.Write(ROM);
    }

    #endregion
}