using System;
using System.IO;
using EMU7800.Core;

namespace EMU7800.Win
{
    internal class InputRecorder : IDisposable
    {
        public const int EMUREC_MAGIC_NUMBER = 0x112358;

        readonly ILogger _logger;
        readonly string _fullName;
        BinaryWriter _binaryWriter;

        #region Constructors

        public InputRecorder(string fullName, string md5Rom, ILogger logger)
        {
            if (fullName == null)
                throw new ArgumentNullException("fullName");
            if (md5Rom == null)
                throw new ArgumentNullException("md5Rom");
            if (logger == null)
                throw new ArgumentNullException("logger");

            _fullName = fullName;
            _logger = logger;

            _binaryWriter = new BinaryWriter(new FileStream(fullName, FileMode.Create));
            _binaryWriter.Write(EMUREC_MAGIC_NUMBER);
            _binaryWriter.Write(md5Rom);
            _binaryWriter.Flush();
        }

        #endregion

        public object OnInputAdvanced(int[] inputBuffer)
        {
            if (_binaryWriter == null || inputBuffer == null)
                return null;

            try
            {
                for (var i = 0; i < inputBuffer.Length; i++)
                {
                    _binaryWriter.Write(inputBuffer[i]);
                }
            }
            catch (IOException ex)
            {
                _logger.WriteLine(ex);
            }
            return null;
        }

        public void Close()
        {
            if (_binaryWriter == null)
                return;
            _binaryWriter.Flush();
            _binaryWriter.Close();
            _logger.WriteLine("End of recording: {0}", _fullName);
         }

        public void Dispose()
        {
            if (_binaryWriter != null)
                _binaryWriter.Dispose();
            _binaryWriter = null;
        }
    }
}