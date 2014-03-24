using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using EMU7800.Core;

namespace EMU7800.Win.DirectX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Emu7800DirectXStatistics
    {
        public int FrameCount;
        public int SegmentCount;
        public int Hz;
        public int FrameWidth;
        public int FrameHeight;
        public IntPtr DeviceWindow;
    }

    public class SafeBufferHandles : IDisposable
    {
        #region Fields

        GCHandle _frameBufferHandle, _paletteHandle;

        #endregion

        public FrameBuffer FrameBuffer { get; private set; }
        public int[] Palette { get; private set; }
        public IntPtr PalettePtr { get { return _paletteHandle.AddrOfPinnedObject(); } }
        public IntPtr FrameBufferPtr { get { return _frameBufferHandle.AddrOfPinnedObject(); } }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (_frameBufferHandle.IsAllocated)
                _frameBufferHandle.Free();
            if (_paletteHandle.IsAllocated)
                _paletteHandle.Free();
        }

        ~SafeBufferHandles()
        {
            Dispose(false);
        }

        #endregion

        #region Constructors

        public SafeBufferHandles(FrameBuffer frameBuffer, int[] palette)
        {
            if (frameBuffer == null)
                throw new ArgumentNullException("frameBuffer");
            if (palette == null)
                throw new ArgumentNullException("palette");
            if (palette.Length != 256)
                throw new ArgumentException("palette must be of size 256.");

            FrameBuffer = frameBuffer;
            Palette = palette;
            _frameBufferHandle = GCHandle.Alloc(frameBuffer.VideoBuffer, GCHandleType.Pinned);
            _paletteHandle = GCHandle.Alloc(palette, GCHandleType.Pinned);
        }

        #endregion
    }

    public class DirectXInitParameters
    {
        public int Adapter { get; set; }
        public Icon Icon { get; set; }
        public bool FullScreen { get; set; }
        public int TargetFrameWidth { get; set; }
        public int TargetFrameHeight { get; set; }
        public int DeviceWindowWidth { get; set; }
        public int DeviceWindowHeight { get; set; }
        public SafeBufferHandles FrameBufferHandles { get; set; }
    }

    public static class DirectXNativeMethods
    {
        public static Emu7800DirectXStatistics Statistics
        {
            get { return GetStatistics(); }
        }

        public static int  HResult         { get; private set; }
        public static bool IsDeviceOk      { get { return !IsDeviceErrored; } }
        public static bool IsDeviceLost    { get { return HResult == 1; } }
        public static bool IsDeviceStopped { get { return HResult == 2; } }
        public static bool IsDeviceErrored { get { return HResult < 0; } }

        public static void Initialize(DirectXInitParameters initParameters)
        {
            HResult = EMU7800DirectX_Initialize(
                initParameters.Adapter,
                initParameters.Icon.Handle,
                initParameters.FullScreen,
                initParameters.FrameBufferHandles.FrameBufferPtr,
                initParameters.FrameBufferHandles.FrameBuffer.VideoBufferByteLength,
                initParameters.FrameBufferHandles.PalettePtr,
                initParameters.FrameBufferHandles.FrameBuffer.VisiblePitch,
                initParameters.TargetFrameWidth,
                initParameters.TargetFrameHeight,
                initParameters.DeviceWindowWidth,
                initParameters.DeviceWindowHeight,
                true);
        }

        public static void PresentFrame(bool showSnow, int dx, int dy)
        {
            HResult = EMU7800DirectX_PresentFrame(showSnow, dx, dy);
        }

        public static void Shutdown()
        {
            EMU7800DirectX_Shutdown();
        }

        #region Helpers

        static Emu7800DirectXStatistics GetStatistics()
        {
            var stats = EMU7800DirectX_GetStatistics();
            return (Emu7800DirectXStatistics)Marshal.PtrToStructure(stats, typeof(Emu7800DirectXStatistics));
        }

        #endregion

        #region DllImports

        [DllImport("EMU7800.DirectX.dll"), SuppressUnmanagedCodeSecurity]
        static extern int EMU7800DirectX_Initialize(int adapter, IntPtr hIcon, bool fullScreen,
            IntPtr videoBuffer, int videoBufferSizeInBytes, IntPtr palette, int stride,
            int targetFrameWidth, int targetFrameHeight, int deviceWindowWidth, int deviceWindowHeight,
            bool usingWorkerThread);

        // unused
        // [DllImport("EMU7800.DirectX.dll"), SuppressUnmanagedCodeSecurity]
        // private static extern int EMU7800DirectX_TryRestoringLostDevice();

        [DllImport("EMU7800.DirectX.dll"), SuppressUnmanagedCodeSecurity]
        static extern int EMU7800DirectX_PresentFrame(bool showSnow, int dx, int dy);

        [DllImport("EMU7800.DirectX.dll"), SuppressUnmanagedCodeSecurity]
        static extern void EMU7800DirectX_Shutdown();

        [DllImport("EMU7800.DirectX.dll"), SuppressUnmanagedCodeSecurity]
        static extern IntPtr EMU7800DirectX_GetStatistics();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr LoadLibrary(string lpLibFileName);

        static DirectXNativeMethods()
        {
            switch (IntPtr.Size)
            {
                case 4:
                    LoadLibrary(@"x86\EMU7800.DirectX.dll");
                    break;
                case 8:
                    LoadLibrary(@"x64\EMU7800.DirectX.dll");
                    break;
            }
        }

        #endregion
    }
}
