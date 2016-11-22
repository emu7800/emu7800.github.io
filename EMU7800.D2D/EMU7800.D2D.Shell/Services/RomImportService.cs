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
        readonly RomPropertiesService _romPropertiesService = new RomPropertiesService();

        #endregion

        public ErrorInfo LastErrorInfo { get; private set; }

        public bool CancelRequested { get; set; }
        public bool DirectoryScanCompleted { get; set; }

        public int FilesExamined { get; private set; }
        public int FilesRecognized { get; private set; }

        public void Import()
        {
            ClearLastErrorInfo();

            var pathSet = _datastoreService.QueryLocalMyDocumentsForRomCandidates().Take(32768);
            ImportWithDefaults(pathSet);
        }

        public void ImportDefaultsIfNecessary()
        {
            ClearLastErrorInfo();

            if (_datastoreService.GetSpecialBinaryInfoFromImportRepository().Any()
                && _datastoreService.GetGameProgramInfoFromImportRepository().Any())
                return;

            ImportDefaults();
        }

        public void ImportDefaults()
        {
            ClearLastErrorInfo();

            ImportWithDefaults(new string[0]);
        }

        public void ImportWithDefaults(IEnumerable<string> pathSet)
        {
            if (pathSet == null)
                throw new ArgumentNullException("pathSet");

            ClearLastErrorInfo();

            pathSet = pathSet.Take(32768)
                .Concat(_datastoreService.QueryProgramFolderForRomCandidates());

            Import(pathSet);
        }

        void Import(IEnumerable<string> pathSet)
        {
            DirectoryScanCompleted = false;
            CancelRequested = false;
            FilesExamined = 0;
            FilesRecognized = 0;

            var csvFileContent = _datastoreService.GetGameProgramInfoFromReferenceRepository();
            var gameProgramInfoSet = _romPropertiesService.ToGameProgramInfo(csvFileContent);
            var gameProgramInfoMd5Dict = _romPropertiesService.ToMD5Dict(gameProgramInfoSet);
            var importedGameProgramInfoMd5Dict = new Dictionary<string, ImportedGameProgramInfo>();
            var importedSpecialBinaryInfoSet = new List<ImportedSpecialBinaryInfo>();

            DirectoryScanCompleted = true;

            foreach (var path in pathSet)
            {
                if (CancelRequested)
                    break;

                FilesExamined++;

                var bytes = _datastoreService.GetRomBytes(path);
                if (bytes == null)
                    continue;

                var md5key = _romBytesService.ToMD5Key(bytes);

                IList<GameProgramInfo> gpiList;
                if (!gameProgramInfoMd5Dict.TryGetValue(md5key, out gpiList))
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
                    ImportedGameProgramInfo igpi;
                    if (!importedGameProgramInfoMd5Dict.TryGetValue(md5key, out igpi))
                    {
                        igpi = new ImportedGameProgramInfo { GameProgramInfo = gpi };
                        importedGameProgramInfoMd5Dict.Add(md5key, igpi);
                    }
                    igpi.StorageKeySet.Add(path);
                }
            }

            if (CancelRequested)
                return;

            var importedGameProgramInfo = importedGameProgramInfoMd5Dict.Values
                .Where(igpi => igpi.StorageKeySet.Count > 0)
                    .OrderBy(igpi => igpi.GameProgramInfo.Title);

            csvFileContent = _romPropertiesService.ToCsvFileContent(importedGameProgramInfo);
            _datastoreService.SetGameProgramInfoToImportRepository(csvFileContent);

            if (_datastoreService.LastErrorInfo != null)
            {
                LastErrorInfo = new ErrorInfo(_datastoreService.LastErrorInfo, "RomImportService.Import: Unable to save ROM import data.");
                CancelRequested = true;
                return;
            }

            csvFileContent = _romPropertiesService.ToCsvFileContent(importedSpecialBinaryInfoSet);
            _datastoreService.SetSpecialBinaryInfoToImportRepository(csvFileContent);

            if (_datastoreService.LastErrorInfo != null)
            {
                LastErrorInfo = new ErrorInfo(_datastoreService.LastErrorInfo, "RomImportService.Import: Unable to save ROM import data (special binaries.)");
                CancelRequested = true;
            }
        }

        #region Constructors

        public RomImportService()
        {
            ClearLastErrorInfo();
        }

        #endregion

        #region Helpers

        void ClearLastErrorInfo()
        {
            LastErrorInfo = null;
        }

        #endregion
    }
}
