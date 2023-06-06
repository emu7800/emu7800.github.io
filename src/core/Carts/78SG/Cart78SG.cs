namespace EMU7800.Core;

/// <summary>
/// Atari 7800 SuperGame bankswitched cartridge
/// </summary>
public sealed class Cart78SG : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    // Bank0: 0x00000:0x4000
    // Bank1: 0x04000:0x4000      0x4000:0x4000  Bank6   (ROM or RAM)
    // Bank2: 0x08000:0x4000      0x8000:0x4000  Bank0-7 (0 on startup)
    // Bank3: 0x0c000:0x4000      0xc000:0x4000  Bank7
    // Bank4: 0x10000:0x4000
    // Bank5: 0x14000:0x4000
    // Bank6: 0x18000:0x4000
    // Bank7: 0x1c000:0x4000
    //
    readonly int[] Bank = new[] { 0, 6, 0, 7 };
    readonly byte[] RAM = System.Array.Empty<byte>();

    #region IDevice Members

    const int
        ROM_SHIFT = 14,   // 16 KB, 0x4000
        ROM_SIZE  = 1 << ROM_SHIFT,
        ROM_MASK  = ROM_SIZE - 1,
        RAM_SHIFT = 14,   // 16 KB, 0x4000
        RAM_SIZE  = 1 << RAM_SHIFT,
        RAM_MASK  = RAM_SIZE - 1
        ;

    public override byte this[ushort addr]
    {
        get
        {
            var bankNo = addr >> ROM_SHIFT;
            if (RAM.Length > 0 && bankNo == 1)
            {
                return RAM[addr & RAM_MASK];
            }
            return ROM[(Bank[bankNo] << ROM_SHIFT) | (addr & ROM_MASK)];
        }
        set
        {
            var bankNo = addr >> ROM_SHIFT;
            if (bankNo == 2)
            {
                Bank[2] = value & 7;
            }
            else if (RAM.Length >= 0x4000 && bankNo == 1)
            {
                RAM[addr & RAM_MASK] = value;
            }
        }
    }

    #endregion

    public override string ToString()
        => "EMU7800.Core.Cart78SG" + (RAM.Length > 0 ? "R" : string.Empty);

    public Cart78SG(byte[] romBytes, bool needRAM)
    {
        if (needRAM)
        {
            // This works for titles that use 8KB instead of 16KB
            RAM = new byte[RAM_SIZE];
        }
        LoadRom(romBytes, ROM_SIZE * 8);
    }

    #region Serialization Members

    public Cart78SG(DeserializationContext input) : base(input)
    {
        var version = input.CheckVersion(1, 2);
        LoadRom(input.ReadBytes());
        Bank = input.ReadIntegers(4);
        if (version == 1)
            input.ReadInt32();
        RAM = input.ReadOptionalBytes(RAM_SIZE);
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);
        output.WriteVersion(2);
        output.Write(ROM);
        output.Write(Bank);
        output.WriteOptional(RAM);
    }

    #endregion
}