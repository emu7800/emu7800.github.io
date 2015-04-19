// © Mike Murphy

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using EMU7800.Core;
using EMU7800.Services.Dto;

namespace EMU7800.Services
{
    public class DatastoreService
    {
        #region Fields

        static string _userAppDataStoreRoot;

        #endregion

        public ErrorInfo LastErrorInfo { get; private set; }

        #region ROM Files

        public IEnumerable<string> QueryLocalMyDocumentsForRomCandidates()
        {
            ClearLastErrorInfo();

            var localMyDocumentsPath = EnvironmentGetFolderPath(Environment.SpecialFolder.MyDocuments);
            return QueryForRomCandidates(localMyDocumentsPath);
        }

        public IEnumerable<string> QueryProgramFolderForRomCandidates()
        {
            ClearLastErrorInfo();

            var dir = EnvironmentGetFolderPath(Environment.SpecialFolder.Personal);
            return QueryForRomCandidates(Path.Combine(dir, "Assets"));
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
                    bytes = new byte[0];
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
                    var intData = new int[data.Length / 3];
                    for (var i = 0; i < intData.Length; i++)
                    {
                        intData[i] = Android.Graphics.Color.Rgb(data[3*i], data[3*i + 1], data[3*i + 2]);
                    }
                    using (var image = Android.Graphics.Bitmap.CreateBitmap(intData, width, height, Android.Graphics.Bitmap.Config.Argb8888))
                    {
                        image.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, fs);
                    }
                    fs.Flush(true);
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
                    br.ReadInt32(); // version
                    var settings = new ApplicationSettings
                    {
                        ShowTouchControls = br.ReadBoolean()
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
                    bw.Write(1); // version
                    bw.Write(settings.ShowTouchControls);
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

        string ToLocalAssetsPath(string fileName)
        {
            var dir = EnvironmentGetFolderPath(Environment.SpecialFolder.Personal);
            var root = Path.Combine(dir, "Assets");
            var path = Path.Combine(root, fileName);
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

        string ToGameProgramInfoReferenceRepositoryPath()
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
        }

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
}
