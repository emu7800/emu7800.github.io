using System;

using static EMU7800.Win32.Interop.Direct2DNativeMethods;

namespace EMU7800.Win32.Interop
{
    public class StaticBitmap : IDisposable
    {
        #region Fields

        readonly byte[] _data;

        public IntPtr BitmapPtr { get; private set; }

        public int HR { get; private set; }

        #endregion

        public void Draw(D2D_RECT_F drect)
            => Direct2D_DrawStaticBitmap(BitmapPtr, drect);

        public void Initialize()
        {
            if (BitmapPtr != IntPtr.Zero)
                return;
            var ptr = BitmapPtr;
            unsafe
            {
                fixed (byte* bytes = _data)
                {
                    HR = Direct2D_CreateStaticBitmap(bytes, _data.Length, ref ptr);
                }
            }
            BitmapPtr = ptr;
        }

        #region IDispose Members

        public void Dispose()
        {
            if (BitmapPtr == IntPtr.Zero)
                return;
            Direct2D_ReleaseStaticBitmap(BitmapPtr);
            BitmapPtr = IntPtr.Zero;
            HR = 0;
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Constructors

        public StaticBitmap(byte[] data)
        {
            _data = data ?? Array.Empty<byte>();
            Initialize();
        }

        #endregion
    }
}
