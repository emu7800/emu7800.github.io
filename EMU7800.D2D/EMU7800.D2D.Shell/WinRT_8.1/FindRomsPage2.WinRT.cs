// © Mike Murphy

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using EMU7800.Services;
using EMU7800.Services.Extensions;

namespace EMU7800.D2D.Shell
{
    public partial class FindRomsPage2
    {
        const string ImportedRomsDirName = "ImportedRoms";

        async void StartImport()
        {
            var fp = new FileOpenPicker {SuggestedStartLocation = PickerLocationId.DocumentsLibrary};
            fp.FileTypeFilter.Add(".bin");
            fp.FileTypeFilter.Add(".a26");
            fp.FileTypeFilter.Add(".a78");
            fp.ViewMode = PickerViewMode.List;
            fp.CommitButtonText = "Import";

            IReadOnlyList<IStorageFile> files;
            try
            {
                files = await fp.PickMultipleFilesAsync();
            }
            catch (COMException)
            {
                _labelStep.Text = "Canceled. Unable to launch picker in snapped view.";
                _buttonOk.IsVisible = true;
                _buttonCancel.IsVisible = false;
                return;
            }

            if (files.Count == 0)
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
