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

    public partial class AssetService
    {
        public async Task<byte[]> GetAssetBytesAsync(Asset asset)
        {
            ClearLastErrorInfo();

            lock (_locker)
            {
                if (_resourceCache.ContainsKey(asset))
                    return _resourceCache[asset];
            }

            var assetFilename = _assetToFilenameMapping[asset];

            byte[] bytes = null;
            try
            {
                var file = await Package.Current.InstalledLocation.GetFileAsync(@"Assets\" + assetFilename);
                bytes = await GetBytesAsync(file);
            }
            catch (AggregateException ex)
            {
                LastErrorInfo = new ErrorInfo(ex, "GetAssetBytesAsync: Failure loading asset: {0}", assetFilename);
            }

            lock (_locker)
            {
                if (bytes != null && !_resourceCache.ContainsKey(asset))
                    _resourceCache.Add(asset, bytes);
            }

            return bytes;
        }

        #region Helpers

        async Task<byte[]> GetBytesAsync(IStorageFile file)
        {
            var buffer = await FileIO.ReadBufferAsync(file);

            const int limit = 1 << 22;
            if (buffer.Length > limit)
            {
                LastErrorInfo = new ErrorInfo(LastErrorInfo, "GetBytesAsync: File exceeded {0} size limit.", limit);
                return null;
            }

            using (var dr = DataReader.FromBuffer(buffer))
            {
                var bytes = new byte[buffer.Length];
                dr.ReadBytes(bytes);
                return bytes;
            }
        }

        #endregion
    }

#elif WIN32 || MONODROID

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Dto;

    public partial class AssetService
    {
        public async Task<byte[]> GetAssetBytesAsync(Asset asset)
        {
            ClearLastErrorInfo();

            lock (_locker)
            {
                if (_resourceCache.ContainsKey(asset))
                    return _resourceCache[asset];
            }

            var assetFilename = _assetToFilenameMapping[asset];

            byte[] bytes = null;
            try
            {
#if WIN32
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", assetFilename);
                bytes = await Task.Run(() => File.ReadAllBytes(path));
#elif MONODROID
                using (var input = MonoDroid.MainActivity.App.Assets.Open(assetFilename))
                using (var ms = new MemoryStream())
                {
                    await input.CopyToAsync(ms);
                    bytes = ms.ToArray();
                }
#endif
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                    throw;
                LastErrorInfo = new ErrorInfo(ex, "GetAssetBytesAsync: Failure loading asset: {0}", assetFilename);
            }

            lock (_locker)
            {
                if (bytes != null && !_resourceCache.ContainsKey(asset))
                    _resourceCache.Add(asset, bytes);
            }

            return bytes ?? new byte[0];
        }

        #region Helpers

        static bool IsCriticalException(Exception ex)
        {
            return ex is OutOfMemoryException
                || ex is StackOverflowException
                || ex is System.Threading.ThreadAbortException
                || ex is System.Threading.ThreadInterruptedException
                || ex is TypeInitializationException;
        }

        #endregion
    }

#else

#error "Missing platform symbol for AssetService"

#endif

}
