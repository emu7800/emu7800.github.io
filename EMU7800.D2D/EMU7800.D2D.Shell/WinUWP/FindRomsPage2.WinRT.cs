// © Mike Murphy

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using EMU7800.Services;
using EMU7800.Services.Extensions;
using EMU7800.Services.Dto;

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
                files = Array.Empty<IStorageFile>();
            }

            var csvFileContentResult = await Task.Run(() => new DatastoreService().GetGameProgramInfoFromReferenceRepository());
            var romBytesService = new RomBytesService();
            var gameProgramInfoSet = RomPropertiesService.ToGameProgramInfo(csvFileContentResult.Values.Select(st => st.Line));
            var gameProgramInfoMd5Dict = gameProgramInfoSet
                .GroupBy(gpi => gpi.MD5).ToDictionary(g => g.Key, g => g.ToList());

            var targetFolder = await GetOrCreateImportedRomLocalFolderAsync();

            var anyFiles = false;

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

                anyFiles = true;
            }

            if (anyFiles)
            {
                var pathSet = await QueryForRomCandidatesAsync(targetFolder);
                var result = await Task.Run(() => _romImportService.ImportWithDefaults(pathSet));

                if (_romImportService.CancelRequested)
                {
                    _labelStep.Text = result.IsFail ? "Canceled via internal error" : "Canceled";
                }
                else
                {
                    _labelStep.Text = "Completed";
                }
            }
            else
            {
                _labelStep.Text = "Canceled";
            }

            _buttonOk.IsVisible = true;
            _buttonCancel.IsVisible = false;
        }

        static async Task<StorageFolder> GetOrCreateImportedRomLocalFolderAsync()
        {
            try
            {
                return await ApplicationData.Current.LocalFolder.GetFolderAsync(ImportedRomsDirName);
            }
            catch (FileNotFoundException)
            {
                return await ApplicationData.Current.LocalFolder.CreateFolderAsync(ImportedRomsDirName);
            }
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
            return files
                .Where(IsPathPresent)
                .Where(file => !file.Name.StartsWith("_"))
                .Where(file => filterExtList.Any(ext => file.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .Select(file => file.Path)
                .ToList();
        }

        static bool IsPathPresent(IStorageItem file)
            => file != null && IsPathPresent(file.Path);

        static bool IsPathPresent(string path)
            => !string.IsNullOrEmpty(path);
    }
}
