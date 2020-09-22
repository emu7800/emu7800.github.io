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
    #region ROM Files

        public Results<StringType> QueryLocalMyDocumentsForRomCandidates()
            => Ok(Array.Empty<StringType>()); // not needed in WinRT version

        public Results<StringType> QueryProgramFolderForRomCandidates()
        {
            try
            {
                var folder = Package.Current.InstalledLocation.GetFolder("Assets");
                var folder2 = ApplicationData.Current.LocalFolder; // look for ROMs previously imported
                return Ok(QueryForRomCandidates(folder).Concat(QueryForRomCandidates(folder2)));
            }
            catch (Exception ex)
            {
                return FailResults<StringType>("QueryProgramFolderForRomCandidates: Unexpected exception", ex);
            }
        }

        public Result<BytesType> GetRomBytes(string path)
        {
            try
            {
                var bytes = GetRomBytesImpl(path);
                return Ok(new BytesType(bytes));
            }
            catch (Exception ex)
            {
                return Fail<BytesType>("GetRomBytes", ex);
            }
        }

        static byte[] GetRomBytesImpl(string path)
        {
            byte[] bytes;
            if (path.Contains("|"))
            {
                var splitPath = path.Split('|');
                if (splitPath.Length != 2 || !splitPath[0].EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    return Array.Empty<byte>();
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

        ISet<string> _cachedPersistedDir = new HashSet<string>();

        public bool PersistedMachineExists(GameProgramInfo gameProgramInfo)
        {
            _cachedPersistedDir = GetFilesFromPersistedGameProgramsImpl();
            var name = ToPersistedStateStorageName(gameProgramInfo);
            var oldName = ToPersistedStateStorageOldName(gameProgramInfo);
            return _cachedPersistedDir.Contains(name) || _cachedPersistedDir.Contains(oldName);
        }

        public Result PersistMachine(MachineStateInfo machineStateInfo)
        {
            var name = ToPersistedStateStorageName(machineStateInfo.GameProgramInfo);
            var path = ToPersistedStateStoragePath(name);

            try
            {
                var file = ApplicationData.Current.LocalFolder.CreateFile(path);
                PersistMachineImpl(file, machineStateInfo);
            }
            catch (Exception ex)
            {
                return Fail("PersistMachine", ex);
            }

            var oldName = ToPersistedStateStorageOldName(machineStateInfo.GameProgramInfo);
            var oldPath = ToPersistedStateStoragePath(oldName);

            try
            {
                var file = ApplicationData.Current.LocalFolder.CreateFile(oldPath);
                file.DeleteAsync()
                    .AsTask()
                        .ConfigureAwait(false)
                            .GetAwaiter()
                                .GetResult();
            }
            catch (Exception ex)
            {
                return Fail("PersistMachine", ex);
            }

            _cachedPersistedDir.Add(name);

            return Ok();
        }

        public Result PersistScreenshot(MachineStateInfo machineStateInfo, byte[] data)
        {
            var name = ToScreenshotStorageName(machineStateInfo.GameProgramInfo);
            var path = ToPersistedStateStoragePath(name);

            try
            {
                var file = ApplicationData.Current.LocalFolder.CreateFile(path);
                PersistScreenshotImpl(file, data);
            }
            catch (Exception ex)
            {
                return Fail("PersistScreenshot", ex);
            }

            var oldName = ToScreenshotStorageOldName(machineStateInfo.GameProgramInfo);
            var oldPath = ToPersistedStateStoragePath(oldName);

            try
            {
                var file = ApplicationData.Current.LocalFolder.CreateFile(oldPath);
                file.DeleteAsync()
                    .AsTask()
                        .ConfigureAwait(false)
                            .GetAwaiter()
                                .GetResult();
            }
            catch (Exception ex)
            {
                return Fail("PersistScreenshot", ex);
            }

            return Ok();
        }

        public Result<MachineStateInfo> RestoreMachine(GameProgramInfo gameProgramInfo)
        {
            var name = ToPersistedStateStorageName(gameProgramInfo);
            var path = ToPersistedStateStoragePath(name);

            var oldName = ToPersistedStateStorageOldName(gameProgramInfo);
            var oldPath = ToPersistedStateStoragePath(oldName);

            IStorageFile file;
            try
            {
                try
                {
                    file = ApplicationData.Current.LocalFolder.GetFile(path);
                }
                catch (FileNotFoundException)
                {
                    file = ApplicationData.Current.LocalFolder.GetFile(oldPath);
                }
            }
            catch (Exception ex)
            {
                return Fail<MachineStateInfo>("RestoreMachine(1)", ex);
            }

            try
            {
                return Ok(RestoreMachineImpl(file, gameProgramInfo));
            }
            catch (Exception ex)
            {
                return Fail<MachineStateInfo>("RestoreMachine(2)", ex);
            }
        }

        static void PersistMachineImpl(IStorageFile file, MachineStateInfo machineStateInfo)
        {
            using var stream = file.OpenStreamForWrite();
            using var bw = new BinaryWriter(stream);
            bw.Write(2); // version
            bw.Write(machineStateInfo.FramesPerSecond);
            bw.Write(machineStateInfo.SoundOff);
            bw.Write(machineStateInfo.CurrentPlayerNo);
            bw.Write(machineStateInfo.InterpolationMode);
            machineStateInfo.Machine.Serialize(bw);
            bw.Flush();
        }

        static void PersistScreenshotImpl(IStorageFile file, byte[] data)
        {
            const int width = 320, height = 230;
            using var stream = file.Open(FileAccessMode.ReadWrite);
            var encoder = CreateBitmapEncoder(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, width, height, 96f, 96f, data);
            encoder.Flush();
        }

        static MachineStateInfo RestoreMachineImpl(IStorageFile file, GameProgramInfo gameProgramInfo)
        {
            using var stream = file.OpenStreamForRead();
            using var br = new BinaryReader(stream);
            var version = br.ReadInt32();
            return Fixup(new MachineStateInfo
            {
                FramesPerSecond   = br.ReadInt32(),
                SoundOff          = br.ReadBoolean(),
                CurrentPlayerNo   = br.ReadInt32(),
                InterpolationMode = (version > 1) ? br.ReadInt32() : 0,
                Machine           = MachineBase.Deserialize(br),
                GameProgramInfo   = gameProgramInfo
            });

            static MachineStateInfo Fixup(MachineStateInfo msi)
            {
                if (msi.FramesPerSecond == 0)
                    msi.FramesPerSecond = msi.Machine.FrameHZ;
                return msi;
            }
        }

    #endregion

    #region CSV File IO

        public Results<StringType> GetGameProgramInfoFromReferenceRepository()
        {
            try
            {
                var file = GetAssetFile(RomPropertiesName);
                var lines = file.ReadUtf8Lines();
                return Ok(lines.Select(StringType.ToStringType));
            }
            catch (Exception ex)
            {
                return FailResults<StringType>("GetGameProgramInfoFromReferenceRepository", ex);
            }
        }

        public Results<StringType> GetGameProgramInfoFromImportRepository()
        {
            try
            {
                var file = ApplicationData.Current.LocalFolder.GetFile(ToRomImportsName());
                var lines = file.ReadUtf8Lines();
                return Ok(lines.Select(StringType.ToStringType));
            }
            catch (Exception ex)
            {
                return FailResults<StringType>("GetGameProgramInfoFromImportRepository", ex);
            }
        }

        public Results<StringType> GetSpecialBinaryInfoFromImportRepository()
        {
            try
            {
                var file = ApplicationData.Current.LocalFolder.GetFile(ToRomImportsSpecialBinariesName());
                var lines = file.ReadUtf8Lines();
                return Ok(lines.Select(StringType.ToStringType));
            }
            catch (Exception ex)
            {
                return FailResults<StringType>("GetSpecialBinaryInfoFromImportRepository", ex);
            }
        }

        public Result SetGameProgramInfoToImportRepository(IEnumerable<string> csvFileContent)
        {
            try
            {
                var file = ApplicationData.Current.LocalFolder.CreateFile(ToRomImportsName());
                file.WriteUtf8Lines(csvFileContent);
            }
            catch (Exception ex)
            {
                return Fail("SetGameProgramInfoToImportRepository", ex);
            }
            return Ok();
        }

        public Result SetSpecialBinaryInfoToImportRepository(IEnumerable<string> csvFileContent)
        {
            try
            {
                var file = ApplicationData.Current.LocalFolder.CreateFile(ToRomImportsSpecialBinariesName());
                file.WriteUtf8Lines(csvFileContent);
            }
            catch (Exception ex)
            {
                return Fail("SetSpecialBinaryInfoToImportRepository", ex);
            }
            return Ok();
        }

    #endregion

    #region Global Settings

        public Result<ApplicationSettings> GetSettings()
        {
            try
            {
                var file = ApplicationData.Current.LocalFolder.GetFile(ApplicationSettingsName);
                var settings = GetSettingsImpl(file);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return Fail<ApplicationSettings>("GetSettings: Unexpected exception", ex);
            }
        }

        public Result SaveSettings(ApplicationSettings settings)
        {
            try
            {
                var file = ApplicationData.Current.LocalFolder.CreateFile(ApplicationSettingsName);
                SaveSettingsImpl(file, settings);
            }
            catch (Exception ex)
            {
                return Fail("SaveSettings: Unexpected exception: ", ex);
            }
            return Ok();
        }

        static ApplicationSettings GetSettingsImpl(IStorageFile file)
        {
            using var stream = file.OpenStreamForRead();
            using BinaryReader br = new BinaryReader(stream);
            var version = br.ReadInt32();
            var settings = new ApplicationSettings
            {
                ShowTouchControls = br.ReadBoolean(),
                TouchControlSeparation = version <= 1 ? 0 : br.ReadInt32()
            };
            return settings;
        }

        static void SaveSettingsImpl(IStorageFile file, ApplicationSettings settings)
        {
            using var stream = file.OpenStreamForWrite();
            using var bw = new BinaryWriter(stream);
            bw.Write(2); // version
            bw.Write(settings.ShowTouchControls);
            bw.Write(settings.TouchControlSeparation);
            bw.Flush();
        }

    #endregion

    #region Crash Dumping

        public void DumpCrashReport(Exception ex)
        {
            var name = $"EMU7800_CRASH_REPORT_{Guid.NewGuid()}.txt";
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
        }

    #endregion

    #region Helpers

        // Import filenames include version to force re-import on version upgrade.
        // Otherwise, import files can reference ROMs in prior version distributions.

        const string
            RomPropertiesName = "ROMProperties.csv",
            PersistedGameProgramsName = "PersistedGamePrograms",
            ApplicationSettingsName = "Settings.emusettings";

        static string ToPersistedStateStorageName(GameProgramInfo gameProgramInfo)
        {
            var gpi = gameProgramInfo;
            var fileName = $"{gpi.Title}.{gpi.MachineType}.{gpi.LController}.{gpi.RController}.{gpi.MD5}.emustate";
            return EscapeFileNameChars(fileName);
        }

        static string ToScreenshotStorageName(GameProgramInfo gameProgramInfo)
        {
            var gpi = gameProgramInfo;
            var fileName = $"{gpi.Title}.{gpi.MachineType}.{gpi.LController}.{gpi.RController}.{gpi.MD5}.png";
            return EscapeFileNameChars(fileName);
        }

        static string ToPersistedStateStorageOldName(GameProgramInfo gameProgramInfo)
        {
            var gpi = gameProgramInfo;
            var fileName = $"{gpi.Title}.{gpi.MachineType}.{gpi.LController}.{gpi.RController}.0.emustate";
            return EscapeFileNameChars(fileName);
        }

        static string ToScreenshotStorageOldName(GameProgramInfo gameProgramInfo)
        {
            var gpi = gameProgramInfo;
            var fileName = $"{gpi.Title}.{gpi.MachineType}.{gpi.LController}.{gpi.RController}.0.png";
            return EscapeFileNameChars(fileName);
        }

        static string ToPersistedStateStoragePath(string name)
            => $@"{PersistedGameProgramsName}\{name}";

        static ISet<string> GetFilesFromPersistedGameProgramsImpl()
        {
            IEnumerable<string> fileNames = Array.Empty<string>();
            try
            {
                var folder = ApplicationData.Current.LocalFolder.GetFolder(PersistedGameProgramsName);
                fileNames = folder.GetFiles().Where(IsPathPresent).Select(file => file.Name);
            }
            catch (Exception)
            {
            }
            return new HashSet<string>(fileNames);
        }

        static IStorageFile GetAssetFile(string name)
            => Package.Current.InstalledLocation.GetFile(@"Assets\" + name);

        static byte[] GetZipBytes(IStorageFile file, string entryPath)
        {
            if (string.IsNullOrWhiteSpace(entryPath))
                return Array.Empty<byte>();

            using var stream = file.OpenStreamForRead();
            using var za = new ZipArchive(stream, ZipArchiveMode.Read);
            var entry = za.GetEntry(entryPath);
            using var br = new BinaryReader(entry.Open());
            return br.ReadBytes((int)entry.Length);
        }

        static IEnumerable<StringType> QueryForRomCandidates(StorageFolder folder)
        {
            var files = folder.GetFilesAsync()
                .AsTask()
                    .ConfigureAwait(false)
                        .GetAwaiter()
                            .GetResult();

            var filterExtList = new[] { ".bin", ".a26", ".a78", ".zip" };

            return files
                .Where(IsPathPresent)
                .Where(file => !file.Name.StartsWith("_"))
                .Where(file => filterExtList.Any(ext => file.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .SelectMany(ToPaths);
        }

        static IEnumerable<StringType> ToPaths(IStorageFile file)
        {
            if (!IsPathPresent(file))
                return Array.Empty<StringType>();

            if (!file.Path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return new[] { file.Path }.Select(StringType.ToStringType);

            using var za = new ZipArchive(file.OpenStreamForRead(), ZipArchiveMode.Read);
            return za.Entries.Select(entry => $"{file.Path}|{entry.FullName}").Select(StringType.ToStringType);
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

        static IStorageFile GetStorageFileFromPath(string path)
        {
            var pathPrefix1 = Package.Current.InstalledLocation.Path;
            var pathPrefix2 = ApplicationData.Current.LocalFolder.Path;

            Windows.Foundation.IAsyncOperation<StorageFile> op;

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
            else
            {
                op = StorageFile.GetFileFromPathAsync(path);
            }

            return op.AsTask()
                .ConfigureAwait(false)
                    .GetAwaiter()
                        .GetResult();
        }

        static BitmapEncoder CreateBitmapEncoder(Guid encoderId, IRandomAccessStream stream)
            => BitmapEncoder
                .CreateAsync(encoderId, stream)
                    .AsTask()
                        .ConfigureAwait(false)
                            .GetAwaiter()
                                .GetResult();

        static bool IsPathPresent(IStorageItem file)
            => IsPathPresent(file.Path);

        static bool IsPathPresent(string path)
            => !string.IsNullOrEmpty(path);

        static string ToRomImportsName()
        {
            var versionStr = GetVersionString();
            return $"ROMImports_{versionStr}.csv";
        }

        static string ToRomImportsSpecialBinariesName()
        {
            var versionStr = GetVersionString();
            return $"ROMImports_{versionStr}.sb.csv";
        }

        static string GetVersionString()
        {
            var version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}";
        }

        static Result Ok()
            => new();

        static Result Fail(string message, Exception ex)
            => new (ToResultMessage(message, ex));

        static string ToResultMessage(string message, Exception ex)
            => message + $": Unexpected exception: {ex.GetType().Name}: " + ex.Message;

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

    #region ROM Files

        public (Result, IEnumerable<string>) QueryLocalMyDocumentsForRomCandidates()
        {
#if WIN32
            var path = EnvironmentGetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif MONODROID
            var path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
#endif
            return (Ok(), QueryForRomCandidates(path));
        }

        public (Result, IEnumerable<string>) QueryProgramFolderForRomCandidates()
        {
#if WIN32
            var path = Path.Combine(_currentWorkingDir, "Assets");
#elif MONODROID
            var path = Path.Combine(_userAppDataStoreRoot, "Assets");
#endif
            return (Ok(), QueryForRomCandidates(path));
        }

        public static (Result, byte[]) GetRomBytes(string path)
        {
            if (path.Contains('|'))
            {
                var splitPath = path.Split('|');
                if (splitPath.Length != 2 || !splitPath[0].EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    return (Ok(), Array.Empty<byte>());

                try
                {
                    using var za = new ZipArchive(new FileStream(splitPath[0], FileMode.Open));
                    var entry = za.GetEntry(splitPath[1]);
                    using var br = new BinaryReader(entry.Open());
                    var bytes = br.ReadBytes((int)entry.Length);
                    return (Ok(), bytes);
                }
                catch (Exception ex)
                {
                    if (IsCriticalException(ex))
                        throw;
                    return (Fail("LoadRomBytes: Unable to load ROM bytes from zip archive", ex), Array.Empty<byte>());
                }
            }

            try
            {
                var bytes = File.ReadAllBytes(path);
                return (Ok(), bytes);
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                return (Fail("LoadRomBytes: Unable to load ROM bytes", ex), Array.Empty<byte>());
            }
        }

    #endregion

    #region Machine Persistence

        ISet<string> _cachedPersistedDir;

        public bool PersistedMachineExists(GameProgramInfo gameProgramInfo)
        {
            if (gameProgramInfo == null)
                return false;

            if (_cachedPersistedDir == null)
                _cachedPersistedDir = GetFilesFromPersistedGameProgramsDir();

            var name = ToPersistedStateStorageName(gameProgramInfo);
            var exists = _cachedPersistedDir.Contains(name);
            return exists;
        }

        public Result PersistMachine(MachineStateInfo machineStateInfo)
        {
            EnsurePersistedStateGameProgramsDir();

            var name = ToPersistedStateStorageName(machineStateInfo.GameProgramInfo);
            var path = ToPersistedStateStoragePath(name);

            try
            {
                using var stream = new FileStream(path, FileMode.Create);
                using var bw = new BinaryWriter(stream);
                bw.Write(2); // version
                bw.Write(machineStateInfo.FramesPerSecond);
                bw.Write(machineStateInfo.SoundOff);
                bw.Write(machineStateInfo.CurrentPlayerNo);
                bw.Write(machineStateInfo.InterpolationMode);
                machineStateInfo.Machine.Serialize(bw);
                bw.Flush();
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                return Fail("PersistMachine: Unable to persist machine state", ex);
            }

            _cachedPersistedDir.Add(name);

            return Ok();
        }

        public Result PersistScreenshot(MachineStateInfo machineStateInfo, byte[] data)
        {
            EnsurePersistedStateGameProgramsDir();

            var name = ToScreenshotStorageName(machineStateInfo.GameProgramInfo);
            var path = ToPersistedStateStoragePath(name);

            const int width = 320, height = 230;
            try
            {
                using var fs = new FileStream(path, FileMode.OpenOrCreate);
#if WIN32
                var image = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, BitmapPalettes.Halftone256, data, width * 4);
                var encoder = new PngBitmapEncoder { Interlace = PngInterlaceOption.Off };
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
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                return Fail("PersistScreenshot: Unable to persist screenshot: " + path, ex);
            }

            return Ok();
        }

        public (Result, MachineStateInfo) RestoreMachine(GameProgramInfo gameProgramInfo)
        {
            EnsurePersistedStateGameProgramsDir();

            var name = ToPersistedStateStorageName(gameProgramInfo);
            var path = ToPersistedStateStoragePath(name);

            try
            {
                using var stream = new FileStream(path, FileMode.Open);
                using var br = new BinaryReader(stream);
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
                return (Ok(), machineStateInfo);
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                return (Fail("RestoreMachine: Unable to obtain persisted machine state: " + path, ex), new MachineStateInfo());
            }
        }

    #endregion

    #region CSV File IO

        public static (Result, IEnumerable<string>) GetGameProgramInfoFromReferenceRepository()
        {
            var lines = GetFileTextLines(ToGameProgramInfoReferenceRepositoryPath());
            return (Ok(), lines);
        }

        public (Result, IEnumerable<string>) GetGameProgramInfoFromImportRepository()
        {
            EnsureUserAppDataStoreRoot();
            var lines = GetFileTextLines(ToGameProgramInfoImportRepositoryPath());
            return (Ok(), lines);
        }

        public (Result, IEnumerable<string>) GetSpecialBinaryInfoFromImportRepository()
        {
            EnsureUserAppDataStoreRoot();
            var lines = GetFileTextLines(ToSpecialBinaryInfoImportRepositoryPath());
            return (Ok(), lines);
        }

        public Result SetGameProgramInfoToImportRepository(IEnumerable<string> csvFileContent)
        {
            EnsureUserAppDataStoreRoot();
            return SetFileTextLines(ToGameProgramInfoImportRepositoryPath(), csvFileContent);
        }

        public Result SetSpecialBinaryInfoToImportRepository(IEnumerable<string> csvFileContent)
        {
            EnsureUserAppDataStoreRoot();
            return SetFileTextLines(ToSpecialBinaryInfoImportRepositoryPath(), csvFileContent);
        }

    #endregion

    #region Global Settings

        public (Result, ApplicationSettings) GetSettings()
        {
            EnsureUserAppDataStoreRoot();

            var path = ToLocalUserAppDataPath(ApplicationSettingsName);
            try
            {
                using var stream = new FileStream(path, FileMode.Open);
                using var br = new BinaryReader(stream);
                var version = br.ReadInt32();
                return (Ok(), new()
                {
                    ShowTouchControls = br.ReadBoolean(),
                    TouchControlSeparation = version <= 1 ? 0 : br.ReadInt32()
                });
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                return (Fail("GetSettings: Unable to obtain persisted application settings: " + path, ex), new());
            }
        }

        public Result SaveSettings(ApplicationSettings settings)
        {
            EnsureUserAppDataStoreRoot();

            var path = ToLocalUserAppDataPath(ApplicationSettingsName);
            try
            {
                using var stream = new FileStream(path, FileMode.Create);
                using var bw = new BinaryWriter(stream);
                bw.Write(2); // version
                bw.Write(settings.ShowTouchControls);
                bw.Write(settings.TouchControlSeparation);
                bw.Flush();
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                return Fail("SaveSettings: Unable to persist application settings", ex);
            }
            return Ok();
        }

    #endregion

    #region Crash Dumping

        public static void DumpCrashReport(Exception ex)
        {
             if (string.IsNullOrWhiteSpace(_userAppDataStoreRoot))
                return;

            var filename = $"EMU7800_CRASH_REPORT_{Guid.NewGuid()}.txt";
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
            var fileName = $"{gpi.Title}.{gpi.MachineType}.{gpi.LController}.{gpi.RController}.{saveSlot}.emustate";
            var name = EscapeFileNameChars(fileName);
            return name;
        }

        static string ToScreenshotStorageName(GameProgramInfo gameProgramInfo, int saveSlot = 0)
        {
            var gpi = gameProgramInfo;
            var fileName = $"{gpi.Title}.{gpi.MachineType}.{gpi.LController}.{gpi.RController}.{saveSlot}.png";
            var name = EscapeFileNameChars(fileName);
            return name;
        }

        static string ToPersistedStateStoragePath(string name)
        {
            var persistedGameProgramsDir = Path.Combine(_userAppDataStoreRoot, PersistedGameProgramsName);
            var path = Path.Combine(persistedGameProgramsDir, name);
            return path;
        }

        Result EnsurePersistedStateGameProgramsDir()
        {
            EnsureUserAppDataStoreRoot();

            var persistedGameProgramsDir = Path.Combine(_userAppDataStoreRoot, PersistedGameProgramsName);

            if (!DirectoryExists(persistedGameProgramsDir))
            {
                var result = DirectoryCreateDirectory(persistedGameProgramsDir);
                if (result.IsFail)
                {
                    return Fail("Unable to create PersistedGamePrograms folder: " + persistedGameProgramsDir);
                }
            }

            return Ok();
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
                paths = Array.Empty<string>();
            }
            var files = paths.Select(Path.GetFileName);
            var set = new HashSet<string>(files);
            return set;
        }

        static string ToGameProgramInfoReferenceRepositoryPath() => ToLocalAssetsPath(RomPropertiesName);

        static string ToGameProgramInfoImportRepositoryPath() => ToLocalUserAppDataPath(RomImportsName);

        static string ToSpecialBinaryInfoImportRepositoryPath() => ToLocalUserAppDataPath(RomImportsSpecialBinariesName);

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
                if (!Guid.TryParse(fileName[directoryPrefix.Length..], out Guid guid))
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
                if (DirectoryCreateDirectory(selectedDirectoryPath).IsFail)
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

        static IEnumerable<string> QueryForRomCandidates(string path)
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

        static IEnumerable<string> EnumerateFiles(string path)
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
                        zipPathList = Array.Empty<string>();
                    }
                    foreach (var zippath in zipPathList)
                        yield return zippath;
                }
            }
            while (moveNextResult);
        }

        static string[] GetZipPaths(string filepath)
        {
            using var za = new ZipArchive(new FileStream(filepath, FileMode.Open), ZipArchiveMode.Read);
            return za.Entries
                .Select(entry => $"{filepath}|{entry.FullName}")
                    .ToArray();
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

        static IEnumerable<string> GetFileTextLines(string path)
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

        static Result SetFileTextLines(string path, IEnumerable<string> csvFileContent)
        {
            try
            {
                using var sw = new StreamWriter(new FileStream(path, FileMode.Create), Encoding.UTF8);
                foreach (var line in csvFileContent)
                {
                    sw.WriteLine(line);
                }
                sw.Flush();
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                return Fail("DatastoreService.SetFileTextLines: Unable to write file", ex);
            }
            return Ok();
        }

        static Result Ok()
            => new();

        static Result Fail(string message)
            => new(message);

        static Result Fail(string message, Exception ex)
            => new(message + $": {ex.GetType().Name}: {ex.Message}");

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

        static Result DirectoryCreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                return Fail("Unable to create directory", ex);
            }
            return Ok();
        }

        static string EnvironmentGetFolderPath(Environment.SpecialFolder specialFolder)
        {
            try
            {
                return Environment.GetFolderPath(specialFolder);
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                return string.Empty;
            }
        }

        static bool IsCriticalException(Exception ex)
            => ex is OutOfMemoryException
                  or StackOverflowException
                  or System.Threading.ThreadAbortException
                  or System.Threading.ThreadInterruptedException
                  or TypeInitializationException;

        #endregion

    }

#else

#error "Missing platform symbol for DatastoreService"

#endif

}
