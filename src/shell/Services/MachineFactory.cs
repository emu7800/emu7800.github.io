﻿// © Mike Murphy

using System;
using System.Collections.Generic;
using System.Linq;
using EMU7800.Core;
using EMU7800.Services.Dto;

namespace EMU7800.Services
{
    public class MachineFactory
    {
        public (Result, MachineStateInfo) Create(ImportedGameProgramInfo importedGameProgramInfo, bool use7800Bios = false, bool use7800Hsc = false)
        {
            if (importedGameProgramInfo.StorageKeySet.Count == 0)
                throw new ArgumentException("importedGameProgramInfo.StorageKeySet", nameof(importedGameProgramInfo));

            var romBytes = importedGameProgramInfo.StorageKeySet
                .Select(sk => DatastoreService.GetRomBytes(sk))
                .Where(r => r.Item1.IsOk)
                .Select(r => r.Item2)
                .FirstOrDefault(b => b.Length > 0) ?? Array.Empty<byte>();

            if (romBytes.Length == 0)
            {
                return (Fail("MachineService.Create: Unable to load ROM bytes"), new());
            }

            romBytes = RomBytesService.RemoveA78HeaderIfNecessary(romBytes);

            var gameProgramInfo = importedGameProgramInfo.GameProgramInfo;

            if (gameProgramInfo.CartType == CartType.Unknown)
                gameProgramInfo.CartType = RomBytesService.InferCartTypeFromSize(gameProgramInfo.MachineType, romBytes.Length);

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
                return (Fail("MachineService.Create: Unable to create Cart", ex), new());
            }

            MachineBase machine;
            try
            {
                machine = MachineBase.Create(gameProgramInfo.MachineType, cart, bios7800, hsc7800,
                    gameProgramInfo.LController, gameProgramInfo.RController, NullLogger.Default);
            }
            catch (Emu7800Exception ex)
            {
                return (Fail("MachineService.Create: Unable to create Machine", ex), new());
            }

            return (Ok(), new()
            {
                FramesPerSecond = machine.FrameHZ,
                CurrentPlayerNo = 1,
                GameProgramInfo = gameProgramInfo,
                Machine         = machine
            });
        }

        #region Constructors

        public MachineFactory()
        {
        }

        #endregion

        #region Helpers

        Bios7800 GetBios7800(GameProgramInfo gameProgramInfo)
            => PickFirstBios7800(ToBiosCandidateList(gameProgramInfo));

        IEnumerable<ImportedSpecialBinaryInfo> ToBiosCandidateList(GameProgramInfo gameProgramInfo)
            => GetSpecialBinaryInfoSet()
                .Where(sbi => gameProgramInfo.MachineType == MachineType.A7800NTSC
                                && (sbi.Type == SpecialBinaryType.Bios7800Ntsc || sbi.Type == SpecialBinaryType.Bios7800NtscAlternate)
                           || gameProgramInfo.MachineType == MachineType.A7800PAL
                                && sbi.Type == SpecialBinaryType.Bios7800Pal);

        static Bios7800 PickFirstBios7800(IEnumerable<ImportedSpecialBinaryInfo> specialBinaryInfoSet)
            => specialBinaryInfoSet
                .Select(sbi => DatastoreService.GetRomBytes(sbi.StorageKey))
                .Where(r => r.Item1.IsOk)
                .Select(r => r.Item2)
                .Where(b => b.Length == 4096 || b.Length == 16384)
                .Take(1)
                .Select(b => new Bios7800(b))
                .FirstOrDefault() ?? Bios7800.Default;

        HSC7800 GetHSC7800()
            => PickFirstHSC7800(GetSpecialBinaryInfoSet().Where(sbi => sbi.Type == SpecialBinaryType.Hsc7800));

        static HSC7800 PickFirstHSC7800(IEnumerable<ImportedSpecialBinaryInfo> specialBinaryInfoSet)
            => specialBinaryInfoSet
                .Select(sbi => DatastoreService.GetRomBytes(sbi.StorageKey))
                .Where(r => r.Item1.IsOk)
                .Select(r => r.Item2)
                .Where(b => b.Length > 0)
                .Select(b => new HSC7800(b))
                .FirstOrDefault() ?? HSC7800.Default;

        static IEnumerable<ImportedSpecialBinaryInfo> GetSpecialBinaryInfoSet()
        {
            var (_, lines) = DatastoreService.GetSpecialBinaryInfoFromImportRepository();
            return RomPropertiesService.ToImportedSpecialBinaryInfo(lines);
        }

        static Result Ok()
            => new();

        static Result Fail(string message)
            => new(message);

        static Result Fail(string message, Exception ex)
            => new(message + $": {ex.GetType().Name}: {ex.Message}");

        #endregion
    }
}