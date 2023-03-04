/*
 * Cart.cs
 *
 * An abstraction of a game cart.  Attributable to Kevin Horton's Bankswitching
 * document, the Stella source code, and Eckhard Stolberg's 7800 Bankswitching Guide.
 *
 * Copyright © 2003, 2004, 2010, 2011 Mike Murphy
 *
 */
using System;

namespace EMU7800.Core
{
    public abstract class Cart : IDevice
    {
        public static readonly Cart Default = new UnknownCart();

        static int _multicartBankSelector;

        protected MachineBase M { get; set; } = MachineBase.Default;
        protected internal byte[] ROM { get; set; } = Array.Empty<byte>();

        #region IDevice Members

        public virtual void Reset() {}

        public abstract byte this[ushort addr] { get; set; }

        #endregion

        public virtual void Attach(MachineBase m)
            => M = m;

        public virtual bool Map()
            => false;

        public virtual void StartFrame() {}

        public virtual void EndFrame() {}

        protected internal virtual bool RequestSnooping
            => false;

        /// <summary>
        /// Creates an instance of the specified cart.
        /// </summary>
        /// <param name="romBytes"></param>
        /// <param name="cartType"></param>
        /// <exception cref="Emu7800Exception">Specified CartType is unexpected.</exception>
        public static Cart Create(byte[] romBytes, CartType cartType)
        {
            if (cartType == CartType.Unknown)
            {
                switch (romBytes.Length)
                {
                    case 2048:
                        cartType = CartType.A2K;
                        break;
                    case 4096:
                        cartType = CartType.A4K;
                        break;
                    case 8192:
                        cartType = CartType.A8K;
                        break;
                    case 16384:
                        cartType = CartType.A16K;
                        break;
                    case 32768:
                        cartType = CartType.A32K;
                        break;
                }
            }

            return cartType switch
            {
                CartType.A2K     => new CartA2K(romBytes),
                CartType.A4K     => new CartA4K(romBytes),
                CartType.A8K     => new CartA8K(romBytes),
                CartType.A8KR    => new CartA8KR(romBytes),
                CartType.A16K    => new CartA16K(romBytes),
                CartType.A16KR   => new CartA16KR(romBytes),
                CartType.DC8K    => new CartDC8K(romBytes),
                CartType.PB8K    => new CartPB8K(romBytes),
                CartType.TV8K    => new CartTV8K(romBytes),
                CartType.CBS12K  => new CartCBS12K(romBytes),
                CartType.A32K    => new CartA32K(romBytes),
                CartType.A32KR   => new CartA32KR(romBytes),
                CartType.MN16K   => new CartMN16K(romBytes),
                CartType.DPC     => new CartDPC(romBytes),
                CartType.M32N12K => new CartA2K(romBytes, _multicartBankSelector++),
                CartType.A7808   => new Cart7808(romBytes),
                CartType.A7816   => new Cart7816(romBytes),
                CartType.A7832P  => new Cart7832P(romBytes),
                CartType.A7832   => new Cart7832(romBytes),
                CartType.A7848   => new Cart7848(romBytes),
                CartType.A78SGP  => new Cart78SGP(romBytes),
                CartType.A78SG   => new Cart78SG(romBytes, false),
                CartType.A78SGR  => new Cart78SG(romBytes, true),
                CartType.A78S9   => new Cart78S9(romBytes),
                CartType.A78S4   => new Cart78S4(romBytes, false),
                CartType.A78S4R  => new Cart78S4(romBytes, true),
                CartType.A78AB   => new Cart78AB(romBytes),
                CartType.A78AC   => new Cart78AC(romBytes),
                _                => throw new Emu7800Exception("Unexpected CartType: " + cartType),
            };
        }

        protected void LoadRom(byte[] romBytes, int multicartBankSize, int multicartBankNo)
        {
            ROM = new byte[multicartBankSize];
            Buffer.BlockCopy(romBytes, multicartBankSize*multicartBankNo, ROM, 0, multicartBankSize);
        }

        protected void LoadRom(byte[] romBytes, int minSize)
        {
            if (romBytes.Length >= minSize)
            {
                ROM = romBytes;
            }
            else
            {
                ROM = new byte[minSize];
                Buffer.BlockCopy(romBytes, 0, ROM, 0, romBytes.Length);
            }
        }

        protected void LoadRom(byte[] romBytes)
            => LoadRom(romBytes, romBytes.Length);

        protected Cart() {}

        #region Serialization Members

        protected Cart(DeserializationContext input)
            => input.CheckVersion(1);

        public virtual void GetObjectData(SerializationContext output)
            => output.WriteVersion(1);

        #endregion

        class UnknownCart : Cart
        {
            public override byte this[ushort addr]
            {
                get => 0;
                set {}
            }

            public UnknownCart()
                => ROM = Array.Empty<byte>();

            public override string ToString()
                => "EMU7800.Core.UnknownCart";
        }
    }
}