namespace EMU7800.Core;

/// <summary>
/// Atari 7800 Absolute bankswitched cartridge
/// </summary>
public sealed class Cart78AB : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    // Bank0: 0x00000:0x4000
    // Bank1: 0x04000:0x4000      0x4000:0x4000  Bank0-1 (0 on startup)
    // Bank2: 0x08000:0x4000      0x8000:0x4000  Bank2
    // Bank3: 0x0c000:0x4000      0xc000:0x4000  Bank3
    //
    readonly int[] Bank = new[] { 0, 0, 2, 3 };

    #region IDevice Members

    const int
        ROM_SHIFT = 14, // 16 KB rom size
        ROM_SIZE  = 1 << ROM_SHIFT,
        ROM_MASK  = ROM_SIZE - 1
        ;

    public override byte this[ushort addr]
    {
        get => ROM[(Bank[addr >> ROM_SHIFT] << ROM_SHIFT) | (addr & ROM_MASK)];
        set
        {
            if ((addr >> ROM_SHIFT) == 2)
            {
                Bank[1] = (value - 1) & 1;
            }
        }
    }

    #endregion

    public override string ToString()
        => "EMU7800.Core." + nameof(Cart78AB);

    public Cart78AB(byte[] romBytes)
        =>  LoadRom(romBytes, 0x10000);

    #region Serialization Members

    public Cart78AB(DeserializationContext input) : base(input)
    {
        var version = input.CheckVersion(1, 2);
        LoadRom(input.ReadBytes());
        Bank = input.ReadIntegers(4);
        if (version == 1)
            input.ReadInt32();
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);
        output.WriteVersion(2);
        output.Write(ROM);
        output.Write(Bank);
    }

    #endregion
}