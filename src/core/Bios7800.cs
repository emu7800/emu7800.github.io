/*
 * BIOS7800.cs
 *
 * The BIOS of the Atari 7800.
 *
 * Copyright © 2004 Mike Murphy
 *
 */
using System;
using System.ComponentModel.DataAnnotations;

namespace EMU7800.Core;

public sealed class Bios7800 : IDevice
{
    public static readonly Bios7800 Default = new();

    readonly byte[] ROM;
    readonly ushort Mask;

    public ushort Size => (ushort)ROM.Length;

    #region IDevice Members

    public void Reset() { }

    public byte this[ushort addr]
    {
        get => ROM[addr & Mask];
        set { }
    }

    #endregion

    Bios7800()
    {
        ROM = new byte[1];
        Mask = 0;
    }

    public Bios7800(byte[] rom)
    {
        if (rom is { Length: not 4096 and not 16384 })
            throw new ArgumentException("ROM size not 4096 or 16384", nameof(rom));

        ROM = rom;
        Mask = (ushort)ROM.Length;
        Mask--;
    }

    #region Serialization Members

    public Bios7800(DeserializationContext input)
    {
        input.CheckVersion(1);
        ROM = input.ReadExpectedBytes(4096, 16384);

        Mask = (ushort)ROM.Length;
        Mask--;
    }

    public void GetObjectData(SerializationContext output)
    {
        output.WriteVersion(1);
        output.Write(ROM);
    }

    #endregion
}