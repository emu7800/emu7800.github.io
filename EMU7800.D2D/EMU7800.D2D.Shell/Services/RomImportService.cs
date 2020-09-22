// © Mike Murphy

using System;
using System.Collections.Generic;
using System.Linq;
using EMU7800.Services.Dto;

namespace EMU7800.Services
{
    public class RomImportService
    {
        #region Fields

        readonly DatastoreService _datastoreService = new DatastoreService();
        readonly RomBytesService _romBytesService = new RomBytesService();

        #endregion

        public bool CancelRequested { get; set; }
        public bool DirectoryScanCompleted { get; set; }

        public int FilesExamined { get; private set; }
        public int FilesRecognized { get; private set; }

        public Result Import()
        {
            var result = _datastoreService.QueryLocalMyDocumentsForRomCandidates();
            return ImportWithDefaults(result.Values.Select(st => st.Line).Take(32768));
        }

        public Result ImportDefaultsIfNecessary()
        {
            var result1 = _datastoreService.GetSpecialBinaryInfoFromImportRepository();
            var result2 = _datastoreService.GetGameProgramInfoFromImportRepository();

             if (result1.IsOk && result1.Values.Any()
                && result2.IsOk && result2.Values.Any())
                    return Ok();

            return ImportDefaults();
        }

        public Result ImportDefaults()
            => ImportWithDefaults(Array.Empty<string>());

        public Result ImportWithDefaults(IEnumerable<string> pathSet)
        {
            var result = _datastoreService.QueryProgramFolderForRomCandidates();
            return Import(pathSet.Take(32768).Concat(result.Values.Select(st => st.Line)));
        }

        Result Import(IEnumerable<string> pathSet)
        {
            DirectoryScanCompleted = false;
            CancelRequested = false;
            FilesExamined = 0;
            FilesRecognized = 0;

            var csvFileContentResult = _datastoreService.GetGameProgramInfoFromReferenceRepository();
            var gameProgramInfoSet = RomPropertiesService.ToGameProgramInfo(csvFileContentResult.Values.Select(st => st.Line));
            var gameProgramInfoMd5Dict = gameProgramInfoSet.GroupBy(r => r.MD5).ToDictionary(g => g.Key, g => g.ToList());
            var importedGameProgramInfoMd5Dict = new Dictionary<string, ImportedGameProgramInfo>();
            var importedSpecialBinaryInfoSet = new List<ImportedSpecialBinaryInfo>();

            DirectoryScanCompleted = true;

            foreach (var path in pathSet)
            {
                if (CancelRequested)
                    break;

                FilesExamined++;

                var bytesResult = _datastoreService.GetRomBytes(path);

                if (bytesResult.IsFail || bytesResult.Value.Bytes.Length == 0)
                    continue;

                var md5key = _romBytesService.ToMD5Key(bytesResult.Value.Bytes);

                if (!gameProgramInfoMd5Dict.TryGetValue(md5key, out var gpiList))
                {
                    var specialBinaryType = _romBytesService.ToSpecialBinaryType(md5key);
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
            var result1 = _datastoreService.SetGameProgramInfoToImportRepository(csvFileContent1);

            if (result1.IsFail)
            {
                CancelRequested = true;
                return Fail("RomImportService.Import: Unable to save ROM import data: " + result1.ErrorMessage);
            }

            var csvFileContent2 = RomPropertiesService.ToImportSpecialBinaryCsvFileContent(importedSpecialBinaryInfoSet);
            var result2 = _datastoreService.SetSpecialBinaryInfoToImportRepository(csvFileContent2);

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
            => ResultHelper.Ok();

        static Result Fail(string message)
            => ResultHelper.Fail(message);

        #endregion
    }
}
