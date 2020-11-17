// © Mike Murphy

namespace EMU7800.Services
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class Md5HashService
    {
        readonly static MD5CryptoServiceProvider _cryptoProvider = new();
        readonly static StringBuilder _sb = new();

        public static string ComputeHash(byte[] bytes)
        {
            var hashBytes = _cryptoProvider.ComputeHash(bytes ?? Array.Empty<byte>());
            _sb.Length = 0;
            for (var i = 0; i < 16; i++)
                _sb.AppendFormat("{0:x2}", hashBytes[i]);
            return _sb.ToString();
        }
    }
}
