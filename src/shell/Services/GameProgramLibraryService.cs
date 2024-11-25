﻿// © Mike Murphy

using EMU7800.Assets;
using EMU7800.Core;
using EMU7800.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMU7800.Services;

public class GameProgramLibraryService
{
    public static IEnumerable<GameProgramInfoViewItemCollection> GetGameProgramInfoViewItemCollections(IEnumerable<ImportedGameProgramInfo> importedGameProgramInfoSet)
        => ToGameProgramInfoViewItemRecentsCollection(
            ToDict(importedGameProgramInfoSet.Where(igpi => igpi.PersistedStateExists), _ => "Recents")
        ).Concat(
            ToGameProgramInfoViewItemCollections(
                ToDict(importedGameProgramInfoSet, igpi => MachineTypeUtil.To2600or7800WordString(igpi.GameProgramInfo.MachineType)),
                igpi => igpi.GameProgramInfo.Title,
                ToMachineTypeSubTitle)
        ).Concat(
            ToGameProgramInfoViewItemCollections(
                ToDict(importedGameProgramInfoSet, igpi => igpi.GameProgramInfo.Manufacturer),
                igpi => igpi.GameProgramInfo.Title,
                ToManufacturerSubTitle)
        ).Concat(
            ToGameProgramInfoViewItemCollections(
                ToDict(importedGameProgramInfoSet, igpi => igpi.GameProgramInfo.Author),
                igpi => igpi.GameProgramInfo.Year,
                ToAuthorSubTitle)
        ).ToList();

    public static IList<GameProgramInfoViewItem> GetGameProgramInfoViewItems(string romPath)
        => GetGameProgramInfos(romPath)
            .Select(gpi => new GameProgramInfoViewItem(gpi, $"{gpi.Manufacturer} {gpi.Year}", romPath))
            .ToList();

    public static IList<GameProgramInfo> GetGameProgramInfos(string romPath)
    {
        var bytes = DatastoreService.GetRomBytes(romPath);
        var rawBytes = RomBytesService.RemoveA78HeaderIfNecessary(bytes);
        var md5key = RomBytesService.ToMD5Key(rawBytes);
        var romPropertiesCsv = AssetService.GetAssetByLines(Asset.ROMProperties);
        return RomPropertiesService.ToGameProgramInfo(romPropertiesCsv)
            .Where(gpi => gpi.MD5 == md5key)
            .ToList();
    }

    #region Helpers

    static IEnumerable<GameProgramInfoViewItemCollection> ToGameProgramInfoViewItemCollections(
        Dictionary<string, List<ImportedGameProgramInfo>> dict,
        Func<ImportedGameProgramInfo, string> orderByFunc,
        Func<ImportedGameProgramInfo, string> subTitleFunc)
        => dict.OrderBy(kvp => kvp.Key)
            .Select(kvp => new GameProgramInfoViewItemCollection
            {
                Name = kvp.Key,
                GameProgramInfoViewItemSet = kvp.Value
                    .OrderBy(orderByFunc)
                    .Select(igpi => ToGameProgramInfoViewItem(igpi, subTitleFunc))
                    .ToList(),
            });

    static IEnumerable<GameProgramInfoViewItemCollection> ToGameProgramInfoViewItemRecentsCollection(
        Dictionary<string, List<ImportedGameProgramInfo>> dict)
        => dict.Select(kvp => new GameProgramInfoViewItemCollection
        {
            Name = kvp.Key,
            GameProgramInfoViewItemSet = kvp.Value
                .OrderByDescending(igpi => igpi.PersistedStateAt)
                .ThenBy(igpi => igpi.GameProgramInfo.Title)
                .Select(igpi => ToGameProgramInfoViewItem(igpi, ToMachineTypeSubTitle))
                .ToList()
        });

    static GameProgramInfoViewItem ToGameProgramInfoViewItem(ImportedGameProgramInfo igpi, Func<ImportedGameProgramInfo, string> subTitleFunc)
        => new(igpi, subTitleFunc(igpi));

    static Dictionary<string, List<ImportedGameProgramInfo>> ToDict(IEnumerable<ImportedGameProgramInfo> importedGameProgramInfoSet, Func<ImportedGameProgramInfo, string> keySelector)
        => importedGameProgramInfoSet.GroupBy(keySelector)
            .Where(g => !string.IsNullOrWhiteSpace(g.Key))
            .ToDictionary(g => g.Key, g => g.ToList());

    static string ToAuthorSubTitle(ImportedGameProgramInfo igpi)
        => ToCommaDelimitedString(ToAuthorSubTitleWords(igpi));

    static string ToManufacturerSubTitle(ImportedGameProgramInfo igpi)
        => ToCommaDelimitedString(ToManufacturerSubTitleWords(igpi));

    static string ToMachineTypeSubTitle(ImportedGameProgramInfo igpi)
        => ToCommaDelimitedString(ToMachineTypeSubTitleWords(igpi));

    static List<string> ToAuthorSubTitleWords(ImportedGameProgramInfo igpi)
        => [igpi.GameProgramInfo.Manufacturer,
            ControllerUtil.ToControllerWordString(igpi.GameProgramInfo.LController, AreControllersSame(igpi)),
            AreControllersSame(igpi)
                ? string.Empty
                : ControllerUtil.ToControllerWordString(igpi.GameProgramInfo.RController),
            MachineTypeUtil.ToMachineTypeWordString(igpi.GameProgramInfo.MachineType),
            igpi.GameProgramInfo.Year];

    static List<string> ToManufacturerSubTitleWords(ImportedGameProgramInfo igpi)
        => [igpi.GameProgramInfo.Author,
            ControllerUtil.ToControllerWordString(igpi.GameProgramInfo.LController, AreControllersSame(igpi)),
            AreControllersSame(igpi)
                ? string.Empty
                : ControllerUtil.ToControllerWordString(igpi.GameProgramInfo.RController),
            MachineTypeUtil.ToMachineTypeWordString(igpi.GameProgramInfo.MachineType),
            igpi.GameProgramInfo.Year];

    static List<string> ToMachineTypeSubTitleWords(ImportedGameProgramInfo igpi)
        => [igpi.GameProgramInfo.Manufacturer,
            ControllerUtil.ToControllerWordString(igpi.GameProgramInfo.LController, AreControllersSame(igpi)),
            AreControllersSame(igpi)
                ? string.Empty
                : ControllerUtil.ToControllerWordString(igpi.GameProgramInfo.RController),
            MachineTypeUtil.ToTvTypeWordString(igpi.GameProgramInfo.MachineType),
            igpi.GameProgramInfo.Year];

    static string ToCommaDelimitedString(IEnumerable<string> list)
        => string.Join(", ", list.Where(s => !string.IsNullOrWhiteSpace(s)));

    static bool AreControllersSame(ImportedGameProgramInfo igpi)
        => igpi.GameProgramInfo.LController == igpi.GameProgramInfo.RController;

    #endregion
}