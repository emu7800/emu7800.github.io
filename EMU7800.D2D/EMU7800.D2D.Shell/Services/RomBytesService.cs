// © Mike Murphy

using System;
using System.Text;
using EMU7800.Core;
using EMU7800.Services.Dto;

namespace EMU7800.Services
{
    public class RomBytesService
    {
        public const string
            SPECIALBINARY_BIOS78_NTSC_MD5           = "0763f1ffb006ddbe32e52d497ee848ae",
            SPECIALBINARY_BIOS78_NTSC_ALTERNATE_MD5 = "b32526ea179dc9ab9b2e5f8a2662b298",
            SPECIALBINARY_BIOS78_PAL_MD5            = "397bb566584be7b9764e7a68974c4263",
            SPECIALBINARY_HSC78_MD5                 = "c8a73288ab97226c52602204ab894286";

        const int A78FILE_HEADER_SIZE = 0x80;

        #region Fields

        readonly Md5HashService _md5HashService = new Md5HashService();
        readonly byte[] _atari7800Tag, _actualCartDataStartsHereTag;

        #endregion

        public bool IsA78Format(byte[] bytes)
        {
            if (bytes == null || bytes.Length < A78FILE_HEADER_SIZE)
                return false;

            var offset = 0x01;
            for (var i = 0; i < _atari7800Tag.Length; i++)
                if (bytes[offset + i] != _atari7800Tag[i])
                    return false;
            offset = 0x64;
            for (var i = 0; i < _actualCartDataStartsHereTag.Length; i++)
                if (bytes[offset + i] != _actualCartDataStartsHereTag[i])
                    return true;  // Used to return false when this tag did not match, however, it does not seem strictly required for the a78 format

            return true;
        }

        public GameProgramInfo ToGameProgramInfoFromA78Format(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 0x40)
                bytes = new byte[0x40];

            var title = Encoding.UTF8.GetString(bytes, 0x11, 0x20).Trim('\0');
            var cartSize = (bytes[0x31] << 24) | (bytes[0x32] << 16) | (bytes[0x33] << 8) | bytes[0x34];
            var cartType1 = bytes[0x35];
            var cartType2 = bytes[0x36];
            var usesPokey = (cartType2 & 1) == 1;
            var lcontroller = bytes[0x37];
            var rcontroller = bytes[0x38];
            var region = bytes[0x39];

            var cartType = To78CartType(cartSize, usesPokey, cartType1, cartType2);

            return new GameProgramInfo
            {
                Title       = title,
                MachineType = (region == 0) ? MachineType.A7800NTSC : MachineType.A7800PAL,
                CartType    = cartType,
                LController = (lcontroller == 1) ? Controller.ProLineJoystick : Controller.Lightgun,
                RController = (rcontroller == 1) ? Controller.ProLineJoystick : Controller.Lightgun,
            };
        }

        public byte[] RemoveA78HeaderIfNecessary(byte[] bytes)
        {
            if (!IsA78Format(bytes))
                return bytes;

            var romBytes = new byte[bytes.Length - A78FILE_HEADER_SIZE];
            Buffer.BlockCopy(bytes, A78FILE_HEADER_SIZE, romBytes, 0, romBytes.Length);
            return romBytes;
        }

        public string ToMD5Key(byte[] bytes)
        {
            var rawBytes = RemoveA78HeaderIfNecessary(bytes);
            var stringifiedHash = _md5HashService.ComputeHash(rawBytes);
            return stringifiedHash;
        }

        public SpecialBinaryType ToSpecialBinaryType(string md5key)
        {
            if (string.IsNullOrWhiteSpace(md5key))
                return SpecialBinaryType.None;
            if (md5key.Equals(SPECIALBINARY_BIOS78_NTSC_MD5, StringComparison.OrdinalIgnoreCase))
                return SpecialBinaryType.Bios7800Ntsc;
            if (md5key.Equals(SPECIALBINARY_BIOS78_NTSC_ALTERNATE_MD5, StringComparison.OrdinalIgnoreCase))
                return SpecialBinaryType.Bios7800NtscAlternate;
            if (md5key.Equals(SPECIALBINARY_BIOS78_PAL_MD5, StringComparison.OrdinalIgnoreCase))
                return SpecialBinaryType.Bios7800Pal;
            if (md5key.Equals(SPECIALBINARY_HSC78_MD5, StringComparison.OrdinalIgnoreCase))
                return SpecialBinaryType.Hsc7800;
            return SpecialBinaryType.None;
        }

        public CartType InferCartTypeFromSize(MachineType machineType, int romByteCount)
        {
            switch (machineType)
            {
                case MachineType.A2600NTSC:
                case MachineType.A2600PAL:
                    switch (romByteCount)
                    {
                        case 2048:  return CartType.A2K;
                        case 4096:  return CartType.A4K;
                        case 8192:  return CartType.A8K;
                        case 16384: return CartType.A16K;
                        case 32768: return CartType.A32K;
                    }
                    break;
                case MachineType.A7800NTSC:
                case MachineType.A7800PAL:
                    switch (romByteCount)
                    {
                        case 8192:  return CartType.A7808;
                        case 16384: return CartType.A7816;
                        case 32768: return CartType.A7832;
                        case 49152: return CartType.A7848;
                    }
                    break;
            }
            return CartType.Unknown;
        }

        #region Constructors

        public RomBytesService()
        {
            _atari7800Tag = Encoding.UTF8.GetBytes("ATARI7800");
            _actualCartDataStartsHereTag = Encoding.UTF8.GetBytes("ACTUAL CART DATA STARTS HERE");
        }

        #endregion

        #region Helpers

        static CartType To78CartType(int cartSize, bool usesPokey, byte cartType1, byte cartType2)
        {
            CartType cartType;
            switch (cartType1)
            {
                case 0:
                    if (cartSize > 131072)
                    {
                        cartType = CartType.A78S9;
                        break;
                    }
                    cartType = cartType2 switch
                    {
                        2 or 3             => usesPokey ? CartType.A78SGP : CartType.A78SG,
                        4 or 5 or 6 or 7   => CartType.A78S4R,
                        8 or 9 or 10 or 11 => CartType.A78S4,
                        _                  => To78CartTypeBySize(cartSize, usesPokey),
                    };
                    break;
                case 1:
                    cartType = CartType.A78AB;
                    break;
                case 2:
                    cartType = CartType.A78AC;
                    break;
                default:
                    cartType = To78CartTypeBySize(cartSize, usesPokey);
                    break;
            }
            return cartType;
        }

        static CartType To78CartTypeBySize(int size, bool usesPokey)
        {
            if (size <= 0x2000)
                return CartType.A7808;
            if (size <= 0x4000)
                return CartType.A7816;
            if (size <= 0x8000)
                return usesPokey ? CartType.A7832P : CartType.A7832;
            if (size <= 0xC000)
                return CartType.A7848;
            return CartType.Unknown;
        }

        #endregion
    }
}