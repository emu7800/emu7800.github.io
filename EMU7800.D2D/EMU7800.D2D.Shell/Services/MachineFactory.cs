// © Mike Murphy

using System;
using System.Collections.Generic;
using System.Linq;
using EMU7800.Core;
using EMU7800.Services.Dto;

namespace EMU7800.Services
{
    public class MachineFactory
    {
        #region Fields

        readonly DatastoreService _datastoreService = new DatastoreService();
        readonly RomBytesService _romBytesService = new RomBytesService();

        #endregion

        public Result<MachineStateInfo> Create(ImportedGameProgramInfo importedGameProgramInfo, bool use7800Bios = false, bool use7800Hsc = false)
        {
            if (importedGameProgramInfo.StorageKeySet.Count == 0)
                throw new ArgumentException("importedGameProgramInfo.StorageKeySet", nameof(importedGameProgramInfo));

            var romBytes = importedGameProgramInfo.StorageKeySet
                .Select(sk => _datastoreService.GetRomBytes(sk))
                .Where(r => r.IsOk)
                .Select(r => r.Value.Bytes)
                .FirstOrDefault(b => b.Length > 0) ?? Array.Empty<byte>();

            if (romBytes.Length == 0)
            {
                return Fail<MachineStateInfo>("MachineService.Create: Unable to load ROM bytes");
            }

            romBytes = _romBytesService.RemoveA78HeaderIfNecessary(romBytes);

            var gameProgramInfo = importedGameProgramInfo.GameProgramInfo;

            if (gameProgramInfo.CartType == CartType.Unknown)
                gameProgramInfo.CartType = _romBytesService.InferCartTypeFromSize(gameProgramInfo.MachineType, romBytes.Length);

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
                return Fail<MachineStateInfo>("MachineService.Create: Unable to create Cart", ex);
            }

            MachineBase machine;
            try
            {
                machine = MachineBase.Create(gameProgramInfo.MachineType, cart, bios7800, hsc7800,
                    gameProgramInfo.LController, gameProgramInfo.RController, NullLogger.Default);
            }
            catch (Emu7800Exception ex)
            {
                return Fail<MachineStateInfo>("MachineService.Create: Unable to create Machine", ex);
            }

            return Ok(new MachineStateInfo
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

        Bios7800 PickFirstBios7800(IEnumerable<ImportedSpecialBinaryInfo> specialBinaryInfoSet)
            => specialBinaryInfoSet
                .Select(sbi => _datastoreService.GetRomBytes(sbi.StorageKey))
                .Where(r => r.IsOk)
                .Select(r => r.Value.Bytes)
                .Where(b => b.Length == 4096 || b.Length == 16384)
                .Take(1)
                .Select(b => new Bios7800(b))
                .FirstOrDefault() ?? Bios7800.Default;

        HSC7800 GetHSC7800()
            => PickFirstHSC7800(GetSpecialBinaryInfoSet().Where(sbi => sbi.Type == SpecialBinaryType.Hsc7800));

        HSC7800 PickFirstHSC7800(IEnumerable<ImportedSpecialBinaryInfo> specialBinaryInfoSet)
            => specialBinaryInfoSet
                .Select(sbi => _datastoreService.GetRomBytes(sbi.StorageKey))
                .Where(r => r.IsOk)
                .Select(r => r.Value.Bytes)
                .Where(b => b.Length > 0)
                .Select(b => new HSC7800(b))
                .FirstOrDefault() ?? HSC7800.Default;

        IEnumerable<ImportedSpecialBinaryInfo> GetSpecialBinaryInfoSet()
        {
            var results = _datastoreService.GetSpecialBinaryInfoFromImportRepository();
            var lines = results.IsOk ? results.Values.Select(st => st.Line) : Array.Empty<string>();
            return RomPropertiesService.ToImportedSpecialBinaryInfo(lines);
        }

        static Result<T> Ok<T>(T value) where T : class, new()
            => ResultHelper.Ok(value);

        static Result<T> Fail<T>(string message) where T : class, new()
            => ResultHelper.Fail<T>(message);

        static Result<T> Fail<T>(string message, Exception ex) where T : class, new()
            => ResultHelper.Fail<T>(ToResultMessage(message, ex));

        static string ToResultMessage(string message, Exception ex)
            => message + $": Unexpected exception: {ex.GetType().Name}: " + ex.Message;

        #endregion
    }
}