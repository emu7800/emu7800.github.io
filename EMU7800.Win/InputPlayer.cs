using System;
using System.IO;
using EMU7800.Core;

namespace EMU7800.Win
{
    internal class InputPlayer : IDisposable
    {
        readonly ILogger _logger;
        readonly string _fullName;
        BinaryReader _binaryReader;

        public bool ValidEmuRecFile { get; private set; }
        public string MD5 { get; private set; }

        public event EventHandler<EventArgs> InputEnded;

        #region Constructors

        public InputPlayer(string fullName, ILogger logger)
        {
            if (fullName == null)
                throw new ArgumentNullException("fullName");
            if (logger == null)
                throw new ArgumentNullException("logger");

            _fullName = fullName;
            _logger = logger;

            var magicNumber = 0;
            try
            {
                _binaryReader = new BinaryReader(new FileStream(_fullName, FileMode.Open));
                magicNumber = _binaryReader.ReadInt32();
            }
            catch (IOException ex)
            {
                _logger.WriteLine(ex);
            }
            if (magicNumber == InputRecorder.EMUREC_MAGIC_NUMBER && _binaryReader != null)
            {
                MD5 = _binaryReader.ReadString();
                ValidEmuRecFile = !string.IsNullOrEmpty(MD5);
            }
            if (!ValidEmuRecFile)
            {
                _binaryReader = null;
                _logger.WriteLine("Not a valid or available playback file: {0}", fullName);
            }
        }

        #endregion

        public object OnInputAdvancing(int[] inputBuffer)
        {
            if (inputBuffer == null || _binaryReader == null) return null;
            try
            {
                for (var i = 0; i < inputBuffer.Length; i++)
                {
                    inputBuffer[i] = _binaryReader.ReadInt32();
                }
            }
            catch (EndOfStreamException)
            {
                _binaryReader.Close();
                _binaryReader.Dispose();
                _binaryReader = null;
                if (InputEnded != null)
                {
                    InputEnded(null, new EventArgs());
                    InputEnded = null;
                }
                _logger.WriteLine("End of playback file reached: {0}", _fullName);
            }
            return null;
        }

        public void Close()
        {
            if (_binaryReader == null)
                return;
            _binaryReader.Close();
        }

        public void Dispose()
        {
            if (_binaryReader != null)
                _binaryReader.Dispose();
            _binaryReader = null;
        }
    }
}
