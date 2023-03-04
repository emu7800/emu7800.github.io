/*
 * HSC7800.cs
 *
 * The 7800 High Score cartridge--courtesy of Matthias <matthias@atari8bit.de>.
 *
 *   2KB NVRAM at $1000-$17ff
 *   4KB ROM   at $3000-$3fff
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

    public override void Reset()
    {
        Cart.Reset();
    }

    public override byte this[ushort addr]
    {
        get => (addr & 0xf000) switch
        {
            0x1000 => NVRAM[addr & 0x7ff],
            0x3000 => ROM[addr & 0xfff],
            _ => Cart[addr]
        };
        set
        {
            NVRAM[addr & 0x7ff] = value;
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
        NVRAM = LoadNVRAM();
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

    static byte[] LoadNVRAM()
    {
        try
        {
            var bytes = File.ReadAllBytes(GetHSCPath());
            if (bytes.Length >= 0x1000)
            {
                return bytes;
            }
        }
        catch
        {
        }
        return new byte[0x1000];
    }

    void SaveNVRAM(byte[] bytes)
    {
        try
        {
            File.WriteAllBytes(GetHSCPath(), bytes);
        }
        catch
        {
        }
    }

    static string GetHSCPath()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Saved Games", "EMU7800", ".hscnvram");
}