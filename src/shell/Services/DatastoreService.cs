// © Mike Murphy

namespace EMU7800.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;

    using Core;
    using Dto;

    public class DatastoreService
    {

    #region Fields

        readonly static string _currentWorkingDir = AppDomain.CurrentDomain.BaseDirectory;

        static string _userAppDataStoreRoot;

    #endregion

    #region ROM Files

        public static (Result, IEnumerable<string>) QueryLocalMyDocumentsForRomCandidates()
        {
            var path = EnvironmentGetFolderPath(Environment.SpecialFolder.MyDocuments);
            return (Ok(), QueryForRomCandidates(path));
        }

        public static (Result, IEnumerable<string>) QueryProgramFolderForRomCandidates()
        {
            var path = Path.Combine(_currentWorkingDir, "Assets");
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

        static ISet<string> _cachedPersistedDir;

        public static bool PersistedMachineExists(GameProgramInfo gameProgramInfo)
        {
            if (gameProgramInfo == null)
                return false;

            if (_cachedPersistedDir == null)
                _cachedPersistedDir = GetFilesFromPersistedGameProgramsDir();

            var name = ToPersistedStateStorageName(gameProgramInfo);
            var exists = _cachedPersistedDir.Contains(name);
            return exists;
        }

        public static Result PersistMachine(MachineStateInfo machineStateInfo)
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

        public static Result PersistScreenshot(MachineStateInfo machineStateInfo, byte[] data)
        {
            EnsurePersistedStateGameProgramsDir();

            var name = ToScreenshotStorageName(machineStateInfo.GameProgramInfo);
            var path = ToPersistedStateStoragePath(name);

            // data is 320w x 230h, BGR32 pixel format, width should scale x4

            try
            {
                using var fs = new FileStream(path, FileMode.OpenOrCreate);
                fs.Write(data);
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

        public static (Result, MachineStateInfo) RestoreMachine(GameProgramInfo gameProgramInfo)
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

        public static (Result, IEnumerable<string>) GetGameProgramInfoFromImportRepository()
        {
            EnsureUserAppDataStoreRoot();
            var lines = GetFileTextLines(ToGameProgramInfoImportRepositoryPath());
            return (Ok(), lines);
        }

        public static (Result, IEnumerable<string>) GetSpecialBinaryInfoFromImportRepository()
        {
            EnsureUserAppDataStoreRoot();
            var lines = GetFileTextLines(ToSpecialBinaryInfoImportRepositoryPath());
            return (Ok(), lines);
        }

        public static Result SetGameProgramInfoToImportRepository(IEnumerable<string> csvFileContent)
        {
            EnsureUserAppDataStoreRoot();
            return SetFileTextLines(ToGameProgramInfoImportRepositoryPath(), csvFileContent);
        }

        public static Result SetSpecialBinaryInfoToImportRepository(IEnumerable<string> csvFileContent)
        {
            EnsureUserAppDataStoreRoot();
            return SetFileTextLines(ToSpecialBinaryInfoImportRepositoryPath(), csvFileContent);
        }

    #endregion

    #region Global Settings

        public static (Result, ApplicationSettings) GetSettings()
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

        public static Result SaveSettings(ApplicationSettings settings)
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
            var dir = _currentWorkingDir;
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

        static Result EnsurePersistedStateGameProgramsDir()
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

        static ISet<string> GetFilesFromPersistedGameProgramsDir()
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

        static void EnsureUserAppDataStoreRoot()
        {
            if (_userAppDataStoreRoot == null)
                _userAppDataStoreRoot = DiscoverOrCreateUserAppDataStoreRoot();
        }

        static string DiscoverOrCreateUserAppDataStoreRoot()
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
            try
            {
                return File.ReadAllLines(path, Encoding.UTF8);
            }
            catch (FileNotFoundException)
            {
                return Array.Empty<string>();
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
}
