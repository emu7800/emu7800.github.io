// © Mike Murphy

namespace EMU7800.Services
{

#if WINDOWS_UWP || WINDOWS_APP || WINDOWS_PHONE_APP

    using System;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.Storage;
    using Windows.Storage.Streams;
    using Dto;
    using System.Collections.Generic;
    using System.Linq;

    public partial class AssetService
    {
        public async Task<byte[]> GetAssetBytesAsync(Asset asset)
        {
            lock (_locker)
            {
                if (_resourceCache.ContainsKey(asset))
                    return Ok(new BytesType(_resourceCache[asset]));
            }

            var assetFilename = _assetToFilenameMapping[asset];

            Result<BytesType> result;
            try
            {
                var file = await Package.Current.InstalledLocation.GetFileAsync(@"Assets\" + assetFilename);
                result = await GetBytesAsync(file);
            }
            catch (AggregateException ex)
            {
                return Fail<BytesType>("GetAssetBytesAsync: Failure loading asset: " + assetFilename, ex);
            }

            lock (_locker)
            {
                if (result.IsOk && !_resourceCache.ContainsKey(asset))
                    _resourceCache.Add(asset, result.Value.Bytes);
            }

            return result;
        }

        #region Helpers

        async Task<Result<BytesType>> GetBytesAsync(IStorageFile file)
        {
            var buffer = await FileIO.ReadBufferAsync(file);

            const int limit = 1 << 22;
            if (buffer.Length > limit)
            {
                return Fail<BytesType>($"GetBytesAsync: File exceeded {limit} size limit");
            }

            using var dr = DataReader.FromBuffer(buffer);
            var bytes = new byte[buffer.Length];
            dr.ReadBytes(bytes);
            return Ok(new BytesType(bytes));
        }

        static Result<T> Ok<T>(T value) where T : class, new()
            => ResultHelper.Ok(value);

        static Result<T> Fail<T>(string message, Exception ex) where T : class, new()
            => ResultHelper.Fail<T>(ToResultMessage(message, ex));

        static Result<T> Fail<T>(string message) where T : class, new()
            => ResultHelper.Fail<T>(message);

        static string ToResultMessage(string message, Exception ex)
            => message + $": Unexpected exception: {ex.GetType().Name}: " + ex.Message;

        #endregion
    }

#elif WIN32 || MONODROID

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Dto;

    public partial class AssetService
    {
        public static async Task<byte[]> GetAssetBytesAsync(Asset asset)
        {
            lock (_locker)
            {
                if (_resourceCache.ContainsKey(asset))
                    return _resourceCache[asset];
            }

            var assetFilename = _assetToFilenameMapping[asset];

#if WIN32
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", assetFilename);
            var bytes = await Task.Run(() => File.ReadAllBytes(path));
#elif MONODROID
                using var input = MonoDroid.MainActivity.App.Assets.Open(assetFilename);
                using var ms = new MemoryStream();
                await input.CopyToAsync(ms);
                var bytes = ms.ToArray();
#endif

            lock (_locker)
            {
                if (!_resourceCache.ContainsKey(asset))
                    _resourceCache.Add(asset, bytes);
            }

            return bytes;
        }
    }

#else

#error "Missing platform symbol for AssetService"

#endif

}
