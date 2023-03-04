// © Mike Murphy

using EMU7800.Core;
using EMU7800.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMU7800.Services
{
    public class MachineFactory
    {
        public static MachineStateInfo Create(ImportedGameProgramInfo importedGameProgramInfo)
        {
            if (importedGameProgramInfo.StorageKeySet.Count == 0)
                throw new ArgumentException("importedGameProgramInfo.StorageKeySet", nameof(importedGameProgramInfo));

            var romBytes = importedGameProgramInfo.StorageKeySet
                .Select(sk => DatastoreService.GetRomBytes(sk))
                .FirstOrDefault(b => b.Length > 0) ?? Array.Empty<byte>();

            if (romBytes.Length == 0)
            {
                Error("MachineFactory.Create: No ROM bytes");
                return MachineStateInfo.Default;
            }

            romBytes = RomBytesService.RemoveA78HeaderIfNecessary(romBytes);

            var gameProgramInfo = importedGameProgramInfo.GameProgramInfo;

            if (gameProgramInfo.CartType == CartType.Unknown)
            {
                var inferredCartType = RomBytesService.InferCartTypeFromSize(gameProgramInfo.MachineType, romBytes.Length);
                if (inferredCartType != gameProgramInfo.CartType)
                {
                    gameProgramInfo = gameProgramInfo with { CartType = inferredCartType };
                }
            }

            Cart cart;
            try
            {
                cart = Cart.Create(romBytes, gameProgramInfo.CartType);
            }
            catch (Emu7800Exception ex)
            {
                Error("MachineFactory.Create: Unable to create Cart: " + ex.Message);
                return MachineStateInfo.Default;
            }

            var bios7800 = Bios7800.Default;

            if (MachineTypeUtil.Is7800bios(gameProgramInfo.MachineType))
            {
                bios7800 = GetBios7800(gameProgramInfo);
                Info("7800 BIOS Installed");
            }

            if (MachineTypeUtil.Is7800hsc(gameProgramInfo.MachineType))
            {
                cart = GetHSC7800Cart(cart);
                var suffix = cart is HSC7800 ? "installed" : "not installed because ROM not found";
                Info("7800 High Score Cartridge " + suffix);
            }

            MachineBase machine;
            try
            {
                machine = MachineBase.Create(gameProgramInfo.MachineType, cart, bios7800,
                    gameProgramInfo.LController, gameProgramInfo.RController, NullLogger.Default);
            }
            catch (Emu7800Exception ex)
            {
                Error("MachineFactory.Create: Unable to create Machine: " + ex.Message);
                return MachineStateInfo.Default;
            }

            return new()
            {
                FramesPerSecond = machine.FrameHZ,
                CurrentPlayerNo = 1,
                GameProgramInfo = gameProgramInfo,
                Machine         = machine
            };
        }

        #region Helpers

        static Bios7800 GetBios7800(GameProgramInfo gameProgramInfo)
            => PickFirstBios7800(ToBiosCandidateList(gameProgramInfo));

        static IEnumerable<ImportedSpecialBinaryInfo> ToBiosCandidateList(GameProgramInfo gameProgramInfo)
            => DatastoreService.ImportedSpecialBinaryInfo
                .Where(sbi => MachineTypeUtil.Is7800bios(gameProgramInfo.MachineType) && MachineTypeUtil.IsNTSC(gameProgramInfo.MachineType)
                                && (sbi.Type == SpecialBinaryType.Bios7800Ntsc || sbi.Type == SpecialBinaryType.Bios7800NtscAlternate)
                           || MachineTypeUtil.Is7800bios(gameProgramInfo.MachineType) && MachineTypeUtil.IsPAL(gameProgramInfo.MachineType)
                                && sbi.Type == SpecialBinaryType.Bios7800Pal);

        static Bios7800 PickFirstBios7800(IEnumerable<ImportedSpecialBinaryInfo> specialBinaryInfoSet)
            => specialBinaryInfoSet
                .Select(sbi => DatastoreService.GetRomBytes(sbi.StorageKey))
                .Where(b => b.Length == 4096 || b.Length == 16384)
                .Take(1)
                .Select(b => new Bios7800(b))
                .FirstOrDefault() ?? Bios7800.Default;

        static Cart GetHSC7800Cart(Cart cart)
            => PickFirstHSC7800Cart(DatastoreService.ImportedSpecialBinaryInfo
                .Where(sbi => sbi.Type == SpecialBinaryType.Hsc7800), cart);

        static Cart PickFirstHSC7800Cart(IEnumerable<ImportedSpecialBinaryInfo> specialBinaryInfoSet, Cart cart)
            => specialBinaryInfoSet
                .Select(sbi => DatastoreService.GetRomBytes(sbi.StorageKey))
                .Where(b => b.Length > 0)
                .Select(b => new HSC7800(b, cart))
                .FirstOrDefault() ?? cart;

        static void Info(string message)
            => Console.WriteLine(message);

        static void Error(string message)
            => Info("ERROR: " + message);

        #endregion
    }
}