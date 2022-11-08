using System;

using static EMU7800.Win32.Interop.Direct2DNativeMethods;

namespace EMU7800.Win32.Interop
{
    public class StaticBitmap : IDisposable
    {
        public static readonly StaticBitmap Default = new();

        #region Fields

        readonly byte[] _data = Array.Empty<byte>();

        public IntPtr BitmapPtr { get; private set; }

        public int HR { get; private set; }

        #endregion

        internal void Draw(D2D_RECT_F drect)
            => Direct2D_DrawStaticBitmap(BitmapPtr, drect);

        public void Initialize()
        {
            if (BitmapPtr != IntPtr.Zero || _data.Length == 0)
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

        StaticBitmap() {}

        public StaticBitmap(byte[] data)
        {
            _data = data ?? Array.Empty<byte>();
            Initialize();
        }

        #endregion
    }
}
