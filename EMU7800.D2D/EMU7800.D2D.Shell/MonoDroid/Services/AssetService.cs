// © Mike Murphy

using EMU7800.Services.Dto;
using System;
using System.IO;
using System.Threading.Tasks;

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
                bytes = await GetBytesAsync(assetFilename);
            }
            catch (Exception ex)
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

        async Task<byte[]> GetBytesAsync(string assetFileName)
        {
            using (var s = MonoDroid.MainActivity.App.Assets.Open(assetFileName, Android.Content.Res.Access.Streaming))
            using (var ms = new MemoryStream())
            {
                await s.CopyToAsync(ms);
                return ms.ToArray();
            }
        }

        #endregion
    }
}
