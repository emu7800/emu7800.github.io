// © Mike Murphy

using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace EMU7800.Services
{
    public class Md5HashService
    {
        readonly HashAlgorithmProvider _openedMd5HashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

        public string ComputeHash(byte[] bytes)
        {
            var buffer = CryptographicBuffer.CreateFromByteArray(bytes ?? new byte[0]);
            var hashedData = _openedMd5HashAlgorithm.HashData(buffer);
            var result = CryptographicBuffer.EncodeToHexString(hashedData);
            return result;
        }
    }
}
