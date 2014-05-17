// © Mike Murphy

using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;
using EMU7800.Services.Dto;

namespace EMU7800.Services
{
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
}
