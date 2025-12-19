// © Mike Murphy

using EMU7800.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EMU7800.Services;

public interface IFileSystemAccessor
{
    string AppBaseFolder { get; }
    bool CreateFolder(params string[] pathParts);
    bool FolderExists(params string[] pathParts);
    IEnumerable<string> GetFolders(params string[] pathParts);
    Dictionary<string, DateTime> GetFiles(params string[] pathParts);
    void DeleteFile(params string[] pathParts);
    Stream CreateReadStream(params string[] pathParts);
    Stream CreateWriteStream(params string[] pathParts);
}

public sealed class NullFileSystemAccessor : IFileSystemAccessor
{
    public readonly static NullFileSystemAccessor Default = new();

    public string AppBaseFolder { get; } = string.Empty;

    public bool CreateFolder(params string[] pathParts)
      => false;

    public Stream CreateReadStream(params string[] pathParts)
      => Stream.Null;

    public Stream CreateWriteStream(params string[] pathParts)
      => Stream.Null;

    public void DeleteFile(params string[] pathParts) {}

    public bool FolderExists(params string[] pathParts)
      => false;

    public Dictionary<string, DateTime> GetFiles(params string[] pathParts)
      => [];

    public IEnumerable<string> GetFolders(params string[] pathParts)
      => [];

    NullFileSystemAccessor() {}
}

public sealed class FileSystemAccessor : IFileSystemAccessor
{
    readonly ILogger _logger;

    public string AppBaseFolder
      => AppDomain.CurrentDomain.BaseDirectory;

    public bool CreateFolder(params string[] pathParts)
    {
        var path = Path.Combine(pathParts);
        try
        {
            Directory.CreateDirectory(path);
        }
        catch (Exception ex)
        {
            Error("creating folder", ex, path);
            return false;
        }
        return true;
    }

    public bool FolderExists(params string[] pathParts)
    {
        var path = Path.Combine(pathParts);
        try
        {
            return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    public IEnumerable<string> GetFolders(params string[] pathParts)
    {
        var path = Path.Combine(pathParts);

        if (string.IsNullOrWhiteSpace(path))
            return [];

        IEnumerable<string> folders;
        try
        {
            folders = [.. Directory.EnumerateDirectories(path)];
        }
        catch (Exception ex)
        {
            Error("getting folders", ex, path);
            folders = [];
        }

        return folders;
    }

    public Dictionary<string, DateTime> GetFiles(params string[] pathParts)
    {
        var path = Path.Combine(pathParts);

        if (string.IsNullOrWhiteSpace(path))
            return [];

        FileInfo[] files;
        try
        {
            var di = new DirectoryInfo(path);
            files = di.GetFiles();
        }
        catch (Exception ex)
        {
            Error("getting files from folder", ex, path);
            files = [];
        }

        return files
            .Select(f => new { f.FullName, f.LastWriteTimeUtc })
            .GroupBy(f => f.FullName)
            .ToDictionary(g => g.Key, g => g.First().LastWriteTimeUtc);
    }

    public Stream CreateReadStream(params string[] pathParts)
    {
        var path = Path.Combine(pathParts);
        try
        {
            return File.Open(path, FileMode.Open, FileAccess.Read);
        }
        catch (Exception ex)
        {
            Error("creating read stream", ex, path);
            return Stream.Null;
        }
    }

    public Stream CreateWriteStream(params string[] pathParts)
    {
        var path = Path.Combine(pathParts);
        try
        {
            return File.Open(Path.Combine(pathParts), FileMode.Create, FileAccess.Write);
        }
        catch (Exception ex)
        {
            Error("creating write stream", ex, path);
            return Stream.Null;
        }
    }

    public void DeleteFile(params string[] pathParts)
    {
        var path = Path.Combine(pathParts);
        try
        {
            File.Delete(path);
        }
        catch (Exception ex)
        {
            Error("deleting file", ex, path);
        }
    }

    #region Constructors

    public FileSystemAccessor(ILogger logger)
      => _logger = logger;

    #endregion

    #region Helpers

    void Error(string message, Exception ex, string path)
      => _logger.Log(1, $"Unexpected error {message}: {ToString(ex)} {path}");

    static string ToString(Exception ex) => $"{ex.GetType().Name}: {ex.Message}";

    #endregion
}
