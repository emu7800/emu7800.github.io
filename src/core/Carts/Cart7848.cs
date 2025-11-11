namespace EMU7800.Core;

/// <summary>
/// Atari 7800 non-bankswitched 48KB cartridge
/// </summary>
public sealed class Cart7848 : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    // 0x0000:0xc000              0x4000:0xc000
    //

    #region IDevice Members

    const int
        ROM_SHIFT = 14, // 16 KB rom size
        ROM_SIZE  = 1 << ROM_SHIFT,
        ROM_MASK  = ROM_SIZE - 1
        ;

    public override byte this[ushort addr]
    {
        get => ROM[((addr >> ROM_SHIFT) - 1) << ROM_SHIFT | (addr & ROM_MASK)];
        set {}
    }

    #endregion

    public Cart7848(byte[] romBytes)
        => LoadRom(romBytes, ROM_SIZE * 3);

    #region Serialization Members

    public Cart7848(DeserializationContext input) : base(input)
    {
        input.CheckVersion(1);
        LoadRom(input.ReadExpectedBytes(ROM_SIZE * 3), ROM_SIZE * 3);
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);
        output.WriteVersion(1);
        output.Write(ROM);
    }

    #endregion
}