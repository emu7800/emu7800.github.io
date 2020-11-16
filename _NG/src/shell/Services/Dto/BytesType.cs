using System;

namespace EMU7800.Services.Dto
{
    public class BytesType
    {
        public byte[] Bytes { get; private set; }

        public BytesType() : this(Array.Empty<byte>()) {}
        public BytesType(byte[] bytes) => Bytes = bytes;
    }
}
