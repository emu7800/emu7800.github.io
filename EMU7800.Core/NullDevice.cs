/*
 * NullDevice.cs
 *
 * Default memory mappable device.
 *
 * Copyright © 2003, 2004, 2020 Mike Murphy
 *
 */
using System;

namespace EMU7800.Core
{
    public sealed class NullDevice : IDevice
    {
        public static readonly IDevice Default = new NullDevice();

        #region IDevice Members

        public void Reset()
        {
        }

        public byte this[ushort addr]
        {
            get => 0;
            set { }
        }

        #endregion

        public override string ToString()
            => "NullDevice";

        #region Constructors

        public NullDevice()
        {
        }

        #endregion
    }
}