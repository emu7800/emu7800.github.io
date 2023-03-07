using System;

namespace EMU7800.Core;

/// <summary>
/// Atari 7800 SuperGame S4 bankswitched cartridge
/// </summary>
public sealed class Cart78S4 : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    // Bank0: 0x00000:0x4000
    // Bank1: 0x04000:0x4000      0x4000:0x4000  Bank2
    // Bank2: 0x08000:0x4000      0x8000:0x4000  Bank0 (0 on startup)
    // Bank3: 0x0c000:0x4000      0xc000:0x4000  Bank3
    // Bank4: 0x10000:0x4000
    // Bank5: 0x14000:0x4000
    // Bank6: 0x18000:0x4000
    // Bank7: 0x1c000:0x4000
    //
    readonly byte[] RAM = Array.Empty<byte>();
    readonly int[] Bank = new int[4];

    #region IDevice Members

    const int
        ROM_SHIFT = 14,   // 16 KB, 0x4000
        ROM_SIZE  = 1 << ROM_SHIFT,
        ROM_MASK  = ROM_SIZE - 1,
        RAM_SHIFT = 13,   //  8 KB, 0x2000
        RAM_SIZE  = 1 << RAM_SHIFT,
        RAM_MASK  = RAM_SIZE - 1
        ;

    public override byte this[ushort addr]
    {
        get
        {
            if (RAM.Length > 0 && addr >= 0x6000 && addr < 0x6000 + RAM_SIZE)
            {
                return RAM[addr & RAM_MASK];
            }
            return ROM[(Bank[addr >> ROM_SHIFT] << ROM_SHIFT) | (addr & ROM_MASK)];
        }
        set
        {
            if (RAM.Length > 0 && addr >= 0x6000 && addr < 0x6000 + RAM_SIZE)
            {
                RAM[addr & RAM_MASK] = value;
            }
            else if ((addr >> ROM_SHIFT) == 2)
            {
                Bank[2] = value & 3;
            }
        }
    }

    #endregion

    public override string ToString()
        => "EMU7800.Core.Cart78S4" + (RAM.Length > 0 ? "R" : string.Empty);

    public Cart78S4(byte[] romBytes, bool needRAM)
    {
        if (needRAM)
        {
            RAM = new byte[0x2000];
        }

        LoadRom(romBytes, 0xffff);

        Bank[1] = 2;
        Bank[2] = 0;
        Bank[3] = 3;
    }

    #region Serialization Members

    public Cart78S4(DeserializationContext input) : base(input)
    {
        var version = input.CheckVersion(1, 2);
        LoadRom(input.ReadBytes());
        Bank = input.ReadIntegers(4);
        if (version == 1)
            input.ReadInt32();
        RAM = input.ReadOptionalBytes();
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