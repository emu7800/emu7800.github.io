// © Mike Murphy

namespace EMU7800.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Dto;

    public static partial class AssetService
    {
        public static async Task<byte[]> GetAssetBytesAsync(Asset asset)
        {
            lock (_locker)
            {
                if (_resourceCache.ContainsKey(asset))
                    return _resourceCache[asset];
            }

            var assetFilename = _assetToFilenameMapping[asset];

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", assetFilename);
            var bytes = await Task.Run(() => File.ReadAllBytes(path));

            lock (_locker)
            {
                if (!_resourceCache.ContainsKey(asset))
                    _resourceCache.Add(asset, bytes);
            }

            return bytes;
        }
    }
}
