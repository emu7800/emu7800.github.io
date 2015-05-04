// © Mike Murphy

using System.Security.Cryptography;
using System.Text;

namespace EMU7800.Services
{
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
}
