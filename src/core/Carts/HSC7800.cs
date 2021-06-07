/*
 * HSC7800.cs
 *
 * The 7800 High Score cartridge--courtesy of Matthias <matthias@atari8bit.de>.
 *
 */
using System;

namespace EMU7800.Core
{
    public sealed class HSC7800 : IDevice
    {
        public static readonly HSC7800 Default = new();

        readonly byte[] ROM;
        readonly ushort Mask;

        public ushort Size { get => (ushort)ROM.Length; }

        #region IDevice Members

        public void Reset()
        {
        }

        public byte this[ushort addr]
        {
            get => ROM[addr & Mask];
            set { }
        }

        #endregion

        public RAM6116 SRAM { get; } = new RAM6116();

        #region Constructors

        HSC7800()
        {
            ROM = new byte[1];
            Mask = 0;
        }

        public HSC7800(byte[] hscRom)
        {
            if (hscRom.Length != 4096)
                throw new ArgumentException("ROM size not 4096", nameof(hscRom));

            ROM = hscRom;
            Mask = (ushort)(ROM.Length - 1);
        }

        #endregion

        #region Serialization Members

        public HSC7800(DeserializationContext input)
        {
            input.CheckVersion(1);
            ROM = input.ReadExpectedBytes(4096);
            SRAM = input.ReadRAM6116();

            Mask = (ushort)(ROM.Length - 1);
        }

        public void GetObjectData(SerializationContext output)
        {
            output.WriteVersion(1);
            output.Write(ROM);
            output.Write(SRAM);
        }

        #endregion
    }
}