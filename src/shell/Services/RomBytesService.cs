// � Mike Murphy

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
            SPECIALBINARY_HSC78_MD5                 = "c8a73288ab97226c52602204ab894286"
            ;

        const int
            A78FILE_HEADER_SIZE = 0x80,
            A78VERSION          = 0,
            A78CARTSIZE         = 0x31,
            A78CARTTYPE         = 0x35,
            A78TVTYPE           = 0x39,
            A78SAVEDATA         = 0x3a,
            A78XMREQ            = 0x3f
            ;

        static readonly MD5 MD5 = MD5.Create();
        static readonly byte[] Atari7800Tag = new byte[] { 0x41, 0x54, 0x41, 0x52, 0x49, 0x37, 0x38, 0x30, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        static readonly uint[] HexStringLookup = CreateHexStringLookupTable();

        #endregion

        public static bool IsA78Format(byte[] bytes)
        {
            if (bytes == null || bytes.Length < A78FILE_HEADER_SIZE)
            {
                return false;
            }

            for (var i = 0; i < Atari7800Tag.Length; i++)
            {
                if (Atari7800Tag[i] != bytes[1 + i])
                {
                    if (Atari7800Tag[i] == 0 && bytes[1 + i] != 0x20)
                    {
                        return false;
                    }
                }
            }

            // Not required for identification:
            // at offset 0x64, ACTUAL CART DATA STARTS HERE

            return true;
        }

        public static GameProgramInfo ToGameProgramInfoFromA78Format(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 0x40)
                bytes = new byte[0x40];

            var version  = bytes[A78VERSION];
            var title    = Encoding.UTF8.GetString(bytes, 0x11, 0x20).Trim('\0');
            var cartSize = (bytes[A78CARTSIZE] << 24) | (bytes[A78CARTSIZE+1] << 16) | (bytes[A78CARTSIZE+2] << 8) | bytes[A78CARTSIZE+3];
            var cartType = To78CartType(cartSize, bytes[A78CARTTYPE], bytes[A78CARTTYPE+1]);

            var tvType   = bytes[A78TVTYPE];
            var saveData = version >= 2 ? bytes[A78SAVEDATA] : (byte)0;
            var xmReq    = bytes[A78XMREQ];

            var machineType
                = tvType == 0 && xmReq    == 1 ? MachineType.A7800NTSCxm :
                  tvType == 1 && xmReq    == 1 ? MachineType.A7800PALxm :
                  tvType == 0 && saveData == 1 ? MachineType.A7800NTSChsc :
                  tvType == 1 && saveData == 1 ? MachineType.A7800PALhsc :
                  tvType == 0                  ? MachineType.A7800NTSC :
                  tvType == 1                  ? MachineType.A7800PAL :
                  MachineType.Unknown;

            return new()
            {
                Title       = title,
                MachineType = machineType,
                CartType    = cartType,
                LController = ToController(bytes[0x37]),
                RController = ToController(bytes[0x38]),
            };

            static Controller ToController(byte b)
                => b switch {
                    0 => Controller.None,
                    1 => Controller.ProLineJoystick,
                    2 => Controller.Lightgun,
                    _ => Controller.None
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
            => MachineTypeUtil.Is2600(machineType) ? romByteCount switch
            {
                 2048 => CartType.A2K,
                 4096 => CartType.A4K,
                 8192 => CartType.A8K,
                16384 => CartType.A16K,
                32768 => CartType.A32K,
                _     => CartType.Unknown,
            }
            : MachineTypeUtil.Is7800(machineType) ? romByteCount switch
            {
                 8192 => CartType.A7808,
                16384 => CartType.A7816,
                32768 => CartType.A7832,
                49152 => CartType.A7848,
                _     => CartType.Unknown,
            }
            : CartType.Unknown;

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
      Right Controller: {ControllerUtil.ToString(gpi.RController)}
      Header Version  : {ToHex(bytes[A78VERSION])}
      Cart Type       : {ToHex(bytes[A78CARTTYPE])} {ToHex(bytes[A78CARTTYPE+1])}
      TV HSC XM       : {ToHex(bytes[A78TVTYPE])} {ToHex(bytes[A78SAVEDATA])} {ToHex(bytes[A78XMREQ])}
");
            }

            var rawBytes = RemoveA78HeaderIfNecessary(bytes);
            var md5 = ToMD5Key(rawBytes);

            printLineFn(@$"
MD5 : {md5}
Size: {rawBytes.Length} {(isA78Format ? "(excluding A78 header)" : string.Empty)}");
            static string ToHex(byte b) => "$" + b.ToString("X2");
        }

        #region Helpers

        static CartType To78CartType(int cartSize, byte cartType1, byte cartType2)
        {
            // Absolute, e.g., F18 Hornet
            if ((cartType1 & 1) != 0)
            {
                return CartType.A78AB;
            }

            // Activision, e.g., Double Dragon & Rampage
            if ((cartType1 & 2) != 0)
            {
                return CartType.A78AC;
            }

            var pokeyAt4k         = (cartType2 & 1) != 0;
            var superCart         = (cartType2 & 2) != 0;
            var superCartRamAt4k  = (cartType2 & 3) != 0;
            var bankOf144kRomAt4k = (cartType2 & 4) != 0;
            //var bank6At4k         = (cartType2 & 5) != 0;
            //var bankedRamAt4k     = (cartType2 & 6) != 0;
            //var pokeyAt0450       = (cartType2 & 7) != 0;
            //var mirrorRamAt4k     = (cartType2 & 8) != 0;

            // TODO: this will need to become more nuanced:
            if (cartSize > 131072)
            {
                return CartType.A78S9;
            }

            if (superCart)
            {
                return pokeyAt4k ? CartType.A78SGP : CartType.A78SG;
            }

            if (superCartRamAt4k)
            {
                return CartType.A78S4R;
            }

            if (bankOf144kRomAt4k)
            {
                return CartType.A78S4;
            }

            return To78CartTypeBySize(cartSize, pokeyAt4k);
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