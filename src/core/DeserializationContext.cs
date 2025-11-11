using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using EMU7800.Core.Extensions;

namespace EMU7800.Core;

/// <summary>
/// A context for deserializing <see cref="MachineBase"/> objects.
/// </summary>
public class DeserializationContext
{
    #region Fields

    readonly BinaryReader _binaryReader;

    #endregion

    public string ReadString()
        => _binaryReader.ReadString();

    public bool ReadBoolean()
        => _binaryReader.ReadBoolean();

    public byte ReadByte()
        => _binaryReader.ReadByte();

    public ushort ReadUInt16()
        => _binaryReader.ReadUInt16();

    public int ReadInt32()
        => _binaryReader.ReadInt32();

    public uint ReadUInt32()
        => _binaryReader.ReadUInt32();

    public long ReadInt64()
        => _binaryReader.ReadInt64();

    public ulong ReadUInt64()
        => _binaryReader.ReadUInt64();

    public double ReadDouble()
        => _binaryReader.ReadDouble();

    public byte[] ReadBytes()
    {
        var count = _binaryReader.ReadInt32();
        return count switch
        {
            <= 0 => [],
            <= 0x40000 => _binaryReader.ReadBytes(count),
            _ => throw new SerializationException("Byte array length too large")
        };
    }

    public byte[] ReadExpectedBytes(params int[] expectedSizes)
    {
        var count = _binaryReader.ReadInt32();
        SerializationException.ThrowIf(expectedSizes.All(t => t != count), "Byte array length incorrect");
        return _binaryReader.ReadBytes(count);
    }

    public byte[] ReadOptionalBytes(params int[] expectedSizes)
        => _binaryReader.ReadBoolean() ? ReadExpectedBytes(expectedSizes) : [];

    public ushort[] ReadUnsignedShorts(params int[] expectedSizes)
    {
        var bytes = ReadExpectedBytes(expectedSizes.Select(t => t << 1).ToArray());
        var ushorts = new ushort[bytes.Length >> 1];
        Buffer.BlockCopy(bytes, 0, ushorts, 0, bytes.Length);
        return ushorts;
    }

    public int[] ReadIntegers(params int[] expectedSizes)
    {
        var bytes = ReadExpectedBytes(expectedSizes.Select(t => t << 2).ToArray());
        var integers = new int[bytes.Length >> 2];
        Buffer.BlockCopy(bytes, 0, integers, 0, bytes.Length);
        return integers;
    }

    public uint[] ReadUnsignedIntegers(params int[] expectedSizes)
    {
        var bytes = ReadExpectedBytes(expectedSizes.Select(t => t << 2).ToArray());
        var uints = new uint[bytes.Length >> 2];
        Buffer.BlockCopy(bytes, 0, uints, 0, bytes.Length);
        return uints;
    }

    public bool[] ReadBooleans(params int[] expectedSizes)
    {
        var bytes = ReadExpectedBytes(expectedSizes);
        var booleans = new bool[bytes.Length];
        for (var i = 0; i < bytes.Length; i++)
            booleans[i] = bytes[i] != 0;
        return booleans;
    }

    public int CheckVersion(params int[] validVersions)
    {
        var magicNumber = _binaryReader.ReadInt32();
        SerializationException.ThrowIf(magicNumber != 0x78000087, "Magic number not found");
        var version = _binaryReader.ReadInt32();
        SerializationException.ThrowIf(validVersions.All(t => t != version), "Invalid version number found");
        return version;
    }

    public MachineBase ReadMachine()
        => CreateMachine(_binaryReader.ReadString());

    public MachineBase CreateMachine(string typeName)
    => typeName.Split('.')[^1] switch
    {
        nameof(Machine2600NTSC) => new Machine2600NTSC(this),
        nameof(Machine2600PAL)  => new Machine2600PAL(this),
        nameof(Machine7800NTSC) => new Machine7800NTSC(this),
        nameof(Machine7800PAL)  => new Machine7800PAL(this),
        _ => throw new SerializationException($"Unable to resolve type name: '{typeName}'")
    };

    public AddressSpace ReadAddressSpace(MachineBase m, int addrSpaceShift, int pageShift)
        => new(this, m, addrSpaceShift, pageShift);

    public M6502 ReadM6502(MachineBase m, int runClocksMultiple)
        => new(this, m, runClocksMultiple);

    public Maria ReadMaria(Machine7800 m, int scanlines)
        => new(this, m, scanlines);

    public PIA ReadPIA(MachineBase m)
        => new(this, m);

    public TIA ReadTIA(MachineBase m)
        => new(this, m);

    public TIASound ReadTIASound(MachineBase m, int cpuClocksPerSample)
        => new(this, m, cpuClocksPerSample);

    public RAM6116 ReadRAM6116()
        => new(this);

    public InputState ReadInputState()
        => new(this);

    public Bios7800 ReadOptionalBios7800()
        => ReadBoolean() ? new Bios7800(this) : Bios7800.Default;

    public PokeySound ReadOptionalPokeySound(MachineBase m)
        => ReadBoolean() ? new PokeySound(this, m) : PokeySound.Default;

    public YM2151 ReadOptionalYM2151(MachineBase m)
        => ReadBoolean() ? new YM2151(this, m) : YM2151.Default;

    public NVRAM2k ReadNVRAM2k()
        => new(this);

    public Cart ReadCart(MachineBase m)
        => CreateCart(m, _binaryReader.ReadString());

    public Cart CreateCart(MachineBase m, string typeName)
        => typeName.Split('.')[^1] switch
        {
            nameof(CartA2K)         => new CartA2K(this),
            nameof(CartA4K)         => new CartA4K(this),
            nameof(CartA8K)         => new CartA8K(this),
            nameof(CartA8KR)        => new CartA8KR(this),
            nameof(CartA16K)        => new CartA16K(this),
            nameof(CartA16KR)       => new CartA16KR(this),
            nameof(CartDC8K)        => new CartDC8K(this),
            nameof(CartPB8K)        => new CartPB8K(this),
            nameof(CartTV8K)        => new CartTV8K(this),
            nameof(CartCBS12K)      => new CartCBS12K(this),
            nameof(CartA32K)        => new CartA32K(this),
            nameof(CartA32KR)       => new CartA32KR(this),
            nameof(CartMN16K)       => new CartMN16K(this),
            nameof(CartDPC)         => new CartDPC(this),
            nameof(Cart7808)        => new Cart7808(this),
            nameof(Cart7816)        => new Cart7816(this),
            nameof(Cart7832P)       => new Cart7832P(this, m),
            nameof(Cart7832PL)      => new Cart7832PL(this, m),
            nameof(Cart7832)        => new Cart7832(this),
            nameof(Cart7848)        => new Cart7848(this),
            nameof(Cart78SGP)       => new Cart78SGP(this, m),
            nameof(Cart78SG)        => new Cart78SG(this),
            nameof(Cart78S9)        => new Cart78S9(this),
            nameof(Cart78S9PL)      => new Cart78S9PL(this, m),
            nameof(Cart78S4)        => new Cart78S4(this),
            nameof(Cart78AB)        => new Cart78AB(this),
            nameof(Cart78AC)        => new Cart78AC(this),
            nameof(Cart78BB32K)     => new Cart78BB32K(this),
            nameof(Cart78BB32KP)    => new Cart78BB32KP(this, m),
            nameof(Cart78BB32KRPL)  => new Cart78BB32KRPL(this, m),
            nameof(Cart78BB48K)     => new Cart78BB48K(this),
            nameof(Cart78BB48KP)    => new Cart78BB48KP(this, m),
            nameof(Cart78BB52K)     => new Cart78BB52K(this),
            nameof(Cart78BB52KP)    => new Cart78BB52KP(this, m),
            nameof(Cart78BB128K)    => new Cart78BB128K(this),
            nameof(Cart78BB128KR)   => new Cart78BB128KR(this),
            nameof(Cart78BB128KRPL) => new Cart78BB128KRPL(this, m),
            nameof(Cart78BB128KP)   => new Cart78BB128KP(this, m),
            nameof(HSC7800)         => new HSC7800(this),
            nameof(XM7800)          => new XM7800(this, m),
            _ => throw new SerializationException($"Unable to resolve type name: '{typeName}'")
        };

    #region Constructors

    /// <summary>
    /// Instantiates a new instance of <see cref="DeserializationContext"/>.
    /// </summary>
    /// <param name="binaryReader"/>
    internal DeserializationContext(BinaryReader binaryReader)
    {
        _binaryReader = binaryReader;
    }

    #endregion
}