using System;

namespace EMU7800.D2D.Interop
{
    public class StaticBitmap : IDisposable
    {
        public int HR { get; private set; }

        #region IDisposable Members

    ~StaticBitmap()
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

        public StaticBitmap(byte[] data)
        {
        }
    }
}