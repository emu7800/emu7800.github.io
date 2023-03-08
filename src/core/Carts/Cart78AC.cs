namespace EMU7800.Core;

/// <summary>
/// Atari 7800 Activision bankswitched cartridge
/// </summary>
public sealed class Cart78AC : Cart
{
    //
    // Cart Format                 Mapping to ROM Address Space
    // Bank0 : 0x00000:0x2000
    // Bank1 : 0x02000:0x2000
    // Bank2 : 0x04000:0x2000      0x4000:0x2000  Bank13
    // Bank3 : 0x06000:0x2000      0x6000:0x2000  Bank12
    // Bank4 : 0x08000:0x2000      0x8000:0x2000  Bank15
    // Bank5 : 0x0a000:0x2000      0xa000:0x2000  Bank(2*n)   n in [0-7], n=0 on startup
    // Bank6 : 0x0c000:0x2000      0xc000:0x2000  Bank(2*n+1)
    // Bank7 : 0x0e000:0x2000      0xe000:0x2000  Bank14
    // Bank8 : 0x10000:0x2000
    // Bank9 : 0x12000:0x2000
    // Bank10: 0x14000:0x2000
    // Bank11: 0x16000:0x2000
    // Bank12: 0x18000:0x2000
    // Bank13: 0x1a000:0x2000
    // Bank14: 0x1c000:0x2000
    // Bank15: 0x1e000:0x2000
    //
    // Banks are actually 16KB, but handled as 8KB for implementation ease.
    //
    readonly int[] Bank = new[] { 0, 0, 13, 12, 15, 0, 1, 14 };

    #region IDevice Members

    const int
        ROM_SHIFT = 13,  // 8 KB, 0x2000
        ROM_SIZE  = 1 << ROM_SHIFT,
        ROM_MASK  = ROM_SIZE - 1
        ;

    public override byte this[ushort addr] {
        get => ROM[(Bank[addr >> ROM_SHIFT] << ROM_SHIFT) | (addr & ROM_MASK)];
        set {
            if ((addr & 0xfff0) == 0xff80) {
                Bank[5] = (addr & 7) << 1;
                Bank[6] = Bank[5] + 1;
            }
        }
    }

    #endregion

    public override string ToString()
        => "EMU7800.Core." + nameof(Cart78AC);

    public Cart78AC(byte[] romBytes)
        => LoadRom(romBytes, ROM_SIZE * 16);

    #region Serialization Members

    public Cart78AC(DeserializationContext input) : base(input)
    {
        input.CheckVersion(1);
        LoadRom(input.ReadBytes());
        Bank = input.ReadIntegers(8);
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