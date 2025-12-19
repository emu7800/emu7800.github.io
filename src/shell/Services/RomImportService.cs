// © Mike Murphy

using EMU7800.Assets;
using EMU7800.Services.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMU7800.Services;

public sealed class RomImportService
{
    readonly DatastoreService _datastoreSvc;

    public async Task<ImportedRoms> ImportAsync(IEnumerable<string>? paths = null)
    {
        await Task.Yield();
        return Import();
    }

    public ImportedRoms Import(IEnumerable<string>? paths = null)
      => ImportInternal(paths ?? _datastoreSvc.QueryForROMs());

    #region Constructors

    public RomImportService(DatastoreService datastore)
      => _datastoreSvc = datastore;

    #endregion

    #region Helpers

    ImportedRoms ImportInternal(IEnumerable<string> paths)
    {
        var filesExamined = 0;
        var filesRecognized = 0;

        var romPropertiesCsv = AssetService.GetAssetByLines(Asset.ROMProperties);
        var gameProgramInfoSet = RomPropertiesService.ToGameProgramInfo(romPropertiesCsv);
        var gameProgramInfoMd5Dict = gameProgramInfoSet.GroupBy(r => r.MD5).ToDictionary(g => g.Key, g => g.ToList());
        var importedGameProgramInfoMd5Dict = new Dictionary<string, ImportedGameProgramInfo>();
        var importedSpecialBinaryInfoSet = new List<ImportedSpecialBinaryInfo>();

        foreach (var path in paths)
        {
            filesExamined++;

            var bytes = _datastoreSvc.GetRomBytes(path);

            if (bytes.Length == 0)
                continue;

            var md5key = RomBytesService.ToMD5Key(bytes);

            if (!gameProgramInfoMd5Dict.TryGetValue(md5key, out var gpiList))
            {
                var specialBinaryType = RomBytesService.ToSpecialBinaryType(md5key);
                if (specialBinaryType != SpecialBinaryType.None)
                {
                    importedSpecialBinaryInfoSet.Add(new(specialBinaryType, path));
                }
                continue;
            }

            filesRecognized++;

            foreach (var gpi in gpiList)
            {
                var md5keyPlus = $"{md5key} {gpi.LController} {gpi.RController}";
                if (!importedGameProgramInfoMd5Dict.TryGetValue(md5keyPlus, out var igpi))
                {
                    igpi = new(gpi)
                    {
                        PersistedStateAt = _datastoreSvc.PersistedMachineAt(gpi)
                    };
                    importedGameProgramInfoMd5Dict.Add(md5keyPlus, igpi);
                }
                igpi.StorageKeySet.Add(path);
            }
        }

        var importedGameProgramInfoSet = importedGameProgramInfoMd5Dict.Values
            .Where(igpi => igpi.StorageKeySet.Count > 0)
            .OrderBy(igpi => igpi.GameProgramInfo.Title)
            .ToList();

        return new(
            importedGameProgramInfoSet,
            importedSpecialBinaryInfoSet,
            filesExamined,
            filesRecognized);
    }

    #endregion
}