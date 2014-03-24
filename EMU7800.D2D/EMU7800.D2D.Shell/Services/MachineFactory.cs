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
        readonly RomPropertiesService _romPropertiesService = new RomPropertiesService();
        readonly RomBytesService _romBytesService = new RomBytesService();

        #endregion

        public ErrorInfo LastErrorInfo { get; private set; }

        public MachineStateInfo Create(ImportedGameProgramInfo importedGameProgramInfo, bool use7800Bios = false, bool use7800Hsc = false)
        {
            if (importedGameProgramInfo == null)
                throw new ArgumentNullException("importedGameProgramInfo");
            if (importedGameProgramInfo.StorageKeySet == null || importedGameProgramInfo.StorageKeySet.Count == 0)
                throw new ArgumentException("importedGameProgramInfo.StorageKeySet");

            ClearLastErrorInfo();

            byte[] romBytes = null;
            foreach (var storageKey in importedGameProgramInfo.StorageKeySet)
            {
                romBytes = _datastoreService.GetRomBytes(storageKey);
                if (romBytes != null)
                    break;
            }
            if (romBytes == null)
            {
                LastErrorInfo = new ErrorInfo(_datastoreService.LastErrorInfo, "MachineService: Unable to load ROM bytes.");
                return null;
            }
            romBytes = _romBytesService.RemoveA78HeaderIfNecessary(romBytes);

            var gameProgramInfo = importedGameProgramInfo.GameProgramInfo;

            if (gameProgramInfo.CartType == CartType.None)
                gameProgramInfo.CartType = _romBytesService.InferCartTypeFromSize(gameProgramInfo.MachineType, romBytes.Length);

            if (gameProgramInfo.MachineType != MachineType.A7800NTSC
            &&  gameProgramInfo.MachineType != MachineType.A7800PAL)
                use7800Bios = use7800Hsc = false;

            var bios7800 = use7800Bios ? GetBios7800(gameProgramInfo) : null;
            var hsc7800 = use7800Hsc ? GetHSC7800() : null;

            Cart cart;
            try
            {
                cart = Cart.Create(romBytes, gameProgramInfo.CartType);
            }
            catch (Emu7800Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "MachineService.CreateMachine: Unable to create Cart.");
                return null;
            }

            MachineBase machine;
            try
            {
                machine = MachineBase.Create(gameProgramInfo.MachineType, cart, bios7800, hsc7800,
                    gameProgramInfo.LController, gameProgramInfo.RController, null);
            }
            catch (Emu7800Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "MachineService.CreateMachine: Unable to create Machine.");
                return null;
            }

            var machineStateInfo = new MachineStateInfo
            {
                FramesPerSecond = machine.FrameHZ,
                CurrentPlayerNo = 1,
                GameProgramInfo = gameProgramInfo,
                Machine         = machine
            };
            return machineStateInfo;
        }

        #region Constructors

        public MachineFactory()
        {
            ClearLastErrorInfo();
        }

        #endregion

        #region Helpers

        Bios7800 GetBios7800(GameProgramInfo gameProgramInfo)
        {
            var query = ToBiosCandidateList(gameProgramInfo);
            var bios = PickFirstBios7800(query);
            return bios;
        }

        IEnumerable<ImportedSpecialBinaryInfo> ToBiosCandidateList(GameProgramInfo gameProgramInfo)
        {
            switch (gameProgramInfo.MachineType)
            {
                case MachineType.A7800NTSC:
                    var specialBinaryInfoSet = GetSpecialBinaryInfoSet();
                    return specialBinaryInfoSet.Where(sbi =>
                        sbi.Type == SpecialBinaryType.Bios7800Ntsc ||
                        sbi.Type == SpecialBinaryType.Bios7800NtscAlternate);
                case MachineType.A7800PAL:
                    specialBinaryInfoSet = GetSpecialBinaryInfoSet();
                    return specialBinaryInfoSet.Where(sbi => sbi.Type == SpecialBinaryType.Bios7800Pal);
            }
            return new ImportedSpecialBinaryInfo[0];
        }

        Bios7800 PickFirstBios7800(IEnumerable<ImportedSpecialBinaryInfo> specialBinaryInfoSet)
        {
            var query = from specialBinaryInfo in specialBinaryInfoSet
                        select _datastoreService.GetRomBytes(specialBinaryInfo.StorageKey) into bytes
                        where bytes != null
                        select new Bios7800(bytes);
            return query.FirstOrDefault();
        }

        HSC7800 GetHSC7800()
        {
            var specialBinaryInfoSet = GetSpecialBinaryInfoSet();
            var query = specialBinaryInfoSet.Where(sbi => sbi.Type == SpecialBinaryType.Hsc7800);
            var hsc = PickFirstHSC7800(query);
            return hsc;
        }

        HSC7800 PickFirstHSC7800(IEnumerable<ImportedSpecialBinaryInfo> specialBinaryInfoSet)
        {
            var query = from specialBinaryInfo in specialBinaryInfoSet
                        select _datastoreService.GetRomBytes(specialBinaryInfo.StorageKey) into bytes
                        where bytes != null
                        select new HSC7800(bytes, new byte[0x800]);
            return query.FirstOrDefault();
        }

        IEnumerable<ImportedSpecialBinaryInfo> GetSpecialBinaryInfoSet()
        {
            var csvFileContent = _datastoreService.GetSpecialBinaryInfoFromImportRepository();
            var specialBinaryInfoSet = _romPropertiesService.ToImportedSpecialBinaryInfo(csvFileContent);
            return specialBinaryInfoSet;
        }

        void ClearLastErrorInfo()
        {
            LastErrorInfo = null;
        }

        #endregion
    }
}