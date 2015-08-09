// © Mike Murphy

namespace EMU7800.Services
{

#if WINDOWS_UWP || WINDOWS_APP || WINDOWS_PHONE_APP

    using Windows.Security.Cryptography;
    using Windows.Security.Cryptography.Core;

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

#elif WIN32 || MONODROID

    using System.Security.Cryptography;
    using System.Text;

    public class Md5HashService
    {
        readonly MD5CryptoServiceProvider _cryptoProvider = new MD5CryptoServiceProvider();
        readonly StringBuilder _sb = new StringBuilder();

        public string ComputeHash(byte[] bytes)
        {
            var hashBytes = _cryptoProvider.ComputeHash(bytes ?? new byte[0]);
            _sb.Length = 0;
            for (var i = 0; i < 16; i++)
                _sb.AppendFormat("{0:x2}", hashBytes[i]);
            return _sb.ToString();
        }
    }

#else

#error "Missing platform symbol for Md5HashService"

#endif

}
