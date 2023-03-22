/*
 * RAM6116.cs
 *
 * Implements a 6116 RAM device found in the 7800.
 *
 * Copyright © 2004 Mike Murphy
 *
 */
namespace EMU7800.Core;

public sealed class RAM6116 : IDevice
{
    public static readonly RAM6116 Default = new();

    readonly byte[] RAM = new byte[ROM_SIZE];

    #region IDevice Members

    public void Reset() {}

    const int
        ROM_SHIFT = 11,  // 2 KB, 0x800
        ROM_SIZE  = 1 << ROM_SHIFT,
        ROM_MASK  = ROM_SIZE - 1
        ;

    public byte this[ushort addr]
    {
        get => RAM[addr & ROM_MASK];
        set => RAM[addr & ROM_MASK] = value;
    }

    #endregion

    #region Constructors

    public RAM6116()
    {
    }

    #endregion

    #region Serialization Members

    public RAM6116(DeserializationContext input)
    {
        input.CheckVersion(1);
        RAM = input.ReadExpectedBytes(ROM_SIZE);
    }

    public void GetObjectData(SerializationContext output)
    {
        output.WriteVersion(1);
        output.Write(RAM);
    }

    #endregion
}