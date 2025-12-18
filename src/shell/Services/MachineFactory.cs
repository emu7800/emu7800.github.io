// © Mike Murphy

using EMU7800.Core;
using EMU7800.Core.Extensions;
using EMU7800.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMU7800.Services;

public sealed class MachineFactory
{
    readonly DatastoreService _datastoreSvc;
    readonly List<ImportedSpecialBinaryInfo> _importedSpecialBinaries;
    readonly ILogger _logger;

    public MachineStateInfo Create(ImportedGameProgramInfo importedGameProgramInfo)
    {
        ArgumentException.ThrowIf(importedGameProgramInfo.StorageKeySet.Count == 0, "StorageKeysSet is unexpectedly empty.", nameof(importedGameProgramInfo));

        var romBytes = importedGameProgramInfo.StorageKeySet
            .Select(_datastoreSvc.GetRomBytes)
            .FirstOrDefault(b => b.Length > 0) ?? [];

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

        var cart = Cart.Create(romBytes, gameProgramInfo.CartType);

        var bios7800 = Bios7800.Default;

        if (MachineTypeUtil.Is7800bios(gameProgramInfo.MachineType))
        {
            bios7800 = GetBios7800(gameProgramInfo, _importedSpecialBinaries);
            Info("7800 BIOS Installed");
        }

        if (MachineTypeUtil.Is7800hsc(gameProgramInfo.MachineType))
        {
            var hscRom = GetHSCRom(_importedSpecialBinaries);
            if (hscRom.Length > 0)
            {
                cart = new HSC7800(hscRom, cart);
            }
            var suffix = cart is HSC7800 ? "installed" : "not installed because ROM not found";
            Info("7800 High Score Cartridge " + suffix);
        }

        if (MachineTypeUtil.Is7800xm(gameProgramInfo.MachineType))
        {
            var hscRom = GetHSCRom(_importedSpecialBinaries);
            if (hscRom.Length > 0)
            {
                cart = new XM7800(hscRom, cart);
            }
            var suffix = cart is XM7800 ? "installed" : "not installed because High Score cartridge ROM not found";
            Info("7800 eXpansion Module " + suffix);
        }

        var machine = MachineBase.Create(gameProgramInfo.MachineType, cart, bios7800, gameProgramInfo.LController, gameProgramInfo.RController, NullLogger.Default);

        return new(machine, gameProgramInfo, false, 1, 0);
    }

    #region Constructors

    public MachineFactory(DatastoreService datastoreSvc, List<ImportedSpecialBinaryInfo> importedSpecialBinaries, ILogger logger)
      => (_datastoreSvc, _importedSpecialBinaries, _logger) = (datastoreSvc, importedSpecialBinaries, logger);

    #endregion

    #region Helpers

    Bios7800 GetBios7800(GameProgramInfo gameProgramInfo, IEnumerable<ImportedSpecialBinaryInfo> importedSpecialBinaries)
        => PickFirstBios7800(ToBiosCandidateList(gameProgramInfo, importedSpecialBinaries));

    static IEnumerable<ImportedSpecialBinaryInfo> ToBiosCandidateList(GameProgramInfo gameProgramInfo, IEnumerable<ImportedSpecialBinaryInfo> importedSpecialBinaries)
        => importedSpecialBinaries
            .Where(sbi => MachineTypeUtil.Is7800bios(gameProgramInfo.MachineType) && MachineTypeUtil.IsNTSC(gameProgramInfo.MachineType)
                            && sbi.Type is SpecialBinaryType.Bios7800Ntsc or SpecialBinaryType.Bios7800NtscAlternate
                       || MachineTypeUtil.Is7800bios(gameProgramInfo.MachineType) && MachineTypeUtil.IsPAL(gameProgramInfo.MachineType)
                            && sbi.Type == SpecialBinaryType.Bios7800Pal);

    Bios7800 PickFirstBios7800(IEnumerable<ImportedSpecialBinaryInfo> specialBinaryInfoSet)
        => specialBinaryInfoSet
            .Select(sbi => _datastoreSvc.GetRomBytes(sbi.StorageKey))
            .Where(b => b.Length is 4096 or 16384)
            .Take(1)
            .Select(b => new Bios7800(b))
            .FirstOrDefault() ?? Bios7800.Default;

    byte[] GetHSCRom(IEnumerable<ImportedSpecialBinaryInfo> importedSpecialBinaries)
        => importedSpecialBinaries
            .Where(sbi => sbi.Type == SpecialBinaryType.Hsc7800)
            .Select(sbi => _datastoreSvc.GetRomBytes(sbi.StorageKey))
            .FirstOrDefault() ?? [];

    void Info(string message)
        => _logger.Log(3, message);

    void Error(string message)
        => _logger.Log(1, message);

    #endregion
}