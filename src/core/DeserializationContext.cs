using System;
using System.IO;
using System.Linq;

namespace EMU7800.Core
{
    /// <summary>
    /// A context for deserializing <see cref="MachineBase"/> objects.
    /// </summary>
    public class DeserializationContext
    {
        #region Fields

        readonly BinaryReader _binaryReader;

        #endregion

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

        public BufferElement ReadBufferElement()
        {
            var be = new BufferElement();
            for (var i = 0; i < BufferElement.SIZE; i++)
                be[i] = ReadByte();
            return be;
        }

        public byte[] ReadBytes()
        {
            var count = _binaryReader.ReadInt32();
            if (count <= 0)
                return new byte[0];
            if (count > 0x40000)
                throw new Emu7800SerializationException("Byte array length too large.");
            return _binaryReader.ReadBytes(count);
        }

        public byte[] ReadExpectedBytes(params int[] expectedSizes)
        {
            var count = _binaryReader.ReadInt32();
            if (!expectedSizes.Any(t => t == count))
                throw new Emu7800SerializationException("Byte array length incorrect.");
            return _binaryReader.ReadBytes(count);
        }

        public byte[] ReadOptionalBytes(params int[] expectedSizes)
            => _binaryReader.ReadBoolean() ? ReadExpectedBytes(expectedSizes) : Array.Empty<byte>();

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
                booleans[i] = (bytes[i] != 0);
            return booleans;
        }

        public int CheckVersion(params int[] validVersions)
        {
            var magicNumber = _binaryReader.ReadInt32();
            if (magicNumber != 0x78000087)
                throw new Emu7800SerializationException("Magic number not found.");
            var version = _binaryReader.ReadInt32();
            if (!validVersions.Any(t => t == version))
                throw new Emu7800SerializationException("Invalid version number found.");
            return version;
        }

        public MachineBase ReadMachine()
        {
            var typeName = _binaryReader.ReadString();
            if (string.IsNullOrWhiteSpace(typeName))
                throw new Emu7800SerializationException("Invalid type name.");

            return typeName switch
            {
                "EMU7800.Core.Machine2600NTSC" => new Machine2600NTSC(this),
                "EMU7800.Core.Machine2600PAL"  => new Machine2600PAL(this),
                "EMU7800.Core.Machine7800NTSC" => new Machine7800NTSC(this),
                "EMU7800.Core.Machine7800PAL"  => new Machine7800PAL(this),
                _                              => throw new Emu7800SerializationException("Unable to resolve type name: " + typeName),
            };
        }

        public AddressSpace ReadAddressSpace(MachineBase m, int addrSpaceShift, int pageShift)
            => new AddressSpace(this, m, addrSpaceShift, pageShift);

        public M6502 ReadM6502(MachineBase m, int runClocksMultiple)
            => new M6502(this, m, runClocksMultiple);

        public Maria ReadMaria(Machine7800 m, int scanlines)
            => new Maria(this, m, scanlines);

        public PIA ReadPIA(MachineBase m)
            => new PIA(this, m);

        public TIA ReadTIA(MachineBase m)
            => new TIA(this, m);

        public TIASound ReadTIASound(MachineBase m, int cpuClocksPerSample)
            => new TIASound(this, m, cpuClocksPerSample);

        public RAM6116 ReadRAM6116()
            => new RAM6116(this);

        public InputState ReadInputState()
            => new InputState(this);

        public HSC7800 ReadOptionalHSC7800()
            => ReadBoolean() ? new HSC7800(this) : HSC7800.Default;

        public Bios7800 ReadOptionalBios7800()
            => ReadBoolean() ? new Bios7800(this) : Bios7800.Default;

        public PokeySound ReadOptionalPokeySound(MachineBase m)
            => ReadBoolean() ? new PokeySound(this, m) : PokeySound.Default;

        public Cart ReadCart(MachineBase m)
        {
            var typeName = _binaryReader.ReadString();
            if (string.IsNullOrWhiteSpace(typeName))
                throw new Emu7800SerializationException("Invalid type name.");

            return typeName switch
            {
                "EMU7800.Core.CartA2K"      => new CartA2K(this),
                "EMU7800.Core.CartA4K"      => new CartA4K(this),
                "EMU7800.Core.CartA8K"      => new CartA8K(this),
                "EMU7800.Core.CartA8KR"     => new CartA8KR(this),
                "EMU7800.Core.CartA16K"     => new CartA16K(this),
                "EMU7800.Core.CartA16KR"    => new CartA16KR(this),
                "EMU7800.Core.CartDC8K"     => new CartDC8K(this),
                "EMU7800.Core.CartPB8K"     => new CartPB8K(this),
                "EMU7800.Core.CartTV8K"     => new CartTV8K(this),
                "EMU7800.Core.CartCBS12K"   => new CartCBS12K(this),
                "EMU7800.Core.CartA32K"     => new CartA32K(this),
                "EMU7800.Core.CartA32KR"    => new CartA32KR(this),
                "EMU7800.Core.CartMN16K"    => new CartMN16K(this),
                "EMU7800.Core.CartDPC"      => new CartDPC(this),
                "EMU7800.Core.Cart7808"     => new Cart7808(this),
                "EMU7800.Core.Cart7816"     => new Cart7816(this),
                "EMU7800.Core.Cart7832P"    => new Cart7832P(this, m),
                "EMU7800.Core.Cart7832"     => new Cart7832(this),
                "EMU7800.Core.Cart7848"     => new Cart7848(this),
                "EMU7800.Core.Cart78SGP"    => new Cart78SGP(this, m),
                "EMU7800.Core.Cart78SG"     => new Cart78SG(this),
                "EMU7800.Core.Cart78S9"     => new Cart78S9(this),
                "EMU7800.Core.Cart78S4"     => new Cart78S4(this),
                "EMU7800.Core.Cart78AB"     => new Cart78AB(this),
                "EMU7800.Core.Cart78AC"     => new Cart78AC(this),
                _                           => throw new Emu7800SerializationException("Unable to resolve type name: " + typeName),
            };
        }

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
}
