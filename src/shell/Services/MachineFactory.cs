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
        public static MachineStateInfo Create(ImportedGameProgramInfo importedGameProgramInfo, bool use7800Bios = false, bool use7800Hsc = false)
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

            if (gameProgramInfo.MachineType != MachineType.A7800NTSC
            &&  gameProgramInfo.MachineType != MachineType.A7800PAL)
                use7800Bios = use7800Hsc = false;

            var bios7800 = use7800Bios ? GetBios7800(gameProgramInfo) : Bios7800.Default;
            var hsc7800 = use7800Hsc ? GetHSC7800() : HSC7800.Default;

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

            MachineBase machine;
            try
            {
                machine = MachineBase.Create(gameProgramInfo.MachineType, cart, bios7800, hsc7800,
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
                .Where(sbi => gameProgramInfo.MachineType == MachineType.A7800NTSC
                                && (sbi.Type == SpecialBinaryType.Bios7800Ntsc || sbi.Type == SpecialBinaryType.Bios7800NtscAlternate)
                           || gameProgramInfo.MachineType == MachineType.A7800PAL
                                && sbi.Type == SpecialBinaryType.Bios7800Pal);

        static Bios7800 PickFirstBios7800(IEnumerable<ImportedSpecialBinaryInfo> specialBinaryInfoSet)
            => specialBinaryInfoSet
                .Select(sbi => DatastoreService.GetRomBytes(sbi.StorageKey))
                .Where(b => b.Length == 4096 || b.Length == 16384)
                .Take(1)
                .Select(b => new Bios7800(b))
                .FirstOrDefault() ?? Bios7800.Default;

        static HSC7800 GetHSC7800()
            => PickFirstHSC7800(DatastoreService.ImportedSpecialBinaryInfo
                .Where(sbi => sbi.Type == SpecialBinaryType.Hsc7800));

        static HSC7800 PickFirstHSC7800(IEnumerable<ImportedSpecialBinaryInfo> specialBinaryInfoSet)
            => specialBinaryInfoSet
                .Select(sbi => DatastoreService.GetRomBytes(sbi.StorageKey))
                .Where(b => b.Length > 0)
                .Select(b => new HSC7800(b))
                .FirstOrDefault() ?? HSC7800.Default;

        static void Error(string message)
            => Console.WriteLine("ERROR: " + message);

        #endregion
    }
}