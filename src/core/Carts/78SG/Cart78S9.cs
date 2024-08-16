namespace EMU7800.Core;

/// <summary>
/// Atari 7800 SuperGame S9 bankswitched cartridge
/// </summary>
public sealed class Cart78S9 : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    // Bank0: 0x00000:0x4000
    // Bank1: 0x04000:0x4000      0x4000:0x4000  Bank0
    // Bank2: 0x08000:0x4000      0x8000:0x4000  Bank0-8 (1 on startup)
    // Bank3: 0x0c000:0x4000      0xc000:0x4000  Bank8
    // Bank4: 0x10000:0x4000
    // Bank5: 0x14000:0x4000
    // Bank6: 0x18000:0x4000
    // Bank7: 0x1c000:0x4000
    // Bank8: 0x20000:0x4000
    //
    readonly int[] Bank = [0, 0, 1, 8];

    #region IDevice Members

    const int
        ROM_SHIFT = 14,   // 16 KB, 0x4000
        ROM_SIZE  = 1 << ROM_SHIFT,
        ROM_MASK  = ROM_SIZE - 1
        ;

    public override byte this[ushort addr]
    {
        get => ROM[(Bank[addr >> ROM_SHIFT] << ROM_SHIFT) | (addr & ROM_MASK)];
        set
        {
            if (addr >> ROM_SHIFT == 2)
            {
                Bank[2] = (value & 7) + 1;
            }
        }
    }

    #endregion

    public override string ToString()
        => "EMU7800.Core." + nameof(Cart78S9);

    public Cart78S9(byte[] romBytes)
        => LoadRom(romBytes, ROM_SIZE * 9);

    #region Serialization Members

    public Cart78S9(DeserializationContext input) : base(input)
    {
        input.CheckVersion(1);
        LoadRom(input.ReadBytes());
        Bank = input.ReadIntegers(4);
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);
        output.WriteVersion(1);
        output.Write(ROM);
        output.Write(Bank);
    }

    #endregion
}