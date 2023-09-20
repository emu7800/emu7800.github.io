namespace EMU7800.Core;

/// <summary>
/// Atari 7800 Bankset Bankswitched cartridge - 2x128K w/RAM@4000
/// </summary>
public sealed class Cart78BB128KR : Cart78BB
{
    // Address Space   Cart/Device
    // 0x4000:0x4000    0x0000:0x4000 RAM CPU - 16kb bank 6
    // 0x4000:0x4000    0x4000:0x4000 RAM Maria readable - 16kb bank 6
    // 0x8000:0x4000   0xac000:0x4000 ROM CPU readable - 16kb bank 0-7 (0 on startup)  - a:{0, 1}, c:{0, 4, 8, C}
    // 0x8000:0x4000   0xbc000:0x4000 ROM Maria readable - 16kb bank 0-7 (0 on startup) - b:{2, 3}, c:{0, 4, 8, C}
    // 0xC000:0x4000   0x1C000:0x4000 ROM CPU readable - 16kb bank 7
    // 0xC000:0x4000   0x3C000:0x4000 ROM Maria readable - 16kb bank 7
    // 0xC000:0x4000    0x4000:0x4000 RAM CPU writable

    readonly int[] Bank = [0, 6, 0, 7];

    #region IDevice Members

    const int
        ROM_SHIFT     = 18,
        ROM_SIZE      = 1 << ROM_SHIFT,     // 256 KB, 0x40000
        ROMBANK_SHIFT = 14,
        ROMBANK_SIZE  = 1 << ROMBANK_SHIFT, //  16 KB, 0x4000
        ROMBANK_MASK  = ROMBANK_SIZE - 1,
        RAMBANK_SHIFT = 14,
        RAMBANK_SIZE  = 1 << RAMBANK_SHIFT, //  16 KB, 0x4000
        RAMBANK_MASK  = RAMBANK_SIZE - 1
        ;

    public override byte this[ushort addr]
    {
        get
        {
            var bankNo = addr >> ROMBANK_SHIFT;
            return bankNo switch
            {
                1 => RAM[M.Mem.MariaRead << RAMBANK_SHIFT | addr & RAMBANK_MASK],
                _ => ROM[M.Mem.MariaRead << (ROM_SHIFT - 1) | Bank[bankNo] << ROMBANK_SHIFT | addr & ROMBANK_MASK]
            };
        }
        set
        {
            var bankNo = addr >> ROMBANK_SHIFT;
            switch (bankNo)
            {
                case 1:
                    RAM[addr & RAMBANK_MASK] = value;
                    break;
                case 2:
                    Bank[2] = value & 7;
                    break;
                case 3:
                    RAM[RAMBANK_SIZE | addr & RAMBANK_MASK] = value;
                    break;
            }
        }
    }

    #endregion

    public override string ToString()
        => GetType().FullName ?? string.Empty;

    public Cart78BB128KR(byte[] romBytes)
    {
        LoadRom(romBytes, ROM_SIZE);
        InitRam(RAMBANK_SIZE << 1);
    }

    #region Serialization Members

    public Cart78BB128KR(DeserializationContext input) : base(input)
    {
        _ = input.CheckVersion(1);
        LoadRom(input.ReadBytes());
        Bank = input.ReadIntegers(4);
        LoadRam(input.ReadBytes());
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);
        output.WriteVersion(1);
        output.Write(ROM);
        output.Write(Bank);
        output.Write(RAM);
    }

    #endregion
}