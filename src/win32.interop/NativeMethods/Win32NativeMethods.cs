// © Mike Murphy

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800.Win32.Interop
{
    internal unsafe static class Win32NativeMethods
    {
        public static IntPtr Win32_Initialize()
            => Win32_Initialize(
                GetIntPtr(Win32Window.RaiseKeyboardKeyPressedDelegate),
                GetIntPtr(Win32Window.RaiseMouseMovedDelegate),
                GetIntPtr(Win32Window.RaiseMouseButtonChangedDelegate),
                GetIntPtr(Win32Window.RaiseMouseWheelChangedDelegate),
                GetIntPtr(Win32Window.RaiseLURCycleDelegate),
                GetIntPtr(Win32Window.RaiseVisibilityChangedDelegate),
                GetIntPtr(Win32Window.RaiseResizedDelegate));

        static IntPtr GetIntPtr<TDelegate>(TDelegate d) where TDelegate : notnull
            => Marshal.GetFunctionPointerForDelegate(d);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        static extern IntPtr Win32_Initialize(
            IntPtr keyboardkeypressedcb,
            IntPtr mousemovedcb,
            IntPtr mousebuttonchangedcb,
            IntPtr mousewheelchangedcb,
            IntPtr lurcyclecb,
            IntPtr visibilitychangedcb,
            IntPtr resizedcb);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Win32_ProcessEvents(IntPtr hWnd);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Win32_Quit();
    }
}
