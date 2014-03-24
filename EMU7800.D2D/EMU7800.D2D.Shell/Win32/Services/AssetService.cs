// © Mike Murphy

using System;
using System.IO;
using System.Threading.Tasks;
using EMU7800.Services.Dto;

namespace EMU7800.Services
{
    public partial class AssetService
    {
        #region Fields

        static readonly string _currentWorkingDir = AppDomain.CurrentDomain.BaseDirectory;

        #endregion

        public async Task<byte[]> GetAssetBytesAsync(Asset asset)
        {
            ClearLastErrorInfo();

            lock (_locker)
            {
                if (_resourceCache.ContainsKey(asset))
                    return _resourceCache[asset];
            }

            var assetFilename = _assetToFilenameMapping[asset];
            var path = ToLocalAssetsPath(assetFilename);

            byte[] bytes = null;
            try
            {
                bytes = await Task.Run(() => File.ReadAllBytes(path));
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

            return bytes;
        }

        #region Helpers

        string ToLocalAssetsPath(string fileName)
        {
            var root = Path.Combine(_currentWorkingDir, "Assets");
            var path = Path.Combine(root, fileName);
            return path;
        }

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
}
