// © Mike Murphy

using EMU7800.Core;
using EMU7800.Services.Dto;
using System;
using System.Security.Cryptography;
using System.Text;

namespace EMU7800.Services
{
    public class RomBytesService
    {
        #region Fields

        const string
            SPECIALBINARY_BIOS78_NTSC_MD5           = "0763f1ffb006ddbe32e52d497ee848ae",
            SPECIALBINARY_BIOS78_NTSC_ALTERNATE_MD5 = "b32526ea179dc9ab9b2e5f8a2662b298",
            SPECIALBINARY_BIOS78_PAL_MD5            = "397bb566584be7b9764e7a68974c4263",
            SPECIALBINARY_HSC78_MD5                 = "c8a73288ab97226c52602204ab894286";

        const int A78FILE_HEADER_SIZE = 0x80;

        static readonly MD5 MD5 = MD5.Create();
        static readonly byte[] Atari7800Tag = Encoding.UTF8.GetBytes("ATARI7800");
        static readonly byte[] ActualCartDataStartsHereTag = Encoding.UTF8.GetBytes("ACTUAL CART DATA STARTS HERE");
        static readonly uint[] HexStringLookup = CreateHexStringLookupTable();

        #endregion

        public static bool IsA78Format(byte[] bytes)
        {
            if (bytes == null || bytes.Length < A78FILE_HEADER_SIZE)
                return false;

            var offset = 0x01;
            for (var i = 0; i < Atari7800Tag.Length; i++)
                if (bytes[offset + i] != Atari7800Tag[i])
                    return false;
            offset = 0x64;
            for (var i = 0; i < ActualCartDataStartsHereTag.Length; i++)
                if (bytes[offset + i] != ActualCartDataStartsHereTag[i])
                    return true;  // Used to return false when this tag did not match, however, it does not seem strictly required for the a78 format

            return true;
        }

        public static GameProgramInfo ToGameProgramInfoFromA78Format(byte[] bytes)
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

            return new()
            {
                Title       = title,
                MachineType = (region == 0) ? MachineType.A7800NTSC : MachineType.A7800PAL,
                CartType    = cartType,
                LController = (lcontroller == 1) ? Controller.ProLineJoystick : Controller.Lightgun,
                RController = (rcontroller == 1) ? Controller.ProLineJoystick : Controller.Lightgun,
            };
        }

        public static byte[] RemoveA78HeaderIfNecessary(byte[] bytes)
        {
            if (!IsA78Format(bytes))
                return bytes;

            var romBytes = new byte[bytes.Length - A78FILE_HEADER_SIZE];
            Buffer.BlockCopy(bytes, A78FILE_HEADER_SIZE, romBytes, 0, romBytes.Length);
            return romBytes;
        }

        public static string ToMD5Key(byte[] bytes)
            => ToHex(MD5.ComputeHash(RemoveA78HeaderIfNecessary(bytes ?? Array.Empty<byte>())));

        public static SpecialBinaryType ToSpecialBinaryType(string md5key)
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

        public static CartType InferCartTypeFromSize(MachineType machineType, int romByteCount)
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

        public static void DumpBin(string path, Action<string> printLineFn)
        {
            printLineFn(@$"
File: {path}");

            byte[] bytes;
            try
            {
                bytes = System.IO.File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                printLineFn($"Unable to read ROM: {ex.GetType().Name}: {ex.Message}");
                return;
            }

            var isA78Format = IsA78Format(bytes);

            if (isA78Format)
            {
                var gpi = ToGameProgramInfoFromA78Format(bytes);
                printLineFn(@$"
A78 : Title           : {gpi.Title}
      MachineType     : {MachineTypeUtil.ToString(gpi.MachineType)}
      CartType        : {CartTypeUtil.ToString(gpi.CartType)} ({CartTypeUtil.ToCartTypeWordString(gpi.CartType)})
      Left Controller : {ControllerUtil.ToString(gpi.LController)}
      Right Controller: {ControllerUtil.ToString(gpi.RController)}");
            }

            var rawBytes = RemoveA78HeaderIfNecessary(bytes);
            var md5 = ToMD5Key(rawBytes);

            printLineFn(@$"
MD5 : {md5}
Size: {rawBytes.Length} {(isA78Format ? "(excluding A78 header)" : string.Empty)}");
        }

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

        static uint[] CreateHexStringLookupTable()
        {
            var result = new uint[0x100];
            for (int i = 0; i < result.Length; i++)
            {
                var s = i.ToString("x2");
                result[i] = s[0] + ((uint)s[1] << 0x10);
            }
            return result;
        }

        static string ToHex(byte[] bytes)
        {
            var result = new char[bytes.Length * 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                var val = HexStringLookup[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 0x10);
            }
            return new string(result, 0, result.Length);
        }

        #endregion
    }
}