// © Mike Murphy

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using EMU7800.Services;
using EMU7800.Services.Extensions;

namespace EMU7800.D2D.Shell
{
    public partial class FindRomsPage2
    {
        const string ImportedRomsDirName = "ImportedRoms";

        FileOpenPicker _fp;

        enum ImportStates { NotStarted, Started, Resumable, Resumed }
        ImportStates _importState = ImportStates.NotStarted;

        public override void OnNavigatingHere()
        {
            switch (_importState)
            {
                case ImportStates.NotStarted:
                    _importState = ImportStates.Started;
                    StartImport2();
                    break;
                case ImportStates.Resumable:
                    _importState = ImportStates.Resumed;
                    ResumeImport();
                    break;
            }
        }

        public override void OnNavigatingAway()
        {
            switch (_importState)
            {
                case ImportStates.Started:
                    _importState = ImportStates.Resumable;
                    break;
            }
        }

        void StartImport()
        {
            _importState = ImportStates.NotStarted;
        }

        void StartImport2()
        {
            _fp = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
            _fp.FileTypeFilter.Add(".bin");
            _fp.FileTypeFilter.Add(".a26");
            _fp.FileTypeFilter.Add(".a78");
            _fp.ViewMode = PickerViewMode.List;
            _fp.CommitButtonText = "Import";
            _fp.PickMultipleFilesAndContinue();
        }

        async void ResumeImport()
        {
            var files = (_fp == null ? Enumerable.Empty<IStorageFile>() : _fp.ContinuationData.Values.Cast<IStorageFile>())
                .ToArray();

            _fp = null;

            if (!files.Any())
            {
                _labelStep.Text = "Canceled.";
                _buttonOk.IsVisible = true;
                _buttonCancel.IsVisible = false;
                return;
            }

            var csvFileContent = await Task.Run(() => new DatastoreService().GetGameProgramInfoFromReferenceRepository());
            var romBytesService = new RomBytesService();
            var romPropertiesService = new RomPropertiesService();
            var gameProgramInfoSet = romPropertiesService.ToGameProgramInfo(csvFileContent);
            var gameProgramInfoMd5Dict = romPropertiesService.ToMD5Dict(gameProgramInfoSet);

            var targetFolder = await GetOrCreateImportedRomLocalFolderAsync();

            foreach (var file in files)
            {
                var bytes = await file.GetBytesAsync();
                if (bytes == null)
                    continue;

                var md5Key = romBytesService.ToMD5Key(bytes);
                if (!gameProgramInfoMd5Dict.ContainsKey(md5Key))
                    continue;

                var desiredNewName = md5Key + "_" + file.Name;
                await ImportFileAsync(targetFolder, file, desiredNewName);
            }

            var pathSet = await QueryForRomCandidatesAsync(targetFolder);
            _romImportService.ImportWithDefaults(pathSet);

            if (_romImportService.CancelRequested)
            {
                _labelStep.Text = _romImportService.LastErrorInfo != null ? "Canceled via internal error." : "Canceled.";
            }
            else
            {
                _labelStep.Text = "Completed.";
            }

            _buttonOk.IsVisible = true;
            _buttonCancel.IsVisible = false;
        }

        static async Task<StorageFolder> GetOrCreateImportedRomLocalFolderAsync()
        {
            StorageFolder folder = null;
            try
            {
                folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(ImportedRomsDirName);
            }
            catch (FileNotFoundException)
            {
            }

            if (folder != null)
                return folder;

            try
            {
                folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(ImportedRomsDirName);
            }
            catch (Exception)
            {
                return null;
            }

            return folder;
        }

        static async Task<bool> ImportFileAsync(IStorageFolder targetFolder, IStorageFile sourceFile, string desiredNewName)
        {
            try
            {
                await sourceFile.CopyAsync(targetFolder, desiredNewName);
            }
            catch
            {
                return false;
            }
            return true;
        }

        static async Task<IEnumerable<string>> QueryForRomCandidatesAsync(IStorageFolder folder)
        {
            var files = await folder.GetFilesAsync();

            var filterExtList = new[] { ".bin", ".a26", ".a78", ".zip" };

            var list = files
                .Where(IsPathPresent)
                .Where(file => !file.Name.StartsWith("_"))
                .Where(file => filterExtList.Any(ext => file.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .Select(file => file.Path)
                .ToArray();

            return list;
        }

        static bool IsPathPresent(IStorageItem file)
        {
            return file != null && IsPathPresent(file.Path);
        }

        static bool IsPathPresent(string path)
        {
            return !string.IsNullOrEmpty(path);
        }
    }
}
