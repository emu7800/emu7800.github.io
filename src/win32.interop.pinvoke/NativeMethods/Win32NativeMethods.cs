// © Mike Murphy

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800.Win32.Interop
{
    internal unsafe static class Win32NativeMethods
    {
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static void RaiseKeyboardKeyPressed(ushort vKey, byte down) => Win32Window.KeyboardKeyPressed(vKey, down != 0);

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static void RaiseMouseMoved(int x, int y, int dx, int dy) => Win32Window.MouseMoved(x, y, dx, dy);

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static void RaiseMouseButtonChanged(int x, int y, byte down) => Win32Window.MouseButtonChanged(x, y, down != 0);

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static void RaiseMouseWheelChanged(int x, int y, int delta) => Win32Window.MouseWheelChanged(x, y, delta);

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static void RaiseLURCycle() => Win32Window.LURCycle();

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static void RaiseVisibilityChanged(bool isVisible) => Win32Window.VisibilityChanged(isVisible);

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static void RaiseResized(int w, int h) => Win32Window.Resized(w, h);

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static void RaiseDeviceChanged() => Win32Window.DeviceChanged();

        public static IntPtr Win32_Initialize()
            => Win32_Initialize(
                (delegate* unmanaged [Cdecl]<ushort, byte, void>)&RaiseKeyboardKeyPressed,
                (delegate* unmanaged [Cdecl]<int, int, int, int, void>)&RaiseMouseMoved,
                (delegate* unmanaged [Cdecl]<int, int, byte, void>)&RaiseMouseButtonChanged,
                (delegate* unmanaged [Cdecl]<int, int, int, void>)&RaiseMouseWheelChanged,
                (delegate* unmanaged [Cdecl]<void>)&RaiseLURCycle,
                (delegate* unmanaged [Cdecl]<bool, void>)&RaiseVisibilityChanged,
                (delegate* unmanaged [Cdecl]<int, int, void>)&RaiseResized,
                (delegate* unmanaged [Cdecl]<void>)&RaiseDeviceChanged);

        [DllImport("EMU7800.Win32.Interop.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        static extern IntPtr Win32_Initialize(
            delegate* unmanaged [Cdecl]<ushort, byte, void> keyboardkeypressedcb,
            delegate* unmanaged [Cdecl]<int, int, int, int, void> mousemovedcb,
            delegate* unmanaged [Cdecl]<int, int, byte, void> mousebuttonchangedcb,
            delegate* unmanaged [Cdecl]<int, int, int, void> mousewheelchangedcb,
            delegate* unmanaged [Cdecl]<void> lurcyclecb,
            delegate* unmanaged [Cdecl]<bool, void> visibilitychangedcb,
            delegate* unmanaged [Cdecl]<int, int, void> resizedcb,
            delegate* unmanaged [Cdecl]<void> devicechangedcb);

        [DllImport("EMU7800.Win32.Interop.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Win32_ProcessEvents(IntPtr hWnd, int nCmdShow);

        [DllImport("EMU7800.Win32.Interop.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Win32_Quit();
    }
}
