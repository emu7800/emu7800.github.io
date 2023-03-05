/*
 * HSC7800.cs
 *
 * The 7800 High Score cartridge--courtesy of Matthias <matthias@atari8bit.de>.
 *
 *   2KB NVRAM         $1000-$17ff
 *   4KB ROM           $3000-$3fff
 *
 */
using System;
using System.IO;

namespace EMU7800.Core;

public sealed class HSC7800 : Cart
{
    readonly byte[] NVRAM;
    readonly Cart Cart = Default;

    #region IDevice Members

    const int
        ROM_SHIFT   = 12, // 4 KB rom size
        ROM_SIZE    = 1 << ROM_SHIFT,
        ROM_MASK    = ROM_SIZE - 1,
        NVRAM_SHIFT = 11, // 2 KB nvram size
        NVRAM_SIZE  = 1 << NVRAM_SHIFT,
        NVRAM_MASK  = NVRAM_SIZE - 1
        ;

    public override void Reset()
    {
        base.Reset();
        Cart.Reset();
    }

    public override byte this[ushort addr]
    {
        get => (addr & 0xf000) switch
        {
            0x1000 => NVRAM[addr & NVRAM_MASK],
            0x3000 => ROM[addr & ROM_MASK],
            _      => Cart[addr]
        };
        set
        {
            NVRAM[addr & NVRAM_MASK] = value;
        }
    }

    #endregion

    public override bool Map()
    {
        M?.Mem.Map(0x1000, 0x800, this);
        M?.Mem.Map(0x3000, 0x1000, this);
        M?.Mem.Map(0x4000, 0xc000, Cart);
        return true;
    }

    #region Constructors

    HSC7800()
    {
        ROM = new byte[0x1000];
        NVRAM = new byte[0x800];
        LoadNVRAM(NVRAM);
    }

    public HSC7800(byte[] hscRom, Cart cart) : this()
    {
        if (hscRom.Length != ROM.Length)
            throw new ArgumentException($"ROM size not {ROM.Length}", nameof(hscRom));

        LoadRom(hscRom);
        Cart = cart;
    }

    #endregion

    #region Serialization Members

    public HSC7800(DeserializationContext input) : this()
    {
        input.CheckVersion(1);
        LoadRom(input.ReadBytes());
        Cart = input.ReadCart(M);
    }

    public override void GetObjectData(SerializationContext output)
    {
        output.WriteVersion(1);
        output.Write(ROM);
        output.Write(Cart);
        SaveNVRAM(NVRAM);
    }

    #endregion

    static void LoadNVRAM(byte[] bytes)
    {
        try
        {
            var readBytes = File.ReadAllBytes(GetHSCNVRAMPath());
            if (readBytes.Length == bytes.Length)
            {
                Buffer.BlockCopy(readBytes, 0, bytes, 0, bytes.Length);
            }
        }
        catch
        {
        }
    }

    static void SaveNVRAM(byte[] bytes)
    {
        try
        {
            File.WriteAllBytes(GetHSCNVRAMPath(), bytes);
        }
        catch
        {
        }
    }

    static string GetHSCNVRAMPath()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Saved Games", "EMU7800", ".hscnvram");
}