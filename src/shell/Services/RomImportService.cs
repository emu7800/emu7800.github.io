// © Mike Murphy

using EMU7800.Assets;
using EMU7800.Services.Dto;
using System.Collections.Generic;
using System.Linq;

namespace EMU7800.Services
{
    public class RomImportService
    {
        public static bool CancelRequested { get; set; }
        public static bool DirectoryScanCompleted { get; set; }

        public static int FilesExamined { get; private set; }
        public static int FilesRecognized { get; private set; }

        public static Result Import()
        {
            var (queryResult, lines) = DatastoreService.QueryROMSFolder();
            return queryResult.IsOk ? Import(lines) : Fail(queryResult.ErrorMessage);
        }

        public static Result ImportDefaultsIfNecessary()
            => DatastoreService.ImportedGameProgramInfo.Any()
                && DatastoreService.ImportedSpecialBinaryInfo.Any() ? Ok() : Import();

        #region Helpers

        static Result Import(IEnumerable<string> paths)
        {
            DirectoryScanCompleted = false;
            CancelRequested = false;
            FilesExamined = 0;
            FilesRecognized = 0;

            var romPropertiesCsv = AssetService.GetAssetByLines(Asset.ROMProperties);
            var gameProgramInfoSet = RomPropertiesService.ToGameProgramInfo(romPropertiesCsv);
            var gameProgramInfoMd5Dict = gameProgramInfoSet.GroupBy(r => r.MD5).ToDictionary(g => g.Key, g => g.ToList());
            var importedGameProgramInfoMd5Dict = new Dictionary<string, ImportedGameProgramInfo>();
            var importedSpecialBinaryInfoSet = new List<ImportedSpecialBinaryInfo>();

            DirectoryScanCompleted = true;

            foreach (var path in paths)
            {
                if (CancelRequested)
                    break;

                FilesExamined++;

                var (getBytesResult, bytes) = DatastoreService.GetRomBytes(path);

                if (getBytesResult.IsFail || bytes.Length == 0)
                    continue;

                var md5key = RomBytesService.ToMD5Key(bytes);

                if (!gameProgramInfoMd5Dict.TryGetValue(md5key, out var gpiList))
                {
                    var specialBinaryType = RomBytesService.ToSpecialBinaryType(md5key);
                    if (specialBinaryType != SpecialBinaryType.None)
                    {
                        var sbi = new ImportedSpecialBinaryInfo { Type = specialBinaryType, StorageKey = path };
                        importedSpecialBinaryInfoSet.Add(sbi);
                    }
                    continue;
                }

                FilesRecognized++;

                foreach (var gpi in gpiList)
                {
                    if (!importedGameProgramInfoMd5Dict.TryGetValue(md5key, out var igpi))
                    {
                        igpi = new ImportedGameProgramInfo { GameProgramInfo = gpi };
                        importedGameProgramInfoMd5Dict.Add(md5key, igpi);
                    }
                    igpi.StorageKeySet.Add(path);
                }
            }

            if (CancelRequested)
                return Fail("RomImportService.Import: Cancel requested");

            DatastoreService.ImportedGameProgramInfo = importedGameProgramInfoMd5Dict.Values
                .Where(igpi => igpi.StorageKeySet.Count > 0)
                    .OrderBy(igpi => igpi.GameProgramInfo.Title);

            DatastoreService.ImportedSpecialBinaryInfo = importedSpecialBinaryInfoSet;

            return Ok();
        }

        static Result Ok()
            => new();

        static Result Fail(string message)
            => new (message);

        #endregion
    }
}
