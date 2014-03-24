using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace EMU7800.Win
{
    /// <summary>
    /// An abstraction that enhances filesystem pathing to support file archives (currently just .zip archives.)
    /// </summary>
    public class RomFileAccessor
    {
        /// <summary>
        /// Examine the specified path to determine if it can denote a distinct ROM.
        /// </summary>
        /// <param name="path"></param>
        public bool IsValidRomFileName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var splitPath = path.Split(new[] { ',' });
            if (splitPath.Length > 1)
            {
                var ext0 = Path.GetExtension(splitPath[0]) ?? string.Empty;
                if (!ext0.Equals(".zip", StringComparison.OrdinalIgnoreCase))
                    return false;
                splitPath[0] = splitPath[1];
            }

            const string notRomFileExtensions = ".exe|.dll|.pdb|.csv|.xml|.emu|.emurec|.zip";
            var ext = Path.GetExtension(splitPath[0]) ?? string.Empty;
            return (notRomFileExtensions.IndexOf(ext, StringComparison.OrdinalIgnoreCase) < 0);
        }

        /// <summary>
        /// Returns an array of bytes from the specified ROM file.
        /// Members of .zip archives are supported via the custom convention: x:\rooted.path.to.file.zip,zipmembername
        /// The 128 byte header is stripped from *.a78 files.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IOException"></exception>
        public byte[] GetRomBytes(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");
            return ReadRomBytes(path);
        }

        /// <summary>
        /// Returns a list of filenames found in the specified path, when it is a directory.
        /// The following extensions are filtered: .exe, .dll, .pdb, .csv, .xml, .emu, .emurec.
        /// Any .zip files found have their members enumerated using the convention: x:\rooted.path.to.file.zip,zipmembername
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IOException"></exception>
        public IEnumerable<string> GetRomFullNames(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");
            var isDir = (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
            return isDir ? GetFiles(path) : GetFiles(Path.GetDirectoryName(path));
        }

        #region Helpers

        static IEnumerable<string> GetFiles(string path)
        {
            const string extensionsToAvoid = ".exe|.dll|.pdb|.csv|.xml|.emu|.emurec";
            var query = from file in Directory.EnumerateFiles(path)
                        let ext = Path.GetExtension(file) ?? string.Empty
                        where (extensionsToAvoid.IndexOf(ext, StringComparison.OrdinalIgnoreCase) < 0)
                        select file;

            foreach (var fullName in query)
            {
                var ext = Path.GetExtension(fullName) ?? string.Empty;
                if (ext.Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    using (var zip = new ZipArchive(File.OpenRead(fullName)))
                    {
                        foreach (var ze in zip.Entries)
                            yield return fullName + "," + ze.Name;
                    }
                }
                else
                {
                    yield return fullName;
                }
            }
        }

        static byte[] ReadRomBytes(string path)
        {
            byte[] bytes = null;
            var isA78 = false;

            var splitPath = path.Split(new[] { ',' });
            var ext0 = Path.GetExtension(splitPath[0]) ?? string.Empty;

            if (splitPath.Length == 1)
            {
                isA78 = ext0.Equals(".a78", StringComparison.OrdinalIgnoreCase);
                using (var fs = new FileStream(splitPath[0], FileMode.Open))
                using (var br = new BinaryReader(fs))
                {
                    bytes = br.ReadBytes(0x25000);
                }
            }
            else if (ext0.Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                var ext1 = Path.GetExtension(splitPath[1]) ?? string.Empty;
                isA78 = ext1.Equals(".a78", StringComparison.OrdinalIgnoreCase);
                using (var ms = new MemoryStream())
                using (var zip = new ZipArchive(File.OpenRead(splitPath[0])))
                {
                    foreach (var ze in zip.Entries.Where(ze => ze.Name.Equals(splitPath[1], StringComparison.OrdinalIgnoreCase)))
                    {
                        using (var zs = ze.Open())
                        {
                            zs.CopyTo(ms);
                        }
                        bytes = ms.ToArray();
                        break;
                    }
                }
            }

            if (bytes == null)
                return null;

            if (isA78)
            {
                const int offset = 128;
                var newBytes = new byte[bytes.Length - offset];
                Buffer.BlockCopy(bytes, offset, newBytes, 0, newBytes.Length);
                bytes = newBytes;
            }

            return bytes;
        }

        #endregion
    }
}
