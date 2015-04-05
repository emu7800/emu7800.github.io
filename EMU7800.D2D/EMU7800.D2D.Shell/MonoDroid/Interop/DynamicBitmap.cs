using System;

namespace EMU7800.D2D.Interop
{
    public sealed class DynamicBitmap : IDisposable
    {
        public int HR { get; private set; }

        public int CopyFromMemory(byte[] data)
        {
            return 0;
        }

        #region IDisposable Members

        ~DynamicBitmap()
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

        public DynamicBitmap(SizeU size)
        {
        }
    };
}