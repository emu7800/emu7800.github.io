// © Mike Murphy

namespace EMU7800.Services
{

#if WINDOWS_UWP || WINDOWS_APP || WINDOWS_PHONE_APP

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
    using Core;
    using Dto;
    using Extensions;

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
                    FramesPerSecond = br.ReadInt32(),
                    SoundOff = br.ReadBoolean(),
                    CurrentPlayerNo = br.ReadInt32(),
                    InterpolationMode = (version > 1) ? br.ReadInt32() : 0,
                    Machine = MachineBase.Deserialize(br),
                    GameProgramInfo = gameProgramInfo
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
                var file = ApplicationData.Current.LocalFolder.CreateFile(ApplicationSettingsName);
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
                var version = br.ReadInt32();
                var settings = new ApplicationSettings
                {
                    ShowTouchControls = br.ReadBoolean(),
                    TouchControlSeparation = version <= 1 ? 0 : br.ReadInt32()
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
                bw.Write(2); // version
                bw.Write(settings.ShowTouchControls);
                bw.Write(settings.TouchControlSeparation);
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
            RomPropertiesName = "ROMProperties.csv",
            RomImportsNameFormat = "ROMImports_{0}.csv",
            RomImportsSpecialBinariesNameFormat = "ROMImports_{0}.sb.csv",
            PersistedGameProgramsName = "PersistedGamePrograms",
            ApplicationSettingsName = "Settings.emusettings";

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

            var filterExtList = new[] { ".bin", ".a26", ".a78", ".zip" };

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
            Windows.Foundation.IAsyncOperation<StorageFile> op = null;

            var pathPrefix1 = Package.Current.InstalledLocation.Path;
            var pathPrefix2 = ApplicationData.Current.LocalFolder.Path;

            if (path.StartsWith(pathPrefix1, StringComparison.OrdinalIgnoreCase))
            {
                var shortPath = path.Substring(pathPrefix1.Length + 1);
                op = Package.Current.InstalledLocation.GetFileAsync(shortPath);
            }
            else if (path.StartsWith(pathPrefix2, StringComparison.OrdinalIgnoreCase))
            {
                var shortPath = path.Substring(pathPrefix2.Length + 1);
                op = ApplicationData.Current.LocalFolder.GetFileAsync(shortPath);
            }

            if (op == null)
                op = StorageFile.GetFileFromPathAsync(path);

            return op.AsTask()
                .ConfigureAwait(false)
                    .GetAwaiter()
                        .GetResult();
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

#elif WIN32 || MONODROID

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
#if WIN32
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
#endif
    using Core;
    using Dto;

    public class DatastoreService
    {

        #region Fields

#if WIN32
        readonly static string _currentWorkingDir = AppDomain.CurrentDomain.BaseDirectory;
#elif MONODROID
        static bool _assetsCopied;
#endif

        static string _userAppDataStoreRoot;

        #endregion

        public ErrorInfo LastErrorInfo { get; private set; }

        #region ROM Files

        public IEnumerable<string> QueryLocalMyDocumentsForRomCandidates()
        {
            ClearLastErrorInfo();
#if WIN32
            var path = EnvironmentGetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif MONODROID
            var path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
#endif
            return QueryForRomCandidates(path);
        }

        public IEnumerable<string> QueryProgramFolderForRomCandidates()
        {
            ClearLastErrorInfo();
#if WIN32
            var path = Path.Combine(_currentWorkingDir, "Assets");
#elif MONODROID
            var path = Path.Combine(_userAppDataStoreRoot, "Assets");
#endif
            return QueryForRomCandidates(path);
        }

        public byte[] GetRomBytes(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            ClearLastErrorInfo();

            byte[] bytes = null;

            if (path.Contains('|'))
            {
                var splitPath = path.Split('|');
                if (splitPath.Length != 2 || !splitPath[0].EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    return null;

                try
                {
                    using (var za = new ZipArchive(new FileStream(splitPath[0], FileMode.Open)))
                    {
                        var entry = za.GetEntry(splitPath[1]);
                        using (var br = new BinaryReader(entry.Open()))
                        {
                            bytes = br.ReadBytes((int)entry.Length);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (IsCriticalException(ex))
                        throw;
                    LastErrorInfo = new ErrorInfo(ex, "LoadRomBytes: Unable to load ROM bytes from zip archive.");
                }
                return bytes;
            }

            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                LastErrorInfo = new ErrorInfo(ex, "LoadRomBytes: Unable to load ROM bytes.");
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
                _cachedPersistedDir = GetFilesFromPersistedGameProgramsDir();

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
            EnsurePersistedStateGameProgramsDir();

            var name = ToPersistedStateStorageName(machineStateInfo.GameProgramInfo);
            var path = ToPersistedStateStoragePath(name);

            try
            {
                using (var stream = new FileStream(path, FileMode.Create))
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
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                LastErrorInfo = new ErrorInfo(ex, "PersistMachine: Unable to persist machine state.");
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
            EnsurePersistedStateGameProgramsDir();

            var name = ToScreenshotStorageName(machineStateInfo.GameProgramInfo);
            var path = ToPersistedStateStoragePath(name);

            const int width = 320, height = 230;
            try
            {
                using (var fs = new FileStream(path, FileMode.OpenOrCreate))
                {
#if WIN32
                    var image = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, BitmapPalettes.Halftone256, data, width*4);
                    var encoder = new PngBitmapEncoder {Interlace = PngInterlaceOption.Off};
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fs);
#elif MONODROID
                    var intData = new int[data.Length / 3];
                    for (var i = 0; i < intData.Length; i++)
                    {
                        intData[i] = Android.Graphics.Color.Rgb(data[3*i], data[3*i + 1], data[3*i + 2]);
                    }
                    using (var image = Android.Graphics.Bitmap.CreateBitmap(intData, width, height, Android.Graphics.Bitmap.Config.Argb8888))
                    {
                        image.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, fs);
                    }
#endif
                    fs.Flush(true);
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                LastErrorInfo = new ErrorInfo(ex, "PersistScreenshot: Unable to persist screenshot: {0}", path);
            }
        }

        public MachineStateInfo RestoreMachine(GameProgramInfo gameProgramInfo)
        {
            if (gameProgramInfo == null)
                throw new ArgumentNullException("gameProgramInfo");

            ClearLastErrorInfo();
            EnsurePersistedStateGameProgramsDir();

            var name = ToPersistedStateStorageName(gameProgramInfo);
            var path = ToPersistedStateStoragePath(name);

            try
            {
                using (var stream = new FileStream(path, FileMode.Open))
                using (var br = new BinaryReader(stream))
                {
                    var version = br.ReadInt32();
                    var machineStateInfo = new MachineStateInfo
                    {
                        FramesPerSecond     = br.ReadInt32(),
                        SoundOff            = br.ReadBoolean(),
                        CurrentPlayerNo     = br.ReadInt32(),
                        InterpolationMode   = (version > 1) ? br.ReadInt32() : 0,
                        Machine             = MachineBase.Deserialize(br),
                        GameProgramInfo     = gameProgramInfo
                    };
                    if (machineStateInfo.FramesPerSecond == 0)
                        machineStateInfo.FramesPerSecond = machineStateInfo.Machine.FrameHZ;
                    return machineStateInfo;
                }
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                LastErrorInfo = new ErrorInfo(ex, "RestoreMachine: Unable to obtain persisted machine state: {0}", path);
                return null;
            }
        }

        #endregion

        #region CSV File IO

        public IEnumerable<string> GetGameProgramInfoFromReferenceRepository()
        {
            ClearLastErrorInfo();
            var lines = GetFileTextLines(ToGameProgramInfoReferenceRepositoryPath());
            return lines.ToArray();
        }

        public IEnumerable<string> GetGameProgramInfoFromImportRepository()
        {
            ClearLastErrorInfo();
            EnsureUserAppDataStoreRoot();
            var lines = GetFileTextLines(ToGameProgramInfoImportRepositoryPath());
            return lines.ToArray();
        }

        public IEnumerable<string> GetSpecialBinaryInfoFromImportRepository()
        {
            ClearLastErrorInfo();
            EnsureUserAppDataStoreRoot();
            var lines = GetFileTextLines(ToSpecialBinaryInfoImportRepositoryPath());
            return lines.ToArray();
        }

        public void SetGameProgramInfoToImportRepository(IEnumerable<string> csvFileContent)
        {
            ClearLastErrorInfo();
            EnsureUserAppDataStoreRoot();
            SetFileTextLines(ToGameProgramInfoImportRepositoryPath(), csvFileContent);
        }

        public void SetSpecialBinaryInfoToImportRepository(IEnumerable<string> csvFileContent)
        {
            ClearLastErrorInfo();
            EnsureUserAppDataStoreRoot();
            SetFileTextLines(ToSpecialBinaryInfoImportRepositoryPath(), csvFileContent);
        }

        #endregion

        #region Global Settings

        public ApplicationSettings GetSettings()
        {
            ClearLastErrorInfo();
            EnsureUserAppDataStoreRoot();

            var path = ToLocalUserAppDataPath(ApplicationSettingsName);
            try
            {
                using (var stream = new FileStream(path, FileMode.Open))
                using (var br = new BinaryReader(stream))
                {
                    var version = br.ReadInt32();
                    var settings = new ApplicationSettings
                    {
                        ShowTouchControls = br.ReadBoolean(),
                        TouchControlSeparation = version <= 1 ? 0 : br.ReadInt32()
                    };
                    return settings;
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                LastErrorInfo = new ErrorInfo(ex, "GetSettings: Unable to obtain persisted application settings: {0}", path);
                return null;
            }
        }

        public void SaveSettings(ApplicationSettings settings)
        {
            EnsureUserAppDataStoreRoot();

            var path = ToLocalUserAppDataPath(ApplicationSettingsName);
            try
            {
                using (var stream = new FileStream(path, FileMode.Create))
                using (var bw = new BinaryWriter(stream))
                {
                    bw.Write(2); // version
                    bw.Write(settings.ShowTouchControls);
                    bw.Write(settings.TouchControlSeparation);
                    bw.Flush();
                }
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                LastErrorInfo = new ErrorInfo(ex, "SaveSettings: Unable to persist application settings.");
            }
        }

        #endregion

        #region Crash Dumping

        public void DumpCrashReport(Exception ex)
        {
            if (ex == null)
                return;
            if (string.IsNullOrWhiteSpace(_userAppDataStoreRoot))
                return;

            var filename = string.Format("EMU7800_CRASH_REPORT_{0}.txt", Guid.NewGuid());
            var path = ToLocalUserAppDataPath(filename);
            if (string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                File.WriteAllText(path, ex.ToString());
            }
            catch (Exception ex2)
            {
                if (IsCriticalException(ex2))
                    throw;
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

        const string
            RomPropertiesName              = "ROMProperties.csv",
            RomImportsName                 = "ROMImports.csv",
            RomImportsSpecialBinariesName  = "ROMImports.sb.csv",
            PersistedGameProgramsName      = "PersistedGamePrograms",
            ApplicationSettingsName        = "Settings.emusettings";

        static string ToLocalAssetsPath(string fileName)
        {
#if WIN32
            var dir = _currentWorkingDir;
#elif MONODROID
            var dir = _userAppDataStoreRoot;
#endif
            var root = Path.Combine(dir, "Assets");
            var path = string.IsNullOrWhiteSpace(fileName) ? root : Path.Combine(root, fileName);
            return path;
        }

        static string ToLocalUserAppDataPath(string fileName)
        {
            var path = Path.Combine(_userAppDataStoreRoot, fileName);
            return path;
        }

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
            var persistedGameProgramsDir = Path.Combine(_userAppDataStoreRoot, PersistedGameProgramsName);
            var path = Path.Combine(persistedGameProgramsDir, name);
            return path;
        }

        void EnsurePersistedStateGameProgramsDir()
        {
            EnsureUserAppDataStoreRoot();
            var persistedGameProgramsDir = Path.Combine(_userAppDataStoreRoot, PersistedGameProgramsName);

            if (!DirectoryExists(persistedGameProgramsDir))
            {
                DirectoryCreateDirectory(persistedGameProgramsDir);
                if (LastErrorInfo != null)
                {
                    LastErrorInfo = new ErrorInfo(LastErrorInfo, "Unable to create PersistedGamePrograms folder: {0}", persistedGameProgramsDir);
                }
            }
        }

        ISet<string> GetFilesFromPersistedGameProgramsDir()
        {
            EnsurePersistedStateGameProgramsDir();
            var persistedGameProgramsDir = Path.Combine(_userAppDataStoreRoot, PersistedGameProgramsName);
            string[] paths;
            try
            {
                paths = Directory.GetFiles(persistedGameProgramsDir);
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                paths = new string[0];
            }
            var files = paths.Select(Path.GetFileName);
            var set = new HashSet<string>(files);
            return set;
        }

        static string ToGameProgramInfoReferenceRepositoryPath()
        {
            return ToLocalAssetsPath(RomPropertiesName);
        }

        static string ToGameProgramInfoImportRepositoryPath()
        {
            return ToLocalUserAppDataPath(RomImportsName);
        }

        static string ToSpecialBinaryInfoImportRepositoryPath()
        {
            return ToLocalUserAppDataPath(RomImportsSpecialBinariesName);
        }

        void EnsureUserAppDataStoreRoot()
        {
            if (_userAppDataStoreRoot == null)
                _userAppDataStoreRoot = DiscoverOrCreateUserAppDataStoreRoot();
#if MONODROID
            if (_userAppDataStoreRoot != null && !_assetsCopied)
            {
                CopyAssetFile(RomPropertiesName);
                CopyAssetFile("roms.zip");
                _assetsCopied = true;
            }
#endif
        }

#if MONODROID
        void CopyAssetFile(string fileName)
        {
            DirectoryCreateDirectory(ToLocalAssetsPath(null));
            using (var s = MonoDroid.MainActivity.App.Assets.Open(fileName, Android.Content.Res.Access.Streaming))
            using (var fs = new FileStream(ToLocalAssetsPath(fileName), FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                s.CopyTo(fs);
                fs.Flush(true);
                fs.Close();
            }
        }
#endif

        string DiscoverOrCreateUserAppDataStoreRoot()
        {
            const string directoryPrefix = "EMU7800.";

            var appDataRoot = EnvironmentGetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(appDataRoot))
                throw new ApplicationException("Unable to probe SpecialFolder.LocalApplicationData.");

            string selectedDirectoryPath = null;
            var selectedDirectoryPathCreationTimeUtc = DateTime.MinValue;

            var directories = EnumerateDirectories(appDataRoot);
            foreach (var directory in directories)
            {
                var fileName = Path.GetFileName(directory);
                if (fileName == null)
                    continue;
                if (fileName.Length <= directoryPrefix.Length || !fileName.StartsWith(directoryPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;
                Guid guid;
                if (!Guid.TryParse(fileName.Substring(directoryPrefix.Length), out guid))
                    continue;
                var directoryCreationTimeUtc = Directory.GetCreationTimeUtc(directory);
                if (directoryCreationTimeUtc <= selectedDirectoryPathCreationTimeUtc)
                    continue;
                selectedDirectoryPath = directory;
                selectedDirectoryPathCreationTimeUtc = directoryCreationTimeUtc;
            }

            if (selectedDirectoryPath == null)
            {
                var directoryName = directoryPrefix + Guid.NewGuid();
                selectedDirectoryPath = Path.Combine(appDataRoot, directoryName);
                if (!DirectoryCreateDirectory(selectedDirectoryPath))
                    throw new ApplicationException("Unable to create LocalApplicationData folder: " + directoryName);
            }

            return selectedDirectoryPath;
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

        IEnumerable<string> QueryForRomCandidates(string path)
        {
            var stack = new Stack<string>();
            stack.Push(path);

            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                foreach (var filepath in EnumerateFiles(dir))
                    yield return filepath;
                foreach (var dirpath in EnumerateDirectories(dir))
                    stack.Push(dirpath);
            }
        }

        IEnumerable<string> EnumerateFiles(string path)
        {
            IEnumerator<string> enumerator;
            try
            {
                var enumerable = Directory.EnumerateFiles(path);
                enumerator = enumerable.GetEnumerator();
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                var enumerable = Enumerable.Empty<string>();
                enumerator = enumerable.GetEnumerator();
            }

            bool moveNextResult;
            do
            {
                try
                {
                    moveNextResult = enumerator.MoveNext();
                }
                catch (Exception ex)
                {
                    if (IsCriticalException(ex))
                        throw;
                    moveNextResult = false;
                }
                if (!moveNextResult)
                    continue;

                var filepath = enumerator.Current;
                if (filepath.Contains(@"\_ReSharper.")) // known to contains lots of .bin files
                    continue;
                if (filepath.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
                    yield return filepath;
                if (filepath.EndsWith(".a26", StringComparison.OrdinalIgnoreCase))
                    yield return filepath;
                if (filepath.EndsWith(".a78", StringComparison.OrdinalIgnoreCase))
                    yield return filepath;
                if (filepath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    string[] zipPathList;
                    try
                    {
                        zipPathList = GetZipPaths(filepath);
                    }
                    catch (Exception ex)
                    {
                        if (IsCriticalException(ex))
                            throw;
                        zipPathList = new string[0];
                    }
                    foreach (var zippath in zipPathList)
                        yield return zippath;
                }
            }
            while (moveNextResult);
        }

        static string[] GetZipPaths(string filepath)
        {
            using (var za = new ZipArchive(new FileStream(filepath, FileMode.Open), ZipArchiveMode.Read))
            {
                var zipPathList = za.Entries
                    .Select(entry => string.Format("{0}|{1}", filepath, entry.FullName))
                        .ToArray();
                return zipPathList;
            }
        }

        static IEnumerable<string> EnumerateDirectories(string path)
        {
            IEnumerator<string> enumerator;
            try
            {
                var enumerable = Directory.EnumerateDirectories(path);
                enumerator = enumerable.GetEnumerator();
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                var enumerable = Enumerable.Empty<string>();
                enumerator = enumerable.GetEnumerator();
            }

            bool moveNextResult;
            do
            {
                try
                {
                    moveNextResult = enumerator.MoveNext();
                }
                catch (Exception ex)
                {
                    if (IsCriticalException(ex))
                        throw;
                    moveNextResult = false;
                }
                if (moveNextResult)
                {
                    yield return enumerator.Current;
                }
            }
            while (moveNextResult);
        }

        IEnumerable<string> GetFileTextLines(string path)
        {
            StreamReader fs = null;
            while (true)
            {
                string line;
                try
                {
                    if (fs == null)
                        fs = new StreamReader(new FileStream(path, FileMode.Open), Encoding.UTF8);
                    line = fs.ReadLine();
                }
                catch (Exception ex)
                {
                    if (IsCriticalException(ex))
                        throw;
                    if (fs != null)
                        fs.Dispose();
                    LastErrorInfo = new ErrorInfo(ex, "GetFileTextLines: Unable to read file.");
                    yield break;
                }
                if (line == null)
                {
                    fs.Dispose();
                    yield break;
                }
                yield return line;
            }
        }

        void SetFileTextLines(string path, IEnumerable<string> csvFileContent)
        {
            try
            {
                using (var sw = new StreamWriter(new FileStream(path, FileMode.Create), Encoding.UTF8))
                {
                    foreach (var line in csvFileContent)
                    {
                        sw.WriteLine(line);
                    }
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                LastErrorInfo = new ErrorInfo(ex, "DatastoreService.SetFileTextLines: Unable to write file.");
            }
        }

        static bool DirectoryExists(string path)
        {
            try
            {
                return Directory.Exists(path);
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                return false;
            }
        }

        bool DirectoryCreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                LastErrorInfo = new ErrorInfo(ex, "Exception during Directory.CreateDirectory of {0}", path);
                return false;
            }
            return true;
        }

        string EnvironmentGetFolderPath(Environment.SpecialFolder specialFolder)
        {
            try
            {
                var path = Environment.GetFolderPath(specialFolder);
                return path;
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                LastErrorInfo = new ErrorInfo(ex, "Exception during Environment.GetFolderPath of {0}", specialFolder);
                return null;
            }
        }

        static bool IsCriticalException(Exception ex)
        {
            return ex is OutOfMemoryException
                || ex is StackOverflowException
                || ex is System.Threading.ThreadAbortException
                || ex is System.Threading.ThreadInterruptedException
                || ex is TypeInitializationException;
        }

        void ClearLastErrorInfo()
        {
            LastErrorInfo = null;
        }

        #endregion

    }

#else

#error "Missing platform symbol for DatastoreService"

#endif

}
