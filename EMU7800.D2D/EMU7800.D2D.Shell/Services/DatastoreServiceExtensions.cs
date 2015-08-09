// © Mike Murphy

#if WINDOWS_UWP || WINDOWS_APP || WINDOWS_PHONE_APP

namespace EMU7800.Services.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Windows.Graphics.Imaging;
    using Windows.Storage;
    using Windows.Storage.Search;
    using Windows.Storage.Streams;

    public static class DatastoreServiceExtensions
    {
        public static StorageFolder GetFolder(this IStorageFolder folder, string name)
        {
            var subFolder = folder.GetFolderAsync(name)
                .AsTask()
                    .ConfigureAwait(false)
                        .GetAwaiter()
                            .GetResult();
            return subFolder;
        }

        public static IStorageFile GetFile(this IStorageFolder folder, string name)
        {
            var file = folder.GetFileAsync(name)
               .AsTask()
                   .ConfigureAwait(false)
                       .GetAwaiter()
                           .GetResult();
            return file;
        }

        public static IEnumerable<IStorageFile> GetFiles(this StorageFileQueryResult sfqr)
        {
            var result = sfqr.GetFilesAsync()
                .AsTask()
                    .ConfigureAwait(false)
                        .GetAwaiter()
                            .GetResult();
            return result;
        }

        public static IEnumerable<IStorageFile> GetFiles(this IStorageFolder folder)
        {
            var result = folder.GetFilesAsync()
                .AsTask()
                    .ConfigureAwait(false)
                        .GetAwaiter()
                            .GetResult();
            return result;
        }

        public static byte[] GetBytes(this IStorageFile file)
        {
            var buffer = FileIO.ReadBufferAsync(file)
                    .AsTask()
                        .ConfigureAwait(false)
                            .GetAwaiter()
                                .GetResult();

            const int limit = 1 << 22;
            if (buffer.Length > limit)
                throw new InvalidOperationException("File exceeded size limit: " + limit);

            using (var dr = DataReader.FromBuffer(buffer))
            {
                var bytes = new byte[buffer.Length];
                dr.ReadBytes(bytes);
                return bytes;
            }
        }

        public async static Task<byte[]> GetBytesAsync(this IStorageFile file)
        {
            var buffer = await FileIO.ReadBufferAsync(file);

            const int limit = 1 << 22;
            if (buffer.Length > limit)
                throw new InvalidOperationException("File exceeded size limit: " + limit);

            using (var dr = DataReader.FromBuffer(buffer))
            {
                var bytes = new byte[buffer.Length];
                dr.ReadBytes(bytes);
                return bytes;
            }
        }

        public static IEnumerable<string> ReadUtf8Lines(this IStorageFile file)
        {
            var result = FileIO.ReadLinesAsync(file, UnicodeEncoding.Utf8)
                .AsTask()
                    .ConfigureAwait(false)
                        .GetAwaiter()
                            .GetResult();
            return result;
        }

        public static void WriteUtf8Text(this IStorageFile file, string text)
        {
            FileIO.WriteTextAsync(file, text ?? string.Empty, UnicodeEncoding.Utf8)
                .AsTask()
                   .ConfigureAwait(false)
                       .GetAwaiter()
                           .GetResult();
        }

        public static void WriteUtf8Lines(this IStorageFile file, IEnumerable<string> lines)
        {
            FileIO.WriteLinesAsync(file, lines ?? new string[0], UnicodeEncoding.Utf8)
                .AsTask()
                   .ConfigureAwait(false)
                       .GetAwaiter()
                           .GetResult();
        }

        public static StorageFile CreateFile(this IStorageFolder folder, string desiredName)
        {
            var file = folder.CreateFileAsync(desiredName, CreationCollisionOption.ReplaceExisting)
                .AsTask()
                    .ConfigureAwait(false)
                        .GetAwaiter()
                            .GetResult();
            return file;
        }

        public static Stream OpenStreamForRead(this IStorageFile file)
        {
            var stream = file.OpenStreamForReadAsync()
                .ConfigureAwait(false)
                    .GetAwaiter()
                        .GetResult();
            return stream;
        }

        public static Stream OpenStreamForWrite(this IStorageFile file)
        {
            var stream = file.OpenStreamForWriteAsync()
                .ConfigureAwait(false)
                    .GetAwaiter()
                        .GetResult();
            return stream;
        }

        public static IRandomAccessStream Open(this IStorageFile file, FileAccessMode accessMode)
        {
            var stream = file.OpenAsync(accessMode)
                .AsTask()
                    .ConfigureAwait(false)
                        .GetAwaiter()
                            .GetResult();
            return stream;
        }

        public static void Flush(this BitmapEncoder encoder)
        {
            encoder.FlushAsync()
                .AsTask()
                    .ConfigureAwait(false)
                        .GetAwaiter()
                            .GetResult();
        }
    }
}

#endif
