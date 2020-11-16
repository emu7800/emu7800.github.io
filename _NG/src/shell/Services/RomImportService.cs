// © Mike Murphy

using System;
using System.Collections.Generic;
using System.Linq;
using EMU7800.Services.Dto;

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
            var (_, lines) = DatastoreService.QueryLocalMyDocumentsForRomCandidates();
            return ImportWithDefaults(lines.Take(32768));
        }

        public static Result ImportDefaultsIfNecessary()
        {
            var (result1, lines1) = DatastoreService.GetSpecialBinaryInfoFromImportRepository();
            var (result2, lines2) = DatastoreService.GetGameProgramInfoFromImportRepository();

             if (result1.IsOk && lines1.Any()
                && result2.IsOk && lines2.Any())
                    return Ok();

            return ImportDefaults();
        }

        public static Result ImportDefaults()
            => ImportWithDefaults(Array.Empty<string>());

        public static Result ImportWithDefaults(IEnumerable<string> pathSet)
        {
            var (_, lines) = DatastoreService.QueryProgramFolderForRomCandidates();
            return Import(pathSet.Take(32768).Concat(lines));
        }

        static Result Import(IEnumerable<string> pathSet)
        {
            DirectoryScanCompleted = false;
            CancelRequested = false;
            FilesExamined = 0;
            FilesRecognized = 0;

            var (_, csvFileContent) = DatastoreService.GetGameProgramInfoFromReferenceRepository();
            var gameProgramInfoSet = RomPropertiesService.ToGameProgramInfo(csvFileContent);
            var gameProgramInfoMd5Dict = gameProgramInfoSet.GroupBy(r => r.MD5).ToDictionary(g => g.Key, g => g.ToList());
            var importedGameProgramInfoMd5Dict = new Dictionary<string, ImportedGameProgramInfo>();
            var importedSpecialBinaryInfoSet = new List<ImportedSpecialBinaryInfo>();

            DirectoryScanCompleted = true;

            foreach (var path in pathSet)
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

            var importedGameProgramInfo = importedGameProgramInfoMd5Dict.Values
                .Where(igpi => igpi.StorageKeySet.Count > 0)
                    .OrderBy(igpi => igpi.GameProgramInfo.Title);

            var csvFileContent1 = RomPropertiesService.ToImportRepositoryCsvFileContent(importedGameProgramInfo);
            var result1 = DatastoreService.SetGameProgramInfoToImportRepository(csvFileContent1);

            if (result1.IsFail)
            {
                CancelRequested = true;
                return Fail("RomImportService.Import: Unable to save ROM import data: " + result1.ErrorMessage);
            }

            var csvFileContent2 = RomPropertiesService.ToImportSpecialBinaryCsvFileContent(importedSpecialBinaryInfoSet);
            var result2 = DatastoreService.SetSpecialBinaryInfoToImportRepository(csvFileContent2);

            if (result2.IsFail)
            {
                CancelRequested = true;
                return Fail("RomImportService.Import: Unable to save ROM import data (special binaries): " + result2.ErrorMessage);
            }

            return Ok();
        }

        #region Constructors

        public RomImportService()
        {
        }

        #endregion

        #region Helpers

        static Result Ok()
            => new();

        static Result Fail(string message)
            => new (message);

        #endregion
    }
}
