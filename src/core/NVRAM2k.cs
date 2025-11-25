/*
 * NVRAM2k.cs
 *
 * Implements a non-volatile 2KB memory device.
 *
 * Copyright © 2023 Mike Murphy
 *
 */
using System;

namespace EMU7800.Core;

public sealed class NVRAM2k : IDevice
{
    public static Func<string, int, ReadOnlyMemory<byte>> ReadNVRAMBytes { get; set; } = (_, count) => new byte[count];
    public static Action<string, ReadOnlyMemory<byte>> WriteNVRAMBytes { get; set; } = (_, _) => {};

    readonly string _fileName;

    byte[] NVRAM
    {
        get
        {
            if (field.Length == 0)
            {
                field = ReadNVRAMBytes(_fileName, NVRAM_SIZE).ToArray();
            }
            return field;
        }
        set;
    } = [];

    #region IDevice

    const int
        NVRAM_SHIFT = 11, // 2 KB size
        NVRAM_SIZE  = 1 << NVRAM_SHIFT,
        NVRAM_MASK  = NVRAM_SIZE - 1
        ;

    public byte this[ushort addr]
    {
        get => NVRAM[addr & NVRAM_MASK];
        set => NVRAM[addr & NVRAM_MASK] = value;
    }

    public void Reset()
    {
    }

    #endregion

    #region Constructors

    public NVRAM2k(string fileName)
    {
        _fileName = fileName;
    }

    #endregion

    #region Serialization Members

    public NVRAM2k(DeserializationContext input)
    {
        input.CheckVersion(1);
        _fileName = input.ReadString();
    }

    public void GetObjectData(SerializationContext output)
    {
        output.WriteVersion(1);
        output.Write(_fileName);
        WriteNVRAMBytes(_fileName, NVRAM);
    }

    #endregion
}
