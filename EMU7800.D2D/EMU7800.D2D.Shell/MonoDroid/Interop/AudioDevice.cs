using System;

namespace EMU7800.D2D.Interop
{
    public sealed class AudioDevice : IDisposable
    {
        public int ErrorCode { get; private set; }

        public int BuffersQueued { get; private set; }

        public int SubmitBuffer(byte[] buffer)
        {
            return 0;
        }

        public uint GetWaveOutVolume()
        {
            return 0;
        }

        public void SetWaveOutVolume(uint dwVolume)
        {
        }

        #region IDisposable Members

        ~AudioDevice()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        public AudioDevice(int frequency, int bufferSizeInBytes, int queueLength)
        {
        }
    }
}