// © Mike Murphy

using EMU7800.Core;
using EMU7800.Services.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace EMU7800.Services;

public sealed class DatastoreService
{
    public readonly static DatastoreService Default = new(NullFileSystemAccessor.Default, NullLogger.Default);

    const string
        SavedGamesFolderName            = ".emu7800savedgames",
        PersistedGameProgramsFolderName = "PersistedGamePrograms",
        NvramFolderName                 = "nvram",
        ApplicationSettingsFileName     = "Settings.emusettings";

    readonly IFileSystemAccessor _fileSystemAccessor;
    readonly ILogger _logger;

    Dictionary<string, DateTime>? _cachedPersistedDir;

    #region BasePaths

    static string AppBaseFolder
      => AppDomain.CurrentDomain.BaseDirectory;

    string[] SaveGamesEmu7800Folder
    {
        get
        {
            if (field.Length == 0)
            {
                var saveFolder = field;

                string[] preferredSaveFolder = [Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), SavedGamesFolderName];

                // For backward compatibility, use legacy folder if it exists and the preferred folder does not.
                // Remove sometime in the future when this location is no longer used.
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (_fileSystemAccessor.FolderExists(preferredSaveFolder))
                    {
                        saveFolder = preferredSaveFolder;
                    }
                    else
                    {
                        string[] legacySaveFolder = [Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Saved Games", "EMU7800"];
                        if (_fileSystemAccessor.FolderExists(legacySaveFolder))
                        {
                            saveFolder = legacySaveFolder;
                        }
                    }
                }

                if (saveFolder.Length == 0)
                {
                    saveFolder = preferredSaveFolder;
                    if (!_fileSystemAccessor.CreateFolder(saveFolder))
                    {
                        saveFolder = [AppBaseFolder, SavedGamesFolderName];
                        if (!_fileSystemAccessor.CreateFolder(saveFolder))
                        {
                            throw new ApplicationException($"{nameof(SaveGamesEmu7800Folder)}: Unable to find/create \"save games\" folder.");
                        }
                    }
                }

                _ = _fileSystemAccessor.CreateFolder([..saveFolder, PersistedGameProgramsFolderName]);
                _ = _fileSystemAccessor.CreateFolder([..saveFolder, NvramFolderName]);

                field = saveFolder;

                Info(nameof(SaveGamesEmu7800Folder), "Using folder: " + ToString(saveFolder));
            }
            return field;
        }
    } = [];

    #endregion

    #region ROM Files

    public IEnumerable<string> QueryForROMs()
    {
        string[] folder = [..SaveGamesEmu7800Folder, PersistedGameProgramsFolderName];
        string[] otherLocation =
#if DEBUG
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? [Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "Programs", "EMU7800"] : [];
#else
        [];
#endif
        return [..QueryForRomCandidates(folder), ..QueryForRomCandidates(AppBaseFolder), ..QueryForRomCandidates(otherLocation)];
    }

    public byte[] GetRomBytes(string path)
    {
        if (path.Contains('|'))
        {
            var splitPath = path.Split('|');
            if (splitPath.Length != 2 || !splitPath[0].EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return [];

            using var stream = _fileSystemAccessor.CreateReadStream(splitPath[0]);
            if (stream == Stream.Null)
            {
                Error(nameof(GetRomBytes), "Unable to read ROM bytes because of previous error.");
                return [];
            }

            try
            {
                using var za = new ZipArchive(stream);
                var entry = za.GetEntry(splitPath[1]);
                if (entry == null)
                    return [];
                using var br = new BinaryReader(entry.Open());
                return br.ReadBytes((int)entry.Length);
            }
            catch (Exception ex)
            {
                Error(nameof(GetRomBytes), "Unable to read ROM bytes from zip archive at {path}", ex);
                return [];
            }
        }

        {
            using var stream = _fileSystemAccessor.CreateReadStream(path);
            if (stream == Stream.Null)
            {
                Error(nameof(GetRomBytes), "Unable to read ROM bytes because of previous error.");
                return [];
            }

            try
            {
                using var br = new BinaryReader(stream);
                return br.ReadBytes((int)stream.Length);
            }
            catch (Exception ex)
            {
                Error(nameof(GetRomBytes), $"Unable to read ROM bytes from {path}", ex);
                return [];
            }
        }
    }

    #endregion

    #region Machine Persistence

    public DateTime PersistedMachineAt(GameProgramInfo gameProgramInfo)
    {
        _cachedPersistedDir ??= _fileSystemAccessor.GetFiles([..SaveGamesEmu7800Folder, PersistedGameProgramsFolderName])
            .Select(kvp => new { FullName = kvp.Key, Pos = kvp.Key.LastIndexOfAny('/', '\\') + 1, LastModified = kvp.Value })
            .Select(r => new { Name = r.FullName[r.Pos..], r.LastModified })
            .GroupBy(r => r.Name)
            .Select(g => new { Name = g.Key, LastModified = g.Select(r => r.LastModified).First() })
            .ToDictionary(r => r.Name, r => r.LastModified);

        var dt = _cachedPersistedDir.TryGetValue(ToPersistedStateStorageName(gameProgramInfo), out var lastUpdated)
            ? lastUpdated : DateTime.MinValue;

        return dt;
    }

    public void PersistMachine(MachineStateInfo machineStateInfo, ReadOnlyMemory<byte> screenshotData)
    {
        string[] folder = [..SaveGamesEmu7800Folder, PersistedGameProgramsFolderName];

        var pssName = ToPersistedStateStorageName(machineStateInfo.GameProgramInfo);
        string[] pssPath = [..folder, pssName];

        NVRAM2k.ReadNVRAMBytes  = ReadNVRAMBytes;
        NVRAM2k.WriteNVRAMBytes = WriteNVRAMBytes;

        {
            using var stream = _fileSystemAccessor.CreateWriteStream(pssPath);
            if (stream == Stream.Null)
            {
                Error(nameof(PersistMachine), "Unable to persist machine state due to previous error.");
                return;
            }

            try
            {
                using var bw = new BinaryWriter(stream);
                bw.Write(2); // version
                bw.Write(machineStateInfo.Machine.FrameHZ);
                bw.Write(machineStateInfo.SoundOff);
                bw.Write(machineStateInfo.CurrentPlayerNo);
                bw.Write(machineStateInfo.InterpolationMode);
                machineStateInfo.Machine.Serialize(bw);
                bw.Flush();
                bw.Close();
            }
            catch (Exception ex)
            {
                Error(nameof(PersistMachine), $"Unable to persist machine state to {ToString(pssPath)}", ex);
            }
        }

        var sssName = ToScreenshotStorageName(machineStateInfo.GameProgramInfo);
        string[] sssPath = [..folder, sssName];

        // data is 320w x 230h, BGR32 pixel format, width should scale x4

        {
            using var stream = _fileSystemAccessor.CreateWriteStream(sssPath);
            if (stream == Stream.Null)
            {
                Error(nameof(PersistMachine), "Unable to persist machine state due to previous error.");
                return;
            }

            try
            {
                using var bw = new BinaryWriter(stream);
                bw.Write(screenshotData.Span);
                bw.Flush();
                stream.Close();
            }
            catch (Exception ex)
            {
                Error(nameof(PersistMachine), $"Unable to persist screenshot to {ToString(sssPath)}", ex);
            }
        }

        _cachedPersistedDir?.Remove(pssName);
        _cachedPersistedDir?.Add(pssName, DateTime.UtcNow);
    }

    public void PurgePersistedMachine(GameProgramInfo gameProgramInfo)
    {
        string[] folder = [..SaveGamesEmu7800Folder, PersistedGameProgramsFolderName];

        var pssName = ToPersistedStateStorageName(gameProgramInfo);
        string[] pssPath = [..folder, pssName];
        string[] sssPath = [..folder, ToScreenshotStorageName(gameProgramInfo)];

        _fileSystemAccessor.DeleteFile(pssPath);
        _fileSystemAccessor.DeleteFile(sssPath);

        _cachedPersistedDir?.Remove(pssName);
    }

    public MachineStateInfo RestoreMachine(GameProgramInfo gameProgramInfo)
    {
        string[] folder = [..SaveGamesEmu7800Folder, PersistedGameProgramsFolderName];

        NVRAM2k.ReadNVRAMBytes  = ReadNVRAMBytes;
        NVRAM2k.WriteNVRAMBytes = WriteNVRAMBytes;

        string[] path = [..folder, ToPersistedStateStorageName(gameProgramInfo)];

        using var stream = _fileSystemAccessor.CreateReadStream(path);
        if (stream == Stream.Null)
        {
            Info(nameof(RestoreMachine), $"No persisted state found for {gameProgramInfo.Title}.");
            return MachineStateInfo.Default;
        }

        try
        {
            using var br = new BinaryReader(stream);
            var version = br.ReadInt32();
            var framesPerSecond = br.ReadInt32();
            var soundOff = br.ReadBoolean();
            var currentPlayerNo = br.ReadInt32();
            var interpolationMode = (version > 1) ? br.ReadInt32() : 0;
            var machine = MachineBase.Deserialize(br);
            machine.FrameHZ = framesPerSecond;
            return new(machine, gameProgramInfo, soundOff, currentPlayerNo, interpolationMode);
        }
        catch (FileNotFoundException)
        {
            return MachineStateInfo.Default;
        }
        catch (Exception ex)
        {
            Error(nameof(RestoreMachine), $"Unable to read persisted machine state from {ToString(path)}", ex);
            return MachineStateInfo.Default;
        }
    }

    #endregion

    #region Global Settings

    public ApplicationSettings GetSettings()
    {
        var folder = SaveGamesEmu7800Folder;
        string[] path = [..folder, ApplicationSettingsFileName];

        using var stream = _fileSystemAccessor.CreateReadStream(path);
        if (stream == Stream.Null)
        {
            Error(nameof(GetSettings), "Unable to read persisted application settings because of previous error.");
            return new();
        }

        try
        {
            using var br = new BinaryReader(stream);
            var version = br.ReadInt32();
            return new()
            {
                ShowTouchControls = br.ReadBoolean(),
                TouchControlSeparation = version <= 1 ? 0 : br.ReadInt32()
            };
        }
        catch (FileNotFoundException)
        {
            return new();
        }
        catch (Exception ex)
        {
            Error(nameof(GetSettings), $"Unable to read persisted application settings from {ToString(path)}", ex);
            return new();
        }
    }

    public void SaveSettings(ApplicationSettings settings)
    {
        var folder = SaveGamesEmu7800Folder;
        string[] path = [..folder, ApplicationSettingsFileName];

        using var stream = _fileSystemAccessor.CreateWriteStream(path);
        if (stream == Stream.Null)
        {
            Error(nameof(SaveSettings), "Unable to persist application settings of previous error.");
            return;
        }

        try
        {
            using var bw = new BinaryWriter(stream);
            bw.Write(2); // version
            bw.Write(settings.ShowTouchControls);
            bw.Write(settings.TouchControlSeparation);
            bw.Flush();
        }
        catch (Exception ex)
        {
            Error(nameof(SaveSettings), $"Unable to persist application settings to {ToString(path)}: ", ex);
        }
    }

    #endregion

    #region Crash Dumping

    public void DumpCrashReport(Exception ex)
    {
        using var stream = _fileSystemAccessor.CreateWriteStream([..SaveGamesEmu7800Folder, $"EMU7800_CRASH_REPORT_{Guid.CreateVersion7()}.txt"]);
        try
        {
            using var sw = new StreamWriter(stream);
            sw.WriteLine(ex.ToString());
            sw.Flush();
            sw.Close();
        }
        catch
        {
        }
    }

    #endregion

    #region Constructors

    public DatastoreService(ILogger logger)
      : this(new FileSystemAccessor(logger), logger) {}

    public DatastoreService(IFileSystemAccessor fileSystemAccessor, ILogger logger)
      => (_fileSystemAccessor, _logger) = (fileSystemAccessor, logger);

    #endregion

    #region Helpers

    static string ToPersistedStateStorageName(GameProgramInfo gameProgramInfo, int saveSlot = 0)
    {
        var gpi = gameProgramInfo;
        var fileName = $"{gpi.Title}.{gpi.MachineType}.{gpi.LController}.{gpi.RController}.{saveSlot}.emustate";
        return EscapeFileNameChars(fileName);
    }

    static string ToScreenshotStorageName(GameProgramInfo gameProgramInfo, int saveSlot = 0)
    {
        var gpi = gameProgramInfo;
        var fileName = $"{gpi.Title}.{gpi.MachineType}.{gpi.LController}.{gpi.RController}.{saveSlot}.bgr32.320x230.scrdata";
        return EscapeFileNameChars(fileName);
    }

    ReadOnlyMemory<byte> ReadNVRAMBytes(string fileName, int count)
    {
        using var stream = _fileSystemAccessor.CreateReadStream([..SaveGamesEmu7800Folder, NvramFolderName, fileName]);
        if (stream == Stream.Null)
        {
            Error(nameof(ReadNVRAMBytes), "Unable to read because of previous error.");
            return new byte[count];
        }

        try
        {
            using var br = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            return br.ReadBytes(count);
        }
        catch (Exception ex)
        {
            if (ex is not FileNotFoundException && ex is not DirectoryNotFoundException)
            {
                Error(nameof(ReadNVRAMBytes), "Unable to read NVRAM", ex);
            }
            return new byte[count];
        }
    }

    void WriteNVRAMBytes(string fileName, ReadOnlyMemory<byte> nvramBytes)
    {
        using var stream = _fileSystemAccessor.CreateWriteStream([..SaveGamesEmu7800Folder, NvramFolderName, fileName]);
        if (stream == Stream.Null)
        {
            Error(nameof(WriteNVRAMBytes), "Unable to write because of previous error.");
            return;
        }

        try
        {
            using var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
            bw.Write(nvramBytes.Span);
            bw.Flush();
            bw.Close();
        }
        catch (Exception ex)
        {
            Error(nameof(WriteNVRAMBytes), "Unable to write NVRAM", ex);
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

    IEnumerable<string> QueryForRomCandidates(params string[] path)
    {
        if (path.Length == 0)
            yield break;

        var stack = new Stack<string[]>();
        stack.Push(path);

        while (stack.Count > 0)
        {
            var folder = stack.Pop();
            foreach (var filePath in EnumerateFiles(folder))
                yield return filePath;
            foreach (var subfolder in _fileSystemAccessor.GetFolders(folder))
                stack.Push([subfolder]);
        }
    }

    IEnumerable<string> EnumerateFiles(string[] path)
    {
        var filesDict = _fileSystemAccessor.GetFiles(path);

        foreach (var filePath in filesDict.Select(kvp => kvp.Key))
        {
            if (filePath.Contains(@"_ReSharper.")) // known to contains lots of .bin files
                continue;
            if (HasOneOfExt(filePath, ".bin", ".a26", ".a78"))
                yield return filePath;
            if (HasOneOfExt(filePath, ".zip"))
            {
                string[] zipPathList;
                try
                {
                    zipPathList = GetZipPaths(filePath);
                }
                catch (Exception ex)
                {
                    Error(nameof(EnumerateFiles), "Unable to read zip archive", ex);
                    zipPathList = [];
                }
                foreach (var zippath in zipPathList)
                    yield return zippath;
            }
        }

        static bool HasOneOfExt(string filePath, params string[] exts)
            => exts.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    string[] GetZipPaths(string path)
    {
        using var stream = _fileSystemAccessor.CreateReadStream(path);
        using var za = new ZipArchive(stream, ZipArchiveMode.Read);
        return [.. za.Entries.Select(entry => $"{path}|{entry.FullName}")];
    }

    void Info(string method, string message)
      => Info(string.Join(": ", method, message));

    void Info(string message)
        => _logger.Log(3, message);

    void Error(string method, string message)
      => Error(string.Join(": ", method, message));

    void Error(string method, string message, Exception ex)
      => Error(string.Join(": ", method, message, ToString(ex)));

    void Error(string message)
      => _logger.Log(1, message);

    static string ToString(Exception? ex)
      => ex is not null ? $"{ex.GetType().Name}: {ex.Message}" : string.Empty;

    static string ToString(string[] path)
      => string.Join("|", path);

    #endregion
}

