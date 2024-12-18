﻿// © Mike Murphy

using EMU7800.Core;
using EMU7800.Services.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace EMU7800.Services;

public class DatastoreService
{
    #region Fields

    static string SaveGamesEmu7800Folder = string.Empty;

    #endregion

    #region ROM Files

    public static IEnumerable<string> QueryROMSFolder()
    {
        EnsureSaveGamesEmu7800FolderExists();
        var q1 = QueryForRomCandidates(SaveGamesEmu7800Folder);
        var q2 = QueryForRomCandidates(AppDomain.CurrentDomain.BaseDirectory);
        return q1.Concat(q2);
    }

    public static byte[] GetRomBytes(string path)
    {
        if (path.Contains('|'))
        {
            var splitPath = path.Split('|');
            if (splitPath.Length != 2 || !splitPath[0].EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return [];

            try
            {
                using var za = new ZipArchive(new FileStream(splitPath[0], FileMode.Open));
                var entry = za.GetEntry(splitPath[1]);
                if (entry == null)
                    return [];
                using var br = new BinaryReader(entry.Open());
                return br.ReadBytes((int)entry.Length);
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                Error($"GetRomBytes: Unable to read ROM bytes from zip archive at {path}: " + ToString(ex));
                return [];
            }
        }

        try
        {
            return File.ReadAllBytes(path);
        }
        catch (Exception ex)
        {
            if (IsCriticalException(ex))
                throw;
            Error($"GetRomBytes: Unable to read ROM bytes from {path}: " + ToString(ex));
            return [];
        }
    }

    #endregion

    #region Machine Persistence

    static bool HasPersistedDirBeenScanned;
    static Dictionary<string, DateTime> _cachedPersistedDir = [];

    public static DateTime PersistedMachineAt(GameProgramInfo gameProgramInfo)
    {
        if (!HasPersistedDirBeenScanned)
        {
            _cachedPersistedDir = GetFilesFromPersistedGameProgramsDir();
            HasPersistedDirBeenScanned = true;
        }

        var name = ToPersistedStateStorageName(gameProgramInfo);

        return _cachedPersistedDir.TryGetValue(name, out var lastUpdated) ? lastUpdated : DateTime.MinValue;
    }

    public static void PersistMachine(MachineStateInfo machineStateInfo, ReadOnlyMemory<byte> screenshotData)
    {
        EnsurePersistedGameProgramsFolderExists();

        var pssName = ToPersistedStateStorageName(machineStateInfo.GameProgramInfo);
        var pssPath = ToPersistedStateStoragePath(pssName);

        try
        {
            using var fs = new FileStream(pssPath, FileMode.Create);
            using var bw = new BinaryWriter(fs);
            bw.Write(2); // version
            bw.Write(machineStateInfo.FramesPerSecond);
            bw.Write(machineStateInfo.SoundOff);
            bw.Write(machineStateInfo.CurrentPlayerNo);
            bw.Write(machineStateInfo.InterpolationMode);
            machineStateInfo.Machine.Serialize(bw);
            bw.Flush();
            bw.Close();
        }
        catch (Exception ex)
        {
            if (IsCriticalException(ex))
                throw;
            Error($"PersistMachine: Unable to persist machine state to {pssPath}: " + ToString(ex));
        }

        var sssName = ToScreenshotStorageName(machineStateInfo.GameProgramInfo);
        var sssPath = ToPersistedStateStoragePath(sssName);

        // data is 320w x 230h, BGR32 pixel format, width should scale x4

        try
        {
            using var fs = new FileStream(sssPath, FileMode.OpenOrCreate);
            fs.Write(screenshotData.Span);
            fs.Flush(true);
            fs.Close();
        }
        catch (Exception ex)
        {
            if (IsCriticalException(ex))
                throw;
            Error($"PersistScreenshot: Unable to persist screenshot to {pssPath}: " + ToString(ex));
        }

        _cachedPersistedDir.Remove(pssName);
        _cachedPersistedDir.Add(pssName, DateTime.UtcNow);
    }

    public static void PurgePersistedMachine(GameProgramInfo gameProgramInfo)
    {
        var pssName = ToPersistedStateStorageName(gameProgramInfo);
        var pssPath = ToPersistedStateStoragePath(pssName);

        var sssName = ToScreenshotStorageName(gameProgramInfo);
        var sssPath = ToPersistedStateStoragePath(sssName);

        try { File.Delete(pssPath); } catch (Exception ex) { Error($"PurgePersistedMachine: Unable to delete: {pssPath}: " + ToString(ex)); }
        try { File.Delete(sssPath); } catch (Exception ex) { Error($"PurgePersistedMachine: Unable to delete: {sssPath}: " + ToString(ex)); }

        _cachedPersistedDir.Remove(pssName);
    }

    public static MachineStateInfo RestoreMachine(GameProgramInfo gameProgramInfo)
    {
        EnsurePersistedGameProgramsFolderExists();

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
            {
                machineStateInfo = machineStateInfo with { FramesPerSecond = machineStateInfo.Machine.FrameHZ };
            }
            return machineStateInfo;
        }
        catch (Exception ex)
        {
            if (IsCriticalException(ex))
                throw;
            Error($"RestoreMachine: Unable to read persisted machine state from {path}: " + ToString(ex));
            return MachineStateInfo.Default;
        }
    }

    #endregion

    #region Imported Game Program Info

    public static IEnumerable<ImportedGameProgramInfo> ImportedGameProgramInfo { get; set; } = Array.Empty<ImportedGameProgramInfo>();
    public static IEnumerable<ImportedSpecialBinaryInfo> ImportedSpecialBinaryInfo { get; set; } = Array.Empty<ImportedSpecialBinaryInfo>();

    #endregion

    #region Global Settings

    public static ApplicationSettings GetSettings()
    {
        EnsureSaveGamesEmu7800FolderExists();

        var path = ToLocalUserDataStoragePath(ApplicationSettingsName);
        try
        {
            using var stream = new FileStream(path, FileMode.Open);
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
            if (IsCriticalException(ex))
                throw;
            Error($"GetSettings: Unable to read persisted application settings from {path}: " + ToString(ex));
            return new();
        }
    }

    public static void SaveSettings(ApplicationSettings settings)
    {
        EnsureSaveGamesEmu7800FolderExists();

        var path = ToLocalUserDataStoragePath(ApplicationSettingsName);
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
            Error($"SaveSettings: Unable to persist application settings to {path}: " + ToString(ex));
        }
    }

    #endregion

    #region Crash Dumping

    public static void DumpCrashReport(Exception ex)
    {
        if (string.IsNullOrWhiteSpace(SaveGamesEmu7800Folder))
            return;

        var filename = $"EMU7800_CRASH_REPORT_{Guid.NewGuid()}.txt";
        var path = ToLocalUserDataStoragePath(filename);
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

    #region Helpers

    const string
        PersistedGameProgramsName = "PersistedGamePrograms",
        ApplicationSettingsName = "Settings.emusettings";

    static string ToLocalUserDataStoragePath(string fileName)
        => Path.Combine(SaveGamesEmu7800Folder, fileName);

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

    static string ToPersistedStateStoragePath(string name)
        => Path.Combine(SaveGamesEmu7800Folder, PersistedGameProgramsName, name);

    static void EnsurePersistedGameProgramsFolderExists()
    {
        EnsureSaveGamesEmu7800FolderExists();
        var folder = Path.Combine(SaveGamesEmu7800Folder, PersistedGameProgramsName);
        if (!DirectoryExists(folder))
        {
            DirectoryCreateDirectory(folder);
        }
    }

    static Dictionary<string, DateTime> GetFilesFromPersistedGameProgramsDir()
    {
        EnsurePersistedGameProgramsFolderExists();
        var folder = Path.Combine(SaveGamesEmu7800Folder, PersistedGameProgramsName);
        FileInfo[] files;
        try
        {
            var di = new DirectoryInfo(folder);
            files = di.GetFiles();
        }
        catch (Exception ex)
        {
            if (IsCriticalException(ex))
                throw;
            files = [];
        }
        return files.Select(f => (f.Name, f.LastWriteTimeUtc))
                    .ToDictionary(kvp => kvp.Name, kvp => kvp.LastWriteTimeUtc);
    }

    static void EnsureSaveGamesEmu7800FolderExists()
    {
        if (SaveGamesEmu7800Folder.Length == 0)
        {
            SaveGamesEmu7800Folder = Path.Combine(EnvironmentGetFolderPath(Environment.SpecialFolder.UserProfile), "Saved Games", "EMU7800");
        }
        if (!DirectoryExists(SaveGamesEmu7800Folder))
        {
            DirectoryCreateDirectory(SaveGamesEmu7800Folder);
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
                    zipPathList = [];
                }
                foreach (var zippath in zipPathList)
                    yield return zippath;
            }
        }
        while (moveNextResult);

        enumerator.Dispose();
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

        enumerator.Dispose();
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

    static void DirectoryCreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
        }
        catch (Exception ex)
        {
            if (IsCriticalException(ex))
                throw;
            Error("Unable to create directory: " + ToString(ex));
        }
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

    static void Error(string message)
        => Console.WriteLine("ERROR: " + message);

    static string ToString(Exception ex)
        => $"{ex.GetType().Name}: {ex.Message}";

    static bool IsCriticalException(Exception ex)
        => ex is OutOfMemoryException
              or StackOverflowException
              or System.Threading.ThreadAbortException
              or System.Threading.ThreadInterruptedException
              or TypeInitializationException;

    #endregion
}