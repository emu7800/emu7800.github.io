// © Mike Murphy

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using EMU7800.Core;
using EMU7800.Services.Dto;
using EMU7800.Services.Extensions;

namespace EMU7800.Services
{
    public class DatastoreService
    {
        public ErrorInfo LastErrorInfo { get; private set; }

        #region ROM Files

        public IEnumerable<string> QueryLocalMyDocumentsForRomCandidates()
        {
            // not needed in WinRT version
            ClearLastErrorInfo();
            return new string[0];
        }

        public IEnumerable<string> QueryProgramFolderForRomCandidates()
        {
            ClearLastErrorInfo();

            try
            {
                var folder = Package.Current.InstalledLocation.GetFolder("Assets");
                var folder2 = ApplicationData.Current.LocalFolder; // look for ROMs previously imported
                return QueryForRomCandidates(folder).Concat(QueryForRomCandidates(folder2));
            }
            catch (Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "QueryProgramFolderForRomCandidates: Unexpected exception.");
                return new string[0];
            }
        }

        public byte[] GetRomBytes(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            ClearLastErrorInfo();

            try
            {
                return GetRomBytesImpl(path);
            }
            catch (Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "GetRomBytes: Unexpected exception.");
                return null;
            }
        }

        static byte[] GetRomBytesImpl(string path)
        {
            byte[] bytes;
            if (path.Contains("|"))
            {
                var splitPath = path.Split('|');
                if (splitPath.Length != 2 || !splitPath[0].EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    return null;
                var zipFile = GetStorageFileFromPath(splitPath[0]);
                bytes = GetZipBytes(zipFile, splitPath[1]);
            }
            else
            {
                var file = GetStorageFileFromPath(path);
                bytes = file.GetBytes();
            }
            return bytes;
        }

        #endregion

        #region Machine Persistence

        ISet<string> _cachedPersistedDir;

        public bool PersistedMachineExists(GameProgramInfo gameProgramInfo)
        {
            ClearLastErrorInfo();

            if (gameProgramInfo == null)
                return false;

            if (_cachedPersistedDir == null)
                _cachedPersistedDir = GetFilesFromPersistedGameProgramsImpl();
            var name = ToPersistedStateStorageName(gameProgramInfo);
            var exists = _cachedPersistedDir.Contains(name);
            return exists;
        }

        public void PersistMachine(MachineStateInfo machineStateInfo)
        {
            if (machineStateInfo == null)
                throw new ArgumentNullException("machineStateInfo");
            if (machineStateInfo.Machine == null)
                throw new ArgumentException("machineStateInfo.Machine is unspecified");
            if (machineStateInfo.GameProgramInfo == null)
                throw new ArgumentException("machineStateInfo.GameProgramInfo is unspecified");

            ClearLastErrorInfo();

            var name = ToPersistedStateStorageName(machineStateInfo.GameProgramInfo);
            var path = ToPersistedStateStoragePath(name);

            try
            {
                var file = ApplicationData.Current.LocalFolder.CreateFile(path);
                PersistMachineImpl(file, machineStateInfo);
            }
            catch (Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "PersistMachine: Unexpected exception.");
            }

            if (_cachedPersistedDir != null)
                _cachedPersistedDir.Add(name);
        }

        public void PersistScreenshot(MachineStateInfo machineStateInfo, byte[] data)
        {
            if (machineStateInfo == null)
                throw new ArgumentNullException("machineStateInfo");
            if (machineStateInfo.Machine == null)
                throw new ArgumentException("machineStateInfo.Machine is unspecified");
            if (machineStateInfo.GameProgramInfo == null)
                throw new ArgumentException("machineStateInfo.GameProgramInfo is unspecified");
            if (data == null)
                throw new ArgumentNullException("data");

            ClearLastErrorInfo();

            var name = ToScreenshotStorageName(machineStateInfo.GameProgramInfo);
            var path = ToPersistedStateStoragePath(name);

            try
            {
                var file = ApplicationData.Current.LocalFolder.CreateFile(path);
                PersistScreenshotImpl(file, data);
            }
            catch (Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "PersistScreenshot: Unexpected exception.");
            }
        }

        public MachineStateInfo RestoreMachine(GameProgramInfo gameProgramInfo)
        {
            if (gameProgramInfo == null)
                throw new ArgumentNullException("gameProgramInfo");

            ClearLastErrorInfo();

            var name = ToPersistedStateStorageName(gameProgramInfo);
            var path = ToPersistedStateStoragePath(name);

            try
            {
                var file = ApplicationData.Current.LocalFolder.GetFile(path);
                var machineStateInfo = RestoreMachineImpl(file, gameProgramInfo);
                return machineStateInfo;
            }
            catch (Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "RestoreMachine: Unexpected exception.");
                return null;
            }
        }

        static void PersistMachineImpl(IStorageFile file, MachineStateInfo machineStateInfo)
        {
            if (file == null)
                return;

            using (var stream = file.OpenStreamForWrite())
            using (var bw = new BinaryWriter(stream))
            {
                bw.Write(2); // version
                bw.Write(machineStateInfo.FramesPerSecond);
                bw.Write(machineStateInfo.SoundOff);
                bw.Write(machineStateInfo.CurrentPlayerNo);
                bw.Write(machineStateInfo.InterpolationMode);
                machineStateInfo.Machine.Serialize(bw);
                bw.Flush();
            }
        }

        static void PersistScreenshotImpl(IStorageFile file, byte[] data)
        {
            if (file == null)
                return;

            const int width = 320, height = 230;
            using (var stream = file.Open(FileAccessMode.ReadWrite))
            {
                var encoder = CreateBitmapEncoder(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, width, height, 96f, 96f, data);
                encoder.Flush();
            }
        }

        static MachineStateInfo RestoreMachineImpl(IStorageFile file, GameProgramInfo gameProgramInfo)
        {
            if (file == null)
                return null;

            using (var stream = file.OpenStreamForRead())
            using (var br = new BinaryReader(stream))
            {
                var version = br.ReadInt32();
                var machineStateInfo = new MachineStateInfo
                {
                    FramesPerSecond   = br.ReadInt32(),
                    SoundOff          = br.ReadBoolean(),
                    CurrentPlayerNo   = br.ReadInt32(),
                    InterpolationMode = (version > 1) ? br.ReadInt32() : 0,
                    Machine           = MachineBase.Deserialize(br),
                    GameProgramInfo   = gameProgramInfo
                };

                if (machineStateInfo.FramesPerSecond == 0)
                    machineStateInfo.FramesPerSecond = machineStateInfo.Machine.FrameHZ;

                return machineStateInfo;
            }
        }

        #endregion

        #region CSV File IO

        public IEnumerable<string> GetGameProgramInfoFromReferenceRepository()
        {
            ClearLastErrorInfo();

            try
            {
                var file = GetAssetFile(RomPropertiesName);
                var lines = file.ReadUtf8Lines();
                return lines.ToArray();
            }
            catch (Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "GetGameProgramInfoFromReferenceRepository: Unexpected exception.");
                return new string[0];
            }
        }

        public IEnumerable<string> GetGameProgramInfoFromImportRepository()
        {
            ClearLastErrorInfo();

            try
            {
                var file = ApplicationData.Current.LocalFolder.GetFile(ToRomImportsName());
                var lines = file.ReadUtf8Lines();
                return lines.ToArray();
            }
            catch (Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "GetGameProgramInfoFromImportRepository: Unexpected exception.");
                return new string[0];
            }
        }

        public IEnumerable<string> GetSpecialBinaryInfoFromImportRepository()
        {
            ClearLastErrorInfo();

            try
            {
                var file = ApplicationData.Current.LocalFolder.GetFile(ToRomImportsSpecialBinariesName());
                var lines = file.ReadUtf8Lines();
                return lines.ToArray();
            }
            catch (Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "GetSpecialBinaryInfoFromImportRepository: Unexpected exception.");
                return new string[0];
            }
        }

        public void SetGameProgramInfoToImportRepository(IEnumerable<string> csvFileContent)
        {
            if (csvFileContent == null)
                throw new ArgumentNullException("csvFileContent");
            ClearLastErrorInfo();

            try
            {
                var file = ApplicationData.Current.LocalFolder.CreateFile(ToRomImportsName());
                file.WriteUtf8Lines(csvFileContent);
            }
            catch (Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "SetGameProgramInfoToImportRepository: Unexpected exception.");
            }
        }

        public void SetSpecialBinaryInfoToImportRepository(IEnumerable<string> csvFileContent)
        {
            if (csvFileContent == null)
                throw new ArgumentNullException("csvFileContent");
            ClearLastErrorInfo();

            try
            {
                var file = ApplicationData.Current.LocalFolder.CreateFile(ToRomImportsSpecialBinariesName());
                file.WriteUtf8Lines(csvFileContent);
            }
            catch (Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "SetSpecialBinaryInfoToImportRepository: Unexpected exception.");
            }
        }

        #endregion

        #region Global Settings

        public ApplicationSettings GetSettings()
        {
            ClearLastErrorInfo();

            try
            {
                var file = ApplicationData.Current.LocalFolder.GetFile(ApplicationSettingsName);
                var settings = GetSettingsImpl(file);
                return settings;
            }
            catch (Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "GetSettings: Unexpected exception.");
                return null;
            }
        }

        public void SaveSettings(ApplicationSettings settings)
        {
            ClearLastErrorInfo();
            if (settings == null)
                return;

            try
            {
                var file = ApplicationData.Current.LocalFolder.GetFile(ApplicationSettingsName);
                SaveSettingsImpl(file, settings);
            }
            catch (Exception ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "SaveSettings: Unexpected exception.");
            }
        }

        static ApplicationSettings GetSettingsImpl(IStorageFile file)
        {
            if (file == null)
                return null;

            using (var stream = file.OpenStreamForRead())
            using (var br = new BinaryReader(stream))
            {
                br.ReadInt32(); // version
                var settings = new ApplicationSettings
                {
                    ShowTouchControls = br.ReadBoolean()
                };
                return settings;
            }
        }

        static void SaveSettingsImpl(IStorageFile file, ApplicationSettings settings)
        {
            if (file == null || settings == null)
                return;

            using (var stream = file.OpenStreamForWrite())
            using (var bw = new BinaryWriter(stream))
            {
                bw.Write(1); // version
                bw.Write(settings.ShowTouchControls);
                bw.Flush();
            }
        }

        #endregion

        #region Crash Dumping

        public void DumpCrashReport(Exception ex)
        {
            if (ex == null)
                return;

            var name = string.Format("EMU7800_CRASH_REPORT_{0}.txt", Guid.NewGuid());
            try
            {
                var file = ApplicationData.Current.LocalFolder.CreateFile(name);
                file.WriteUtf8Text(ex.ToString());
            }
            catch (IOException)
            {
            }
        }

        #endregion

        #region Constructors

        public DatastoreService()
        {
            ClearLastErrorInfo();
        }

        #endregion

        #region Helpers

        // Import filenames include version to force re-import on version upgrade.
        // Otherwise, import files can reference ROMs in prior version distributions.

        const string
            RomPropertiesName                   = "ROMProperties.csv",
            RomImportsNameFormat                = "ROMImports_{0}.csv",
            RomImportsSpecialBinariesNameFormat = "ROMImports_{0}.sb.csv",
            PersistedGameProgramsName           = "PersistedGamePrograms",
            ApplicationSettingsName             = "Settings.emusettings";

        static string ToPersistedStateStorageName(GameProgramInfo gameProgramInfo, int saveSlot = 0)
        {
            var gpi = gameProgramInfo;
            var fileName = string.Format("{0}.{1}.{2}.{3}.{4}.emustate",
                gpi.Title, gpi.MachineType, gpi.LController, gpi.RController, saveSlot);
            var name = EscapeFileNameChars(fileName);
            return name;
        }

        static string ToScreenshotStorageName(GameProgramInfo gameProgramInfo, int saveSlot = 0)
        {
            var gpi = gameProgramInfo;
            var fileName = string.Format("{0}.{1}.{2}.{3}.{4}.png",
                gpi.Title, gpi.MachineType, gpi.LController, gpi.RController, saveSlot);
            var name = EscapeFileNameChars(fileName);
            return name;
        }

        static string ToPersistedStateStoragePath(string name)
        {
            var path = string.Format(@"{0}\{1}", PersistedGameProgramsName, name);
            return path;
        }

        static ISet<string> GetFilesFromPersistedGameProgramsImpl()
        {
            IStorageFolder folder = null;
            try
            {
                folder = ApplicationData.Current.LocalFolder.GetFolder(PersistedGameProgramsName);
            }
            catch (FileNotFoundException)
            {
            }
            catch (IOException)
            {
            }
            catch (Exception)
            {
                return new HashSet<string>();
            }

            var query = folder != null
                ? folder.GetFiles()
                    .Where(IsPathPresent)
                    .Select(file => file.Name)
                : new string[0];

            var set = ToHashSet(query);
            return set;
        }

        static ISet<T> ToHashSet<T>(IEnumerable<T> source)
        {
            var set = new HashSet<T>();
            foreach (var item in source)
                set.Add(item);
            return set;
        }

        static IStorageFile GetAssetFile(string name)
        {
            return Package.Current.InstalledLocation.GetFile(@"Assets\" + name);
        }

        static byte[] GetZipBytes(IStorageFile file, string entryPath)
        {
            if (file == null || string.IsNullOrWhiteSpace(entryPath))
                return null;

            using (var stream = file.OpenStreamForRead())
            using (var za = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                var entry = za.GetEntry(entryPath);
                using (var br = new BinaryReader(entry.Open()))
                {
                    return br.ReadBytes((int)entry.Length);
                }
            }
        }

        static IEnumerable<string> QueryForRomCandidates(StorageFolder folder)
        {
            var files = folder.GetFilesAsync()
                .AsTask()
                    .ConfigureAwait(false)
                        .GetAwaiter()
                            .GetResult();
            var filterExtList = new[] {".bin", ".a26", ".a78", ".zip"};
            var list = files
                .Where(IsPathPresent)
                    .Where(file => !file.Name.StartsWith("_"))
                .Where(file => filterExtList.Any(ext => file.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                        .SelectMany(ToPaths);
            return list;
        }

        static IEnumerable<string> ToPaths(IStorageFile file)
        {
            if (!IsPathPresent(file))
                return new string[0];

            if (!file.Path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return new[] { file.Path };

            using (var za = new ZipArchive(file.OpenStreamForRead(), ZipArchiveMode.Read))
            {
                var zipPathList = za.Entries
                    .Select(entry => string.Format("{0}|{1}", file.Path, entry.FullName));
                return zipPathList;
            }
        }

        static string EscapeFileNameChars(string fileName)
        {
            var needEscaping = false;
            for (var i = 0; i < fileName.Length; i++)
            {
                var ch = fileName[i];
                if (!IsCharAcceptable(ch))
                {
                    needEscaping = true;
                    break;
                }
            }

            if (!needEscaping)
                return fileName;

            var sb = new StringBuilder();
            for (var i = 0; i < fileName.Length; i++)
            {
                var ch = fileName[i];
                if (IsCharAcceptable(ch))
                    sb.Append(ch);
                else
                    sb.AppendFormat("{0:x2}", (byte)ch);
            }

            return sb.ToString();
        }

        static bool IsCharAcceptable(char ch)
        {
            const string whiteList = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ._ ";
            for (var i = 0; i < whiteList.Length; i++)
            {
                if (ch == whiteList[i])
                    return true;
            }
            return false;
        }

        void ClearLastErrorInfo()
        {
            LastErrorInfo = null;
        }

        static IStorageFile GetStorageFileFromPath(string path)
        {
            var file = StorageFile.GetFileFromPathAsync(path)
                .AsTask()
                    .ConfigureAwait(false)
                        .GetAwaiter()
                            .GetResult();
            return file;
        }

        static BitmapEncoder CreateBitmapEncoder(Guid encoderId, IRandomAccessStream stream)
        {
            var encoder = BitmapEncoder
                .CreateAsync(encoderId, stream)
                    .AsTask()
                        .ConfigureAwait(false)
                            .GetAwaiter()
                                .GetResult();
            return encoder;
        }

        static bool IsPathPresent(IStorageItem file)
        {
            return file != null && IsPathPresent(file.Path);
        }

        static bool IsPathPresent(string path)
        {
            return !string.IsNullOrEmpty(path);
        }

        static string ToRomImportsName()
        {
            var versionStr = GetVersionString();
            return string.Format(RomImportsNameFormat, versionStr);
        }

        static string ToRomImportsSpecialBinariesName()
        {
            var versionStr = GetVersionString();
            return string.Format(RomImportsSpecialBinariesNameFormat, versionStr);
        }

        static string GetVersionString()
        {
            var version = Package.Current.Id.Version;
            var versionStr = string.Format("{0}.{1}.0.0", version.Major, version.Minor);
            return versionStr;
        }

        #endregion
    }
}
